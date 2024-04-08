namespace Orchestrator.DataModels;

public class MirrorItemInfo(ConfigInfo config)
{
    public ConfigInfo Config { get; } = new (config);
    public MirrorStatus Status { get; set; }
    public DateTime LastSyncAt { get; set; }
    public UInt64 Size { get; set; }

    public MirrorItemInfo Clone()
    {
        return new MirrorItemInfo(config)
        {
            Status = Status,
            LastSyncAt = LastSyncAt,
            Size = Size
        };
    }
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