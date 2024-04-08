using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Orchestrator.DataModels;
using Orchestrator.Utils;

namespace Orchestrator.Services;

public class JobQueue
{
    private readonly IConfiguration _conf;
    private readonly ILogger<JobQueue> _log;
    private readonly ConcurrentQueue<SyncJob> _pendingQueue = new();
    private readonly ReaderWriterLockSlim _rwLock = new();
    private readonly IStateStore _stateStore;
    private readonly ConcurrentDictionary<Guid, SyncJob> _syncingDict = new();

    public JobQueue(IConfiguration conf, ILogger<JobQueue> log, IStateStore stateStore)
    {
        _conf = conf;
        _log = log;
        _stateStore = stateStore;

        Reload();
    }

    public TimeSpan CoolDown { get; set; } = TimeSpan.FromMinutes(5);

    public void Reload()
    {
        _stateStore.Reload();
        var syncJobs = _stateStore.GetMirrorItemInfos()
            .Select(x => x.Value)
            .Where(x => x.Config.Info.Type == SyncType.Sync)
            .Select(x => new SyncJob(x, x.LastSyncAt.Add(x.Config.Sync!.Interval)));

        using var guard = new ScopeWriteLock(_rwLock);
        _pendingQueue.Clear();
        // DO NOT clear _syncingDict since workers may still be working on them
        // and we need to update their status
        _syncingDict.ForEach(x => x.Value.Stale = true);
        syncJobs.ForEach(x => _pendingQueue.Enqueue(x));
    }

    public IEnumerable<KeyValuePair<string, MirrorItemInfo>> GetMirrorItems()
    {
        return _stateStore.GetMirrorItemInfos();
    }

    public MirrorItemInfo? GetMirrorItemById(string id)
    {
        return _stateStore.GetMirrorItemInfoById(id);
    }

    public (int, int) GetQueueStatus()
    {
        return (_pendingQueue.Count, _syncingDict.Count);
    }

    private void CheckLostJobs()
    {
        // we need to acquire an write lock here
        // in case other threads will read or write the dictionary
        List<SyncJob> jobs = [];
        using (var guard = new ScopeWriteLock(_rwLock))
        {
            foreach (var (guid, job) in _syncingDict)
                if (job.TaskStartedAt.Add(job.MirrorItem.Config.Sync!.Timeout).Add(CoolDown) < DateTime.Now)
                {
                    jobs.Add(job);
                    _syncingDict.Remove(guid, out _);
                }
        }

        foreach (var job in jobs)
        {
            _log.LogWarning("Job {guid}({id}) took too long, marking as failed", job.Guid,
                job.MirrorItem.Config.Id);
            _stateStore.SetMirrorInfo(new SavedInfo
            {
                Id = job.MirrorItem.Config.Id,
                Status = MirrorStatus.Failed,
                LastSyncAt = job.MirrorItem.LastSyncAt,
                Size = job.MirrorItem.Size
            });
            var newJob = new SyncJob(job);
            newJob.TaskShouldStartAt = DateTime.Now;
            _pendingQueue.Enqueue(newJob);
        }
    }

    public bool TryGetNewJob([MaybeNullWhen(false)] out SyncJob job)
    {
        // worker should report a fail job if time exceeds limit
        // so we assume worker encountered an error if it takes too long
        CheckLostJobs();

        using var _ = new ScopeReadLock(_rwLock);

        var hasJob = _pendingQueue.TryDequeue(out job);
        if (!hasJob) return false;

        // is sync interval passed?
        if (job!.TaskShouldStartAt > DateTime.Now)
        {
            var x = DateTime.Now;
            // enqueue the item again
            _pendingQueue.Enqueue(job);
            return false;
        }

        _syncingDict[job.Guid] = job;
        job.TaskStartedAt = DateTime.Now;
        _stateStore.SetMirrorInfo(new SavedInfo
        {
            Id = job.MirrorItem.Config.Id,
            Status = MirrorStatus.Syncing,
            LastSyncAt = job.MirrorItem.LastSyncAt,
            Size = job.MirrorItem.Size
        });
        return true;
    }

    public void UpdateJobStatus(Guid guid, MirrorStatus status)
    {
        using var guard = new ScopeReadLock(_rwLock);
        if (!_syncingDict.TryRemove(guid, out var job))
        {
            _log.LogWarning("Job not found: {guid}", guid);
            return;
        }

        if (status != MirrorStatus.Failed && status != MirrorStatus.Succeeded)
        {
            _log.LogWarning("Unsupported job status: {status}", status);
            return;
        }

        if (status == MirrorStatus.Failed)
            _stateStore.SetMirrorInfo(new SavedInfo
            {
                Id = job.MirrorItem.Config.Id,
                Status = status,
                LastSyncAt = job.MirrorItem.LastSyncAt,
                Size = job.MirrorItem.Size
            });
        else // if (status == MirrorStatus.Succeeded)
            _stateStore.SetMirrorInfo(new SavedInfo
            {
                Id = job.MirrorItem.Config.Id,
                Status = status,
                LastSyncAt = DateTime.Now,
                Size = job.MirrorItem.Size
            });

        if (job.Stale) return;
        _pendingQueue.Enqueue(new SyncJob(job));
    }
}