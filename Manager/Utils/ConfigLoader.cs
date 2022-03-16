using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using Manager.Jobs;
using Manager.Models;
using Microsoft.Extensions.Logging;
using Quartz;
using YamlDotNet.Serialization;

namespace Manager.Utils;

public class ConfigLoader
{
    /// <summary>
    /// Load YAML mirror configs 
    /// </summary>
    /// <param name="mirrorContext">Mirror status context</param>
    /// <param name="mapper">Auto mapper instance</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="schedulerFactory">Quartz scheduler factory instance</param>
    public static async Task LoadConfigAsync(MirrorContext mirrorContext, IMapper mapper, ILogger logger,
        ISchedulerFactory schedulerFactory)
    {
        var deserializer = new DeserializerBuilder().Build();

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

            // Create sync job for normal mirror
            if (mirrorConfig.Type != Mirror.MirrorType.Normal) continue;
            var jobDetail = JobBuilder.Create<SyncJob>()
                .WithIdentity($"sync-job-{mirrorConfig.Id}", "sync-group")
                .UsingJobData(Constants.JobDataMapImage, mirrorConfig.ProviderImage)
                .UsingJobData(Constants.JobDataMapUpStream, mirrorConfig.Upstream)
                .UsingJobData(Constants.JobDataMapExtraArgs, mirrorConfig.ExtraArgs)
                .Build();
            var trigger = TriggerBuilder.Create()
                .WithIdentity($"sync-trigger-{mirrorConfig.Id}", "sync-group")
                .WithCronSchedule(mirrorConfig.Cron)
                .Build();
            var scheduler = await schedulerFactory.GetScheduler();

            // First delete old job, will return false if not existed
            await scheduler.DeleteJob(jobDetail.Key);
            logger.LogInformation("Created Mirror Job Config {JobName} {JobGroup}", jobDetail.Key.Name,
                jobDetail.Key.Group);
            await scheduler.ScheduleJob(jobDetail, trigger);
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