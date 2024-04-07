namespace Orchestrator.DataModels;

public enum SyncType
{
    Sync,
    ReverseProxy,
    Cached,
    Other,
}

public enum PullStrategy
{
    Always,
    IfNotExists,
    Never,
}

public class I18nField
{
    public string En { get; set; }
    public string Zh { get; set; }
}

public class MirrorInfoRaw
{
    public I18nField Name { get; set; }
    public I18nField Description { get; set; }
    public string Type { get; set; }
    public string Upstream { get; set; }
}

public class MirrorInfo
{
    public I18nField Name { get; set; }
    public I18nField Description { get; set; }
    public SyncType Type { get; set; }
    public string Upstream { get; set; }
}

public class VolumeInfo
{
    public string Src { get; set; }
    public string Dst { get; set; }
    public bool ReadOnly { get; set; }
}

public class SyncInfoRaw
{
    public string JobName { get; set; }
    public string Interval { get; set; }
    public string Timeout { get; set; }
    public string Image { get; set; }
    public string Pull { get; set; }
    public List<VolumeInfo> Volumes { get; set; }
    public List<string> Command { get; set; }
    public List<string> Environments { get; set; }
}

public class SyncInfo
{
    public string JobName { get; set; }
    public TimeSpan Interval { get; set; }
    public TimeSpan Timeout { get; set; }
    public string Image { get; set; }
    public PullStrategy Pull { get; set; }
    public List<VolumeInfo> Volumes { get; set; }
    public List<string> Command { get; set; }
    public List<string> Environments { get; set; }

    public SyncInfo()
    {
    }

    public SyncInfo(SyncInfoRaw raw)
    {
        JobName = raw.JobName;
        Interval = TimeSpan.Parse(raw.Interval);
        Timeout = TimeSpan.Parse(raw.Timeout);
        Image = raw.Image;
        Pull = raw.Pull switch
        {
            "always" => PullStrategy.Always,
            "ifNotExists" => PullStrategy.IfNotExists,
            "never" => PullStrategy.Never,
            _ => PullStrategy.Never,
        };
        Volumes = raw.Volumes;
        Command = raw.Command;
        Environments = raw.Environments;
    }
}

public class ConfigInfoRaw
{
    public string Id { get; set; }
    public MirrorInfoRaw Info { get; set; }
    public SyncInfoRaw? Sync { get; set; }
}

public class ConfigInfo
{
    public string Id { get; set; }
    public MirrorInfo Info { get; set; }
    public SyncInfo? Sync { get; set; }

    public ConfigInfo()
    {
    }

    public ConfigInfo(ConfigInfoRaw rawConf)
    {
        Id = rawConf.Id;
        Info = new MirrorInfo
        {
            Name = rawConf.Info.Name,
            Description = rawConf.Info.Description,
            Type = rawConf.Info.Type switch
            {
                "sync" => SyncType.Sync,
                "reverseProxy" => SyncType.ReverseProxy,
                "cached" => SyncType.Cached,
                _ => SyncType.Other,
            },
            Upstream = rawConf.Info.Upstream,
        };
        if (rawConf.Sync != null)
        {
            Sync = new SyncInfo(rawConf.Sync);
        }
    }
}