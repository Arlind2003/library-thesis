using Caktoje.Exceptions;

namespace Caktoje.Services;

public class StorageService
{
    private readonly IConfiguration _configuration;
    private readonly string _targetPath;

    public StorageService(IConfiguration configuration)
    {
        _configuration = configuration;
        _targetPath = _configuration["Files:ImagesPath"] ?? throw new CriticalConfigurationException("Image upload folder not configured");
    }

    /// <summary>
    /// Saves a stream to a specific physical location.
    /// </summary>
    private static async Task SaveFile(Stream stream, string fileName, string directory)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var fullPath = Path.Combine(directory, fileName);

        using var fileStream = new FileStream(fullPath, FileMode.Create);
        await stream.CopyToAsync(fileStream);
    }

    /// <summary>
    /// Processes an uploaded image, generates a unique name, and saves it.
    /// </summary>
    public async Task<FileSaveResult> SaveImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty or null.");

        var fileExtension = Path.GetExtension(file.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

        var imageDirectory = Path.Combine(Directory.GetCurrentDirectory(), _targetPath);

        using (var stream = file.OpenReadStream())
        {
            await SaveFile(stream, uniqueFileName, imageDirectory);
        }

        return new FileSaveResult(uniqueFileName, imageDirectory);
    }
}
public class FileSaveResult
{
    public string FileName { get; set; }
    public string Directory { get; set; }

    public FileSaveResult(string fileName, string directory)
    {
        FileName = fileName;
        Directory = directory;
    }
}