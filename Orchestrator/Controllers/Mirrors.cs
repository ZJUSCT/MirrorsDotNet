using Microsoft.AspNetCore.Mvc;
using Orchestrator.DataModels;
using Orchestrator.Services;
using Orchestrator.Utils;

namespace Orchestrator.Controllers;

[ApiController]
[Route("/mirrors")]
public class Mirrors(IConfiguration conf, JobQueue jobQueue) : CustomControllerBase(conf)
{
    private static string StatusToString(MirrorStatus status)
    {
        return status switch
        {
            MirrorStatus.Succeeded => "succeeded",
            MirrorStatus.Syncing => "syncing",
            MirrorStatus.Failed => "failed",
            MirrorStatus.Cached => "cached",
            _ => "unknown"
        };
    }

    [HttpGet("")]
    public ActionResult<IList<MirrorItemDto>> GetAllMirrors()
    {
        var mirrors = jobQueue.GetMirrorItems();
        return Ok(mirrors
            .Select(x => x.Value)
            .Select(x => new MirrorItemDto
            (
                x.Config.Id,
                "TODO",
                x.Config.Info.Name,
                x.Config.Info.Description,
                x.Config.Info.Upstream,
                x.Size.ToString(),
                StatusToString(x.Status),
                x.LastSyncAt.ToUnixTimeSeconds().ToString(),
                x.NextSyncAt().ToUnixTimeSeconds().ToString(),
                x.LastSyncAt.ToUnixTimeSeconds().ToString(),
                []
            )));
    }

    [HttpGet("{id}")]
    public ActionResult<MirrorItemDto> GetMirrorById([FromRoute] string id)
    {
        var mirror = jobQueue.GetMirrorItemById(id);
        return mirror == null ? NotFound() : Ok(mirror);
    }

    public record MirrorItemDto(
        string Id,
        string Url,
        I18NField Name,
        I18NField Desc,
        string Upstream,
        string Size,
        string Status,
        string LastUpdated,
        string NextScheduled,
        string LastSuccess,
        string[] Files);
}