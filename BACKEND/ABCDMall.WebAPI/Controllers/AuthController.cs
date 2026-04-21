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
                refreshToken = result.Value.RefreshToken
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
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
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
            var tempSlug = dto.ShopName.Trim().ToLower().Replace(" ", "-").Replace("đ", "d");

            // Khởi tạo các thông tin cơ bản để Shop "nổi" lên trên Mall
            var shopRequest = new UpsertManagedShopRequestDto
            {
                Name = dto.ShopName,
                Slug = tempSlug,
                Category = "Coming Soon", // Gán tạm để nổi bật trên Mall
                Floor = "Đang cập nhật",
                LocationSlot = "Đang cập nhật",
                Summary = "Cửa hàng đang trong quá trình setup.",
                Description = "Thông tin chi tiết sẽ được cập nhật sớm...",
                OpenHours = "09:00 - 22:00"
            };

            // Gọi service của module Shops để sinh ra data vật lý
            // response.ShopId chính là ID kết nối giữa tài khoản Manager và Shop này
            await _shopManagerService.CreateMyShopAsync(response.ShopId!, shopRequest);
        }
        catch (Exception ex)
        {
            // Log ra console nếu lỗi, nhưng không làm sập quá trình trả về kết quả đăng ký
            Console.WriteLine($"[CẢNH BÁO] Không thể tạo tự động shop vật lý: {ex.Message}");
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