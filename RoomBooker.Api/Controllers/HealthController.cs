using Microsoft.AspNetCore.Mvc;

namespace RoomBooker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                status = "OK",
                time = DateTime.UtcNow
            });
        }
    }
}
