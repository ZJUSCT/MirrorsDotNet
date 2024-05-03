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
        LastSuccessAt = info.LastSuccessAt;
        Size = info.Size;
    }

    public ConfigInfo Config { get; }
    public MirrorStatus Status { get; set; }
    public DateTime LastSyncAt { get; set; }
    public DateTime LastSuccessAt { get; set; }
    public ulong Size { get; set; }

    public DateTime NextSyncAt()
    {
        return Config.Sync == null
            ? DateTimeConstants.UnixEpoch
            : Config.Sync.Interval.GetNextSyncTime(LastSyncAt);
    }
}

public class SyncJob(MirrorItemInfo mirrorItemInfo, DateTime shouldStartAt, string workerId = "")
{
    public Guid Guid { get; } = Guid.NewGuid();
    public MirrorItemInfo MirrorItem { get; } = mirrorItemInfo;
    public bool Stale { get; set; } = false;
    public DateTime TaskShouldStartAt { get; set; } = shouldStartAt;
    public DateTime TaskStartedAt { get; set; } = DateTimeConstants.UnixEpoch;
    public string WorkerId { get; set; } = workerId;

    public SyncJob(SyncJob job) : this(job.MirrorItem,
        job.MirrorItem.Config.Sync!.Interval.GetNextSyncTime(DateTime.Now))
    {
    }
}