using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Manager.Models;
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
    private static Mutex _mutex = new();

    public JobController(ILogger<JobController> logger, MirrorContext context)
    {
        _logger = logger;
        _context = context;
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

        _mutex.WaitOne();
        var firstUnDoneJob = _context.SyncJobs.FirstOrDefault(x => x.Status < JobStatus.Succeeded);
        if (firstUnDoneJob != null)
        {
            firstUnDoneJob.WorkerId = workerId;
            firstUnDoneJob.Status = JobStatus.Assigned;
            _context.SaveChanges();
            _logger.LogInformation("Job {JobId} assigned to worker {WorkerId}", firstUnDoneJob.Id, workerId);
            _mutex.ReleaseMutex();
            var jobDto = _mapper.Map<MirrorSyncJobDto>(firstUnDoneJob);
            return Ok(jobDto);
        }
        _mutex.ReleaseMutex();
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

        // Do update
        job.Status = form.Status;
        job.UpdateTime = DateTime.Now;
        job.ErrorMessage = form.ErrorMessage;
        await _context.SaveChangesAsync();
        return Ok();
    }
}