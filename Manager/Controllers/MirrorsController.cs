using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Manager.Models;
using Microsoft.AspNetCore.Mvc;
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
    /// All mirror status
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<List<MirrorDto.MirrorItem>> GetAllMirrors()
    {
        var serializedString = await _cache.GetStringAsync(Utils.Constants.MirrorStatusCacheKey);
        if (serializedString != null)
        {
            return JsonSerializer.Deserialize<List<MirrorDto.MirrorItem>>(serializedString);
        }

        _logger.LogInformation("Status cache miss, regenerating");

        // var packageList = await _mirrorStatusContext.Packages.ToListAsync();
        // var releaseList = await _mirrorStatusContext.Releases.Include(release => release.UrlItems).ToListAsync();
        var res = new List<MirrorDto.MirrorItem>()
        {
            new()
            {
                Id = "debian",
                Url = "/debian",
                Name = new I18N.String
                {
                    En = "Debian",
                    Zh = "Debian 发行版"
                },
                Description = new I18N.String
                {
                    En =
                        "Debian is a free and open-source operating system, based on the Linux kernel, and is the default distribution for the GNU/Linux operating system family.",
                    Zh = "Debian 是一个基于 Linux 内核的免费和开源操作系统，它是 GNU/Linux 操作系统的默认发行版。"
                },
                Status = Mirror.Status.Succeeded,
                LastSuccess = DateTime.Now,
                LastUpdated = DateTime.Now,
                NextScheduled = DateTime.Now + TimeSpan.FromHours(2),
                IsoDict = new Dictionary<string, string>
                {
                    { "amd64-generic-hwe-12.04", "xxx/xxx.iso" },
                    { "amd64-generic-hwe-14.04", "xxx/xxx.iso" },
                    { "amd64-generic-hwe-16.04", "xxx/xxx.iso" },
                    { "amd64-generic-hwe-18.04", "xxx/xxx.iso" },
                    { "amd64-generic-hwe-20.04", "xxx/xxx.iso" },
                    { "amd64-generic-hwe-21.10", "xxx/xxx.iso" },
                    { "amd64-generic-hwe-22.04", "xxx/xxx.iso" },
                }
            }
        };
        await _cache.SetStringAsync(Utils.Constants.MirrorStatusCacheKey, JsonSerializer.Serialize(res));
        _logger.LogInformation("Wrote status to cache");

        return res;
    }

    /// <summary>
    /// Mirror status query with id
    /// </summary>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<MirrorDto.MirrorItem> GetMirror(string id)
    {
        var res = new MirrorDto.MirrorItem
        {
            Id = id,
            Url = $"/{id}",
            Name = new I18N.String
            {
                En = "Debian",
                Zh = "Debian 发行版"
            },
            Description = new I18N.String
            {
                En =
                    "Debian is a free and open-source operating system, based on the Linux kernel, and is the default distribution for the GNU/Linux operating system family.",
                Zh = "Debian 是一个基于 Linux 内核的免费和开源操作系统，它是 GNU/Linux 操作系统的默认发行版。"
            },
            Status = Mirror.Status.Succeeded,
            LastSuccess = DateTime.Now,
            LastUpdated = DateTime.Now,
            NextScheduled = DateTime.Now + TimeSpan.FromHours(2),
            IsoDict = new Dictionary<string, string>
            {
                { "amd64-generic-hwe-12.04", "xxx/xxx.iso" },
                { "amd64-generic-hwe-14.04", "xxx/xxx.iso" },
                { "amd64-generic-hwe-16.04", "xxx/xxx.iso" },
                { "amd64-generic-hwe-18.04", "xxx/xxx.iso" },
                { "amd64-generic-hwe-20.04", "xxx/xxx.iso" },
                { "amd64-generic-hwe-21.10", "xxx/xxx.iso" },
                { "amd64-generic-hwe-22.04", "xxx/xxx.iso" },
            }
        };

        return res;
    }
}