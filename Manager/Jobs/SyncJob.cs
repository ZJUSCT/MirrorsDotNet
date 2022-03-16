using System;
using System.Threading.Tasks;
using Quartz;

namespace Manager.Jobs;

public class SyncJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await Console.Out.WriteLineAsync("SyncJob is executing.");
    }
}