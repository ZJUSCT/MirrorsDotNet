using System;
using System.Threading.Tasks;
using Manager.Models;
using Manager.Utils;
using Quartz;

namespace Manager.Jobs;

public class SyncJob : IJob
{
    private readonly MirrorContext _mirrorContext;
    
    private SyncJob(MirrorContext mirrorContext)
    {
        _mirrorContext = mirrorContext;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await Console.Out.WriteLineAsync($"SyncJob {context.JobDetail.Key.Name} is executing.");
        var dataMap = context.JobDetail.JobDataMap;
        var id = dataMap.GetString(Constants.JobDataMapMirrorId);
        await _mirrorContext.Mirrors.FindAsync(id);
        await Console.Out.WriteLineAsync($"SyncJob {context.JobDetail.Key.Name} is executing. Mirror-Id: {id}");
    }
}