using Orchestrator.DataModels;

namespace Orchestrator.Services;

public interface IStateStore
{
    void Reload();
    IEnumerable<KeyValuePair<string, MirrorItemInfo>> GetMirrorItemInfos();
    MirrorItemInfo? GetMirrorItemInfoById(string id);
    void SetMirrorInfo(SavedInfo info);
    void SetMirrorInfo(MirrorStatus status, MirrorItemInfo mirrorItemInfo);
}