namespace Orchestrator.DataModels;

public record MirrorZData(
    double version,
    MirrorZSiteInfo site,
    IList<MirrorZCatItem> info,
    IList<MirrorZMirrorItem> mirrors);

public record MirrorZSiteInfo(
    string url,
    string? logo,
    string? logo_darkmode,
    string? abbr,
    string? name,
    string? homepage,
    string? issue,
    string? request,
    string? email,
    string? group,
    string? disk,
    string? note,
    string? big,
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

public static class MirrorZStatic
{
    public static MirrorZSiteInfo SiteInfo = new MirrorZSiteInfo(
        "https://mirrors.zju.edu.cn",
        "https://mirrors.zju.edu.cn/zju-falcon.svg",
        null,
        "ZJU",
        "浙江大学开源软件镜像站",
        "https://www.zjusct.io",
        null,
        null,
        "mirrors@zju.edu.cn",
        null,
        null,
        null,
        null
    );
}