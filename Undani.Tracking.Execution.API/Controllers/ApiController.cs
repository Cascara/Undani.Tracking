using Microsoft.AspNetCore.Mvc;

namespace Undani.Tracking.Execution.API.Controllers
{
    [Produces("application/json")]
    [Route("/")]
    public class ApiController : Controller
    {
        public string GetVersion()
        {
            return "2.0.1";
        }

    }
}