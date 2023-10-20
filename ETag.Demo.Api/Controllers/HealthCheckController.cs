using ETag.Demo.Api.Filters;
using Microsoft.AspNetCore.Mvc;

namespace ETag.Demo.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthCheckController : Controller
    {
        [HttpGet]
        // comment out if global ETag has been disabled
        //[ETag]
        public IActionResult Get()
        {
            return Ok(new { healthy = "OK" });
        }
    }
}
