using ABCDMall.Modules.Users.Application.Common;
using ABCDMall.Modules.Users.Application.DTOs.Auth;
using ABCDMall.Modules.Users.Application.DTOs.Common;
using ABCDMall.Modules.Users.Application.DTOs;

namespace ABCDMall.Modules.Users.Application.Services.Auth;

public interface IUserCommandService
{
    Task<ApplicationResult<LoginResponseDto>> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);

    Task<ApplicationResult<MessageResponseDto>> RequestForgotPasswordOtpAsync(RequestForgotPasswordOtpDto dto, CancellationToken cancellationToken = default);

    Task<ApplicationResult<MessageResponseDto>> ConfirmForgotPasswordOtpAsync(ConfirmForgotPasswordOtpDto dto, CancellationToken cancellationToken = default);

    Task<ApplicationResult<UpdateProfileResponseDto>> UpdateProfileAsync(string userId, UpdateProfileDto dto, CancellationToken cancellationToken = default);

    Task<ApplicationResult<MessageResponseDto>> ApproveProfileUpdateRequestAsync(string requestId, string adminUserId, ProfileUpdateRequestDecisionDto dto, CancellationToken cancellationToken = default);

    Task<ApplicationResult<MessageResponseDto>> RejectProfileUpdateRequestAsync(string requestId, string adminUserId, ProfileUpdateRequestDecisionDto dto, CancellationToken cancellationToken = default);

    Task<ApplicationResult<MessageResponseDto>> ResetPasswordAsync(string userId, ResetPasswordDto dto, CancellationToken cancellationToken = default);

    Task<ApplicationResult<MessageResponseDto>> RequestResetPasswordOtpAsync(string userId, RequestResetPasswordOtpDto dto, CancellationToken cancellationToken = default);

    Task<ApplicationResult<MessageResponseDto>> ConfirmResetPasswordOtpAsync(string userId, ConfirmResetPasswordOtpDto dto, CancellationToken cancellationToken = default);

    Task<ApplicationResult<UserAccountMutationResponseDto>> UpdateUserAccountAsync(string userId, UpdateUserAccountDto dto, CancellationToken cancellationToken = default);

    Task<ApplicationResult<UserAccountMutationResponseDto>> DeleteUserAccountAsync(string userId, CancellationToken cancellationToken = default);

    Task<ApplicationResult<UserAccountMutationResponseDto>> ActivateUserAccountAsync(string userId, CancellationToken cancellationToken = default);

    Task<ApplicationResult<UserAccountMutationResponseDto>> ResendInitialPasswordLinkAsync(string userId, CancellationToken cancellationToken = default);

    Task<ApplicationResult<MessageResponseDto>> CompleteInitialPasswordChangeAsync(CompleteInitialPasswordChangeDto dto, CancellationToken cancellationToken = default);

    Task<ApplicationResult<AccessTokenResponseDto>> RefreshTokenAsync(RefreshTokenDto dto, CancellationToken cancellationToken = default);

    Task<ApplicationResult<MessageResponseDto>> LogoutAsync(RefreshTokenDto dto, CancellationToken cancellationToken = default);

    Task<ApplicationResult<RegisterUserResponseDto>> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default);
}
