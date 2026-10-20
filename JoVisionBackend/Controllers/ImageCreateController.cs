
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace JoVisionBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImageCreateController : ControllerBase
    {
        private readonly string storagePath =
            Path.Combine(Directory.GetCurrentDirectory(), "UploadedImages");

        public class ImageMetadata
        {
            public string Owner { get; set; } = "";
            public DateTime CreationTime { get; set; }
            public DateTime LastModificationTime { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromForm] IFormFile? image,
            [FromForm] string? Owner)
        {
            if (image == null || image.Length == 0 ||
                string.IsNullOrWhiteSpace(Owner))
            {
                return BadRequest("Image and Owner are required.");
            }

            string extension = Path.GetExtension(image.FileName);

            if (!string.Equals(extension, ".jpg",
                    StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(extension, ".jpeg",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Only JPG or JPEG images are allowed.");
            }

            string safeFileName = Path.GetFileName(image.FileName);

            if (string.IsNullOrWhiteSpace(safeFileName))
            {
                return BadRequest("Invalid file name.");
            }

            try
            {
                Directory.CreateDirectory(storagePath);

                string imagePath = Path.Combine(storagePath, safeFileName);
                string metadataPath = Path.ChangeExtension(imagePath, ".json");

                if (System.IO.File.Exists(imagePath) ||
                    System.IO.File.Exists(metadataPath))
                {
                    return Conflict("A file with this name already exists.");
                }

                DateTime now = DateTime.UtcNow;

                var metadata = new ImageMetadata
                {
                    Owner = Owner.Trim(),
                    CreationTime = now,
                    LastModificationTime = now
                };

                await using (var stream = new FileStream(
                    imagePath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None))
                {
                    await image.CopyToAsync(stream);
                }

                try
                {
                    string json = JsonSerializer.Serialize(
                        metadata,
                        new JsonSerializerOptions
                        {
                            WriteIndented = true
                        });

                    await System.IO.File.WriteAllTextAsync(
                        metadataPath, json);
                }
                catch
                {
                    System.IO.File.Delete(imagePath);
                    throw;
                }

                return Created(
                    $"/ImageCreate/{Uri.EscapeDataString(safeFileName)}",
                    metadata);
            }
            catch (IOException)
            {
                return Conflict(
                    "The file already exists or cannot be written.");
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
