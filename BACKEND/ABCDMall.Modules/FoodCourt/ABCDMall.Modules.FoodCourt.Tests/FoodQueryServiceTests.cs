using AutoMapper;
using ABCDMall.Modules.FoodCourt.Application.DTOs.Foods;
using ABCDMall.Modules.FoodCourt.Application.Mappings;
using ABCDMall.Modules.FoodCourt.Application.Services.Foods;
using ABCDMall.Modules.FoodCourt.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ABCDMall.Modules.FoodCourt.Tests;

public sealed class FoodQueryServiceTests
{
    [Fact]
    public async Task GetBySlugAsync_returns_menu_items_for_food_stall_detail()
    {
        var repository = new InMemoryFoodRepository();
        repository.Items.Add(new FoodItem
        {
            Id = "stall-1",
            OwnerShopId = "shop-info-1",
            Name = "Boba Bella",
            Slug = "boba-bella",
            Description = "Milk tea stall",
            ImageUrl = "/images/boba.png",
            CategorySlug = "drinks",
            MenuItems =
            [
                new FoodMenuItem
                {
                    Id = "menu-1",
                    FoodStallId = "stall-1",
                    Name = "Brown Sugar Milk Tea",
                    Price = 49000m,
                    Note = "Best seller",
                    Tag = "Signature",
                    ImageUrl = "/images/milk-tea.png",
                    IngredientsJson = "[\"Black tea\",\"Boba\"]",
                    IsAvailable = true,
                    DisplayOrder = 1
                }
            ]
        });

        var service = new FoodQueryService(repository, CreateMapper(), NullLogger<FoodQueryService>.Instance);

        var result = await service.GetBySlugAsync("boba-bella");

        Assert.NotNull(result);
        Assert.IsType<FoodDetailDto>(result);
        Assert.Single(result!.MenuItems);
        Assert.Equal("Brown Sugar Milk Tea", result.MenuItems[0].Name);
        Assert.Equal("Black tea", result.MenuItems[0].Ingredients[0]);
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
            => Task.FromResult(Items.FirstOrDefault(item => item.OwnerShopId == ownerShopId && item.Id == id));

        public Task<int> CountFoodCourtRentalsAsync(string ownerShopId, CancellationToken cancellationToken = default)
            => Task.FromResult(0);

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
