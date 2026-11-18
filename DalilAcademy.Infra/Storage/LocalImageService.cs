using DalilAcademy.Core.Exceptions;
using DalilAcademy.Core.Storage;

namespace DalilAcademy.Infra.Storage;

public class LocalImageService : IImageService
{
    private readonly string _folder = Path.Combine(
        Directory.GetCurrentDirectory(),
        "wwwroot",
        "images",
        "courses");

    private static readonly string[] Allowed =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    ];

    public async Task<string> SaveCourseImageAsync(
    Stream content,
    string fileName,
    long length)
    {
        if (length <= 0 || length > 2 * 1024 * 1024)
        {
            throw new InvalidFileException(
                "Image must be less than 2 MB.");
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        if (!Allowed.Contains(extension))
        {
            throw new InvalidFileException(
                "Only jpg, jpeg, png and webp are allowed.");
        }

        if (!content.CanSeek)
        {
            throw new InvalidFileException(
                "Image stream must support seeking.");
        }

        var isValidImage = await IsValidImageAsync(content, extension);

        if (!isValidImage)
        {
            throw new InvalidFileException(
                "File content does not match the image extension.");
        }

        Directory.CreateDirectory(_folder);

        var newFileName = $"{Guid.NewGuid()}{extension}";

        var filePath = Path.Combine(_folder, newFileName);

        await using var file = File.Create(filePath);

        await content.CopyToAsync(file);

        return $"/images/courses/{newFileName}";
    }

    private static async Task<bool> IsValidImageAsync(
    Stream content,
    string extension)
    {
        var header = new byte[12];

        var bytesRead = await content.ReadAsync(header.AsMemory());

        content.Position = 0;

        return extension switch
        {
            ".jpg" or ".jpeg" =>
                bytesRead >= 3 &&
                header[0] == 0xFF &&
                header[1] == 0xD8 &&
                header[2] == 0xFF,

            ".png" =>
                bytesRead >= 8 &&
                header[0] == 0x89 &&
                header[1] == 0x50 &&
                header[2] == 0x4E &&
                header[3] == 0x47 &&
                header[4] == 0x0D &&
                header[5] == 0x0A &&
                header[6] == 0x1A &&
                header[7] == 0x0A,

            ".webp" =>
                bytesRead >= 12 &&
                header[0] == 'R' &&
                header[1] == 'I' &&
                header[2] == 'F' &&
                header[3] == 'F' &&
                header[8] == 'W' &&
                header[9] == 'E' &&
                header[10] == 'B' &&
                header[11] == 'P',

            _ => false
        };
    }
    public void DeleteCourseImage(string? imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl))
            return;

        var fileName = Path.GetFileName(imageUrl);

        var filePath = Path.Combine(_folder, fileName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}