using Microsoft.AspNetCore.Mvc;

namespace Orchestrator.Controllers;

[ApiController]
[Route("/")]
public class Root : ControllerBase
{
    [HttpGet("")]
    public ActionResult<string> Ping()
    {
        return Ok($"Mirror orchestrator running ({Version.CommitHash})");
    }
}