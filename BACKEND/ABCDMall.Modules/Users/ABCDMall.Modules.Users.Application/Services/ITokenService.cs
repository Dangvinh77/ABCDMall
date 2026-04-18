using ABCDMall.Modules.Users.Domain.Entities;

namespace ABCDMall.Modules.Users.Application.Services;

public interface ITokenService
{
    string GenerateAccessToken(User user);

    string GenerateRefreshToken();
}
