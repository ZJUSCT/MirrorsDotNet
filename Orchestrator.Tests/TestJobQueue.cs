using Microsoft.Extensions.Logging;
using Orchestrator.DataModels;
using Orchestrator.Services;

namespace Orchestrator.Tests;

public class TestJobQueue
{
    private MirrorItemInfo genMirrorItem(string id, TimeSpan interval, TimeSpan timeout, MirrorStatus status, DateTime lastSyncAt)
    {
        I18NField i18N = new("en", "zh");
        var info = new MirrorInfo { Name = i18N, Description = i18N, Type = SyncType.Sync, Upstream = "" };
        return new MirrorItemInfo(new ConfigInfo
            {
                Id = id, Info = info, Sync = new SyncInfo
                {
                    JobName = $"job-{id}", Interval = interval, Timeout = timeout,
                    Image = "", Pull = PullStrategy.Always, Volumes = [], Command = [], Environments = []
                }
            })
            { Status = status, LastSyncAt = lastSyncAt, };
    }

    private void PrintMirrorStatus(IStateStore store)
    {
        foreach (var kv in store.GetMirrorItemInfos())
        {
            Console.WriteLine($"{kv.Key}: {kv.Value.Status} LSA:{kv.Value.LastSyncAt.ToShortTimeString()}");
        }
    }
    
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        List<MirrorItemInfo> itemInfos =
        [
            genMirrorItem("foo1", TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(2), MirrorStatus.Unknown, DateTime.MinValue),
            genMirrorItem("foo2", TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(2), MirrorStatus.Unknown, DateTime.MinValue),
            genMirrorItem("foo3", TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(2), MirrorStatus.Unknown, DateTime.MinValue),
        ];
        var conf = new MockConfiguration();
        var logger = new MockLogger<JobQueue>();
        var stateStore = new MockStateStore(itemInfos);
        var jobQueue = new JobQueue(conf, logger, stateStore);
        jobQueue.RetryCoolDown = TimeSpan.FromSeconds(5);
        
        PrintMirrorStatus(stateStore);
        
        logger.LogInformation("Get All jobs");
        List<SyncJob> jobs = [];
        bool gotJob;
        SyncJob? job;
        for (var i = 0; i < 3; ++i)
        {
            gotJob = jobQueue.TryGetNewJob(out job);
            Assert.That(gotJob, Is.True);
            jobs.Add(job!);
            logger.LogInformation("Got job {jobId} for {mirrorId}", job!.Guid, job.MirrorItem.Config.Id);
        }
        gotJob = jobQueue.TryGetNewJob(out _);
        Assert.That(gotJob, Is.False);
        PrintMirrorStatus(stateStore);
        
        logger.LogInformation("Finish All jobs");
        foreach (var j in jobs)
        {
            jobQueue.UpdateJobStatus(j.Guid, MirrorStatus.Succeeded);
        }
        PrintMirrorStatus(stateStore);
        
        Thread.Sleep(2100);
        logger.LogInformation("Before interval, no job should be available");
        // should get no job here
        gotJob = jobQueue.TryGetNewJob(out _);
        Assert.That(gotJob, Is.False);
        
        Thread.Sleep(2000);
        // get a job and fail it
        logger.LogInformation("Get a job and fail it");
        gotJob = jobQueue.TryGetNewJob(out job);
        Assert.That(gotJob, Is.True);
        logger.LogInformation("Got job {jobId} for {mirrorId}", job!.Guid, job.MirrorItem.Config.Id);
        PrintMirrorStatus(stateStore);
        logger.LogInformation("Fail the job");
        jobQueue.UpdateJobStatus(job.Guid, MirrorStatus.Failed);
        PrintMirrorStatus(stateStore);
        
        // check if we can get the old job
        Thread.Sleep(5000);
        logger.LogInformation("Check if we can still get the job");
        var oldGuid = job.Guid;
        gotJob = jobQueue.TryGetNewJob(out job);
        Assert.That(gotJob, Is.True);
        logger.LogInformation("Got job {jobId} for {mirrorId}", job!.Guid, job.MirrorItem.Config.Id);
        Assert.That(job!.Guid, Is.EqualTo(oldGuid));
        Assert.That(job!.FailedCount, Is.EqualTo(1));
        PrintMirrorStatus(stateStore);
        jobQueue.UpdateJobStatus(job.Guid, MirrorStatus.Failed);
        
        // make a lost job
        logger.LogInformation("Get a job");
        gotJob = jobQueue.TryGetNewJob(out job);
        logger.LogInformation("Got job {jobId} for {mirrorId}", job!.Guid, job.MirrorItem.Config.Id);
        Assert.That(gotJob, Is.True);
        PrintMirrorStatus(stateStore);
        
        Thread.Sleep(7100);
        logger.LogInformation("Made it lost");
        PrintMirrorStatus(stateStore);
        
        // finish all jobs
        jobs.Clear();
        logger.LogInformation("Finish all jobs");
        gotJob = jobQueue.TryGetNewJob(out job);
        Assert.That(gotJob, Is.True);
        jobs.Add(job!);
        logger.LogInformation("Got job {jobId} for {mirrorId}", job!.Guid, job.MirrorItem.Config.Id);
        // The lost job is in CD
        Thread.Sleep(4100);
        for (var i = 1; i < 3; ++i)
        {
            gotJob = jobQueue.TryGetNewJob(out job);
            Assert.That(gotJob, Is.True);
            jobs.Add(job!);
            logger.LogInformation("Got job {jobId} for {mirrorId}", job!.Guid, job.MirrorItem.Config.Id);
        }
        foreach (var j in jobs)
        {
            jobQueue.UpdateJobStatus(j.Guid, MirrorStatus.Succeeded);
        }
        PrintMirrorStatus(stateStore);
        
        // should get no job here
        gotJob = jobQueue.TryGetNewJob(out _);
        Assert.That(gotJob, Is.False);
        
        Assert.Pass();
    }
}