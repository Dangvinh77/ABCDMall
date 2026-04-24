using ABCDMall.Modules.Users.Application.Services;
using Microsoft.AspNetCore.Http;

namespace ABCDMall.Modules.Users.Infrastructure.Services;

public sealed class LocalFileStorageService : IFileStorageService
{
    public Task<string> SaveProfileAvatarAsync(IFormFile file, CancellationToken cancellationToken = default)
        => SaveFileAsync(file, Path.Combine("wwwroot", "images", "profiles"), "/images/profiles", cancellationToken);

    public Task<string> SaveCccdImageAsync(IFormFile file, CancellationToken cancellationToken = default)
        => SaveFileAsync(file, Path.Combine("wwwroot", "images", "cccd"), "/images/cccd", cancellationToken);

    public Task<string> SaveContractImageAsync(IFormFile file, CancellationToken cancellationToken = default)
        => SaveFileAsync(file, Path.Combine("wwwroot", "images", "contracts"), "/images/contracts", cancellationToken);

    public Task<string> SaveShopLogoAsync(IFormFile file, CancellationToken cancellationToken = default)
        => SaveFileAsync(file, Path.Combine("wwwroot", "images", "shops", "logos"), "/images/shops/logos", cancellationToken);

    public Task<string> SaveShopCoverAsync(IFormFile file, CancellationToken cancellationToken = default)
        => SaveFileAsync(file, Path.Combine("wwwroot", "images", "shops", "covers"), "/images/shops/covers", cancellationToken);

    public Task<string> SaveShopProductImageAsync(IFormFile file, CancellationToken cancellationToken = default)
        => SaveFileAsync(file, Path.Combine("wwwroot", "images", "shops", "products"), "/images/shops/products", cancellationToken);

    private static async Task<string> SaveFileAsync(IFormFile file, string relativeFolder, string publicRoot, CancellationToken cancellationToken)
    {
        var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), relativeFolder);
        Directory.CreateDirectory(uploadsRoot);

        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsRoot, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream, cancellationToken);

        return $"{publicRoot}/{fileName}";
    }
}
