using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Manager.Models
{
    /*
     * MirrorZ DataFormat
     * version: 1.5
     * ref: https://github.com/mirrorz-org/mirrorz#data-format-v15-draft
     */
    public class MirrorZ
    {
        private const double FormatVersion = 1.5;

        public class SiteInfo
        {
            [JsonPropertyName("url")] [Required] public string Url { get; set; }  // TODO: [FIXME] real notnull for `url` and `abbr` attributes
            [JsonPropertyName("logo")] public string LogoUrl { get; set; }
            [JsonPropertyName("logo_darkmode")] public string LogoDarkUrl { get; set; }
            [JsonPropertyName("abbr")] [Required] public string Abbreviation { get; set; }
            [JsonPropertyName("name")] public string Name { get; set; }
            [JsonPropertyName("homepage")] public string HomePage { get; set; }
            [JsonPropertyName("issue")] public string Issue { get; set; }
            [JsonPropertyName("request")] public string Request { get; set; }
            [JsonPropertyName("email")] public string Email { get; set; }
            [JsonPropertyName("group")] public string TelegramGroup { get; set; }
            [JsonPropertyName("disk")] public string DiskStatus { get; set; }
            [JsonPropertyName("note")] public string Notice { get; set; }
            [JsonPropertyName("big")] public string BigFileUrl { get; set; }
        }

        public class UrlItem
        {
            [JsonPropertyName("name")] public string Name { get; set; }
            [JsonPropertyName("url")] public string Url { get; set; }
        }

        public class ReleaseInfo
        {
            [JsonPropertyName("distro")] public string MappedName { get; set; }
            [JsonPropertyName("category")] public ReleaseType Category { get; set; }
            [JsonPropertyName("urls")] public UrlItem[] UrlItems { get; set; }
        }

        public class PackageInfo
        {
            [JsonPropertyName("cname")] public string MappedName { get; set; }
            [JsonPropertyName("desc")] public string Description { get; set; }
            [JsonPropertyName("url")] public string Url { get; set; }
            [JsonPropertyName("status")] public string Status { get; set; }
            [JsonPropertyName("help")] public string HelpUrl { get; set; }
            [JsonPropertyName("upstream")] public string Upstream { get; set; }
            [JsonPropertyName("size")] public string Size { get; set; }
        }

        public class DataFormat
        {
            [JsonPropertyName("version")] public double Version { get; } = FormatVersion;
            [JsonPropertyName("site")] public SiteInfo Site { get; set; }
            [JsonPropertyName("info")] public ReleaseInfo[] Releases { get; set; }
            [JsonPropertyName("mirrors")] public PackageInfo[] Packages { get; set; }
        }
    }
}