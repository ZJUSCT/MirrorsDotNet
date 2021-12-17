#define OLD_SHIM

using Manager.Models;
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

    public WebHookController(ILogger<WebHookController> logger, MirrorConfigContext mirrorConfigContext,
        MirrorStatusContext mirrorStatusContext)
    {
        _logger = logger;
        _mirrorConfigContext = mirrorConfigContext;
        _mirrorStatusContext = mirrorStatusContext;
    }

    /// <summary>
    /// Update Package Status
    /// </summary>
    /// <param name="packageName">package name (should match with config file)</param>
    [HttpPatch("package/{packageName}")]
    public void UpdatePackageSyncStatus(string packageName)
    {
        ; // TODO: Pure update logic
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

    /// <summary>
    /// Hot reload configs
    /// </summary>
    [HttpPost("reload")]
    public void ReloadConfigs()
    {
        ; // TODO: Reload configs
    }
}