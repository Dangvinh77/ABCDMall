using ABCDMall.Modules.Users.Application.Services;
using ABCDMall.Modules.Users.Domain.Entities;
using ABCDMall.Modules.Users.Services;

namespace ABCDMall.Modules.Users.Infrastructure.Services;

public sealed class TokenService : ITokenService
{
    private readonly JwtService _jwtService;

    public TokenService(JwtService jwtService)
    {
        _jwtService = jwtService;
    }

    public string GenerateAccessToken(User user)
        => _jwtService.GenerateAccessToken(user);

    public string GenerateRefreshToken()
        => _jwtService.GenerateRefreshToken();
}
