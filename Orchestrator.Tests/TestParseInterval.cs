using Orchestrator.DataModels;

namespace Orchestrator.Tests;

public class TestParseInterval
{
    private TimeSpan ParseInterval(string interval)
    {
        return new IntervalInfo(new IntervalInfoRaw("free", interval)).IntervalFree!.Value;
    }
    
    [Test]
    public void TestParses()
    {
        Assert.Multiple(() =>
        {
            Assert.That(ParseInterval("3d"), Is.EqualTo(TimeSpan.FromDays(3)));
            Assert.That(ParseInterval("2w"), Is.EqualTo(TimeSpan.FromDays(14)));
            Assert.That(ParseInterval("75m"), Is.EqualTo(TimeSpan.FromMinutes(75)));
            Assert.That(ParseInterval("3h65m"), Is.EqualTo(TimeSpan.FromHours(3) + TimeSpan.FromMinutes(65)));
            Assert.That(ParseInterval("1h4m"), Is.EqualTo(TimeSpan.FromHours(1) + TimeSpan.FromMinutes(4)));
            Assert.That(ParseInterval("22m 3d"), Is.EqualTo(TimeSpan.FromMinutes(22) + TimeSpan.FromDays(3)));
        });
    }
}