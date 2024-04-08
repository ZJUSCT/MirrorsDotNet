using Orchestrator.DataModels;
using Orchestrator.Services;

namespace Orchestrator.Tests;

public class MockStateStore : IStateStore
{
    private List<MirrorItemInfo> _newMirrorItems = [];
    private Dictionary<string, MirrorItemInfo> _mirrorItems = [];

    public MockStateStore(IEnumerable<MirrorItemInfo> mirrorItems)
    {
        _newMirrorItems = mirrorItems.ToList();
    }
    
    public void Reload()
    {
        _mirrorItems.Clear();
        _newMirrorItems.ForEach(x => _mirrorItems[x.Config.Id] = x);
    }

    public IEnumerable<KeyValuePair<string, MirrorItemInfo>> GetMirrorItemInfos()
    {
        return _mirrorItems.ToDictionary(kv => kv.Key, kv => kv.Value.Clone());
    }

    public void SetMirrorInfo(SavedInfo info)
    {
        var item = _mirrorItems.FirstOrDefault(x => x.Key == info.Id);
        if (item.Key == null)
        {
            return;
        }

        item.Value.Status = info.Status;
        item.Value.LastSyncAt = info.LastSyncAt;
        item.Value.Size = info.Size;
    }

    public void SetMirrorItems(IEnumerable<MirrorItemInfo> items)
    {
        _newMirrorItems = items.ToList();
    }
}