using Microsoft.AspNetCore.Mvc;
using Orchestrator.Services;
using Orchestrator.Utils;

namespace Orchestrator.Controllers;

[ApiController]
[Route("/webhook")]
public class WebHook(IConfiguration conf, ILogger<WebHook> log, JobQueue jobQueue) : CustomControllerBase(conf)
{
    private bool CheckWebhookToken()
    {
        return Request.Headers.TryGetValue("X-Webhook-Token", out var values)
               && values.Contains(conf["WebhookToken"]);
    }

    [HttpPost("reload")]
    public IActionResult Reload()
    {
        if (!CheckWebhookToken())
        {
            log.LogInformation("Unauthorized request to /webhook/reload from {ip}", GetRequestIp());
            return Unauthorized(null);
        }

        var mirrors = jobQueue.GetMirrorItems().Select(x => x.Value);
        var (pending, syncing) = jobQueue.GetQueueStatus();
        log.LogInformation("Configuration reloaded");
        return Ok(Success(new
        {
            pending,
            syncing,
            data = mirrors
        }));
    }
}