using ABCDMall.Modules.Users.Application.Services;
using Microsoft.AspNetCore.Http;

namespace ABCDMall.Modules.Users.Infrastructure.Services;

public sealed class LocalFileStorageService : IFileStorageService
{
    private static readonly HashSet<string> SupportedBiddingImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp",
        ".gif",
        ".bmp",
        ".avif"
    };

    public Task<string> SaveProfileAvatarAsync(IFormFile file, CancellationToken cancellationToken = default)
        => SaveFileAsync(file, Path.Combine("wwwroot", "images", "profiles"), "/images/profiles", cancellationToken);

    public Task<string> SaveContractImageAsync(IFormFile file, CancellationToken cancellationToken = default)
        => SaveFileAsync(file, Path.Combine("wwwroot", "images", "contracts"), "/images/contracts", cancellationToken);

    public Task<string> SaveShopLogoAsync(IFormFile file, CancellationToken cancellationToken = default)
        => SaveFileAsync(file, Path.Combine("wwwroot", "images", "shops", "logos"), "/images/shops/logos", cancellationToken);

    public Task<string> SaveShopCoverAsync(IFormFile file, CancellationToken cancellationToken = default)
        => SaveFileAsync(file, Path.Combine("wwwroot", "images", "shops", "covers"), "/images/shops/covers", cancellationToken);

    public Task<string> SaveShopProductImageAsync(IFormFile file, CancellationToken cancellationToken = default)
        => SaveFileAsync(file, Path.Combine("wwwroot", "images", "shops", "products"), "/images/shops/products", cancellationToken);

    public Task<string> SaveBiddingImageAsync(IFormFile file, CancellationToken cancellationToken = default)
        => SaveFileAsync(
            file,
            Path.Combine("wwwroot", "images", "bidding"),
            "/images/bidding",
            cancellationToken,
            SupportedBiddingImageExtensions);

    private static async Task<string> SaveFileAsync(
        IFormFile file,
        string relativeFolder,
        string publicRoot,
        CancellationToken cancellationToken,
        IReadOnlySet<string>? allowedExtensions = null)
    {
        if (file.Length <= 0)
        {
            throw new InvalidOperationException("Uploaded file is empty.");
        }

        var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), relativeFolder);
        Directory.CreateDirectory(uploadsRoot);

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            throw new InvalidOperationException("Uploaded file must include an extension.");
        }

        if (allowedExtensions is not null && !allowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException(
                $"Unsupported image format. Allowed formats: {string.Join(", ", allowedExtensions.OrderBy(x => x))}.");
        }

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsRoot, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream, cancellationToken);

        return $"{publicRoot}/{fileName}";
    }
}
