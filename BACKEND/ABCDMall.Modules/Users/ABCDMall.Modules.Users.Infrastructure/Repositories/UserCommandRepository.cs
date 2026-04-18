using ABCDMall.Modules.Users.Application.Services.Auth;
using ABCDMall.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Users.Infrastructure;

public sealed class UserCommandRepository : IUserCommandRepository
{
    private readonly MallDbContext _context;

    public UserCommandRepository(MallDbContext context)
    {
        _context = context;
    }

    public Task<User?> GetUserByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
        => _context.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == normalizedEmail, cancellationToken);

    public Task<User?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default)
        => _context.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

    public Task<bool> ExistsUserByEmailAsync(string normalizedEmail, string? excludedUserId = null, CancellationToken cancellationToken = default)
        => _context.Users.AnyAsync(
            x => x.Email.ToLower() == normalizedEmail && (excludedUserId == null || x.Id != excludedUserId),
            cancellationToken);

    public Task<bool> ExistsUserByCccdAsync(string normalizedCccd, string? excludedUserId = null, CancellationToken cancellationToken = default)
        => _context.Users.AnyAsync(
            x => x.CCCD == normalizedCccd && (excludedUserId == null || x.Id != excludedUserId),
            cancellationToken);

    public Task<bool> ExistsShopInfoByCccdAsync(string normalizedCccd, string? excludedShopId = null, CancellationToken cancellationToken = default)
        => _context.ShopInfos.AnyAsync(
            x => x.CCCD == normalizedCccd && (excludedShopId == null || x.Id != excludedShopId),
            cancellationToken);

    public Task<ShopInfo?> GetShopInfoByIdAsync(string shopId, CancellationToken cancellationToken = default)
        => _context.ShopInfos.FirstOrDefaultAsync(x => x.Id == shopId, cancellationToken);

    public async Task RemoveUnusedForgotPasswordOtpsAsync(string normalizedEmail, CancellationToken cancellationToken = default)
    {
        var items = await _context.ForgotPasswordOtps
            .Where(x => x.Email.ToLower() == normalizedEmail && !x.IsUsed)
            .ToListAsync(cancellationToken);
        _context.ForgotPasswordOtps.RemoveRange(items);
    }

    public Task AddForgotPasswordOtpAsync(ForgotPasswordOtp otp, CancellationToken cancellationToken = default)
        => _context.ForgotPasswordOtps.AddAsync(otp, cancellationToken).AsTask();

    public Task<ForgotPasswordOtp?> GetForgotPasswordOtpAsync(string normalizedEmail, string otp, CancellationToken cancellationToken = default)
        => _context.ForgotPasswordOtps
            .Where(x => x.Email.ToLower() == normalizedEmail && x.Otp == otp && !x.IsUsed)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task RemoveUnusedPasswordResetOtpsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var items = await _context.PasswordResetOtps
            .Where(x => x.UserId == userId && !x.IsUsed)
            .ToListAsync(cancellationToken);
        _context.PasswordResetOtps.RemoveRange(items);
    }

    public Task AddPasswordResetOtpAsync(PasswordResetOtp otp, CancellationToken cancellationToken = default)
        => _context.PasswordResetOtps.AddAsync(otp, cancellationToken).AsTask();

    public Task<PasswordResetOtp?> GetPasswordResetOtpAsync(string userId, string otp, CancellationToken cancellationToken = default)
        => _context.PasswordResetOtps
            .Where(x => x.UserId == userId && x.Otp == otp && !x.IsUsed)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public Task AddProfileUpdateHistoryAsync(ProfileUpdateHistory history, CancellationToken cancellationToken = default)
        => _context.ProfileUpdateHistories.AddAsync(history, cancellationToken).AsTask();

    public Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
        => _context.RefreshTokens.AddAsync(refreshToken, cancellationToken).AsTask();

    public Task<RefreshToken?> GetRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        => _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken, cancellationToken);

    public Task AddShopInfoAsync(ShopInfo shopInfo, CancellationToken cancellationToken = default)
        => _context.ShopInfos.AddAsync(shopInfo, cancellationToken).AsTask();

    public Task AddUserAsync(User user, CancellationToken cancellationToken = default)
        => _context.Users.AddAsync(user, cancellationToken).AsTask();

    public async Task RemoveUserRelatedDataAsync(string userId, CancellationToken cancellationToken = default)
    {
        var refreshTokens = await _context.RefreshTokens.Where(x => x.UserId == userId).ToListAsync(cancellationToken);
        var passwordResetOtps = await _context.PasswordResetOtps.Where(x => x.UserId == userId).ToListAsync(cancellationToken);
        var forgotPasswordOtps = await _context.ForgotPasswordOtps.Where(x => x.UserId == userId).ToListAsync(cancellationToken);

        _context.RefreshTokens.RemoveRange(refreshTokens);
        _context.PasswordResetOtps.RemoveRange(passwordResetOtps);
        _context.ForgotPasswordOtps.RemoveRange(forgotPasswordOtps);
    }

    public Task RemoveUserAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Remove(user);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
