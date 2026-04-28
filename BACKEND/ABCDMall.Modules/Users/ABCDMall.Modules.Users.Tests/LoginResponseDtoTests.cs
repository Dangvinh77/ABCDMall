using ABCDMall.Modules.Users.Application.DTOs.Auth;
using Xunit;

namespace ABCDMall.Modules.Users.Tests;

public class LoginResponseDtoTests
{
    [Fact]
    public void LoginResponseDto_exposes_password_change_metadata()
    {
        var dto = new LoginResponseDto
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token",
            RequiresPasswordChange = true,
            PasswordSetupToken = "setup-token",
            Message = "You must change your password before continuing"
        };

        Assert.True(dto.RequiresPasswordChange);
        Assert.Equal("setup-token", dto.PasswordSetupToken);
        Assert.Equal("You must change your password before continuing", dto.Message);
    }
}
