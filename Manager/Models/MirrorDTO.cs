using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Manager.Models;

public class MirrorDto
{
    public class MirrorItem
    {
        [Key]
        public string Id { get; set; }
        public string Url { get; set; }
        
        public I18N.String Name { get; set; }
        [JsonPropertyName("desc")]
        public I18N.String Description { get; set; }
        
        public Mirror.Status Status { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime NextScheduled { get; set; }
        public DateTime LastSuccess { get; set; }
        
        public Dictionary<string, string> IsoDict { get; set; }
    }
}
