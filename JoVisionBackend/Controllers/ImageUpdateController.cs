
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace JoVisionBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImageUpdateController : ControllerBase
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
        public async Task<IActionResult> Update(
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

            string fileName = Path.GetFileName(image.FileName);

            if (string.IsNullOrWhiteSpace(fileName) ||
                !string.Equals(fileName, image.FileName, StringComparison.Ordinal))
            {
                return BadRequest("Invalid file name.");
            }

            string imagePath = Path.Combine(storagePath, fileName);
            string metadataPath = Path.ChangeExtension(imagePath, ".json");

            try
            {
                if (!System.IO.File.Exists(imagePath) ||
                    !System.IO.File.Exists(metadataPath))
                {
                    return BadRequest("The image does not exist.");
                }

                string json = await System.IO.File.ReadAllTextAsync(metadataPath);

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
                    Owner.Trim(),
                    StringComparison.OrdinalIgnoreCase))
                {
                    return StatusCode(
                        StatusCodes.Status403Forbidden,
                        "You are not the owner of this file.");
                }

                string temporaryPath = Path.Combine(
                    storagePath,
                    Guid.NewGuid().ToString("N") + ".tmp");

                try
                {
                    await using (var stream = new FileStream(
                        temporaryPath,
                        FileMode.CreateNew,
                        FileAccess.Write,
                        FileShare.None))
                    {
                        await image.CopyToAsync(stream);
                    }

                    System.IO.File.Move(temporaryPath, imagePath, true);
                }
                finally
                {
                    if (System.IO.File.Exists(temporaryPath))
                    {
                        System.IO.File.Delete(temporaryPath);
                    }
                }

                metadata.LastModificationTime = DateTime.UtcNow;

                string updatedJson = JsonSerializer.Serialize(
                    metadata,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

                await System.IO.File.WriteAllTextAsync(
                    metadataPath, updatedJson);

                return Ok("Image updated successfully.");
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
