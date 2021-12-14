using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Manager.Controllers;

/**
 * This webhook controller is a temporary shim for the old-mirror scripts.
 * It collects sync status from cron scripts.
 */

[ApiController]
[Route("[controller]")]
public class WebHookController : ControllerBase
{
    private readonly ILogger<WebHookController> _logger;

    public WebHookController(ILogger<WebHookController> logger)
    {
        _logger = logger;
    }

    [HttpPatch("package/{packageName}")]
    public void UpdatePackageSyncStatus(string packageName)
    {
        ;
    }

    [HttpPatch("package/{releaseName}")]
    public void UpdateReleaseSyncStatus(string releaseName)
    {
        ;
    }
}