using ABCDMall.Modules.Users.Application.Common;
using ABCDMall.Modules.Users.Application.DTOs.Auth;

namespace ABCDMall.Modules.Users.Application.Services.Auth;

public interface IDevOtpDebugService
{
    Task<ApplicationResult<DebugOtpLookupResponseDto>> GetOtpAsync(
        DebugOtpLookupRequestDto dto,
        CancellationToken cancellationToken = default);
}
