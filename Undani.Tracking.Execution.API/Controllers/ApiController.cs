using Microsoft.AspNetCore.Mvc;

namespace Undani.Tracking.Execution.API.Controllers
{
    [Produces("application/json")]
    [Route("/")]
    public class ApiController : Controller
    {
        public string GetVersion()
        {
            return "Tracking v2.0.1 2022-03-17T12:04:25 / .Net Core 2.1";
        }

    }
}