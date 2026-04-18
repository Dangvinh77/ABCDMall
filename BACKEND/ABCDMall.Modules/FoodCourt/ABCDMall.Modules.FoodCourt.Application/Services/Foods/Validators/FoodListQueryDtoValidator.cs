using ABCDMall.Modules.FoodCourt.Application.DTOs.Foods;
using FluentValidation;

namespace ABCDMall.Modules.FoodCourt.Application.Services.Foods.Validators;

public sealed class FoodListQueryDtoValidator : AbstractValidator<FoodListQueryDto>
{
    public FoodListQueryDtoValidator()
    {
        RuleFor(x => x.Keyword)
            .MaximumLength(100);
    }
}

