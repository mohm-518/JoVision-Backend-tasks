
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace JoVisionBackend.Controllers
{
    public enum FilterType
    {
        ByModificationDate,
        ByCreationDateDescending,
        ByCreationDateAscending,
        ByOwner
    }

    [ApiController]
    [Route("[controller]")]
    public class ImageFilterController : ControllerBase
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

        [HttpPost]
        public IActionResult Filter(
            [FromForm] DateTime? CreationDate,
            [FromForm] DateTime? ModificationDate,
            [FromForm] string? Owner,
            [FromForm] FilterType? FilterType)
        {
            if (!Directory.Exists(storagePath))
            {
                return Ok(new List<JsonData>());
            }

            if (!FilterType.HasValue ||
                !Enum.IsDefined(typeof(FilterType), FilterType.Value))
            {
                return BadRequest("Invalid FilterType.");
            }

            if (FilterType.Value == Controllers.FilterType.ByCreationDateDescending ||
                FilterType.Value == Controllers.FilterType.ByCreationDateAscending)
            {
                if (!CreationDate.HasValue)
                {
                    return BadRequest("CreationDate is required.");
                }
            }

            if (FilterType.Value == Controllers.FilterType.ByModificationDate)
            {
                if (!ModificationDate.HasValue)
                {
                    return BadRequest("ModificationDate is required.");
                }
            }

            if (FilterType.Value == Controllers.FilterType.ByOwner &&
                string.IsNullOrWhiteSpace(Owner))
            {
                return BadRequest("Owner is required.");
            }

            try
            {
                var files = new List<JsonData>();

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

                    string fileName =
                        Path.GetFileNameWithoutExtension(metadataPath);

                    string jpgPath = Path.Combine(storagePath, fileName + ".jpg");
                    string jpegPath = Path.Combine(storagePath, fileName + ".jpeg");

                    if (!System.IO.File.Exists(jpgPath) &&
                        !System.IO.File.Exists(jpegPath))
                    {
                        continue;
                    }

                    files.Add(new JsonData
                    {
                        FileName = System.IO.File.Exists(jpgPath)
                            ? fileName + ".jpg"
                            : fileName + ".jpeg",
                        Owner = metadata.Owner
                    });
                }

                IEnumerable<JsonData> result = files;

                switch (FilterType.Value)
                {
                    case Controllers.FilterType.ByModificationDate:
                        result = files.Where(file =>
                        {
                            string path = Path.Combine(
                                storagePath,
                                Path.ChangeExtension(file.FileName, ".json"));

                            var metadata =
                                JsonSerializer.Deserialize<ImageMetadata>(
                                    System.IO.File.ReadAllText(path));

                            return metadata != null &&
                                metadata.LastModificationTime < ModificationDate!.Value;
                        });
                        break;

                    case Controllers.FilterType.ByCreationDateDescending:
                        result = files.Where(file =>
                        {
                            string path = Path.Combine(
                                storagePath,
                                Path.ChangeExtension(file.FileName, ".json"));

                            var metadata =
                                JsonSerializer.Deserialize<ImageMetadata>(
                                    System.IO.File.ReadAllText(path));

                            return metadata != null &&
                                metadata.CreationTime > CreationDate!.Value;
                        })
                        .OrderByDescending(file =>
                        {
                            string path = Path.Combine(
                                storagePath,
                                Path.ChangeExtension(file.FileName, ".json"));

                            var metadata =
                                JsonSerializer.Deserialize<ImageMetadata>(
                                    System.IO.File.ReadAllText(path));

                            return metadata!.CreationTime;
                        });
                        break;

                    case Controllers.FilterType.ByCreationDateAscending:
                        result = files.Where(file =>
                        {
                            string path = Path.Combine(
                                storagePath,
                                Path.ChangeExtension(file.FileName, ".json"));

                            var metadata =
                                JsonSerializer.Deserialize<ImageMetadata>(
                                    System.IO.File.ReadAllText(path));

                            return metadata != null &&
                                metadata.CreationTime > CreationDate!.Value;
                        })
                        .OrderBy(file =>
                        {
                            string path = Path.Combine(
                                storagePath,
                                Path.ChangeExtension(file.FileName, ".json"));

                            var metadata =
                                JsonSerializer.Deserialize<ImageMetadata>(
                                    System.IO.File.ReadAllText(path));

                            return metadata!.CreationTime;
                        });
                        break;

                    case Controllers.FilterType.ByOwner:
                        result = files.Where(file =>
                            string.Equals(
                                file.Owner,
                                Owner!.Trim(),
                                StringComparison.OrdinalIgnoreCase));
                        break;
                }

                return Ok(result.ToList());
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
