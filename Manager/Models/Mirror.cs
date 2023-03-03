using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Manager.Models;

public enum MirrorStatus
{
    Succeeded,
    Syncing,
    Failed,
    Paused,
    Cached,
    ReverseProxied,
    Unknown,
    Pending
}

public enum MirrorType
{
    Normal,
    Paused,
    ProxyCache,
    ReverseProxy
}

public enum FileType
{
    [EnumMember(Value = "none")] None,
    [EnumMember(Value = "os")] Os,
    [EnumMember(Value = "app")] App,
    [EnumMember(Value = "font")] Font
}

/// <summary>
/// File Index Job Configuration Class
/// </summary>
public class FileIndexConfig
{
    // Basic
    [Key] public string Id { get; set; }
    public FileType Category { get; set; }

    // Use to Generate File List
    public string IndexPath { get; set; }
    public string Pattern { get; set; }
    public string ExcludePattern { get; set; }
    public string SortBy { get; set; }

    // Register result to
    public string RegisterId { get; set; }
}

public class UrlItem
{
    [JsonPropertyName("name")] public string Name { get; set; }
    [Key] [JsonPropertyName("url")] public string Url { get; set; }
    [JsonIgnore] public string SortKey { get; set; }
}

/// <summary>
/// Container Volume
/// </summary>
public class Volume
{
    public string Source { get; set; }
    public string Target { get; set; }
    [JsonPropertyName("ro")] public bool ReadOnly { get; set; }
}

/// <summary>
/// Docker Container Specification
/// </summary>
[Owned]
public class Container
{
    public string Image { get; set; }
    public string Pull { get; set; }
    public List<Volume> Volumes { get; set; }
    public List<string> Command { get; set; }
    public List<string> Environments { get; set; }
    public string Name { get; set; }
}

/// <summary>
/// Mirror Sync Status Class
/// </summary>
public class MirrorItem
{
    // Basic
    [Key] public string Id { get; set; }
    [Required] public I18N.StringBase Name { get; set; }
    [Required] public I18N.StringBase Description { get; set; }
    public string Url { get; set; }
    public string Location { get; set; }
    public string HelpUrl { get; set; }
    public string Size { get; set; } = "N/A";

    // Sync
    public MirrorType Type { get; set; } = MirrorType.Normal;
    public string Upstream { get; set; }
    public string Cron { get; set; }
    public string Timeout { get; set; }
    public Container Container { get; set; }

    // Index
    public FileType IndexedFilesType { get; set; }
    public string TrigIndex { get; set; }
    public List<UrlItem> Files { get; set; }

    // Status
    public MirrorStatus Status { get; set; }
    public DateTime LastUpdated { get; set; }
    public DateTime NextScheduled { get; set; }
    public DateTime LastSuccess { get; set; }

    public void UpdateFromConfig(MirrorConfig config)
    {
        Name = config.Name;
        Description = config.Description;
        Url = config.Url;
        Location = config.Location;
        HelpUrl = config.HelpUrl;
        Type = config.Type;
        Upstream = config.Upstream;
        Cron = config.Cron;
        Timeout = config.Timeout;
        Container = config.Container;
        TrigIndex = config.TrigIndex;

        // Special types that affect the status
        Status = config.Type switch
        {
            MirrorType.ProxyCache => MirrorStatus.Cached,
            MirrorType.ReverseProxy => MirrorStatus.ReverseProxied,
            MirrorType.Paused => MirrorStatus.Paused,
            MirrorType.Normal => Status switch
            {
                MirrorStatus.Cached => MirrorStatus.Unknown,
                MirrorStatus.ReverseProxied => MirrorStatus.Unknown,
                MirrorStatus.Paused => MirrorStatus.Unknown,
                _ => Status
            },
            _ => MirrorStatus.Unknown
        };
    }

    public void UpdateStatus(MirrorStatus status)
    {
        Status = status;
        LastUpdated = DateTime.Now;
        if (status == MirrorStatus.Succeeded)
        {
            LastSuccess = DateTime.Now;
        }
    }
}

/// <summary>
/// Mirror Sync Config Class
/// </summary>
public class MirrorConfig
{
    // Basic
    public string Id { get; set; }
    public I18N.StringBase Name { get; set; }
    public I18N.StringBase Description { get; set; }
    public string Url { get; set; }
    public string Location { get; set; }
    public string HelpUrl { get; set; }

    // Sync
    public MirrorType Type { get; set; } = MirrorType.Normal;
    public string Upstream { get; set; }
    public string Cron { get; set; }
    public string Timeout { get; set; }
    public Container Container { get; set; }

    // Index
    public string TrigIndex { get; set; }
}

/// <summary>
/// Mirror DTO Class for API
/// </summary>
public class MirrorItemDto
{
    [Key] public string Id { get; set; }
    public string Url { get; set; }

    public I18N.StringBase Name { get; set; }
    [JsonPropertyName("desc")] public I18N.StringBase Description { get; set; }
    public string HelpUrl { get; set; }
    public string Upstream { get; set; }
    public string Size { get; set; }

    public MirrorStatus Status { get; set; }
    public DateTime LastUpdated { get; set; }
    public DateTime NextScheduled { get; set; }
    public DateTime LastSuccess { get; set; }

    [JsonPropertyName("type")] public FileType IndexedFilesType { get; set; }
    public List<UrlItem> Files { get; set; }
}

public enum JobStatus
{
    // Un-Done status
    Pending = 0,
    Assigned = 1,
    Running = 2,
    // Done status
    Succeeded = 3,
    Failed = 4
}

public class MirrorSyncJob
{
    [Key] public int Id { get; set; }
    public string MirrorId { get; set; }
    public string Timeout { get; set; }
    public Container Container { get; set; }

    public string WorkerId { get; set; }
    public DateTime ScheduleTime { get; set; }
    public DateTime UpdateTime { get; set; }
    public JobStatus Status { get; set; }
    public string ContainerId { get; set; }
    public string ErrorMessage { get; set; }
}

public class MirrorSyncJobDto
{
    [Key] public int Id { get; set; }
    public string Timeout { get; set; }
    public Container Container { get; set; }
}

public class SyncJobUpdateBody
{
    public string WorkerId { get; set; }
    public int JobId { get; set; }
    public JobStatus Status { get; set; }
    public string ContainerId { get; set; }
    public string ErrorMessage { get; set; }
    public string FileSize { get; set; }
}