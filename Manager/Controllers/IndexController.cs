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
}