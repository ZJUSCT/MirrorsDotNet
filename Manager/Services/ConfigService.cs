using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using Hangfire;
using Hangfire.Storage;
using Manager.Models;
using Manager.Utils;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;

namespace Manager.Services;

public class ConfigService
{
    /// <summary>
    /// Load YAML mirror configs 
    /// </summary>
    /// <param name="mirrorContext">Mirror status context</param>
    /// <param name="mapper">Auto mapper instance</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="jobManager">HungFire RecurringJobManager</param>
    public static async Task LoadConfigAsync(MirrorContext mirrorContext, IMapper mapper, ILogger logger,
        IRecurringJobManager jobManager)
    {
        var deserializer = new DeserializerBuilder().Build();

        // Get all recurring jobs
        var restRecurringJobs = JobStorage.Current.GetConnection().GetRecurringJobs();

        // Load sync configs
        var syncDirInfo = new DirectoryInfo(Constants.SyncConfigPath);
        foreach (var fi in syncDirInfo.GetFiles("*.yml", SearchOption.AllDirectories))
        {
            var mirrorConfig = deserializer.Deserialize<Mirror.MirrorConfig>(await File.ReadAllTextAsync(fi.FullName));
            mirrorConfig.Id = Path.GetFileNameWithoutExtension(fi.Name);

            var mirrorItem = await mirrorContext.Mirrors.FindAsync(mirrorConfig.Id);
            if (mirrorItem == null)
            {
                var newMirrorItem = mapper.Map<Mirror.MirrorItem>(mirrorConfig);
                await mirrorContext.Mirrors.AddAsync(newMirrorItem);
            }
            else
            {
                // Update if existed
                mirrorItem.UpdateFromConfig(mirrorConfig);
            }

            logger.LogInformation("Loaded Mirror Sync Config {ConfigName}", mirrorConfig.Id);

            // Add recurring job
            jobManager.AddOrUpdate<ScheduleService>(
                $"{Constants.HangFireJobPrefix}{mirrorConfig.Id}",
                x => x.Schedule(mirrorConfig.Id),
                mirrorConfig.Cron
            );
            var res = restRecurringJobs.Find(x => x.Id == $"{Constants.HangFireJobPrefix}{mirrorConfig.Id}");
            if (res != null)
            {
                // Remove from rest recurring job list
                restRecurringJobs.Remove(res);
            }

            logger.LogInformation("Update Mirror Sync Job {JobId}", $"{Constants.HangFireJobPrefix}{mirrorConfig.Id}");
        }

        // Remove rest recurring jobs
        foreach (var job in restRecurringJobs)
        {
            jobManager.RemoveIfExists(job.Id);
        }

        // Load index configs
        var indexDirInfo = new DirectoryInfo(Constants.IndexConfigPath);
        foreach (var fi in indexDirInfo.GetFiles("*.yml", SearchOption.AllDirectories))
        {
            var indexConfig =
                deserializer.Deserialize<Mirror.FileIndexConfig>(await File.ReadAllTextAsync(fi.FullName));
            indexConfig.Id = Path.GetFileNameWithoutExtension(fi.Name);

            var indexConfigItem = await mirrorContext.IndexConfigs.FindAsync(indexConfig.Id);
            if (indexConfigItem == null)
            {
                await mirrorContext.IndexConfigs.AddAsync(indexConfig);
            }
            else
            {
                // Update if existed
                mirrorContext.Entry(indexConfigItem).CurrentValues.SetValues(indexConfig);
            }

            logger.LogInformation("Loaded File Index Config {ConfigName}", indexConfig.Id);
        }

        await mirrorContext.SaveChangesAsync();
    }
}
