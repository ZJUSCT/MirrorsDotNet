using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Manager.Models;

public class Mirror
{
    public enum MirrorStatus
    {
        Succeeded,
        Syncing,
        Failed,
        Paused,
        Cached,
        Unknown
    }

    public enum MirrorType
    {
        Normal,
        ProxyCache,
        ReverseProxy
    }

    public enum FileType
    {
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
    /// Mirror Sync Status Class
    /// </summary>
    public class MirrorItem
    {
        // Basic
        [Key] public string Id { get; set; }
        [Required] public I18N.StringBase Name { get; set; }
        [Required] public I18N.StringBase Description { get; set; }
        public string Url { get; set; }
        public string HelpUrl { get; set; }

        // Sync
        public MirrorType Type { get; set; } = MirrorType.Normal;
        public string ProviderImage { get; set; }
        public string Upstream { get; set; }
        public string ExtraArgs { get; set; }
        public string Cron { get; set; }

        // Index
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
            HelpUrl = config.HelpUrl;
            Type = config.Type;
            ProviderImage = config.ProviderImage;
            Upstream = config.Upstream;
            ExtraArgs = config.ExtraArgs;
            Cron = config.Cron;
            TrigIndex = config.TrigIndex;
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
        public string HelpUrl { get; set; }

        // Sync
        public MirrorType Type { get; set; } = MirrorType.Normal;
        public string ProviderImage { get; set; }
        public string Upstream { get; set; }
        public string ExtraArgs { get; set; }
        public string Cron { get; set; }

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

        public MirrorStatus Status { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime NextScheduled { get; set; }
        public DateTime LastSuccess { get; set; }

        public List<UrlItem> Files { get; set; }
    }
}