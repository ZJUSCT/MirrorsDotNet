using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Manager.Models
{
    public enum MirrorState
    {
        Successful,
        Syncing,
        Failed,
        Paused,
    }

    public enum MirrorType
    {
        Normal,
        Cache,
        ReverseProxy,
        Unknown
    }

    public class MirrorStatus
    {
        public DateTime LastStateTime { get; set; } // S/Y/F/P 1600000000
        public DateTime NextSchedule { get; set; } // X 1600000000
        public DateTime AddTime { get; set; } // N 1600000000
        public DateTime LastSuccess { get; set; } // O 1600000000
    }
    
    public enum ReleaseType
    {
        [EnumMember(Value = "os")] Os,
        [EnumMember(Value = "app")] App,
        [EnumMember(Value = "font")] Font
    }
    
    public class MirrorBase
    {
        [Key]
        public string Name { get; set; }
        public string MappedName { get; set; }
        public string Url { get; set; }
        
        public string Cron { get; set; }
        public string ProviderImage { get; set; }
        public string Upstream { get; set; }
        public string ExtraCommand { get; set; }

        public string GetStatus()
        {
            return "";
        }
    }
    
    public class MirrorPackages : MirrorBase
    {
        public string Description { get; set; }
        public string HelpUrl { get; set; }
    }

    public class MirrorRelease : MirrorBase
    {
        public ReleaseType Type { get; set; }
    }
}