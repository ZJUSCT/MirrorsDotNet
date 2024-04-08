using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Orchestrator.Tests;

public class MockConfiguration : IConfiguration
{
    private readonly Dictionary<string, string?> _data = new();

    public IEnumerable<IConfigurationSection> GetChildren()
    {
        throw new NotImplementedException();
    }

    public IChangeToken GetReloadToken()
    {
        throw new NotImplementedException();
    }

    public IConfigurationSection GetSection(string key)
    {
        throw new NotImplementedException();
    }

    public string? this[string key]
    {
        get
        {
            _data.TryGetValue(key, out var value);
            return value;
        }
        set => _data[key] = value;
    }
}