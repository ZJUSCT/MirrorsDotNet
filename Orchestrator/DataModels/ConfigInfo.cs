using System.Diagnostics.CodeAnalysis;

namespace Orchestrator.DataModels;

public enum SyncType
{
    Sync,
    ReverseProxy,
    Cached,
    Other
}

public enum PullStrategy
{
    Always,
    IfNotExists,
    Never
}

public record I18NField(string En, string Zh);

public record MirrorInfoRaw(I18NField Name, I18NField Description, string Type, string Upstream);

public class MirrorInfo
{
    public required I18NField Name { get; init; }
    public required I18NField Description { get; init; }
    public SyncType Type { get; init; }

    public required string Upstream { get; init; }

    public MirrorInfo()
    {
    }

    [SetsRequiredMembers]
    public MirrorInfo(MirrorInfoRaw raw)
    {
        Name = raw.Name;
        Description = raw.Description;
        Type = raw.Type switch
        {
            "sync" => SyncType.Sync,
            "reverseProxy" => SyncType.ReverseProxy,
            "cached" => SyncType.Cached,
            _ => SyncType.Other
        };
        Upstream = raw.Upstream;
    }
}

public record VolumeInfo(string Src, string Dst, bool ReadOnly);

public record SyncInfoRaw(
    string JobName,
    string Interval,
    string Timeout,
    string Image,
    string Pull,
    List<VolumeInfo> Volumes,
    List<string> Command,
    List<string> Environments);

public class SyncInfo
{
    public required string JobName { get; init; }
    public TimeSpan Interval { get; init; }
    public TimeSpan Timeout { get; init; }
    public required string Image { get; init; }
    public PullStrategy Pull { get; init; }
    public required List<VolumeInfo> Volumes { get; init; }
    public required List<string> Command { get; init; }
    public required List<string> Environments { get; init; }

    public SyncInfo()
    {
    }

    [SetsRequiredMembers]
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
            _ => PullStrategy.Never
        };
        Volumes = raw.Volumes;
        Command = raw.Command;
        Environments = raw.Environments;
    }
}

public record ConfigInfoRaw(string Id, MirrorInfoRaw Info, SyncInfoRaw? Sync);

public class ConfigInfo
{
    public required string Id { get; init; }
    public required MirrorInfo Info { get; init; }
    public SyncInfo? Sync { get; init; }

    public ConfigInfo()
    {
    }

    [SetsRequiredMembers]
    public ConfigInfo(ConfigInfoRaw rawConf)
    {
        Id = rawConf.Id;
        Info = new MirrorInfo(rawConf.Info);
        if (rawConf.Sync != null)
        {
            Sync = new SyncInfo(rawConf.Sync);
        }
    }

    [SetsRequiredMembers]
    public ConfigInfo(ConfigInfo conf)
    {
        Id = conf.Id;
        Info = conf.Info;
        Sync = conf.Sync;
    }
}