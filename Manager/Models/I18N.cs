using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using YamlDotNet.Serialization;

namespace Manager.Models;

public class I18N
{
    [Owned]
    public class StringBase
    {
        [JsonPropertyName("zh")][YamlMember(Alias="zh")] public string Zh { get; set; }
        [JsonPropertyName("en")][YamlMember(Alias="en")] public string En { get; set; }
    }
}