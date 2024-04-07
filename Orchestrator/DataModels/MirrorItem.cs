namespace Orchestrator.DataModels;

public class MirrorItemInfo
{
    public ConfigInfo Config { get; set; }
    public MirrorStatus Status { get; set; }
    public DateTime LastSyncAt { get; set; }
}

public class SyncJob(MirrorItemInfo mirrorItemInfo, DateTime shouldStartAt)
{
    public readonly MirrorItemInfo MirrorItem = mirrorItemInfo;
    public readonly Guid Guid = Guid.NewGuid();
    public int FailedCount = 0;
    public DateTime TaskShouldStartAt = shouldStartAt;
    public DateTime TaskStartedAt = DateTime.MinValue;
    public bool Stale = false;

    public SyncJob(SyncJob job) : this(job.MirrorItem, DateTime.Now.Add(job.MirrorItem.Config.Sync!.Interval))
    {
    }
}