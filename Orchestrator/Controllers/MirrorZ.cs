using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Orchestrator.DataModels;
using Orchestrator.Services;
using Orchestrator.Utils;

namespace Orchestrator.Controllers;

[ApiController]
[Route("/mirrorz.json")]
[Produces("application/json")]
public class MirrorZInfo(IConfiguration conf, ILogger<MirrorZInfo> log, JobQueue jobQueue) : CustomControllerBase(conf)
{
    private string TransformStatus(SyncJob job)
    {
        var item = job.MirrorItem;
        if (item.Status == MirrorStatus.Cached) return "C";
        if (item.Status == MirrorStatus.Succeeded)
            return $"S{item.LastSuccessAt.ToUnixTimeSeconds()}X{job.TaskShouldStartAt.ToUnixTimeSeconds()}";
        if (item.Status == MirrorStatus.Syncing)
            return $"Y{job.TaskStartedAt.ToUnixTimeSeconds()}O{item.LastSuccessAt.ToUnixTimeSeconds()}";
        if (item.Status == MirrorStatus.Failed)
            return $"F{item.LastSyncAt.ToUnixTimeSeconds()}O{item.LastSuccessAt.ToUnixTimeSeconds()}";
        return "U";
    }
    
    [HttpGet("")]
    [OutputCache(Duration = 30)]
    public ActionResult<MirrorZData> GetMirrorZData()
    {
        var (pendingJobs, syncingJobs) = jobQueue.GetJobs();
        var jobs = (pendingJobs as IEnumerable<SyncJob>).Concat(syncingJobs).Where(x => x.Stale == false).ToList();
        var transformedItems = jobs
            .Select(x => new MirrorZMirrorItem(
                x.MirrorItem.Config.Id,
                x.MirrorItem.Config.Info.Description.Zh,
                x.MirrorItem.Config.Info.Url,
                TransformStatus(x),
                $"/docs/{x.MirrorItem.Config.Id}",
                x.MirrorItem.Config.Info.Upstream,
                "0"
            )).ToList();

        return new MirrorZData(
            1.7,
            MirrorZStatic.SiteInfo,
            new List<MirrorZCatItem>(),
            transformedItems);
    }
}