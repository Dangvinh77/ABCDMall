using ABCDMall.Modules.UtilityMap.Application.DTOs.Maps;
using FluentValidation;

namespace ABCDMall.Modules.UtilityMap.Application.Services.Maps.Validators;

public class CreateMapLocationRequestDtoValidator : AbstractValidator<CreateMapLocationRequestDto>
{
    public CreateMapLocationRequestDtoValidator()
    {
        RuleFor(x => x.ShopName)
            .NotEmpty().WithMessage("Shop name is required.")
            .MaximumLength(200).WithMessage("Shop name must not exceed 200 characters.");

        RuleFor(x => x.LocationSlot)
            .NotEmpty().WithMessage("Location slot is required.")
            .MaximumLength(50).WithMessage("Location slot must not exceed 50 characters.");

        RuleFor(x => x.X)
            .InclusiveBetween(0, 100).WithMessage("X coordinate must be between 0 and 100.");

        RuleFor(x => x.Y)
            .InclusiveBetween(0, 100).WithMessage("Y coordinate must be between 0 and 100.");
    }
}
