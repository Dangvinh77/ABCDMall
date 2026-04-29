using ABCDMall.Modules.FoodCourt.Application.DTOs.Foods;
using FluentValidation;

namespace ABCDMall.Modules.FoodCourt.Application.Services.Foods.Validators;

public sealed class UpsertFoodManagerRequestDtoValidator : AbstractValidator<UpsertFoodManagerRequestDto>
{
    public UpsertFoodManagerRequestDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Slug)
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(2000);

        RuleFor(x => x.ImageUrl)
            .MaximumLength(1000);

        RuleFor(x => x.CategorySlug)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Location)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.OpenHours)
            .MaximumLength(100);

        RuleFor(x => x.Phone)
            .MaximumLength(50);

        RuleFor(x => x.Promo)
            .MaximumLength(500);

        RuleForEach(x => x.MenuItems)
            .SetValidator(new UpsertFoodMenuItemRequestDtoValidator());
    }
}
