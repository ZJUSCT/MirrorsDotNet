namespace Orchestrator.DataModels;

public enum MirrorStatus
{
    Succeeded,
    Syncing,
    Failed,
    Cached,
    ReverseProxied,
    Unknown
}
