namespace Orchestrator.DataModels;

public record MirrorZData(
    double version,
    MirrorZSiteInfo site,
    IList<MirrorZCatItem> info,
    IList<MirrorZMirrorItem> mirrors,
    string extension,
    IList<MirrorZEndpointInfo> endpoints);

public record MirrorZSiteInfo(
    string url,
    string? logo,
    // string? logo_darkmode,
    string? abbr,
    string? name,
    string? homepage,
    // string? issue,
    // string? request,
    string? email,
    // string? group,
    // string? disk,
    // string? note,
    // string? big,
    bool disable = false);

public record MirrorZCatItemUrl(string name, string url);

public record MirrorZCatItem(string distro, string category, IList<MirrorZCatItemUrl> urls);

public record MirrorZMirrorItem(
    string cname,
    string desc,
    string url,
    string status,
    string help,
    string upstream,
    string size,
    bool disable = false);

public record MirrorZEndpointInfo(
    string Label,
    bool Public,
    string Resolve,
    List<string> Filter,
    List<string> Range
    );

public static class MirrorZStatic
{
    public static MirrorZSiteInfo SiteInfo = new MirrorZSiteInfo(
        "https://mirrors.zju.edu.cn",
        "https://mirrors.zju.edu.cn/zju-falcon.svg",
        "ZJU",
        "浙江大学开源软件镜像站",
        "https://www.zjusct.io",
        "mirrors@zju.edu.cn"
    );
    
    public static List<MirrorZEndpointInfo> EndpointInfos = [new (
        "zju",
        true,
        "mirrors.zju.edu.cn",
        [ "V4", "V6", "SSL", "NOSSL" ],
        [ "COUNTRY:CN",
            "REGION:ZJ",
            "ISP:CERNET",
            "AS4538",
            "AS24367",
            "AS23910",
            "AS23911",
            "210.32.0.0/20",
            "222.205.0.0/17",
            "210.32.128.0/19",
            "210.32.160.0/21",
            "210.32.168.0/22",
            "210.32.172.0/23",
            "210.32.174.0/24",
            "210.32.176.0/20",
            "58.196.192.0/19",
            "58.196.224.0/20" ]
    )];
}