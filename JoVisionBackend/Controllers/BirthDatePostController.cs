
using Microsoft.AspNetCore.Mvc;

namespace JoVisionBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BirthDatePostController : ControllerBase
    {
        [HttpPost]
        public string Post(
            [FromForm] string? name,
            [FromForm] int? years,
            [FromForm] int? months,
            [FromForm] int? days)
        {
            name = string.IsNullOrWhiteSpace(name)
                ? "anonymous"
                : name;

            if (years == null || months == null || days == null)
            {
                return $"Hello {name}, I can’t calculate your age without knowing your birthdate!";
            }

            if (years < 0 || months < 0 || months > 11 ||
                days < 0 || days > 31)
            {
                return "Invalid birthdate data.";
            }

            double age = years.Value
                       + months.Value / 12.0
                       + days.Value / 365.0;

            return $"Hello {name}, your age is approximately {age:F2} years.";
        }
    }
}
