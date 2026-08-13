using ABCDMall.Modules.FoodCourt.Application.DTOs.Foods;
using ABCDMall.Modules.FoodCourt.Application.Services.Foods;
using ABCDMall.Modules.FoodCourt.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ABCDMall.Modules.FoodCourt.Tests;

public sealed class FoodCommandServiceTests
{
    [Fact]
    public async Task CreateAsync_UsesProvidedImageUrl_WhenControllerAlreadySavedTheUpload()
    {
        var repository = new InMemoryFoodRepository();
        var service = new FoodCommandService(repository, NullLogger<FoodCommandService>.Instance);

        await service.CreateAsync(new CreateFoodRequestDto
        {
            Name = "Dookki",
            Description = "Korean buffet",
            ImageUrl = "/images/foodcourt/dookki.png"
        });

        var created = Assert.Single(repository.Items);
        Assert.Equal("/images/foodcourt/dookki.png", created.ImageUrl);
    }

    [Fact]
    public async Task UpdateAsync_PreservesProvidedImageUrl_WithoutAnyAdditionalStorageStep()
    {
        var repository = new InMemoryFoodRepository();
        repository.Items.Add(new FoodItem
        {
            Id = "food-1",
            Name = "Old Name",
            Slug = "old-name",
            Description = "Old description",
            ImageUrl = "/images/foodcourt/original.png"
        });

        var service = new FoodCommandService(repository, NullLogger<FoodCommandService>.Instance);

        var updated = await service.UpdateAsync("food-1", new UpdateFoodRequestDto
        {
            Name = "New Name",
            Description = "New description",
            ImageUrl = "/images/foodcourt/updated.png"
        });

        Assert.True(updated);
        var item = Assert.Single(repository.Items);
        Assert.Equal("/images/foodcourt/updated.png", item.ImageUrl);
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
