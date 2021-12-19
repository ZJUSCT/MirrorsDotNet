using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Manager.Models;

public enum MirrorType
{
    Normal,
    Cache,
    ReverseProxy
}

public enum ReleaseType
{
    [EnumMember(Value = "os")] Os,
    [EnumMember(Value = "app")] App,
    [EnumMember(Value = "font")] Font
}

public class MirrorBase
{
    // Basic Info
    [Key] public string Name { get; set; }
    /// <summary>
    /// Ref: https://github.com/mirrorz-org/mirrorz/blob/master/static/json/cname.json
    /// </summary>
    public string MappedName { get; set; }
    public string Url { get; set; }
    public bool IsNew { get; set; } = false;
    public MirrorType Type { get; set; } = MirrorType.Normal;

    // Sync Info
    public string Cron { get; set; }
    public string ProviderImage { get; set; }
    public string Upstream { get; set; }
    public string ExtraCommand { get; set; }
}

public class MirrorPackage : MirrorBase
{
    public string Description { get; set; }
    public string HelpUrl { get; set; }
}

public class MirrorRelease : MirrorBase
{
    public ReleaseType Category { get; set; }

    // Used to Generate File List
    public string IndexPath { get; set; }
    public string Pattern { get; set; }
    public string SortBy { get; set; }
}