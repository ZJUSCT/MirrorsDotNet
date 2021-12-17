using System.Threading.Tasks;
using Manager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Manager.Controllers;

[ApiController]
[Route("mirrors")]
public class MirrorsController : ControllerBase
{
    private readonly ILogger<MirrorsController> _logger;
    private readonly MirrorZ.SiteInfo _siteInfo;
    private readonly MirrorStatusContext _mirrorStatusContext;

    public MirrorsController(ILogger<MirrorsController> logger, IOptions<MirrorZ.SiteInfo> siteInfo,
        MirrorStatusContext statusContext)
    {
        _logger = logger;
        _siteInfo = siteInfo.Value;
        _mirrorStatusContext = statusContext;
    }

    /// <summary>
    /// Mirror status in MirrorZ format
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<MirrorZ.DataFormat> Get()
    {
        var packageList = await _mirrorStatusContext.Packages.ToListAsync();
        var releaseList = await _mirrorStatusContext.Releases.ToListAsync();
        var res = new MirrorZ.DataFormat
        {
            Site = _siteInfo,
            Packages = packageList,
            Releases = releaseList
        };
        return res;
    }
}