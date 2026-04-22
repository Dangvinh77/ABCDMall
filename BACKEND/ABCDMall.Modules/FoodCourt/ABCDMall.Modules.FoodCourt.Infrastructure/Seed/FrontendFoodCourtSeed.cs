using ABCDMall.Modules.FoodCourt.Application.Helpers;
using ABCDMall.Modules.FoodCourt.Domain.Entities;
using ABCDMall.Modules.FoodCourt.Infrastructure.Persistence.FoodCourt;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.FoodCourt.Infrastructure.Seed;

public static class FrontendFoodCourtSeed
{
    public static async Task SeedAsync(FoodCourtDbContext db, CancellationToken ct = default)
    {
        var existing = await db.FoodItems
            .Where(x => FoodSeeds.Select(seed => seed.Id).Contains(x.Id))
            .ToListAsync(ct);

        foreach (var seed in FoodSeeds)
        {
            var item = existing.FirstOrDefault(x => x.Id == seed.Id);
            if (item is null)
            {
                item = new FoodItem { Id = seed.Id };
                await db.FoodItems.AddAsync(item, ct);
                existing.Add(item);
            }

            item.Name = seed.Name;
            item.Slug = FoodHelperSlug(seed.Slug, seed.Name);
            item.Description = seed.Description;
            item.ImageUrl = seed.ImageUrl;
            item.MallSlug = "ABCD Mall";
            item.CategorySlug = seed.CategorySlug;
        }

        await db.SaveChangesAsync(ct);
    }

    private static string FoodHelperSlug(string? slug, string name)
        => string.IsNullOrWhiteSpace(slug) ? SlugHelper.GenerateSlug(name) : SlugHelper.GenerateSlug(slug);

    private sealed record FoodSeed(
        string Id,
        string Name,
        string? Slug,
        string Description,
        string ImageUrl,
        string CategorySlug);

    private static readonly FoodSeed[] FoodSeeds =
    [
        new(
            "food-001",
            "Lok Lok Hotpot",
            "lok-lok-hotpot",
            "A lively hotpot counter with shared broths, meat platters, and mall-friendly combo sets for groups and families.",
            "/img/Lok Lok Hotpot/out.jpg",
            "hotpot"),
        new(
            "food-002",
            "Boba Bella Milk Tea",
            "boba-bella",
            "Fresh milk tea, fruit tea, and signature brown sugar boba served all day from the central beverage kiosk.",
            "/img/Boba Bella Milk Tea/menu.jpg",
            "drinks"),
        new(
            "food-003",
            "Crystal Jade",
            "crystal-jade",
            "A Chinese dining counter featuring dim sum, noodles, and familiar sharing dishes in a polished mall setting.",
            "/img/crystaljade/menu.webp",
            "chinese"),
        new(
            "food-004",
            "King BBQ",
            "king-bbq",
            "A Korean barbecue favorite with grilled meat sets, buffet options, and fast ordering for lunch and dinner traffic.",
            "/img/King BBQ/out.jpg",
            "bbq"),
        new(
            "food-005",
            "Marukame Udon",
            "marukame-udon",
            "Fresh udon bowls, tempura sides, and Japanese set meals served in a quick-service format for busy mall visitors.",
            "/img/Marukame Udon/out.jpg",
            "japanese")
    ];
}

