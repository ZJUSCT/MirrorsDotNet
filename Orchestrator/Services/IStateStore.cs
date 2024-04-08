using Orchestrator.DataModels;

namespace Orchestrator.Services;

public interface IStateStore
{
    void Reload();
    IEnumerable<KeyValuePair<string, MirrorItemInfo>> GetMirrorItemInfos();
    void SetMirrorInfo(SavedInfo info);
}