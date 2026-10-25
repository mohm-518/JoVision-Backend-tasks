
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace JoVisionBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImageRetrieveController : ControllerBase
    {
        private readonly string storagePath =
            Path.Combine(Directory.GetCurrentDirectory(), "UploadedImages");

        public class ImageMetadata
        {
            public string Owner { get; set; } = "";
            public DateTime CreationTime { get; set; }
            public DateTime LastModificationTime { get; set; }
        }

        [HttpGet]
        public IActionResult Retrieve(
            [FromQuery] string? FileName,
            [FromQuery] string? FileOwner)
        {
            if (string.IsNullOrWhiteSpace(FileName) ||
                string.IsNullOrWhiteSpace(FileOwner))
            {
                return BadRequest("FileName and FileOwner are required.");
            }

            string safeFileName = Path.GetFileName(FileName);

            string extension = Path.GetExtension(safeFileName);

            if (!string.Equals(safeFileName, FileName, StringComparison.Ordinal) ||
                (!string.Equals(extension, ".jpg", StringComparison.OrdinalIgnoreCase) &&
                 !string.Equals(extension, ".jpeg", StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest("Invalid JPG file name.");
            }

            string imagePath = Path.Combine(storagePath, safeFileName);
            string metadataPath = Path.ChangeExtension(imagePath, ".json");

            try
            {
                if (!System.IO.File.Exists(imagePath) ||
                    !System.IO.File.Exists(metadataPath))
                {
                    return NotFound("Image or metadata was not found.");
                }

                string json = System.IO.File.ReadAllText(metadataPath);

                var metadata = JsonSerializer.Deserialize<ImageMetadata>(json);

                if (metadata == null ||
                    string.IsNullOrWhiteSpace(metadata.Owner))
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        "Metadata is invalid.");
                }

                if (!string.Equals(
                    metadata.Owner.Trim(),
                    FileOwner.Trim(),
                    StringComparison.OrdinalIgnoreCase))
                {
                    return StatusCode(
                        StatusCodes.Status403Forbidden,
                        "You are not the owner of this file.");
                }

                return PhysicalFile(
                    imagePath,
                    "image/jpeg",
                    safeFileName);
            }
            catch
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "An internal server error occurred.");
            }
        }
    }
}
