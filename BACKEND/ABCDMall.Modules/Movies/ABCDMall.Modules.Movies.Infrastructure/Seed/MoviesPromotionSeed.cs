using ABCDMall.Modules.Movies.Domain.Entities;
using ABCDMall.Modules.Movies.Domain.Enums;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Movies.Infrastructure.Seed;

public static class MoviesPromotionSeed
{
    public static async Task SeedAsync(MoviesBookingDbContext dbContext, CancellationToken cancellationToken = default)
    {
        // Day 3 chi seed khi bang promotions dang trong de tranh dup data tren local.
        if (await dbContext.Promotions.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;

        // Dung ID co dinh de frontend/test script co the tai su dung giua cac may dev.
        var classicComboId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var coupleComboId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var snackCombos = new[]
        {
            new SnackCombo
            {
                Id = classicComboId,
                Code = "combo-classic",
                Name = "Classic Combo",
                Description = "1 bap + 1 nuoc",
                Price = 89000,
                IsActive = true,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            },
            new SnackCombo
            {
                Id = coupleComboId,
                Code = "combo-couple",
                Name = "Couple Combo",
                Description = "1 bap lon + 2 nuoc",
                Price = 129000,
                IsActive = true,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            }
        };

        // MetadataJson dung de category filter cua Day 3 tra ve nhat quan ma khong can mo schema moi.
        var weekendPromotionId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var momoPromotionId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var couplePromotionId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        var comboPromotionId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
        var birthdayPromotionId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");

        var promotions = new[]
        {
            new Promotion
            {
                Id = weekendPromotionId,
                Code = "WEEKEND10",
                Name = "Weekend 10%",
                Description = "Giam 10% cho booking cuoi tuan",
                Status = PromotionStatus.Active,
                ValidFromUtc = DateTimeOffset.UtcNow.AddDays(-7),
                PercentageValue = 10,
                MaximumDiscountAmount = 30000,
                MinimumSpendAmount = 150000,
                IsAutoApplied = true,
                MetadataJson = "{\"category\":\"weekend\"}",
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            },
            new Promotion
            {
                Id = momoPromotionId,
                Code = "MOMO30",
                Name = "MoMo Discount 30K",
                Description = "Giam 30K khi thanh toan bang MoMo",
                Status = PromotionStatus.Active,
                ValidFromUtc = DateTimeOffset.UtcNow.AddDays(-7),
                FlatDiscountValue = 30000,
                MinimumSpendAmount = 120000,
                IsAutoApplied = false,
                MetadataJson = "{\"category\":\"bank\"}",
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            },
            new Promotion
            {
                Id = couplePromotionId,
                Code = "COUPLE50",
                Name = "Couple Seat 50K",
                Description = "Giam 50K cho ghe doi",
                Status = PromotionStatus.Active,
                ValidFromUtc = DateTimeOffset.UtcNow.AddDays(-7),
                FlatDiscountValue = 50000,
                MinimumSpendAmount = 200000,
                IsAutoApplied = false,
                MetadataJson = "{\"category\":\"ticket\"}",
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            },
            new Promotion
            {
                Id = comboPromotionId,
                Code = "COMBO15",
                Name = "Classic Combo 15K",
                Description = "Giam 15K khi mua combo classic",
                Status = PromotionStatus.Active,
                ValidFromUtc = DateTimeOffset.UtcNow.AddDays(-7),
                FlatDiscountValue = 15000,
                IsAutoApplied = false,
                MetadataJson = "{\"category\":\"combo\"}",
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            },
            new Promotion
            {
                Id = birthdayPromotionId,
                Code = "BIRTHDAY20",
                Name = "Birthday Month 20K",
                Description = "Giam 20K trong thang sinh nhat",
                Status = PromotionStatus.Active,
                ValidFromUtc = DateTimeOffset.UtcNow.AddDays(-7),
                FlatDiscountValue = 20000,
                MinimumSpendAmount = 100000,
                MaxRedemptionsPerCustomer = 1,
                IsAutoApplied = false,
                MetadataJson = "{\"category\":\"member\"}",
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            }
        };

        var rules = new[]
        {
            // Promotion tu dong cho booking vao T7/CN.
            new PromotionRule
            {
                Id = Guid.NewGuid(),
                PromotionId = weekendPromotionId,
                RuleType = PromotionRuleType.BusinessDate,
                RuleValue = "Weekend",
                SortOrder = 1,
                IsRequired = true
            },

            // Promotion theo payment provider.
            new PromotionRule
            {
                Id = Guid.NewGuid(),
                PromotionId = momoPromotionId,
                RuleType = PromotionRuleType.PaymentProvider,
                RuleValue = "Momo",
                SortOrder = 1,
                IsRequired = true
            },

            // Promotion theo loai ghe couple.
            new PromotionRule
            {
                Id = Guid.NewGuid(),
                PromotionId = couplePromotionId,
                RuleType = PromotionRuleType.SeatType,
                RuleValue = "Couple",
                SortOrder = 1,
                IsRequired = true
            },

            // Promotion theo combo snack.
            new PromotionRule
            {
                Id = Guid.NewGuid(),
                PromotionId = comboPromotionId,
                RuleType = PromotionRuleType.Combo,
                RuleValue = classicComboId.ToString(),
                SortOrder = 1,
                IsRequired = true
            },

            // Promotion theo thang sinh nhat, dung truong Birthday trong request.
            new PromotionRule
            {
                Id = Guid.NewGuid(),
                PromotionId = birthdayPromotionId,
                RuleType = PromotionRuleType.BirthdayMonth,
                RuleValue = "CurrentMonth",
                SortOrder = 1,
                IsRequired = true
            }
        };

        // Seed theo thu tu combo -> promotions -> rules de debug DB de doc.
        await dbContext.SnackCombos.AddRangeAsync(snackCombos, cancellationToken);
        await dbContext.Promotions.AddRangeAsync(promotions, cancellationToken);
        await dbContext.PromotionRules.AddRangeAsync(rules, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
