using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Storage;
using Manager.Models;
using Manager.Utils;
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

    public async Task Schedule(string mirrorId)
    {
        // Prepare the context
        using var scope = _serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ScheduleService>>();
        await using var context = scope.ServiceProvider.GetRequiredService<MirrorContext>();

        // Get the config
        var mirrorItem = await context.Mirrors.FindAsync(mirrorId);
        if (mirrorItem == null)
        {
            logger.LogError("Creating a job for a non-existing mirror {Id}", mirrorId);
            return;
        }

        // Skip when last job undone
        var unDoneJob = await context.SyncJobs
            .Where(x => x.Status < JobStatus.Succeeded && x.MirrorId == mirrorId) // TODO: remove hardcode
            .FirstOrDefaultAsync();
        if (unDoneJob != null)
        {
            logger.LogInformation("{Id} has an un-done job {JobId}, skip", mirrorId, unDoneJob.Id);
            return;
        }

        // Create new job
        var newJobItem = new MirrorSyncJob
        {
            MirrorId = mirrorId,
            ProviderImage = mirrorItem.ProviderImage,
            Location = mirrorItem.Location,
            Upstream = mirrorItem.Upstream,
            ExtraArgs = mirrorItem.ExtraArgs,
            ScheduleTime = DateTime.Now,
            UpdateTime = DateTime.Now,
            Status = JobStatus.Pending
        };
        var job = JobStorage.Current.GetConnection().GetRecurringJobs().Single(x => x.Id == $"{Constants.HangFireJobPrefix}{mirrorId}");
        var relatedMirror = await context.Mirrors.FindAsync(mirrorId);
        if (relatedMirror == null)
        {
            logger.LogError("Creating a job for a non-existing mirror {Id}", mirrorId);
            return;
        }

        relatedMirror.NextScheduled = job.NextExecution ?? DateTime.MinValue;
        await context.SyncJobs.AddAsync(newJobItem);
        await context.SaveChangesAsync();
        logger.LogInformation("{Id} is scheduled", mirrorItem.Id);
    }
}