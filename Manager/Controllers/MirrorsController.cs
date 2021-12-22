using System.Text.Json;
using System.Threading.Tasks;
using Manager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Manager.Controllers;

[ApiController]
[Route("mirrors")]
public class MirrorsController : ControllerBase
{
    private readonly ILogger<MirrorsController> _logger;
    private readonly MirrorStatus.SiteInfo _siteInfo;
    private readonly MirrorStatusContext _mirrorStatusContext;
    private readonly IDistributedCache _cache;

    public MirrorsController(ILogger<MirrorsController> logger, IOptions<MirrorStatus.SiteInfo> siteInfo,
        MirrorStatusContext statusContext, IDistributedCache cache)
    {
        _logger = logger;
        _siteInfo = siteInfo.Value;
        _mirrorStatusContext = statusContext;
        _cache = cache;
    }

    /// <summary>
    /// Mirror status in MirrorZ format
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<MirrorStatus.MirrorZFormat> Get()
    {
        var serializedString = await _cache.GetStringAsync(Utils.Constants.MirrorStatusCacheKey);
        if (serializedString != null)
        {
            return JsonSerializer.Deserialize<MirrorStatus.MirrorZFormat>(serializedString);
        }
        _logger.LogInformation("Status cache miss, regenerating");
        var packageList = await _mirrorStatusContext.Packages.ToListAsync();
        var releaseList = await _mirrorStatusContext.Releases.Include(release => release.UrlItems).ToListAsync();
        var res = new MirrorStatus.MirrorZFormat
        {
            Site = _siteInfo,
            Packages = packageList,
            Releases = releaseList
        };
        await _cache.SetStringAsync(Utils.Constants.MirrorStatusCacheKey, JsonSerializer.Serialize(res));
        _logger.LogInformation("Wrote status to cache");
        return res;
    }
}