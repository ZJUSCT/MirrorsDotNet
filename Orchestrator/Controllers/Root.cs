using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Orchestrator.Utils;

namespace Orchestrator.Controllers;

[ApiController]
[Route("/")]
public class Root : ControllerBase
{
    [HttpGet("")]
    [OutputCache(Duration = 30)]
    public ActionResult<string> Ping()
    {
        return Ok($"Mirror orchestrator running ({Version.CommitHash}) {DateTime.Now.ToUnixTimeSeconds()}");
    }
}