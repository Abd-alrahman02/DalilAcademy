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

        Directory.CreateDirectory(_folder);

        var newFileName = $"{Guid.NewGuid()}{extension}";

        var filePath = Path.Combine(_folder, newFileName);

        await using var file = File.Create(filePath);

        await content.CopyToAsync(file);

        return $"/images/courses/{newFileName}";
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