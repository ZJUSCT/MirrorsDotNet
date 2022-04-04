using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    
    public JobController(ILogger<JobController> logger, MirrorContext context)
    {
        _logger = logger;
        _context = context;
    }
    
    [HttpGet]
    public async Task<List<MirrorSyncJob>> GetAllJobs()
    {
        return await _context.SyncJobs.ToListAsync();
    }
}