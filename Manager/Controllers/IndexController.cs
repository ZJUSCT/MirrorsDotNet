using Microsoft.AspNetCore.Mvc;

namespace Manager.Controllers;

[ApiController]
[Route("")]
public class IndexController : Controller
{
    /// <summary>
    /// Home index
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public ActionResult Index()
    {
        return Content("[Mirrors.NET] Manager API Working...");
    }

    /// <summary>
    /// Terms
    /// </summary>
    /// <returns></returns>
    [HttpGet("terms")]
    public ActionResult Terms()
    {
        return Content("Non-commercial use only.");
    }

    /// <summary>
    /// License
    /// </summary>
    /// <returns></returns>
    [HttpGet("license")]
    public ActionResult License()
    {
        return Content("Internal Usage Only");
    }
}