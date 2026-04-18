using ABCDMall.Modules.FoodCourt.Application.DTOs.Foods;
using FluentValidation;

namespace ABCDMall.Modules.FoodCourt.Application.Services.Foods.Validators;

public sealed class UpdateFoodRequestDtoValidator : AbstractValidator<UpdateFoodRequestDto>
{
    public UpdateFoodRequestDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.ImageUrl).MaximumLength(1000);
    }
}
