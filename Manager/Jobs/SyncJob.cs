using System;
using System.Threading.Tasks;
using Manager.Utils;
using Quartz;

namespace Manager.Jobs;

public class SyncJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await Console.Out.WriteLineAsync($"SyncJob {context.JobDetail.Key.Name} is executing.");
        var dataMap = context.JobDetail.JobDataMap;
        var image = dataMap.GetString(Constants.JobDataMapImage);
        var upStream = dataMap.GetString(Constants.JobDataMapUpStream);
        var extraArgs = dataMap.GetString(Constants.JobDataMapExtraArgs);
        await Console.Out.WriteLineAsync($"SyncJob {context.JobDetail.Key.Name} is executing. image: {image}, upStream: {upStream}, extraArgs: {extraArgs}");
    }
}