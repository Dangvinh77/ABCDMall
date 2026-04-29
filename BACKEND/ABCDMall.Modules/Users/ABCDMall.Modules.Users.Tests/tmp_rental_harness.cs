using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ABCDMall.Modules.Users.Application.Common;
using ABCDMall.Modules.Users.Application.DTOs;
using ABCDMall.Modules.Users.Application.Services;
using ABCDMall.Modules.Users.Application.Services.RentalAreas;
using ABCDMall.Modules.Users.Domain.Entities;
using ABCDMall.Modules.Users.Infrastructure;
using ABCDMall.Modules.UtilityMap.Domain.Entities;
using ABCDMall.Modules.UtilityMap.Infrastructure.Persistence.UtilityMap;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

public sealed class PatchedRentalAreaCommandRepository : IRentalAreaCommandRepository
{
    private readonly MallDbContext _context;
    private readonly UtilityMapDbContext _utilityMapContext;
    private readonly Dictionary<string, RentalArea> _transientRentalAreas = new(StringComparer.OrdinalIgnoreCase);

    public PatchedRentalAreaCommandRepository(MallDbContext context, UtilityMapDbContext utilityMapContext)
    {
        _context = context;
        _utilityMapContext = utilityMapContext;
    }

    public Task<bool> ExistsRentalAreaByCodeAsync(string normalizedAreaCode, CancellationToken cancellationToken = default)
        => _context.RentalAreas.AnyAsync(x => x.AreaCode.ToLower() == normalizedAreaCode, cancellationToken);

    public async Task<RentalArea?> GetRentalAreaByIdAsync(string rentalAreaId, CancellationToken cancellationToken = default)
    {
        if (_transientRentalAreas.TryGetValue(rentalAreaId, out var cachedRentalArea))
        {
            return cachedRentalArea;
        }

        var existingRentalArea = await _context.RentalAreas.FirstOrDefaultAsync(x => x.Id == rentalAreaId, cancellationToken);
        if (existingRentalArea is not null)
        {
            return existingRentalArea;
        }

        if (!int.TryParse(rentalAreaId, out var mapLocationId))
        {
            return null;
        }

        var mapLocation = await _utilityMapContext.MapLocations
            .Include(x => x.FloorPlan)
            .FirstOrDefaultAsync(x => x.Id == mapLocationId, cancellationToken);

        if (mapLocation is null)
        {
            return null;
        }

        var persistedByAreaCode = await _context.RentalAreas
            .FirstOrDefaultAsync(x => x.AreaCode == mapLocation.LocationSlot, cancellationToken);
        if (persistedByAreaCode is not null)
        {
            _transientRentalAreas[rentalAreaId] = persistedByAreaCode;
            return persistedByAreaCode;
        }

        var bridgedRentalArea = new RentalArea
        {
            Id = rentalAreaId,
            AreaCode = mapLocation.LocationSlot,
            Floor = mapLocation.FloorPlan?.FloorLevel ?? string.Empty,
            AreaName = string.IsNullOrWhiteSpace(mapLocation.ShopName) ? $"Map Slot {mapLocation.LocationSlot}" : mapLocation.ShopName,
            Size = string.Empty,
            MonthlyRent = 0,
            Status = string.Equals(mapLocation.Status, "Available", StringComparison.OrdinalIgnoreCase) ? "Available" : "Rented",
            TenantName = string.IsNullOrWhiteSpace(mapLocation.ShopName) ? null : mapLocation.ShopName,
            ShopInfoId = mapLocation.ShopInfoId,
            CreatedAt = DateTime.UtcNow
        };

        _transientRentalAreas[rentalAreaId] = bridgedRentalArea;
        return bridgedRentalArea;
    }

    public Task AddRentalAreaAsync(RentalArea rentalArea, CancellationToken cancellationToken = default)
        => _context.RentalAreas.AddAsync(rentalArea, cancellationToken).AsTask();

    public Task<User?> GetManagerByCccdAsync(string normalizedCccd, CancellationToken cancellationToken = default)
        => _context.Users.FirstOrDefaultAsync(x => x.CCCD == normalizedCccd && x.Role == "Manager", cancellationToken);

