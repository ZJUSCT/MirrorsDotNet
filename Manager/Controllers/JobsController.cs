using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Manager.Models;
using Manager.Services;
using Manager.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Manager.Controllers;

[ApiController]
[Route("jobs")]
public class JobController : ControllerBase
{
    private readonly ILogger<JobController> _logger;
    private readonly MirrorContext _context;
    private readonly IMapper _mapper;
    private readonly IIndexService _indexService;
    private readonly IMetricExporterService _exporter;
    private static readonly Mutex Mutex = new();

    public JobController(ILogger<JobController> logger, MirrorContext context, IMapper mapper,
        IIndexService indexService, IMetricExporterService exporter)
    {
        _logger = logger;
        _context = context;
        _mapper = mapper;
        _indexService = indexService;
        _exporter = exporter;
    }

    /// <summary>
    /// Get all jobs
    /// </summary>
    /// <param name="showDone">show finished jobs</param>
    /// <returns>list of all jobs</returns>
    /// <response code="200">Returns list of jobs</response>
    [HttpGet]
    public async Task<List<MirrorSyncJob>> GetAllJobs([FromQuery] bool showDone = false)
    {
        if (showDone)
        {
            return await _context.SyncJobs.ToListAsync();
        }

        return await _context.SyncJobs.Where(j => j.Status < JobStatus.Succeeded).ToListAsync();
    }

    /// <summary>
    /// Request a new job
    /// </summary>
    /// <param name="workerId">worker id</param>
    /// <returns>job information</returns>
    /// <response code="200">Returns a new job</response>
    /// <response code="204">No new job</response>
    [HttpPost("request")]
    public IActionResult GetOneJob([FromForm(Name = "worker_id")] string workerId)
    {
        if (string.IsNullOrEmpty(workerId))
        {
            return BadRequest("Worker id is required");
        }

        Mutex.WaitOne();
        using var transaction = _context.Database.BeginTransaction();
        var firstUnDoneJob =
            _context.SyncJobs.FirstOrDefault(x => x.Status == JobStatus.Pending);
        if (firstUnDoneJob != null)
        {
            var relatedMirrorItem = _context.Mirrors.FirstOrDefault(x => x.Id == firstUnDoneJob.MirrorId);
            if (relatedMirrorItem == null)
            {
                Mutex.ReleaseMutex();
                return BadRequest("Related mirror item not found");
            }

            relatedMirrorItem.Status = MirrorStatus.Syncing;
            firstUnDoneJob.WorkerId = workerId;
            firstUnDoneJob.Status = JobStatus.Assigned;
            _context.SaveChanges();
            transaction.Commit();
            _logger.LogInformation("Job {JobId} for mirror {MirrorId} assigned to worker {WorkerId}", firstUnDoneJob.Id,
                firstUnDoneJob.MirrorId, workerId);
            Mutex.ReleaseMutex();
            var jobDto = _mapper.Map<MirrorSyncJobDto>(firstUnDoneJob);
            return Ok(jobDto);
        }

        Mutex.ReleaseMutex();
        _logger.LogInformation("Worker {WorkerId} requested, but no job to be assigned", workerId);
        return NoContent();
    }
    
    private async Task CheckTimeoutJob()
    {
        // iterate all running jobs to check timeout
        var runningJobs = await _context.SyncJobs
            .Where(x => x.Status == JobStatus.Assigned)
            .ToListAsync();
        var timeoutJobs = runningJobs.Where(x => x.UpdateTime.Add(TimeHelper.ParseTimeSpan(x.Timeout)) < DateTime.Now).ToList();

        // update status
        foreach (var job in timeoutJobs)
        {
            var relatedMirrorItem = await _context.Mirrors.FirstOrDefaultAsync(x => x.Id == job.MirrorId);
            if (relatedMirrorItem == null)
            {
                continue;
            }

            relatedMirrorItem.UpdateStatus(MirrorStatus.Failed);
            job.Status = JobStatus.Failed;
            job.ErrorMessage = "Timeout";
            await _context.SaveChangesAsync();
            _exporter.ExportMirrorState(relatedMirrorItem.Id, relatedMirrorItem.Status);
            _logger.LogInformation("Job {JobId} for mirror {MirrorId} timeout", job.Id, job.MirrorId);
        }
    }

    /// <summary>
    /// Update job status
    /// </summary>
    /// <param name="jobId">job id</param>
    /// <param name="body">{WorkerId, JobId, Status, ErrorMessage}</param>
    /// <response code="204">Update Success</response>
    /// <response code="400">Bad Request: format error</response>
    /// <response code="404">Job not found</response>
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [HttpPut("{jobId:int}")]
    public async Task<IActionResult> UpdateJobStatus([FromRoute] int jobId, [FromBody] SyncJobUpdateBody body)
    {
        // Various checks
        if (jobId <= 0)
        {
            return BadRequest("Job id is required");
        }

        if (body == null)
        {
            return BadRequest("Job status is required");
        }

        if (body.JobId != jobId)
        {
            return BadRequest("Job id is not match");
        }

        // Get job item
        var job = await _context.SyncJobs.FirstOrDefaultAsync(x => x.Id == jobId);
        if (job == null)
        {
            return NotFound();
        }

        if (job.WorkerId != body.WorkerId)
        {
            return BadRequest("Worker id does not match");
        }

        var relatedMirrorItem = await _context.Mirrors.FirstOrDefaultAsync(x => x.Id == job.MirrorId);
        if (relatedMirrorItem == null)
        {
            return BadRequest("Related mirror item not found");
        }

        // Do update
        Mutex.WaitOne();
        await using var transaction = await _context.Database.BeginTransactionAsync();
        job.Status = body.Status;
        job.ContainerId = body.ContainerId;
        job.UpdateTime = DateTime.Now;
        job.ErrorMessage = body.ErrorMessage;
        switch (body.Status)
        {
            case JobStatus.Running:
                relatedMirrorItem.UpdateStatus(MirrorStatus.Syncing);
                break;
            case JobStatus.Succeeded:
                relatedMirrorItem.UpdateStatus(MirrorStatus.Succeeded);
                relatedMirrorItem.Size = body.FileSize;
                break;
            case JobStatus.Failed:
                relatedMirrorItem.UpdateStatus(MirrorStatus.Failed);
                break;
            case JobStatus.Pending:
            case JobStatus.Assigned:
            default:
                throw new ArgumentOutOfRangeException();
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        // Check timeout for other running jobs
        await CheckTimeoutJob();

        Mutex.ReleaseMutex();

        _exporter.ExportMirrorState(relatedMirrorItem.Id, relatedMirrorItem.Status);
        _logger.LogInformation("Updated job {JobId} status to {Status}", jobId, body.Status);

        // Generate index
        if (body.Status == JobStatus.Succeeded && relatedMirrorItem.TrigIndex != null)
        {
            await _indexService.GenIndexAsync(relatedMirrorItem.TrigIndex);
        }

        return NoContent();
    }
}