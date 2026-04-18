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
            "Ocean Blue Seafood Buffet",
            "ocean-blue",
            "A premium seafood counter featuring grilled lobster, oysters, sushi, and rotating weekend buffet specials for groups and families.",
            "https://images.unsplash.com/photo-1565680018434-b513d5e5fd47?q=80&w=1600&auto=format&fit=crop",
            "seafood"),
        new(
            "food-002",
            "Boba Bella Milk Tea",
            "boba-bella",
            "Fresh milk tea, fruit tea, and signature brown sugar boba served all day from the central beverage kiosk.",
            "https://images.unsplash.com/photo-1558857563-c0c4a4d2a9ee?q=80&w=1600&auto=format&fit=crop",
            "drinks"),
        new(
            "food-003",
            "Babushka A La Carte",
            "babushka-a-la-carte",
            "An international comfort-food station with pasta, baked rice, and lunch set menus designed for office crowds.",
            "https://images.unsplash.com/photo-1504674900247-0877df9cc836?q=80&w=1600&auto=format&fit=crop",
            "international"),
        new(
            "food-004",
            "Saigon Grill Express",
            "saigon-grill-express",
            "Vietnamese grilled specialties with fast-service rice plates, noodles, and flame-cooked meat skewers.",
            "https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?q=80&w=1600&auto=format&fit=crop",
            "vietnamese"),
        new(
            "food-005",
            "Tokyo Ramen Station",
            "tokyo-ramen-station",
            "Japanese ramen bowls, gyoza, and crispy karaage prepared in an open-kitchen concept near the family seating area.",
            "https://images.unsplash.com/photo-1617093727343-374698b1b08d?q=80&w=1600&auto=format&fit=crop",
            "japanese")
    ];
}