    public async Task<ShopInfo?> GetShopInfoByManagerAsync(User manager, string normalizedCccd, CancellationToken cancellationToken = default)
    {
        var shopInfo = !string.IsNullOrWhiteSpace(manager.ShopId)
            ? await _context.ShopInfos.FirstOrDefaultAsync(x => x.Id == manager.ShopId, cancellationToken)
            : null;

        return shopInfo ?? await _context.ShopInfos.FirstOrDefaultAsync(x => x.CCCD == normalizedCccd, cancellationToken);
    }

    public Task AddShopInfoAsync(ShopInfo shopInfo, CancellationToken cancellationToken = default)
        => _context.ShopInfos.AddAsync(shopInfo, cancellationToken).AsTask();

    public async Task<ShopInfo?> GetShopInfoByRentalAreaAsync(string rentalLocation, string? tenantName, string? shopInfoId, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(shopInfoId))
        {
            var linkedShopInfo = await _context.ShopInfos.FirstOrDefaultAsync(x => x.Id == shopInfoId, cancellationToken);
            if (linkedShopInfo is not null)
            {
                return linkedShopInfo;
            }
        }

        var shopInfo = await _context.ShopInfos.FirstOrDefaultAsync(x => x.RentalLocation == rentalLocation && x.ShopName == tenantName, cancellationToken);
        return shopInfo ?? await _context.ShopInfos.FirstOrDefaultAsync(x => x.RentalLocation == rentalLocation, cancellationToken);
    }

    public Task AddMonthlyBillAsync(ShopMonthlyBill monthlyBill, CancellationToken cancellationToken = default)
        => _context.ShopMonthlyBills.AddAsync(monthlyBill, cancellationToken).AsTask();

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var changedRentalAreas = _context.ChangeTracker.Entries<RentalArea>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified)
            .Select(entry => entry.Entity)
            .ToList();

        changedRentalAreas.AddRange(_transientRentalAreas.Values);

        foreach (var rentalArea in changedRentalAreas.Distinct())
        {
            var persistedRentalArea = await _context.RentalAreas.FirstOrDefaultAsync(x => x.Id == rentalArea.Id, cancellationToken)
                ?? await _context.RentalAreas.FirstOrDefaultAsync(x => x.AreaCode == rentalArea.AreaCode, cancellationToken);
            if (persistedRentalArea is null)
            {
                persistedRentalArea = new RentalArea
                {
                    Id = rentalArea.Id,
                    AreaCode = rentalArea.AreaCode,
                    Floor = rentalArea.Floor,
                    AreaName = rentalArea.AreaName,
                    Size = rentalArea.Size,
                    MonthlyRent = rentalArea.MonthlyRent,
                    CreatedAt = rentalArea.CreatedAt
                };
                await _context.RentalAreas.AddAsync(persistedRentalArea, cancellationToken);
            }

            persistedRentalArea.AreaCode = rentalArea.AreaCode;
            persistedRentalArea.Floor = rentalArea.Floor;
            persistedRentalArea.AreaName = rentalArea.AreaName;
            persistedRentalArea.Size = rentalArea.Size;
            persistedRentalArea.MonthlyRent = rentalArea.MonthlyRent;
            persistedRentalArea.Status = rentalArea.Status;
            persistedRentalArea.TenantName = rentalArea.TenantName;
            persistedRentalArea.ShopInfoId = rentalArea.ShopInfoId;
            persistedRentalArea.BusinessType = rentalArea.BusinessType;

            MapLocation? mapLocation = null;
            if (int.TryParse(rentalArea.Id, out var mapLocationId))
            {
                mapLocation = await _utilityMapContext.MapLocations.FirstOrDefaultAsync(x => x.Id == mapLocationId, cancellationToken);
            }

            mapLocation ??= await _utilityMapContext.MapLocations.FirstOrDefaultAsync(x => x.LocationSlot == rentalArea.AreaCode, cancellationToken);
            if (mapLocation is null)
            {
                continue;
            }

            mapLocation.Status = string.Equals(rentalArea.Status, "Available", StringComparison.OrdinalIgnoreCase) ? "Available" : "Rented";
            mapLocation.ShopInfoId = rentalArea.ShopInfoId;
            mapLocation.ShopName = rentalArea.TenantName ?? string.Empty;
        }

        await _context.SaveChangesAsync(cancellationToken);
        await _utilityMapContext.SaveChangesAsync(cancellationToken);
    }
}

