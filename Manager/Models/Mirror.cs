using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    
    public class UrlItem
    {
        [JsonPropertyName("name")] public string Name { get; set; }
        [Key] [JsonPropertyName("url")] public string Url { get; set; }
        [JsonIgnore] public string SortKey { get; set; }
    }

    public class MirrorItem
    {
        // Basic
        [Key]
        public string Id { get; set; }
        public I18N.String Name { get; set; }
        public I18N.String Description { get; set; }
        public string Url { get; set; }
        public string HelpUrl { get; set; }

        // Sync
        public MirrorType Type { get; set; } = MirrorType.Normal;
        public string ProviderImage { get; set; }
        public string Upstream { get; set; }
        public string ExtraArgs { get; set; }
        public string Cron { get; set; }

        // Index
        public string IndexId { get; set; }
        public List<UrlItem> FileList { get; set; }

        // Status
        public MirrorStatus MirrorStatus { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime NextScheduled { get; set; }
        public DateTime LastSuccess { get; set; }
    }

    public class MirrorItemDto
    {
        [Key]
        public string Id { get; set; }
        public string Url { get; set; }
        
        public I18N.String Name { get; set; }
        [JsonPropertyName("desc")]
        public I18N.String Description { get; set; }
        
        public Mirror.MirrorStatus MirrorStatus { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime NextScheduled { get; set; }
        public DateTime LastSuccess { get; set; }

        public List<UrlItem> FileList { get; set; }
    }
}