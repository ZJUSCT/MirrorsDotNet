using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Manager.Models;
using Manager.Services;
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
    private static readonly Mutex Mutex = new();

    public JobController(ILogger<JobController> logger, MirrorContext context, IMapper mapper, IIndexService indexService)
    {
        _logger = logger;
        _context = context;
        _mapper = mapper;
        _indexService = indexService;
    }

    /// <summary>
    /// Get all jobs
    /// </summary>
    /// <returns>list of all jobs</returns>
    [HttpGet]
    public async Task<List<MirrorSyncJob>> GetAllJobs()
    {
        return await _context.SyncJobs.ToListAsync();
    }

    /// <summary>
    /// Request a new job
    /// </summary>
    /// <param name="workerId">worker id</param>
    /// <returns>job information</returns>
    [HttpPost("request")]
    public IActionResult GetOneJob([FromForm(Name = "worker_id")] string workerId)
    {
        if (string.IsNullOrEmpty(workerId))
        {
            return BadRequest("Worker id is required");
        }

        Mutex.WaitOne();
        using var transaction = _context.Database.BeginTransaction();
        var firstUnDoneJob = _context.SyncJobs.FirstOrDefault(x => x.Status < JobStatus.Succeeded);
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
            _logger.LogInformation("Job {JobId} for mirror {MirrorId} assigned to worker {WorkerId}", firstUnDoneJob.Id, firstUnDoneJob.MirrorId, workerId);
            Mutex.ReleaseMutex();
            var jobDto = _mapper.Map<MirrorSyncJobDto>(firstUnDoneJob);
            return Ok(jobDto);
        }
        Mutex.ReleaseMutex();
        _logger.LogInformation("Worker {WorkerId} requested, but no job to be assigned", workerId);
        return NotFound();
    }

    /// <summary>
    /// Update job status
    /// </summary>
    /// <param name="jobId">job id</param>
    /// <param name="form">{WorkerId, JobId, Status, ErrorMessage}</param>
    /// <returns></returns>
    [HttpPost("{jobId:int}")]
    public async Task<IActionResult> UpdateJobStatus([FromRoute] int jobId, [FromForm] SyncJobUpdateForm form)
    {
        // Various checks
        if (jobId <= 0)
        {
            return BadRequest("Job id is required");
        }
        if (form == null)
        {
            return BadRequest("Job status is required");
        }
        if (form.JobId != jobId)
        {
            return BadRequest("Job id is not match");
        }

        // Get job item
        var job = await _context.SyncJobs.FirstOrDefaultAsync(x => x.Id == jobId);
        if (job == null)
        {
            return NotFound();
        }
        if (job.WorkerId != form.WorkerId)
        {
            return BadRequest("Worker id does not match");
        }
        var relatedMirrorItem = await _context.Mirrors.FirstOrDefaultAsync(x => x.Id == job.MirrorId);
        if (relatedMirrorItem == null)
        {
            Mutex.ReleaseMutex();
            return BadRequest("Related mirror item not found");
        }

        // Do update
        await using var transaction = await _context.Database.BeginTransactionAsync();
        job.Status = form.Status;
        job.UpdateTime = DateTime.Now;
        job.ErrorMessage = form.ErrorMessage;
        switch (form.Status)
        {
            case JobStatus.Running:
                relatedMirrorItem.UpdateStatus(MirrorStatus.Syncing);
                break;
            case JobStatus.Succeeded:
                relatedMirrorItem.UpdateStatus(MirrorStatus.Succeeded);
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
        _logger.LogInformation("Updated job {JobId} status to {Status}", jobId, form.Status);
        
        // Generate index
        if (form.Status == JobStatus.Succeeded && relatedMirrorItem.TrigIndex != null)
        {
            await _indexService.GenIndexAsync(relatedMirrorItem.TrigIndex);
        }
        return Ok();
    }
}