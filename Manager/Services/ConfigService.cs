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

public class ConfigService : IConfigService
{
    private readonly ILogger<ConfigService> _logger;
    private readonly MirrorContext _context;
    private readonly IMapper _mapper;
    private readonly IRecurringJobManager _jobManager;
    public ConfigService(MirrorContext context, ILogger<ConfigService> logger, IMapper mapper, IRecurringJobManager jobManager)
    {
        _context = context;
        _logger = logger;
        _mapper = mapper;
        _jobManager = jobManager;
    }

    /// <summary>
    /// Load YAML mirror configs 
    /// </summary>
    public async Task LoadConfigAsync()
    {
        var deserializer = new DeserializerBuilder().Build();

        // Get all recurring jobs
        var restRecurringJobs = JobStorage.Current.GetConnection().GetRecurringJobs();

        // Load sync configs
        var syncDirInfo = new DirectoryInfo(Constants.SyncConfigPath);
        foreach (var fi in syncDirInfo.GetFiles("*.yml", SearchOption.AllDirectories))
        {
            var mirrorConfig = deserializer.Deserialize<MirrorConfig>(await File.ReadAllTextAsync(fi.FullName));
            mirrorConfig.Id = Path.GetFileNameWithoutExtension(fi.Name);

            var mirrorItem = await _context.Mirrors.FindAsync(mirrorConfig.Id);
            if (mirrorItem == null)
            {
                var newMirrorItem = _mapper.Map<MirrorItem>(mirrorConfig);
                await _context.Mirrors.AddAsync(newMirrorItem);
            }
            else
            {
                // Update if existed
                mirrorItem.UpdateFromConfig(mirrorConfig);
            }

            _logger.LogInformation("Loaded Mirror Sync Config {ConfigName}", mirrorConfig.Id);

            // Add recurring job
            _jobManager.AddOrUpdate<ScheduleService>(
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

            _logger.LogInformation("Update Mirror Sync Job {JobId}", $"{Constants.HangFireJobPrefix}{mirrorConfig.Id}");
        }

        // Remove rest recurring jobs
        foreach (var job in restRecurringJobs)
        {
            _jobManager.RemoveIfExists(job.Id);
        }

        // Load index configs
        var indexDirInfo = new DirectoryInfo(Constants.IndexConfigPath);
        foreach (var fi in indexDirInfo.GetFiles("*.yml", SearchOption.AllDirectories))
        {
            var indexConfig =
                deserializer.Deserialize<FileIndexConfig>(await File.ReadAllTextAsync(fi.FullName));
            indexConfig.Id = Path.GetFileNameWithoutExtension(fi.Name);

            var indexConfigItem = await _context.IndexConfigs.FindAsync(indexConfig.Id);
            if (indexConfigItem == null)
            {
                await _context.IndexConfigs.AddAsync(indexConfig);
            }
            else
            {
                // Update if existed
                _context.Entry(indexConfigItem).CurrentValues.SetValues(indexConfig);
            }

            _logger.LogInformation("Loaded File Index Config {ConfigName}", indexConfig.Id);
        }

        await _context.SaveChangesAsync();
    }
}