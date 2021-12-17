using Manager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Manager.Controllers;

[ApiController]
[Route("[controller]")]
public class MirrorsController : ControllerBase
{
    private readonly ILogger<MirrorsController> _logger;
    private readonly MirrorZ.SiteInfo _siteInfo;
    private readonly MirrorConfigContext _mirrorConfigs;

    public MirrorsController(ILogger<MirrorsController> logger, IOptions<MirrorZ.SiteInfo> siteInfo,
        MirrorConfigContext configContext)
    {
        _logger = logger;
        _siteInfo = siteInfo.Value;
        _mirrorConfigs = configContext;
    }

    /// <summary>
    /// Mirror status in MirrorZ format
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public MirrorZ.DataFormat Get()
    {
        var res = new MirrorZ.DataFormat
        {
            Site = _siteInfo,
            Packages = new MirrorZ.PackageInfo[]
            {
                new()
                {
                    Url = "/debian",
                    MappedName = "debian",
                    Description = "Debian packages",
                    HelpUrl = "/help/debian",
                    Size = "100G",
                    Status = "S",
                    Upstream = "https://mirrors.tuna.tsinghua.edu.cn"
                }
            },
            Releases = new MirrorZ.ReleaseInfo[]
            {
                new()
                {
                    MappedName = "MeshLab",
                    Category = ReleaseType.App,
                    UrlItems = new MirrorZ.UrlItem[]
                    {
                        new()
                        {
                            Url = "CG/meshlab-1.0.zip",
                            Name = "v1.0"
                        }
                    }
                }
            }
        };
        return res;
    }
}