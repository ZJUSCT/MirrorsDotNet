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
    private readonly IServiceProvider _sp;
    private Dictionary<string, MirrorItemInfo> _mirrorItems = [];
    private readonly ReaderWriterLockSlim _rwLock = new();

    public StateStore(IConfiguration conf, ILogger<StateStore> log, IServiceProvider sp)
    {
        _conf = conf;
        _log = log;
        _sp = sp;
        Reload();
    }

    public void Reload()
    {
        var confPath = _conf["ConfPath"];
        if (string.IsNullOrWhiteSpace(confPath)) throw new Exception("Mirror configs path not set");

        // Load configs from fs
        List<ConfigInfo> confs;
        try
        {
            confs = Directory
                .EnumerateFiles(confPath!, "*.json")
                .Select(path => File.ReadAllText(path, Encoding.UTF8))
                .Select(content => JsonSerializer.Deserialize<ConfigInfoRaw>(content, JsonUtil.DefaultOptions))
                .Select(r => new ConfigInfo(r!))
                .ToList();
        }
        catch (Exception e)
        {
            _log.LogError("Hot reload failed: {e}", e);
            return;
        }

        // Load states from db
        using var scope = _sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrchDbContext>();
        var savedInfos = db.SavedInfos.AsNoTracking().ToList();
        var currentMirrorItems = new Dictionary<string, MirrorItemInfo>(_mirrorItems);

        // 1. remove deleted configs
        foreach (var item in currentMirrorItems)
        {
            if (confs.Any(x => x.Id == item.Key)) continue;

            currentMirrorItems.Remove(item.Key);
        }

        // 2. add new configs to memory and db
        foreach (var conf in confs)
        {
            if (currentMirrorItems.Any(x => x.Key == conf.Id)) continue;

            var newInfo = new MirrorItemInfo(conf)
            {
                Status = MirrorStatus.Unknown,
                LastSyncAt = DateTimeConstants.UnixEpoch
            };
            var savedInfo = savedInfos.FirstOrDefault(x => x.Id == conf.Id);
            if (savedInfo != null)
            {
                newInfo.Status = savedInfo.Status;
                newInfo.LastSyncAt = savedInfo.LastSyncAt.ToLocalTime();
                newInfo.Size = savedInfo.Size;
            }
            else
            {
                db.SavedInfos.Add(new SavedInfo
                {
                    Id = conf.Id,
                    Status = MirrorStatus.Unknown,
                    LastSyncAt = DateTimeConstants.UnixEpoch
                });
            }

            currentMirrorItems.Add(conf.Id, newInfo);
        }

        try
        {
            db.SaveChanges();
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
        using var _ = new ScopeReadLock(_rwLock);
        return _mirrorItems.ToDictionary(kv => kv.Key, kv => new MirrorItemInfo(kv.Value));
    }

    public MirrorItemInfo? GetMirrorItemInfoById(string id)
    {
        using var _ = new ScopeReadLock(_rwLock);
        return _mirrorItems.TryGetValue(id, out var info) ? new MirrorItemInfo(info) : null;
    }

    public void SetMirrorInfo(SavedInfo info)
    {
        var item = _mirrorItems.FirstOrDefault(x => x.Key == info.Id);
        if (item.Key == null) return;

        using (var guard = new ScopeWriteLock(_rwLock))
        {
            item.Value.Status = info.Status;
            item.Value.LastSyncAt = info.LastSyncAt;
            item.Value.Size = info.Size;
        }

        using var scope = _sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrchDbContext>();
        db.SavedInfos.Update(new SavedInfo
        {
            Id = info.Id,
            Status = info.Status,
            LastSyncAt = info.LastSyncAt.ToUniversalTime(),
            Size = info.Size
        });

        try
        {
            db.SaveChanges();
        }
        catch (Exception e)
        {
            _log.LogError("Failed to save mirror info to db: {e}", e);
        }
    }
}