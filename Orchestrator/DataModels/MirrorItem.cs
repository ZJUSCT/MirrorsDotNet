using Orchestrator.Utils;

namespace Orchestrator.DataModels;

public class MirrorItemInfo
{
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

    public ConfigInfo Config { get; init; }
    public MirrorStatus Status { get; set; }
    public DateTime LastSyncAt { get; set; }
    public ulong Size { get; set; }

    public DateTime NextSyncAt()
    {
        return Config.Sync == null
            ? DateTimeConstants.UnixEpoch
            : LastSyncAt.Add(Config.Sync.Interval);
    }
}

public class SyncJob(MirrorItemInfo mirrorItemInfo, DateTime shouldStartAt)
{
    public readonly Guid Guid = Guid.NewGuid();
    public readonly MirrorItemInfo MirrorItem = mirrorItemInfo;
    public bool Stale = false;
    public DateTime TaskShouldStartAt = shouldStartAt;
    public DateTime TaskStartedAt = DateTimeConstants.UnixEpoch;

    public SyncJob(SyncJob job) : this(job.MirrorItem, DateTime.Now.Add(job.MirrorItem.Config.Sync!.Interval))
    {
    }
}