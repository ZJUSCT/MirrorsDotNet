using System;
using System.Linq;
using System.Threading.Tasks;
using Manager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Manager.Services;

public class ScheduleService
{
    private readonly IServiceProvider _serviceProvider;
    public ScheduleService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Schedule(string id)
    {
        // Prepare the context
        using var scope = _serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ScheduleService>>();
        await using var context = scope.ServiceProvider.GetRequiredService<MirrorContext>();

        // Get the config
        var mirrorItem = await context.Mirrors.FindAsync(id);
        if (mirrorItem == null)
        {
            logger.LogError("Creating a job for a non-existing mirror {Id}", id);
            return;
        }

        // Skip when last job undone
        var unDoneJob = await context.SyncJobs
            .Where(x => x.Status < JobStatus.Succeeded && x.MirrorId == id) // TODO: remove hardcode
            .FirstOrDefaultAsync();
        if (unDoneJob != null)
        {
            logger.LogInformation("{Id} has an un-done job {JobId}, skip", id, unDoneJob.Id);
            return;
        }

        // Create new job
        var newJobItem = new MirrorSyncJob
        {
            MirrorId = id,
            ProviderImage = mirrorItem.ProviderImage,
            Location = mirrorItem.Location,
            Upstream = mirrorItem.Upstream,
            ExtraArgs = mirrorItem.ExtraArgs,
            ScheduleTime = DateTime.Now,
            UpdateTime = DateTime.Now,
            Status = JobStatus.Pending
        };
        await context.SyncJobs.AddAsync(newJobItem);
        await context.SaveChangesAsync();
        logger.LogInformation("{Id} is scheduled", mirrorItem.Id);
    }
}