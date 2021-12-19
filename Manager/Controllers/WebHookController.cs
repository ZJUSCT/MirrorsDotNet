﻿#define OLD_SHIM

using System;
using System.Threading.Tasks;
using AutoMapper;
using Manager.Models;
using Manager.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Manager.Controllers;

/// <summary>
///  This webhook controller is a temporary shim for the old-mirror scripts.
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

    public enum Status
    {
        Syncing = 1, // to avoid default enum value 0
        Failed,
        Success,
        Pause
    }

    public WebHookController(ILogger<WebHookController> logger, MirrorConfigContext mirrorConfigContext,
        MirrorStatusContext mirrorStatusContext, IMapper mapper)
    {
        _logger = logger;
        _mirrorConfigContext = mirrorConfigContext;
        _mirrorStatusContext = mirrorStatusContext;
        _mapper = mapper;
    }

#if OLD_SHIM
    /// <summary>
    /// Update Package Status
    /// </summary>
    /// <param name="packageName">package name (should match with config file)</param>
    /// <param name="status">new status</param>
    [HttpPatch("package/{packageName}")]
    public async Task<IActionResult> UpdatePackageSyncStatus(string packageName,
        [FromQuery(Name = "status")] Status status)
    {
        _logger.LogInformation("UpdatePackageSyncStatus: {Name} {Status}", packageName, status);

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
            var statusString = status switch
            {
                Status.Syncing => "Y",
                Status.Failed => "F",
                Status.Success => "S",
                Status.Pause => "P",
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };
            packageStatus.Status = statusString + timeStampString;
            await _mirrorStatusContext.SaveChangesAsync();
        }
        catch (ArgumentOutOfRangeException)
        {
            _logger.LogError("Invalid Status {Status}, check if the webhook url is wrong", status);
            return StatusCode(StatusCodes.Status500InternalServerError, "Invalid Status");
        }

        return Ok();
    }

    /// <summary>
    /// Trig the manager to re-scan the release dir.
    /// </summary>
    /// <param name="releaseName">release name (should match config file)</param>
    [HttpPost("release/{releaseName}")]
    public void UpdateReleaseSyncStatus(string releaseName)
    {
        ; // TODO: Rescan, update index
    }
#endif

    /// <summary>
    /// Hot reload configs
    /// </summary>
    [HttpPost("reload")]
    public async Task ReloadConfigs()
    {
        _logger.LogInformation("Reloading configs");
        await ConfigLoader.LoadConfigAsync(_mirrorConfigContext, _mirrorStatusContext, _mapper, _logger);
    }
}