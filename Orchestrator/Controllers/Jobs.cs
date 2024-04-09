using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Orchestrator.DataModels;
using Orchestrator.Services;
using Orchestrator.Utils;

namespace Orchestrator.Controllers;

[ApiController]
[Route("/jobs")]
public partial class Jobs(IConfiguration conf, ILogger<Jobs> log, JobQueue jobQueue) : CustomControllerBase(conf)
{
    private bool CheckWorkerToken() =>
        Request.Headers.TryGetValue("X-Worker-Token", out var values)
        && values.Contains(Conf["WorkerToken"]);

    [HttpGet("")]
    public ActionResult<GetJobsRes> GetJobs()
    {
        if (!CheckWorkerToken())
        {
            log.LogInformation("Unauthorized request to /jobs from {ip}", GetRequestIp());
            return Unauthorized(null);
        }

        var (pendingJobs, syncingJobs) = jobQueue.GetJobs();
        return Ok(pendingJobs[0]);
    }

    [HttpPost("fetch")]
    public ActionResult<ResponseDto<FetchRes>> FetchNewJob([FromBody] FetchReq req)
    {
        if (!CheckWorkerToken())
        {
            log.LogInformation("Unauthorized request to /jobs/fetch from {ip}", GetRequestIp());
            return Unauthorized(null);
        }

        var hasJob = jobQueue.TryGetNewJob(req.WorkerId, out var job);
        if (!hasJob) return Ok(Error("NO_PENDING_JOB"));

        var syncConfig = job!.MirrorItem.Config.Sync!;
        return Ok(Success(new FetchRes(
            job.Guid,
            syncConfig.JobName,
            syncConfig.Image,
            $"{syncConfig.Timeout.IntervalFree!.Value.Minutes}m",
            syncConfig.Volumes,
            syncConfig.Command,
            syncConfig.Environments
        )));
    }

    [HttpPut("{jobId}")]
    public ActionResult<ResponseDto<object>> UpdateJobStatus([FromRoute] Guid jobId, [FromBody] UpdateReq req)
    {
        if (!CheckWorkerToken())
        {
            log.LogInformation("Unauthorized request to /jobs/fetch from {ip}", GetRequestIp());
            return Unauthorized(null);
        }

        jobQueue.UpdateJobStatus(jobId, req.Status);
        return Ok(Success<object>(null));
    }

    [HttpPost("forceRefresh")]
    public ActionResult<ResponseDto<object>> ForceRefresh([FromBody] ForceRefreshReq req)
    {
        if (!CheckWorkerToken())
        {
            log.LogInformation("Unauthorized request to /jobs/forceRefresh from {ip}", GetRequestIp());
            return Unauthorized(null);
        }

        jobQueue.ForceRefresh(req.MirrorId);
        return Ok(Success<object>(null));
    }

    public record GetJobsRes(
        List<SyncJob> Pending,
        List<SyncJob> Syncing);

    public record FetchReq(string WorkerId);

    public record FetchRes(
        Guid Guid,
        string JobName,
        string Image,
        string Timeout,
        List<VolumeInfo> Volumes,
        List<string> Command,
        List<string> Environments);

    public record UpdateReq(MirrorStatus Status);

    public record ForceRefreshReq(string MirrorId);
}