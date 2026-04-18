using ABCDMall.Modules.FoodCourt.Application.DTOs.Foods;
using FluentValidation;

namespace ABCDMall.Modules.FoodCourt.Application.Services.Foods.Validators;

public sealed class CreateFoodRequestDtoValidator : AbstractValidator<CreateFoodRequestDto>
{
    public CreateFoodRequestDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(2000);

        RuleFor(x => x.ImageUrl)
            .MaximumLength(1000);
    }
}

