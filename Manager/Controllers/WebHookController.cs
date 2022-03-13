using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Manager.Models;
using Manager.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Manager.Controllers;

/// <summary>
/// This webhook controller is a temporary shim for the old-mirror scripts.
/// It collects sync status from cron scripts.
/// </summary>
[ApiController]
[Route("webhook")]
public class WebHookController : ControllerBase
{
    private readonly ILogger<WebHookController> _logger;
    private readonly MirrorConfigContext _mirrorConfigContext;
    private readonly MirrorStatusContext _mirrorStatusContext;
    private readonly IMapper _mapper;
    private readonly IDistributedCache _cache;

    public enum ReportStatus
    {
        Syncing = 1, // to avoid default enum value 0
        Failed,
        Success,
        Pause
    }

    public WebHookController(ILogger<WebHookController> logger, MirrorConfigContext mirrorConfigContext,
        MirrorStatusContext mirrorStatusContext, IMapper mapper, IDistributedCache cache)
    {
        _logger = logger;
        _mirrorConfigContext = mirrorConfigContext;
        _mirrorStatusContext = mirrorStatusContext;
        _mapper = mapper;
        _cache = cache;
    }

    /// <summary>
    /// Webhook index to test if the webhook is working. 
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IActionResult Get()
    {
        _logger.LogInformation("Incoming request at /webhook");
        return Ok("[MirrorsDotNet] Manager webhooks are working.");
    }

    /// <summary>
    /// Update Package Status
    /// </summary>
    /// <param name="packageName">package name (should match with config file)</param>
    /// <param name="reportStatus">new status</param>
    [HttpPatch("package/{packageName}")]
    public async Task<IActionResult> UpdatePackageSyncStatus(string packageName,
        [FromForm(Name = "status")] ReportStatus reportStatus)
    {
        _logger.LogInformation("UpdatePackageSyncStatus: {Name} {Status}", packageName, reportStatus);

        var packageConfig = await _mirrorConfigContext.Packages.FindAsync(packageName);
        if (packageConfig == null)
        {
            _logger.LogError("Package {Name} not found in configs", packageName);
            return NotFound();
        }

        var mappedName = packageConfig.MappedName;
        var packageStatus = await _mirrorStatusContext.Packages.FindAsync(mappedName);
        if (packageStatus == null)
        {
            _logger.LogError("Package {Name} not found in status context, but exists in configs", mappedName);
            return StatusCode(StatusCodes.Status500InternalServerError, "DB Error");
        }

        try
        {
            var timeStampString = $"{TimeStamp.UnixTimeStamp():D10}";
            var statusString = reportStatus switch
            {
                ReportStatus.Syncing => "Y",
                ReportStatus.Failed => "F",
                ReportStatus.Success => "S",
                ReportStatus.Pause => "P",
                _ => throw new ArgumentOutOfRangeException(nameof(reportStatus), reportStatus, null)
            };
            packageStatus.Status = statusString + timeStampString;
            await _mirrorStatusContext.SaveChangesAsync();
        }
        catch (ArgumentOutOfRangeException)
        {
            _logger.LogError("Invalid Status {Status}, check if the webhook url is wrong", reportStatus);
            return StatusCode(StatusCodes.Status500InternalServerError, "Invalid Status");
        }

        await _cache.RemoveAsync(Utils.Constants.MirrorStatusCacheKey);

        return Ok();
    }

    /// <summary>
    /// Trig the manager to re-scan the release dir.
    /// </summary>
    /// <param name="releaseName">release name (should match config file)</param>
    [HttpPost("release/{releaseName}")]
    public async Task<IActionResult> UpdateReleaseSyncStatus(string releaseName)
    {
        _logger.LogInformation("UpdateReleaseSyncStatus: {Name}", releaseName);

        var releaseConfig = await _mirrorConfigContext.Releases.FindAsync(releaseName);
        if (releaseConfig == null)
        {
            _logger.LogError("Release {Name} not found in configs", releaseName);
            return NotFound();
        }

        var mappedName = releaseConfig.MappedName;
        var releaseStatus = await _mirrorStatusContext.Releases
            .Include(release => release.UrlItems)
            .FirstOrDefaultAsync(release => release.MappedName.Equals(mappedName));

        if (releaseStatus == null)
        {
            _logger.LogError("Release {Name} not found in status context, but exists in configs", mappedName);
            return StatusCode(StatusCodes.Status500InternalServerError, "DB Error");
        }

        var newUrlItems = DirWalker.GenIndex(releaseConfig.IndexPath, releaseConfig.Pattern, releaseConfig.SortBy);

        // update url items
        // ref: https://docs.microsoft.com/en-us/ef/core/saving/disconnected-entities#handling-deletes
        foreach (var urlItem in newUrlItems)
        {
            var existingUrlItem = releaseStatus.UrlItems.FirstOrDefault(url => url.Url.Equals(urlItem.Url));

            if (existingUrlItem == null)
            {
                releaseStatus.UrlItems.Add(urlItem);
            }
            else
            {
                _mirrorStatusContext.Entry(existingUrlItem).CurrentValues.SetValues(urlItem);
            }
        }

        foreach (var urlItem in releaseStatus.UrlItems.Where(urlItem => !newUrlItems.Any(url => url.Url.Equals(urlItem.Url))))
        {
            _mirrorStatusContext.Remove(urlItem);
        }

        await _mirrorStatusContext.SaveChangesAsync();
        await _cache.RemoveAsync(Utils.Constants.MirrorStatusCacheKey);

        return Ok();
    }

    /// <summary>
    /// Hot reload configs
    /// </summary>
    [HttpPost("reload")]
    public async Task ReloadConfigs()
    {
        _logger.LogInformation("Reloading configs");
        await ConfigLoader.LoadConfigAsync(_mirrorConfigContext, _mirrorStatusContext, _mapper, _logger);
        await _cache.RemoveAsync(Utils.Constants.MirrorStatusCacheKey);
    }
}