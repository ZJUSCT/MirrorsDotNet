namespace Orchestrator.DataModels;

public class MirrorItemInfo
{
    public ConfigInfo Config { get; init; }
    public MirrorStatus Status { get; set; }
    public DateTime LastSyncAt { get; set; }
    public UInt64 Size { get; set; }

    public MirrorItemInfo(ConfigInfo config)
    {
        Config = new ConfigInfo(config);
    }
    public MirrorItemInfo(MirrorItemInfo info) : this(info.Config)
    {
        Status = info.Status;
        LastSyncAt = info.LastSyncAt;
        Size = info.Size;
    }
}

public class SyncJob(MirrorItemInfo mirrorItemInfo, DateTime shouldStartAt)
{
    public readonly MirrorItemInfo MirrorItem = mirrorItemInfo;
    public readonly Guid Guid = Guid.NewGuid();
    public DateTime TaskShouldStartAt = shouldStartAt;
    public DateTime TaskStartedAt = DateTime.MinValue;
    public bool Stale = false;

    public SyncJob(SyncJob job) : this(job.MirrorItem, DateTime.Now.Add(job.MirrorItem.Config.Sync!.Interval))
    {
    }
}