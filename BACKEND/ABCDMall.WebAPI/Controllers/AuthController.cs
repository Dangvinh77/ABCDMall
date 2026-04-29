using ABCDMall.Modules.Users.Application.Common;
using ABCDMall.Modules.Users.Application.DTOs.Auth;
using ABCDMall.Modules.Users.Application.DTOs.Common;
using ABCDMall.Modules.Users.Application.Services.Auth;
using ABCDMall.Modules.Users.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ABCDMall.Modules.UtilityMap.Application.Services.Maps;
using ABCDMall.Modules.Shops.Application.Services.Manager;
using ABCDMall.Modules.Shops.Application.DTOs;

namespace ABCDMall.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IUserCommandService _userCommandService;
    private readonly IUserQueryService _userQueryService;
    private readonly IMapCommandService _mapCommandService;
    private readonly IShopManagerService _shopManagerService;

    public AuthController(
        IUserCommandService userCommandService,
        IUserQueryService userQueryService,
        IMapCommandService mapCommandService,
        IShopManagerService shopManagerService)
    {
        _userCommandService = userCommandService;
        _userQueryService = userQueryService;
        _mapCommandService = mapCommandService;
        _shopManagerService = shopManagerService;
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

    // --- CẬP NHẬT PHƯƠNG THỨC REGISTER ---
    [Authorize(Roles = "Admin")]
    [RequestSizeLimit(10_000_000)]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromForm] RegisterDto dto)
    {
        // Thực hiện đăng ký tài khoản User/Manager trước
        var result = await _userCommandService.RegisterAsync(dto);

        if (result.Status != ApplicationResultStatus.Ok)
        {
            return result.Status switch
            {
                ApplicationResultStatus.BadRequest => BadRequest(result.Error),
                ApplicationResultStatus.NotFound => NotFound(result.Error),
                ApplicationResultStatus.Unauthorized => Unauthorized(result.Error),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        var response = result.Value!;

        // 1. GÁN VỊ TRÍ TRÊN BẢN ĐỒ (Đổi màu bạc -> Vàng)
        bool slotReserved = false;
        if (dto.MapLocationId.HasValue && !string.IsNullOrWhiteSpace(response.ShopId))
        {
            slotReserved = await _mapCommandService.ReserveSlotAsync(dto.MapLocationId.Value, response.ShopId);
        }

        // ==========================================
        // 2. TẠO TỰ ĐỘNG SHOP VẬT LÝ CHO QUẢN LÝ
        // ==========================================
        try
        {
            // Tạo một URL (slug) hợp lệ từ tên shop (Ví dụ: "Kichi Kichi" -> "kichi-kichi")
            var tempSlug = System.Text.RegularExpressions.Regex.Replace(dto.ShopName.Trim().ToLowerInvariant().Replace(" ", "-"), @"[^a-z0-9-]", string.Empty);

            // Khởi tạo các thông tin cơ bản để Shop "nổi" lên trên Mall
            var shopRequest = new UpsertManagedShopRequestDto
            {
                Name = dto.ShopName,
                Slug = tempSlug,
                Category = "Coming Soon", // Gán tạm để nổi bật trên Mall
                Floor = string.IsNullOrWhiteSpace(dto.Floor) ? "Updating" : dto.Floor,
                LocationSlot = string.IsNullOrWhiteSpace(dto.LocationSlot) ? "Updating" : dto.LocationSlot,
                Summary = "This shop is being prepared and will open soon.",
                Description = "Detailed public shop information will be updated soon.",
                OpenHours = "09:00 - 22:00"
            };

            // Gọi service của module Shops để sinh ra data vật lý
            // response.ShopId chính là ID kết nối giữa tài khoản Manager và Shop này
            await _shopManagerService.CreateMyShopAsync(response.ShopId!, shopRequest);
        }
        catch (Exception ex)
        {
            // Log ra console nếu lỗi, nhưng không làm sập quá trình trả về kết quả đăng ký
            Console.WriteLine($"[WARNING] Could not auto-create physical shop: {ex.Message}");
        }
        // ==========================================

        return Ok(new
        {
            message = response.Message,
            emailSent = response.EmailSent,
            email = response.Email,
            role = response.Role,
            shopId = response.ShopId,
            cccd = response.CCCD,
            shopName = response.ShopName,
            createdAt = response.CreatedAt,
            slotReserved,
            mapLocationId = slotReserved ? dto.MapLocationId : null
        });
    }

    [HttpPost("forgotpassword/request-otp")]
    public async Task<IActionResult> RequestForgotPasswordOtp(RequestForgotPasswordOtpDto dto)
        => FromResult(await _userCommandService.RequestForgotPasswordOtpAsync(dto));

    [HttpPost("forgotpassword/confirm-otp")]
    public async Task<IActionResult> ConfirmForgotPasswordOtp(ConfirmForgotPasswordOtpDto dto)
        => FromResult(await _userCommandService.ConfirmForgotPasswordOtpAsync(dto));

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

    [Authorize]
    [HttpPost("resetpassword")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Invalid token");
        }

        return FromResult(await _userCommandService.ResetPasswordAsync(userId, dto));
    }

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

    [Authorize(Roles = "Admin")]
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
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> UpdateUserAccount(string id, [FromForm] UpdateUserAccountDto dto)
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

    [HttpPost("initial-password/change")]
    public async Task<IActionResult> CompleteInitialPasswordChange(CompleteInitialPasswordChangeDto dto)
        => FromResult(await _userCommandService.CompleteInitialPasswordChangeAsync(dto));

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

