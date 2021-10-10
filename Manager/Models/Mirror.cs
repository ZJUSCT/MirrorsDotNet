using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Manager.Models
{
    public enum MirrorStatus
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
        public string MappedName { get; set; }
        public string Url { get; set; }
        public bool IsNew { get; set; } = false;
        public MirrorType Type { get; set; } = MirrorType.Normal;

        // Sync Info
        public string Cron { get; set; }
        public string ProviderImage { get; set; }
        public string Upstream { get; set; }
        public string ExtraCommand { get; set; }

        // Status Info
        public MirrorStatus Status { get; set; }
        public DateTime LastStateTime { get; set; } // S/Y/F/P 1600000000
        public DateTime NextSchedule { get; set; } // X 1600000000
        public DateTime AddTime { get; set; } // N 1600000000
        public DateTime LastSuccess { get; set; } // O 1600000000

        private static int DataTime2TimeStamp(DateTime time)
        {
            return (int)(time.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        public string GetStatus()
        {
            string res;

            if (Type != MirrorType.Normal)
            {
                res = Type switch
                {
                    MirrorType.Cache => "C",
                    MirrorType.ReverseProxy => "R",
                    _ => "U"
                };
            }
            else
            {
                res = Status switch
                {
                    MirrorStatus.Successful => "S",
                    MirrorStatus.Syncing => "Y",
                    MirrorStatus.Failed => "F",
                    MirrorStatus.Paused => "P",
                    _ => "U"
                };
                res += $"{DataTime2TimeStamp(LastStateTime)}";
                res += $"X{DataTime2TimeStamp(NextSchedule)}";
                
                // New Mirror Flag
                if (IsNew)
                {
                    res += $"N{DataTime2TimeStamp(AddTime)}";
                }

                // Last Success Info for Failed Mirror
                if (Status == MirrorStatus.Failed)
                {
                    res += $"O{DataTime2TimeStamp(LastSuccess)}";
                }
            }
            
            return res;
        }

        public void SetStatus(MirrorStatus status)
        {
            throw new NotImplementedException();
            
            switch (status)
            {
                case MirrorStatus.Successful:
                    break;
                case MirrorStatus.Failed:
                    break;
                case MirrorStatus.Syncing:
                    break;
                case MirrorStatus.Paused:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }
    }

    public class UrlItem
    {
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("url")] public string Url { get; set; }
    }

    public class MirrorPackages : MirrorBase
    {
        public string Description { get; set; }
        public string HelpUrl { get; set; }

        // this filed should be reported by worker after each sync job done
        public UrlItem[] Items { get; set; }

        MirrorZ.ReleaseInfo GetMirrorZData()
        {
            return new MirrorZ.ReleaseInfo() { };
        }
    }

    public class MirrorRelease : MirrorBase
    {
        public ReleaseType Type { get; set; }

        MirrorZ.PackageInfo GetMirrorZData()
        {
            return new MirrorZ.PackageInfo() { };
        }
    }
}