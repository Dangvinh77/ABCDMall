using ABCDMall.Modules.FoodCourt.Application.DTOs.Foods;
using FluentValidation;

namespace ABCDMall.Modules.FoodCourt.Application.Services.Foods.Validators;

public sealed class UpsertFoodMenuItemRequestDtoValidator : AbstractValidator<UpsertFoodMenuItemRequestDto>
{
    public UpsertFoodMenuItemRequestDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Note)
            .MaximumLength(1000);

        RuleFor(x => x.Tag)
            .MaximumLength(100);

        RuleFor(x => x.ImageUrl)
            .MaximumLength(1000);

        RuleForEach(x => x.Ingredients)
            .NotEmpty()
            .MaximumLength(100);
    }
}
