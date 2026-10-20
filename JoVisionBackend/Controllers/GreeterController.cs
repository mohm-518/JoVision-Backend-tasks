
using Microsoft.AspNetCore.Mvc;

namespace JoVisionBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GreeterController : ControllerBase
    {
        [HttpGet]
        public string Get([FromQuery] string? name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "anonymous";
            }

            return $"Hello {name}";
        }
    }
}
