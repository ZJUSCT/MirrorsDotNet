using System.Threading.Tasks;
using AutoMapper;
using Hangfire;
using Manager.Models;
using Manager.Services;
using Microsoft.AspNetCore.Mvc;
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
    private readonly MirrorContext _context;
    private readonly IMapper _mapper;
    private readonly IRecurringJobManager _jobManager;
    private readonly IIndexService _indexService;
    private readonly IConfigService _configService;

    public WebHookController(ILogger<WebHookController> logger, MirrorContext context, IMapper mapper, IRecurringJobManager jobManager, IIndexService indexService, IConfigService configService)
    {
        _logger = logger;
        _context = context;
        _mapper = mapper;
        _jobManager = jobManager;
        _indexService = indexService;
        _configService = configService;
    }

    /// <summary>
    /// Webhook index to test if the webhook is working
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IActionResult Get()
    {
        _logger.LogInformation("Incoming request at /webhook");
        return Ok("[Mirrors.NET] Manager webhooks are working.");
    }

    /// <summary>
    /// Update mirror sync status
    /// </summary>
    /// <param name="id">mirror id (should match with config file)</param>
    /// <param name="reportStatus">new status</param>
    [HttpPatch("sync/{id}")]
    public async Task<IActionResult> UpdateSyncStatus(string id,
        [FromForm(Name = "status")] MirrorStatus reportStatus)
    {
        _logger.LogInformation("Update Mirror Sync Status: {Id} {Status}", id, reportStatus);

        var mirrorConfig = await _context.Mirrors.FindAsync(id);
        if (mirrorConfig == null)
        {
            _logger.LogError("Mirror {Name} not found", id);
            return NotFound();
        }
        mirrorConfig.UpdateStatus(reportStatus);

        return Ok();
    }

    /// <summary>
    /// Trig the manager to re-gen the file index
    /// </summary>
    /// <param name="id">file index id (should match config file)</param>
    [HttpPost("index/{id}")]
    public async Task<IActionResult> UpdateFileIndex(string id)
    {
        await _indexService.GenIndexAsync(id);
        return Ok();
    }

    /// <summary>
    /// Hot reload configs
    /// </summary>
    [HttpPost("reload")]
    public async Task ReloadConfigs()
    {
        _logger.LogInformation("Reloading configs");
        await _configService.LoadConfigAsync();
    }
}