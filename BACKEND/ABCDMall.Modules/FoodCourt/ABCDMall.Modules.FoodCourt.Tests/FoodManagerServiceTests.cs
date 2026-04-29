using ABCDMall.Modules.FoodCourt.Application.DTOs.Foods;
using ABCDMall.Modules.FoodCourt.Application.Mappings;
using ABCDMall.Modules.FoodCourt.Application.Services.Foods;
using ABCDMall.Modules.FoodCourt.Domain.Entities;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ABCDMall.Modules.FoodCourt.Tests;

public sealed class FoodManagerServiceTests
{
    [Fact]
    public async Task CreateMyFoodStallAsync_rejects_when_manager_has_no_available_food_court_slot()
    {
        var repository = new InMemoryFoodRepository
        {
            FoodCourtRentalCount = 1
        };
        repository.Items.Add(new FoodItem
        {
            Id = "stall-1",
            OwnerShopId = "shop-info-1",
            Name = "Boba Bella",
            Slug = "boba-bella"
        });

        var service = new FoodManagerService(repository, CreateMapper(), NullLogger<FoodManagerService>.Instance);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateMyFoodStallAsync("shop-info-1", new UpsertFoodManagerRequestDto
            {
                Name = "Takoyaki Town",
                Slug = "takoyaki-town",
                Description = "Japanese street food",
                CategorySlug = "asian",
                Location = "FC-02",
                OpenHours = "09:00 - 22:00",
                MenuItems =
                [
                    new UpsertFoodMenuItemRequestDto
                    {
                        Name = "Takoyaki Box",
                        Price = 59000m
                    }
                ]
            }));

        Assert.Equal("All rented food-court slots already have a managed stall.", exception.Message);
    }

    [Fact]
    public async Task AddMenuItemAsync_returns_null_when_stall_is_not_owned_by_manager()
    {
        var repository = new InMemoryFoodRepository();
        repository.Items.Add(new FoodItem
        {
            Id = "stall-1",
            OwnerShopId = "shop-info-2",
            Name = "Other Stall",
            Slug = "other-stall"
        });

        var service = new FoodManagerService(repository, CreateMapper(), NullLogger<FoodManagerService>.Instance);

        var result = await service.AddMenuItemAsync("shop-info-1", "stall-1", new UpsertFoodMenuItemRequestDto
        {
            Name = "Milk Tea",
            Price = 49000m,
            Note = "Best seller",
            Tag = "Signature",
            Ingredients = ["Black tea", "Boba"]
        });

        Assert.Null(result);
    }

    private static IMapper CreateMapper()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => { }, typeof(FoodProfile));
        return services.BuildServiceProvider().GetRequiredService<IMapper>();
    }

    private sealed class InMemoryFoodRepository : IFoodRepository
    {
        public List<FoodItem> Items { get; } = [];
        public int FoodCourtRentalCount { get; set; }

        public Task<IReadOnlyList<FoodItem>> GetFoodsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<FoodItem>>(Items);

        public Task<FoodItem?> GetFoodByIdAsync(string id, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.FirstOrDefault(item => item.Id == id));

        public Task<FoodItem?> GetFoodBySlugAsync(string slug, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.FirstOrDefault(item => item.Slug == slug));

        public Task<FoodItem?> GetFoodDetailByIdAsync(string id, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.FirstOrDefault(item => item.Id == id));

        public Task<FoodItem?> GetFoodDetailBySlugAsync(string slug, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.FirstOrDefault(item => item.Slug == slug));

        public Task<IReadOnlyList<FoodItem>> GetManagedFoodStallsAsync(string ownerShopId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<FoodItem>>(Items.Where(item => item.OwnerShopId == ownerShopId).ToList());

        public Task<FoodItem?> GetManagedFoodStallByIdAsync(string ownerShopId, string id, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.FirstOrDefault(item => item.Id == id && item.OwnerShopId == ownerShopId));

        public Task<int> CountFoodCourtRentalsAsync(string ownerShopId, CancellationToken cancellationToken = default)
            => Task.FromResult(ownerShopId == "shop-info-1" ? FoodCourtRentalCount : 0);

        public Task<int> CountManagedStallsAsync(string ownerShopId, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.Count(item => item.OwnerShopId == ownerShopId));

        public Task<IReadOnlyList<AvailableFoodCourtLocationDto>> GetAvailableFoodCourtLocationsAsync(string ownerShopId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<AvailableFoodCourtLocationDto>>([]);

        public Task<bool> SlugExistsAsync(string slug, string? excludingFoodId = null, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.Any(item => item.Slug == slug && item.Id != excludingFoodId));

        public Task CreateFoodAsync(FoodItem item, CancellationToken cancellationToken = default)
        {
            Items.Add(item);
            return Task.CompletedTask;
        }

        public Task UpdateFoodAsync(string id, FoodItem item, CancellationToken cancellationToken = default)
        {
            var index = Items.FindIndex(existing => existing.Id == id);
            if (index >= 0)
            {
                Items[index] = item;
            }

            return Task.CompletedTask;
        }

        public Task DeleteFoodAsync(string id, CancellationToken cancellationToken = default)
        {
            Items.RemoveAll(item => item.Id == id);
            return Task.CompletedTask;
        }
    }
}
