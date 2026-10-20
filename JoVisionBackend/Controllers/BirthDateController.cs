
using Microsoft.AspNetCore.Mvc;

namespace JoVisionBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BirthDateController : ControllerBase
    {
        [HttpGet]
        public string Get(
            [FromQuery] string? name = null,
            [FromQuery] int? years = null,
            [FromQuery] int? months = null,
            [FromQuery] int? days = null)
        {
            if (years == null || months == null || days == null)
            {
                name = string.IsNullOrWhiteSpace(name) ? "anonymous" : name;
                return $"Hello {name}, I can’t calculate your age without knowing your birthdate!";
            }

            if (years < 0 || months < 0 || months > 11 ||
                days < 0 || days > 31)
            {
                return "Invalid birthdate data.";
            }

            double age = years.Value + months.Value / 12.0 + days.Value / 365.0;

            name = string.IsNullOrWhiteSpace(name) ? "anonymous" : name;

            return $"Hello {name}, your age is approximately {age:F2} years.";
        }
    }
}
