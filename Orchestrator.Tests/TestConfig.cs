using System.Text.Json;
using Orchestrator.DataModels;

namespace Orchestrator.Tests;

public class TestConfig
{
    private ConfigInfo GenItem(string id)
    {
        I18NField i18N = new("en", "zh");
        var info = new MirrorInfo { Name = i18N, Description = i18N, Type = SyncType.Sync, Upstream = "", Url = "" };
        return new ConfigInfo
        {
            Id = id, Info = info,
            Sync = new SyncInfo
            {
                JobName = $"job-{id}", Interval = new IntervalInfo(TimeSpan.FromSeconds(10)), Timeout = new IntervalInfo(TimeSpan.FromMinutes(1)),
                Image = "", Pull = PullStrategy.Always, Volumes = [], Command = [], Environments = []
            }
        };
    }
    
    [Test]
    public void TestDeserializeConfig()
    {
        var config = @"{
  ""$schema"": ""../Schemas/mirror-item.schema.json"",
  ""id"": ""foo"",
  ""info"": {
    ""name"": {
      ""en"": ""Foo Bar"",
      ""zh"": ""福报""
    },
    ""description"": {
      ""en"": ""This is a foo bar module"",
      ""zh"": ""这是一个福报模块""
    },
    ""type"": ""sync"",
    ""upstream"": ""rsync://foo.example.com/bar/""
  },
  ""sync"": {
    ""jobName"": ""job-rsync-foo"",
    ""interval"": ""4h"",
    ""timeout"": ""5h"",
    ""image"": ""foo/bar:latest"",
    ""pull"": ""never"",
    ""volumes"": [
      {
        ""src"": ""/data/foo"",
        ""dst"": ""/data""
      },
      {
        ""src"": ""/var/log/foo"",
        ""dst"": ""/log/foo""
      },
      {
        ""src"": ""/scripts/rsync.sh"",
        ""dst"": ""/rsync.sh"",
        ""readOnly"": true
      }
    ],
    ""command"": [""/bin/bash"", ""/rsync.sh""],
    ""environments"": [
      ""TZ=Asia/Shanghai"",
      ""RSYNC_UPSTREAM=foo.example.com::bar""
    ]
  }
}";
        var item = JsonSerializer.Deserialize<ConfigInfoRaw>(config, new JsonSerializerOptions{PropertyNameCaseInsensitive = true});
        Assert.That(config != null, Is.True);
    }

    [Test]
    public void TestSerialization()
    {
      var item = GenItem("foo");
      var json = JsonSerializer.Serialize(item);
      Console.WriteLine(json);
      Assert.Pass();
    }
}