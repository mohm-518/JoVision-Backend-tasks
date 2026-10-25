
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace JoVisionBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImageTransferOwnershipController : ControllerBase
    {
        private readonly string storagePath =
            Path.Combine(Directory.GetCurrentDirectory(), "UploadedImages");

        public class ImageMetadata
        {
            public string Owner { get; set; } = "";
            public DateTime CreationTime { get; set; }
            public DateTime LastModificationTime { get; set; }
        }

        public class JsonData
        {
            public string FileName { get; set; } = "";
            public string Owner { get; set; } = "";
        }

        [HttpGet]
        public IActionResult TransferOwnership(
            [FromQuery] string? OldOwner,
            [FromQuery] string? NewOwner)
        {
            if (string.IsNullOrWhiteSpace(OldOwner) ||
                string.IsNullOrWhiteSpace(NewOwner))
            {
                return BadRequest("OldOwner and NewOwner are required.");
            }

            if (!Directory.Exists(storagePath))
            {
                return Ok(new List<JsonData>());
            }

            try
            {
                var metadataFiles = Directory.GetFiles(storagePath, "*.json");

                foreach (string metadataPath in metadataFiles)
                {
                    string json = System.IO.File.ReadAllText(metadataPath);

                    var metadata =
                        JsonSerializer.Deserialize<ImageMetadata>(json);

                    if (metadata == null ||
                        string.IsNullOrWhiteSpace(metadata.Owner))
                    {
                        continue;
                    }

                    if (string.Equals(
                        metadata.Owner.Trim(),
                        OldOwner.Trim(),
                        StringComparison.OrdinalIgnoreCase))
                    {
                        metadata.Owner = NewOwner.Trim();
                        metadata.LastModificationTime = DateTime.UtcNow;

                        string updatedJson = JsonSerializer.Serialize(
                            metadata,
                            new JsonSerializerOptions
                            {
                                WriteIndented = true
                            });

                        System.IO.File.WriteAllText(metadataPath, updatedJson);
                    }
                }

                var result = new List<JsonData>();

                foreach (string metadataPath in Directory.GetFiles(storagePath, "*.json"))
                {
                    string json = System.IO.File.ReadAllText(metadataPath);

                    var metadata =
                        JsonSerializer.Deserialize<ImageMetadata>(json);

                    if (metadata == null ||
                        string.IsNullOrWhiteSpace(metadata.Owner))
                    {
                        continue;
                    }

                    if (!string.Equals(
                        metadata.Owner.Trim(),
                        NewOwner.Trim(),
                        StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string fileName = Path.GetFileNameWithoutExtension(metadataPath);

                    string jpgPath = Path.Combine(storagePath, fileName + ".jpg");
                    string jpegPath = Path.Combine(storagePath, fileName + ".jpeg");

                    if (System.IO.File.Exists(jpgPath))
                    {
                        result.Add(new JsonData
                        {
                            FileName = fileName + ".jpg",
                            Owner = metadata.Owner
                        });
                    }
                    else if (System.IO.File.Exists(jpegPath))
                    {
                        result.Add(new JsonData
                        {
                            FileName = fileName + ".jpeg",
                            Owner = metadata.Owner
                        });
                    }
                }

                return Ok(result);
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
