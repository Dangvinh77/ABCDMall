using Microsoft.AspNetCore.Http;

namespace ABCDMall.Modules.Users.Application.Services;

public interface IFileStorageService
{
    Task<string> SaveProfileAvatarAsync(IFormFile file, CancellationToken cancellationToken = default);

    Task<string> SaveContractImageAsync(IFormFile file, CancellationToken cancellationToken = default);

    Task<string> SaveShopLogoAsync(IFormFile file, CancellationToken cancellationToken = default);

    Task<string> SaveShopCoverAsync(IFormFile file, CancellationToken cancellationToken = default);

    Task<string> SaveShopProductImageAsync(IFormFile file, CancellationToken cancellationToken = default);

    Task<string> SaveBiddingImageAsync(IFormFile file, CancellationToken cancellationToken = default);
}
