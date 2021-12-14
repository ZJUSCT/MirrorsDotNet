#define OLD_SHIM

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Manager.Controllers;

/// <summary>
///  This webhook controller is a temporary shim for the old-mirror scripts.
/// It collects sync status from cron scripts.
/// </summary>
[ApiController]
[Route("[controller]")]
public class WebHookController : ControllerBase
{
    private readonly ILogger<WebHookController> _logger;

    public WebHookController(ILogger<WebHookController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Update Package Status
    /// </summary>
    /// <param name="packageName"></param>
    [HttpPatch("package/{packageName}")]
    public void UpdatePackageSyncStatus(string packageName)
    {
        ;
    }

    /// <summary>
    /// Trig the manager to re-scan the release dir.
    /// </summary>
    /// <param name="releaseName"></param>
    [HttpPost("release/{releaseName}")]
    public void UpdateReleaseSyncStatus(string releaseName)
    {
        ;
    }
}