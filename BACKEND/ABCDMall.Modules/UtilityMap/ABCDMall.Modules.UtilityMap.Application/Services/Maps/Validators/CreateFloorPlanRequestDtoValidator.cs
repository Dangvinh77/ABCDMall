using ABCDMall.Modules.UtilityMap.Application.DTOs.Maps;
using FluentValidation;

namespace ABCDMall.Modules.UtilityMap.Application.Services.Maps.Validators;

public class CreateFloorPlanRequestDtoValidator : AbstractValidator<CreateFloorPlanRequestDto>
{
    public CreateFloorPlanRequestDtoValidator()
    {
        RuleFor(x => x.FloorLevel)
            .NotEmpty().WithMessage("Floor level is required.")
            .MaximumLength(50).WithMessage("Floor level must not exceed 50 characters.");
    }
}
