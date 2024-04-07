using Orchestrator.DataModels;
using Orchestrator.Services;

namespace Orchestrator.Tests;

public class MockStateStore : IStateStore
{
    public void Reload()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<KeyValuePair<string, MirrorItemInfo>> GetMirrorItemInfos()
    {
        throw new NotImplementedException();
    }

    public void SetMirrorInfo(string id, MirrorStatus status, DateTime lastSyncAt)
    {
        throw new NotImplementedException();
    }
}