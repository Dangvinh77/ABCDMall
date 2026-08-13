using ABCDMall.Modules.Users.Application.Common;
using ABCDMall.Modules.Users.Application.DTOs.Auth;
using ABCDMall.Modules.Users.Application.DTOs.Common;
using ABCDMall.Modules.Users.Application.Services.Auth;
using ABCDMall.Modules.Users.Application.DTOs;
using ABCDMall.Modules.UtilityMap.Application.Services.Maps;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ABCDMall.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IUserCommandService _userCommandService;
    private readonly IUserQueryService _userQueryService;
    private readonly IDevOtpDebugService _devOtpDebugService;
    private readonly IMapCommandService _mapCommandService;
    private readonly IWebHostEnvironment _environment;

    public AuthController(
        IUserCommandService userCommandService,
        IUserQueryService userQueryService,
        IDevOtpDebugService devOtpDebugService,
        IMapCommandService mapCommandService,
        IWebHostEnvironment environment)
    {
        _userCommandService = userCommandService;
        _userQueryService = userQueryService;
        _devOtpDebugService = devOtpDebugService;
        _mapCommandService = mapCommandService;
        _environment = environment;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var result = await _userCommandService.LoginAsync(dto);
        return result.Status switch
        {
            ApplicationResultStatus.Ok => Ok(new
            {
                accessToken = result.Value!.AccessToken,
                refreshToken = result.Value.RefreshToken,
                requiresPasswordChange = result.Value.RequiresPasswordChange,
                passwordSetupToken = result.Value.PasswordSetupToken,
                message = result.Value.Message
            }),
            ApplicationResultStatus.BadRequest when result.Value?.RequiresOtp == true => BadRequest(new
            {
                requiresOtp = true,
                message = result.Value.Message
            }),
            ApplicationResultStatus.Unauthorized when result.Value?.RequiresOtp == true => Unauthorized(new
            {
                requiresOtp = true,
                message = result.Value.Message
            }),
            ApplicationResultStatus.BadRequest => BadRequest(result.Error),
            ApplicationResultStatus.Unauthorized => Unauthorized(result.Error),
            ApplicationResultStatus.NotFound => NotFound(result.Error),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpPost("forgotpassword/request-otp")]
    public async Task<IActionResult> RequestForgotPasswordOtp(RequestForgotPasswordOtpDto dto)
        => FromResult(await _userCommandService.RequestForgotPasswordOtpAsync(dto));

    [HttpPost("forgotpassword/confirm-otp")]
    public async Task<IActionResult> ConfirmForgotPasswordOtp(ConfirmForgotPasswordOtpDto dto)
        => FromResult(await _userCommandService.ConfirmForgotPasswordOtpAsync(dto));

    [Authorize(Roles = "Admin")]
    [HttpPost("debug/otp")]
    public async Task<IActionResult> GetDebugOtp(DebugOtpLookupRequestDto dto)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        var result = await _devOtpDebugService.GetOtpAsync(dto);
        return result.Status switch
        {
            ApplicationResultStatus.Ok => Ok(result.Value),
            ApplicationResultStatus.BadRequest => BadRequest(result.Error),
            ApplicationResultStatus.NotFound => NotFound(result.Error),
            ApplicationResultStatus.Unauthorized => Unauthorized(result.Error),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [Authorize]
    [HttpGet("getprofile")]
    public async Task<ActionResult<UserProfileResponseDto>> GetProfile()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Invalid token");
        }

        var profile = await _userQueryService.GetProfileAsync(userId);
        return profile is null ? Unauthorized("Invalid token") : Ok(profile);
    }

    [Authorize]
    [HttpPut("updateprofile")]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileDto dto)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Invalid token");
        }

        var result = await _userCommandService.UpdateProfileAsync(userId, dto);
        return result.Status switch
        {
            ApplicationResultStatus.Ok => Ok(new
            {
                message = result.Value!.Message,
                profile = result.Value.Profile
            }),
            ApplicationResultStatus.Unauthorized => Unauthorized(result.Error),
            ApplicationResultStatus.BadRequest => BadRequest(result.Error),
            ApplicationResultStatus.NotFound => NotFound(result.Error),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [Authorize]
    [HttpGet("profile-update-history")]
    public async Task<ActionResult<IReadOnlyList<ProfileUpdateHistoryResponseDto>>> GetProfileUpdateHistory()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Invalid token");
        }

        var history = await _userQueryService.GetProfileUpdateHistoryAsync(userId);
        return Ok(history);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("profile-update-requests")]
    public async Task<ActionResult<IReadOnlyList<ProfileUpdateRequestResponseDto>>> GetProfileUpdateRequests([FromQuery] string? status = "Pending")
        => Ok(await _userQueryService.GetProfileUpdateRequestsAsync(status));

    [Authorize]
    [HttpGet("profile-update-requests/me")]
    public async Task<ActionResult<IReadOnlyList<ProfileUpdateRequestResponseDto>>> GetMyProfileUpdateRequests([FromQuery] string? status = "Pending")
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Invalid token");
        }

        return Ok(await _userQueryService.GetMyProfileUpdateRequestsAsync(userId, status));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("profile-update-requests/{id}/approve")]
    public async Task<IActionResult> ApproveProfileUpdateRequest(string id, ProfileUpdateRequestDecisionDto dto)
    {
        var adminUserId = GetCurrentUserId();
        if (string.IsNullOrEmpty(adminUserId))
        {
            return Unauthorized("Invalid token");
        }

        return FromResult(await _userCommandService.ApproveProfileUpdateRequestAsync(id, adminUserId, dto));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("profile-update-requests/{id}/reject")]
    public async Task<IActionResult> RejectProfileUpdateRequest(string id, ProfileUpdateRequestDecisionDto dto)
    {
        var adminUserId = GetCurrentUserId();
        if (string.IsNullOrEmpty(adminUserId))
        {
            return Unauthorized("Invalid token");
        }

        return FromResult(await _userCommandService.RejectProfileUpdateRequestAsync(id, adminUserId, dto));
    }

    [HttpPost("initial-password/change")]
    public async Task<IActionResult> CompleteInitialPasswordChange(CompleteInitialPasswordChangeDto dto)
        => FromResult(await _userCommandService.CompleteInitialPasswordChangeAsync(dto));

    [Authorize]
    [HttpPost("resetpassword/request-otp")]
    public async Task<IActionResult> RequestResetPasswordOtp(RequestResetPasswordOtpDto dto)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Invalid token");
        }

        return FromResult(await _userCommandService.RequestResetPasswordOtpAsync(userId, dto));
    }

    [Authorize]
    [HttpPost("resetpassword/confirm-otp")]
    public async Task<IActionResult> ConfirmResetPasswordOtp(ConfirmResetPasswordOtpDto dto)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Invalid token");
        }

        return FromResult(await _userCommandService.ConfirmResetPasswordOtpAsync(userId, dto));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("users")]
    public async Task<ActionResult<IReadOnlyList<UserSummaryResponseDto>>> GetUsers()
        => Ok(await _userQueryService.GetUsersAsync());

    [Authorize(Roles = "MoviesAdmin,Admin")]
    [HttpGet("movies-admins")]
    public async Task<ActionResult<IReadOnlyList<UserSummaryResponseDto>>> GetMoviesAdmins()
        => Ok(await _userQueryService.GetUsersByRoleAsync("MoviesAdmin"));

    [Authorize(Roles = "MoviesAdmin,Admin")]
    [HttpPost("movies-admins")]
    public async Task<IActionResult> RegisterMoviesAdmin(RegisterDto dto)
    {
        dto.Role = "MoviesAdmin";
        var result = await _userCommandService.RegisterAsync(dto);
        return result.Status switch
        {
            ApplicationResultStatus.Ok => Ok(new
            {
                message = result.Value!.Message,
                emailSent = result.Value.EmailSent,
                email = result.Value.Email,
                role = result.Value.Role,
                createdAt = result.Value.CreatedAt
            }),
            ApplicationResultStatus.BadRequest => BadRequest(result.Error),
            ApplicationResultStatus.NotFound => NotFound(result.Error),
            ApplicationResultStatus.Unauthorized => Unauthorized(result.Error),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [Authorize(Roles = "MoviesAdmin,Admin")]
    [HttpPut("movies-admins/{id}")]
    public async Task<IActionResult> UpdateMoviesAdmin(string id, UpdateUserAccountDto dto)
    {
        dto.Role = "MoviesAdmin";
        var result = await _userCommandService.UpdateUserAccountAsync(id, dto);
        return result.Status switch
        {
            ApplicationResultStatus.Ok => Ok(new
            {
                message = result.Value!.Message,
                emailSent = result.Value.EmailSent
            }),
            ApplicationResultStatus.BadRequest => BadRequest(result.Error),
            ApplicationResultStatus.NotFound => NotFound(result.Error),
            ApplicationResultStatus.Unauthorized => Unauthorized(result.Error),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [Authorize(Roles = "MoviesAdmin,Admin")]
    [HttpDelete("movies-admins/{id}")]
    public async Task<IActionResult> DeleteMoviesAdmin(string id)
    {
        var result = await _userCommandService.DeleteUserAccountAsync(id);
        return result.Status switch
        {
            ApplicationResultStatus.Ok => Ok(new
            {
                message = result.Value!.Message,
                emailSent = result.Value.EmailSent
            }),
            ApplicationResultStatus.BadRequest => BadRequest(result.Error),
            ApplicationResultStatus.NotFound => NotFound(result.Error),
            ApplicationResultStatus.Unauthorized => Unauthorized(result.Error),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("users/{id}")]
    public async Task<IActionResult> UpdateUserAccount(string id, UpdateUserAccountDto dto)
    {
        var result = await _userCommandService.UpdateUserAccountAsync(id, dto);
        return result.Status switch
        {
            ApplicationResultStatus.Ok => Ok(new
            {
                message = result.Value!.Message,
                emailSent = result.Value.EmailSent
            }),
            ApplicationResultStatus.BadRequest => BadRequest(result.Error),
            ApplicationResultStatus.NotFound => NotFound(result.Error),
            ApplicationResultStatus.Unauthorized => Unauthorized(result.Error),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUserAccount(string id)
    {
        var result = await _userCommandService.DeleteUserAccountAsync(id);
        return result.Status switch
        {
            ApplicationResultStatus.Ok => Ok(new
            {
                message = result.Value!.Message,
                emailSent = result.Value.EmailSent
            }),
            ApplicationResultStatus.BadRequest => BadRequest(result.Error),
            ApplicationResultStatus.NotFound => NotFound(result.Error),
            ApplicationResultStatus.Unauthorized => Unauthorized(result.Error),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("users/{id}/activate")]
    public async Task<IActionResult> ActivateUserAccount(string id)
    {
        var result = await _userCommandService.ActivateUserAccountAsync(id);
        return result.Status switch
        {
            ApplicationResultStatus.Ok => Ok(new
            {
                message = result.Value!.Message,
                emailSent = result.Value.EmailSent
            }),
            ApplicationResultStatus.BadRequest => BadRequest(result.Error),
            ApplicationResultStatus.NotFound => NotFound(result.Error),
            ApplicationResultStatus.Unauthorized => Unauthorized(result.Error),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("users/{id}/resend-initial-password")]
    public async Task<IActionResult> ResendInitialPasswordLink(string id)
    {
        var result = await _userCommandService.ResendInitialPasswordLinkAsync(id);
        return result.Status switch
        {
            ApplicationResultStatus.Ok => Ok(new
            {
                message = result.Value!.Message,
                emailSent = result.Value.EmailSent
            }),
            ApplicationResultStatus.BadRequest => BadRequest(result.Error),
            ApplicationResultStatus.NotFound => NotFound(result.Error),
            ApplicationResultStatus.Unauthorized => Unauthorized(result.Error),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken(RefreshTokenDto dto)
    {
        var result = await _userCommandService.RefreshTokenAsync(dto);
        return result.Status switch
        {
            ApplicationResultStatus.Ok => Ok(new { accessToken = result.Value!.AccessToken }),
            ApplicationResultStatus.Unauthorized => Unauthorized(result.Error),
            ApplicationResultStatus.BadRequest => BadRequest(result.Error),
            ApplicationResultStatus.NotFound => NotFound(result.Error),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshTokenDto dto)
        => FromResult(await _userCommandService.LogoutAsync(dto), unwrapMessageOnSuccess: true);

    [Authorize(Roles = "Admin")]
    [HttpPost("register")]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> Register([FromForm] RegisterDto dto)
    {
        var result = await _userCommandService.RegisterAsync(dto);
        if (result.Status == ApplicationResultStatus.Ok
            && dto.MapLocationId.HasValue
            && !string.IsNullOrWhiteSpace(result.Value?.ShopId))
        {
            await _mapCommandService.ReserveSlotAsync(dto.MapLocationId.Value, result.Value.ShopId);
        }

        return result.Status switch
        {
            ApplicationResultStatus.Ok => Ok(new
            {
                message = result.Value!.Message,
                emailSent = result.Value.EmailSent,
                email = result.Value.Email,
                role = result.Value.Role,
                shopId = result.Value.ShopId,
                cccd = result.Value.CCCD,
                shopName = result.Value.ShopName,
                createdAt = result.Value.CreatedAt
            }),
            ApplicationResultStatus.BadRequest => BadRequest(result.Error),
            ApplicationResultStatus.NotFound => NotFound(result.Error),
            ApplicationResultStatus.Unauthorized => Unauthorized(result.Error),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    private IActionResult FromResult(ApplicationResult<MessageResponseDto> result, bool unwrapMessageOnSuccess = false)
    {
        return result.Status switch
        {
            ApplicationResultStatus.Ok when unwrapMessageOnSuccess => Ok(result.Value!.Message),
            ApplicationResultStatus.Ok => Ok(new { message = result.Value!.Message }),
            ApplicationResultStatus.BadRequest => BadRequest(result.Error),
            ApplicationResultStatus.NotFound => NotFound(result.Error),
            ApplicationResultStatus.Unauthorized => Unauthorized(result.Error),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
    }
}
