using ABCDMall.Modules.Users.Application.Services.Auth;
using ABCDMall.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Users.Infrastructure;

public sealed class UserReadRepository : IUserReadRepository
{
    private readonly MallDbContext _context;

    public UserReadRepository(MallDbContext context)
    {
        _context = context;
    }

    public Task<User?> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return _context.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<ProfileUpdateHistory>> GetProfileUpdateHistoryAsync(string userId, int take, CancellationToken cancellationToken = default)
    {
        return await _context.ProfileUpdateHistories
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.UpdatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetUsersByRoleAsync(string role, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Where(x => x.Role == role)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<string, string>> GetShopNamesByIdsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ShopInfos
            .Where(x => !string.IsNullOrWhiteSpace(x.Id))
            .ToDictionaryAsync(x => x.Id!, x => x.ShopName, cancellationToken);
    }
}
