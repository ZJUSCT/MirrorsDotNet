using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Manager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Manager.Controllers;

[ApiController]
[Route("mirrors")]
public class MirrorsController : ControllerBase
{
    private readonly ILogger<MirrorsController> _logger;
    // private readonly MirrorStatus.SiteInfo _siteInfo;
    // private readonly MirrorStatusContext _mirrorStatusContext;
    private readonly IDistributedCache _cache;

    public MirrorsController(ILogger<MirrorsController> logger, IDistributedCache cache)
    {
        _logger = logger;
        _cache = cache;
    }

    /// <summary>
    /// All mirror status
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<List<Mirror.MirrorItemDto>> GetAllMirrors()
    {
        var serializedString = await _cache.GetStringAsync(Utils.Constants.MirrorStatusCacheKey);
        if (serializedString != null)
        {
            return JsonSerializer.Deserialize<List<Mirror.MirrorItemDto>>(serializedString);
        }

        _logger.LogInformation("Status cache miss, regenerating");

        // var packageList = await _mirrorStatusContext.Packages.ToListAsync();
        // var releaseList = await _mirrorStatusContext.Releases.Include(release => release.UrlItems).ToListAsync();
        var res = new List<Mirror.MirrorItemDto>()
        {
            new()
            {
                Id = "debian",
                Url = "/debian",
                Name = new I18N.StringBase
                {
                    En = "Debian",
                    Zh = "Debian 发行版"
                },
                Description = new I18N.StringBase
                {
                    En =
                        "Debian is a free and open-source operating system, based on the Linux kernel, and is the default distribution for the GNU/Linux operating system family.",
                    Zh = "Debian 是一个基于 Linux 内核的免费和开源操作系统，它是 GNU/Linux 操作系统的默认发行版。"
                },
                Status = Mirror.MirrorStatus.Succeeded,
                LastSuccess = DateTime.Now,
                LastUpdated = DateTime.Now,
                NextScheduled = DateTime.Now + TimeSpan.FromHours(2),
                FileList = new List<Mirror.UrlItem>()
                {
                    new()
                    {
                        Name = "amd64-generic-hwe-12.04",
                        Url = "xxx/xxx.iso"
                    },
                    new()
                    {
                        Name = "amd64-generic-hwe-14.04",
                        Url = "xxx/xxx.iso"
                    },
                    new()
                    {
                        Name = "amd64-generic-hwe-16.04",
                        Url = "xxx/xxx.iso"
                    },
                    new()
                    {
                        Name = "amd64-generic-hwe-18.04",
                        Url = "xxx/xxx.iso"
                    },
                    new()
                    {
                        Name = "amd64-generic-hwe-20.04",
                        Url = "xxx/xxx.iso"
                    },
                    new()
                    {
                        Name = "amd64-generic-hwe-21.10",
                        Url = "xxx/xxx.iso"
                    }
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
    public async Task<Mirror.MirrorItemDto> GetMirror(string id)
    {
        var res = new Mirror.MirrorItemDto()
        {
            Id = id,
            Url = $"/{id}",
            Name = new I18N.StringBase
            {
                En = "Debian",
                Zh = "Debian 发行版"
            },
            Description = new I18N.StringBase
            {
                En =
                    "Debian is a free and open-source operating system, based on the Linux kernel, and is the default distribution for the GNU/Linux operating system family.",
                Zh = "Debian 是一个基于 Linux 内核的免费和开源操作系统，它是 GNU/Linux 操作系统的默认发行版。"
            },
            Status = Mirror.MirrorStatus.Succeeded,
            LastSuccess = DateTime.Now,
            LastUpdated = DateTime.Now,
            NextScheduled = DateTime.Now + TimeSpan.FromHours(2),
            FileList = new List<Mirror.UrlItem>()
            {
                new()
                {
                    Name = "amd64-generic-hwe-12.04",
                    Url = "xxx/xxx.iso"
                },
                new()
                {
                    Name = "amd64-generic-hwe-14.04",
                    Url = "xxx/xxx.iso"
                },
                new()
                {
                    Name = "amd64-generic-hwe-16.04",
                    Url = "xxx/xxx.iso"
                },
                new()
                {
                    Name = "amd64-generic-hwe-18.04",
                    Url = "xxx/xxx.iso"
                },
                new()
                {
                    Name = "amd64-generic-hwe-20.04",
                    Url = "xxx/xxx.iso"
                },
                new()
                {
                    Name = "amd64-generic-hwe-21.10",
                    Url = "xxx/xxx.iso"
                }
            }
        };

        return res;
    }
}