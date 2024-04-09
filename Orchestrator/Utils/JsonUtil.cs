using System.Text.Json;

namespace Orchestrator.Utils;

public static class JsonUtil
{
    public static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
}