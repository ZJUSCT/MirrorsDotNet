using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Manager.Models;
using Manager.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Core;

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
    private readonly IDistributedCache _cache;
    private readonly ISchedulerFactory _schedulerFactory;

    public WebHookController(ILogger<WebHookController> logger, MirrorContext context, IMapper mapper, IDistributedCache cache, ISchedulerFactory schedulerFactory)
    {
        _logger = logger;
        _context = context;
        _mapper = mapper;
        _cache = cache;
        _schedulerFactory = schedulerFactory;
    }

    /// <summary>
    /// Webhook index to test if the webhook is working. 
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IActionResult Get()
    {
        _logger.LogInformation("Incoming request at /webhook");
        return Ok("[Mirrors.NET] Manager webhooks are working.");
    }

    /// <summary>
    /// Update Mirror Sync Status
    /// </summary>
    /// <param name="id">mirror id (should match with config file)</param>
    /// <param name="reportStatus">new status</param>
    [HttpPatch("sync/{id}")]
    public async Task<IActionResult> UpdateSyncStatus(string id,
        [FromForm(Name = "status")] Mirror.MirrorStatus reportStatus)
    {
        _logger.LogInformation("Update Mirror Sync Status: {Id} {Status}", id, reportStatus);

        var mirrorConfig = await _context.Mirrors.FindAsync(id);
        if (mirrorConfig == null)
        {
            _logger.LogError("Mirror {Name} not found", id);
            return NotFound();
        }
        mirrorConfig.UpdateStatus(reportStatus);

        await _cache.RemoveAsync(Constants.MirrorAllCacheKey);
        await _cache.RemoveAsync(Constants.MirrorItemCacheKeyPrefix + id);

        return Ok();
    }

    /// <summary>
    /// Trig the manager to re-scan the release dir.
    /// </summary>
    /// <param name="id">file index id (should match config file)</param>
    [HttpPost("index/{id}")]
    public async Task<IActionResult> UpdateFileIndex(string id)
    {
        _logger.LogInformation("UpdateReleaseSyncStatus: {Id}", id);

        var indexConfig = await _context.IndexConfigs.FindAsync(id);
        if (indexConfig == null)
        {
            _logger.LogError("Index config {Id} not found", id);
            return NotFound();
        }

        var registerTargetId = indexConfig.RegisterId;
        var targetMirrorItem = await _context.Mirrors.FindAsync(registerTargetId);

        if (targetMirrorItem == null)
        {
            _logger.LogError("Target mirror {Id} not found", registerTargetId);
            return StatusCode(StatusCodes.Status500InternalServerError, "Index Config Error");
        }

        var newUrlItems = DirWalker.GenIndex(indexConfig.IndexPath, indexConfig.Pattern, indexConfig.SortBy);

        // update url items
        // ref: https://docs.microsoft.com/en-us/ef/core/saving/disconnected-entities#handling-deletes
        foreach (var urlItem in newUrlItems)
        {
            var existingUrlItem = targetMirrorItem.Files.FirstOrDefault(url => url.Url.Equals(urlItem.Url));

            if (existingUrlItem == null)
            {
                targetMirrorItem.Files.Add(urlItem);
            }
            else
            {
                _context.Entry(existingUrlItem).CurrentValues.SetValues(urlItem);
            }
        }

        foreach (var urlItem in targetMirrorItem.Files.Where(urlItem => !newUrlItems.Any(url => url.Url.Equals(urlItem.Url))))
        {
            _context.Remove(urlItem);
        }

        await _context.SaveChangesAsync();
        await _cache.RemoveAsync(Constants.MirrorAllCacheKey);
        await _cache.RemoveAsync(Constants.MirrorItemCacheKeyPrefix + registerTargetId);

        return Ok();
    }

    /// <summary>
    /// Hot reload configs
    /// </summary>
    [HttpPost("reload")]
    public async Task ReloadConfigs()
    {
        _logger.LogInformation("Reloading configs");
        await ConfigLoader.LoadConfigAsync(_context, _mapper, _logger, _schedulerFactory);
        await _cache.RemoveAsync(Utils.Constants.MirrorAllCacheKey);
    }
}