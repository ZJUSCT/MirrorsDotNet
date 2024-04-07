using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Orchestrator.DataModels;
using Orchestrator.Utils;

namespace Orchestrator.Services;

public class StateStore : IStateStore
{
    private readonly IConfiguration _conf;
    private readonly ILogger<StateStore> _log;
    private readonly OrchContext _db;
    private ReaderWriterLockSlim _rwLock = new();
    private Dictionary<string, MirrorItemInfo> _mirrorItems = [];

    public StateStore(IConfiguration conf, ILogger<StateStore> log, OrchContext db)
    {
        _conf = conf;
        _log = log;
        _db = db;
        Reload();
    }

    public void Reload()
    {
        var confPath = _conf["ConfPath"];
        if (string.IsNullOrWhiteSpace(confPath))
        {
            throw new Exception("Mirror configs path not set");
        }

        // Load configs from fs
        List<ConfigInfo> confs;
        try
        {
            confs = Directory
                .EnumerateFiles(confPath!, "*.json")
                .Select(path => File.ReadAllText(path, Encoding.UTF8))
                .Select(content => JsonSerializer.Deserialize<ConfigInfoRaw>(content))
                .Select(r => new ConfigInfo(r!))
                .ToList();
        }
        catch (Exception e)
        {
            _log.LogError("Hot reload failed: {e}", e);
            return;
        }

        // Load states from db
        var savedInfos = _db.SavedInfos.AsNoTracking().ToList();
        var currentMirrorItems = new Dictionary<string, MirrorItemInfo>(_mirrorItems);

        // 1. remove deleted configs
        foreach (var item in currentMirrorItems)
        {
            if (confs.Any(x => x.Id == item.Key))
            {
                continue;
            }

            currentMirrorItems.Remove(item.Key);
        }

        // 2. add new configs to memory and db
        foreach (var conf in confs)
        {
            if (currentMirrorItems.Any(x => x.Key == conf.Id))
            {
                continue;
            }

            var newInfo = new MirrorItemInfo
            {
                Config = conf,
                Status = MirrorStatus.Unknown,
                LastSyncAt = DateTime.MinValue,
            };
            var savedInfo = savedInfos.FirstOrDefault(x => x.Id == conf.Id);
            if (savedInfo != null)
            {
                newInfo.Status = savedInfo.Status;
                newInfo.LastSyncAt = savedInfo.LastSyncAt.ToLocalTime();
            }
            else
            {
                _db.SavedInfos.Add(new SavedInfo
                {
                    Id = conf.Id,
                    Status = MirrorStatus.Unknown,
                    LastSyncAt = DateTime.MinValue,
                });
            }

            currentMirrorItems.Add(conf.Id, newInfo);
        }

        try
        {
            _db.SaveChanges();
        }
        catch (Exception e)
        {
            _log.LogError("Failed to save new mirror items: {e}", e);
        }

        // 3. apply changes
        using var guard = new ScopeWriteLock(_rwLock);
        _mirrorItems = currentMirrorItems;
    }

    public IEnumerable<KeyValuePair<string, MirrorItemInfo>> GetMirrorItemInfos()
    {
        return _mirrorItems;
    }

    public void SetMirrorInfo(string id, MirrorStatus status, DateTime lastSyncAt)
    {
        var item = _mirrorItems.FirstOrDefault(x => x.Key == id);
        if (item.Key == null)
        {
            return;
        }

        using (var guard = new ScopeWriteLock(_rwLock))
        {
            item.Value.Status = status;
            item.Value.LastSyncAt = lastSyncAt;
        }

        _db.SavedInfos.Update(new SavedInfo
        {
            Id = id,
            Status = status,
            LastSyncAt = lastSyncAt.ToUniversalTime(),
        });

        try
        {
            _db.SaveChanges();
        }
        catch (Exception e)
        {
            _log.LogError("Failed to save mirror info to db: {e}", e);
        }
    }
}