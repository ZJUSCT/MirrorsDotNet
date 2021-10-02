using System;
using Manager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Manager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MirrorsController : ControllerBase
    {
        private readonly ILogger<MirrorsController> _logger;
        private readonly MirrorZ.SiteInfo _siteInfo;

        public MirrorsController(ILogger<MirrorsController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _siteInfo = configuration.GetSection("SiteInfo").Get<MirrorZ.SiteInfo>();
        }

        [HttpGet]
        public MirrorZ.DataFormat Get()
        {
            var res = new MirrorZ.DataFormat
            {
                Site = _siteInfo,
                Packages = new MirrorZ.PackageInfo[]
                {
                    new()
                    {
                        Url = "/debian",
                        MappedName = "debian",
                        Description = "Debian packages",
                        HelpUrl = "/help/debian",
                        Size = "100G",
                        Status = "S",
                        Upstream = "https://mirrors.tuna.tsinghua.edu.cn"
                    }
                },
                Releases = new MirrorZ.ReleaseInfo[]
                {
                    new()
                    {
                        MappedName = "MeshLab",
                        Category = MirrorZ.ReleaseType.App,
                        UrlItems = new MirrorZ.UrlItem[]
                        {
                            new()
                            {
                                Url = "CG/meshlab-1.0.zip",
                                Name = "v1.0"
                            }
                        }
                    }
                }
            };
            return res;
        }
    }
}