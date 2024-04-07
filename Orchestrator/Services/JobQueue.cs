using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Orchestrator.DataModels;
using Orchestrator.Utils;

namespace Orchestrator.Services;

public class JobQueue
{
    private readonly IConfiguration _conf;
    private readonly ILogger<JobQueue> _log;
    private readonly IStateStore _stateStore;
    private ConcurrentDictionary<Guid, SyncJob> _syncingDict = new();
    private ConcurrentQueue<SyncJob> _pendingQueue = new();
    private ConcurrentQueue<SyncJob> _recloseQueue = new();
    private ReaderWriterLockSlim _rwLock = new();

    public JobQueue(IConfiguration conf, ILogger<JobQueue> log, IStateStore stateStore)
    {
        _conf = conf;
        _log = log;
        _stateStore = stateStore;

        Reload();
    }

    private void Reload()
    {
        _stateStore.Reload();
        var syncJobs = _stateStore.GetMirrorItemInfos()
            .Select(x => x.Value)
            .Where(x => x.Config.Info.Type == SyncType.Sync)
            .Select(x => new SyncJob(x, x.LastSyncAt.Add(x.Config.Sync!.Interval)));

        using var guard = new ScopeWriteLock(_rwLock);
        _pendingQueue.Clear();
        _recloseQueue.Clear();
        // DO NOT clear _syncingDict since workers may still be working on them
        // and we need to update their status
        _syncingDict.ForEach(x => x.Value.Stale = true);
        syncJobs.ForEach(x => _pendingQueue.Enqueue(x));
    }

    private void CheckLostJobs()
    {
        // we need to acquire an write lock here
        // in case other threads will read or write the dictionary
        List<SyncJob> jobs = [];
        using (var guard = new ScopeWriteLock(_rwLock))
        {
            foreach (var (guid, job) in _syncingDict)
            {
                if (job.TaskStartedAt.Add(job.MirrorItem.Config.Sync!.Timeout) < DateTime.Now.AddMinutes(5))
                {
                    jobs.Add(job);
                    _syncingDict.Remove(guid, out _);
                }
            }
        }

        foreach (var job in jobs)
        {
            _log.LogWarning("Job {guid}({id}) took too long, marking as failed", job.Guid,
                job.MirrorItem.Config.Id);
            _stateStore.SetMirrorInfo(job.MirrorItem.Config.Id, MirrorStatus.Failed, job.MirrorItem.LastSyncAt);
            _pendingQueue.Enqueue(new SyncJob(job));
        }
    }

    public bool TryGetNewJob([MaybeNullWhen(false)] out SyncJob job)
    {
        // worker should report a fail job if time exceeds limit
        // so we assume worker encountered an error if it takes too long
        CheckLostJobs();

        using var _ = new ScopeReadLock(_rwLock);

        // is there any job in reclose queue?
        var hasRecloseJob = _recloseQueue.TryDequeue(out job);
        if (hasRecloseJob)
        {
            _syncingDict[job!.Guid] = job;
            job.TaskStartedAt = DateTime.Now;
            _stateStore.SetMirrorInfo(job.MirrorItem.Config.Id, MirrorStatus.Syncing, job.MirrorItem.LastSyncAt);
            return true;
        }

        var hasJob = _pendingQueue.TryDequeue(out job);
        if (!hasJob) return false;

        // is sync interval passed?
        if (job!.TaskShouldStartAt < DateTime.Now)
        {
            // enqueue the item again
            _pendingQueue.Enqueue(job);
            return false;
        }

        _syncingDict[job.Guid] = job;
        job.TaskStartedAt = DateTime.Now;
        _stateStore.SetMirrorInfo(job.MirrorItem.Config.Id, MirrorStatus.Syncing, DateTime.Now);
        return true;
    }

    public void UpdateJobStatus(Guid guid, MirrorStatus status)
    {
        using var guard = new ScopeReadLock(_rwLock);
        if (!_syncingDict.TryGetValue(guid, out var job))
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
        {
            _stateStore.SetMirrorInfo(job.MirrorItem.Config.Id, status, job.MirrorItem.LastSyncAt);
            if (job.Stale)
            {
                return;
            }

            job.FailedCount++;
            if (job.FailedCount < 3)
            {
                job.TaskShouldStartAt = DateTime.Now.AddMinutes(5);
                _recloseQueue.Enqueue(job);
            }
        }
        else // if (status == MirrorStatus.Succeeded)
        {
            _stateStore.SetMirrorInfo(job.MirrorItem.Config.Id, status, DateTime.Now);
        }

        if (job.Stale) return;
        _pendingQueue.Enqueue(new SyncJob(job));
    }
}