public sealed class FakeFileStorageService : IFileStorageService
{
    public Task<string> SaveProfileAvatarAsync(IFormFile file, CancellationToken cancellationToken = default) => Task.FromResult("/files/avatar.png");
    public Task<string> SaveCccdImageAsync(IFormFile file, CancellationToken cancellationToken = default) => Task.FromResult("/files/cccd.png");
    public Task<string> SaveContractImageAsync(IFormFile file, CancellationToken cancellationToken = default) => Task.FromResult("/files/contract.png");
    public Task<string> SaveShopLogoAsync(IFormFile file, CancellationToken cancellationToken = default) => Task.FromResult("/files/shop-logo.png");
    public Task<string> SaveShopCoverAsync(IFormFile file, CancellationToken cancellationToken = default) => Task.FromResult("/files/shop-cover.png");
    public Task<string> SaveShopProductImageAsync(IFormFile file, CancellationToken cancellationToken = default) => Task.FromResult("/files/shop-product.png");
}

static MallDbContext CreateMallContext()
{
    var options = new DbContextOptionsBuilder<MallDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
        .Options;
    return new MallDbContext(options);
}

static UtilityMapDbContext CreateUtilityMapContext()
{
    var options = new DbContextOptionsBuilder<UtilityMapDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
        .Options;
    return new UtilityMapDbContext(options);
}

async Task<string> RunScenario(bool withLegacyRow)
{
    await using var mallContext = CreateMallContext();
    await using var utilityMapContext = CreateUtilityMapContext();

    var suffix = withLegacyRow ? "032" : "031";
    mallContext.Users.Add(new User
    {
        Id = $"users-manager-{suffix}",
        Email = $"manager{suffix}@abcdmall.local",
        Role = "Manager",
        FullName = $"Prospect Manager {suffix}",
        CCCD = $"0892040000{suffix}",
        IsActive = true
    });

    if (withLegacyRow)
    {
        mallContext.RentalAreas.Add(new RentalArea
        {
            Id = "legacy-rental-1",
            AreaCode = "1-09",
            Floor = "Tang 1",
            AreaName = "Legacy Slot 1-09",
            Status = "Available"
        });
    }

    utilityMapContext.FloorPlans.Add(new FloorPlan
    {
        Id = 1,
        FloorLevel = "Tang 1",
        Description = "Test floor",
        BlueprintImageUrl = "/maps/test.png",
    });
    utilityMapContext.MapLocations.Add(new MapLocation
    {
        Id = 22,
        FloorPlanId = 1,
        ShopName = "Placeholder",
        LocationSlot = "1-09",
        ShopUrl = "/shops/pedro",
        X = 1,
        Y = 1,
        StorefrontImageUrl = "/img/test.png",
        Status = "Available"
    });
    await mallContext.SaveChangesAsync();
    await utilityMapContext.SaveChangesAsync();

    var service = new RentalAreaCommandService(null!, new PatchedRentalAreaCommandRepository(mallContext, utilityMapContext), new FakeFileStorageService());
    var result = await service.RegisterTenantAsync("22", new RegisterTenantDto
    {
        CCCD = $"0892040000{suffix}",
        Location = "1-09",
        BusinessType = "Shop",
        StartDate = DateTime.Today.AddDays(1),
        ElectricityFee = 3500m,
        WaterFee = 15000m,
        ServiceFee = 600000m,
        LeaseTermDays = 180,
        ContractImage = new FormFile(Stream.Null, 0, 0, "contract", "contract.png")
    });

    var rentalAreas = await mallContext.RentalAreas.Where(x => x.AreaCode == "1-09").ToListAsync();
    return $"status={result.Status}; error={result.Error}; count={rentalAreas.Count}; ids={string.Join(',', rentalAreas.Select(x => x.Id))}; business={string.Join(',', rentalAreas.Select(x => x.BusinessType))}; state={string.Join(',', rentalAreas.Select(x => x.Status))}";
}

Console.WriteLine("scenario_no_legacy=" + await RunScenario(false));
Console.WriteLine("scenario_with_legacy=" + await RunScenario(true));
