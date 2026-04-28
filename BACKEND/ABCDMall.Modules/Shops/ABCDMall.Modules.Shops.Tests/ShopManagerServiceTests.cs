using System.Linq;
using ABCDMall.Modules.Shops.Application.DTOs;
using ABCDMall.Modules.Shops.Application.Services.Manager;
using ABCDMall.Modules.Shops.Domain.Entities;
using Xunit;

namespace ABCDMall.Modules.Shops.Tests;

public class ShopManagerServiceTests
{
    [Fact]
    public async Task CreateMyShopAsync_assigns_owner_shop_id_and_normalizes_tags()
    {
        var repository = new FakeShopManagerRepository();
        var service = new ShopManagerService(repository);

        var result = await service.CreateMyShopAsync("shop-info-1", new UpsertManagedShopRequestDto
        {
            Name = "Book World",
            Slug = "Book World",
            Category = "Books",
            Floor = "Floor 2",
            LocationSlot = "B2-09",
            Summary = "Books and stationery",
            Description = "Large bookstore",
            CoverImageUrl = "/covers/book-world.png",
            LogoUrl = "/logos/book-world.png",
            Tags = ["Books", " books ", "Stationery"]
        });

        Assert.Equal("shop-info-1", repository.AddedShop!.OwnerShopId);
        Assert.Equal("book-world", result.Slug);
        Assert.Equal(2, result.Tags.Count());
        Assert.Contains("Books", result.Tags);
        Assert.Contains("Stationery", result.Tags);
    }

    [Fact]
    public async Task UpdateMyShopAsync_returns_null_when_shop_does_not_belong_to_owner()
    {
        var repository = new FakeShopManagerRepository();
        var service = new ShopManagerService(repository);

        var result = await service.UpdateMyShopAsync("shop-info-1", "missing-shop", new UpsertManagedShopRequestDto
        {
            Name = "Book World",
            Slug = "book-world",
            Category = "Books",
            Floor = "Floor 2",
            LocationSlot = "B2-09",
            Summary = "Books and stationery",
            Description = "Large bookstore",
            CoverImageUrl = "/covers/book-world.png"
        });

        Assert.Null(result);
    }

    private sealed class FakeShopManagerRepository : IShopManagerRepository
    {
        public Shop? AddedShop { get; private set; }

        public Task<IReadOnlyList<Shop>> GetShopsByOwnerAsync(string ownerShopId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Shop>>([]);

        public Task<Shop?> GetShopByIdAndOwnerAsync(string id, string ownerShopId, CancellationToken cancellationToken = default)
            => Task.FromResult<Shop?>(null);

        public Task<bool> ExistsSlugAsync(string slug, string? excludedShopId = null, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task AddShopAsync(Shop shop, CancellationToken cancellationToken = default)
        {
            AddedShop = shop;
            return Task.CompletedTask;
        }

        public void RemoveShop(Shop shop)
        {
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
