using ABCDMall.Modules.Events.Application.DTOs.Events;
using FluentValidation;

namespace ABCDMall.Modules.Events.Application.Services.Events.Validators;

public sealed class UpdateEventRequestDtoValidator : AbstractValidator<UpdateEventRequestDto>
{
    public UpdateEventRequestDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.ImageUrl).MaximumLength(1000);
        RuleFor(x => x.StartDateTime).NotEmpty();
        RuleFor(x => x.EndDateTime).GreaterThan(x => x.StartDateTime);
        RuleFor(x => x.LocationType).InclusiveBetween(1, 5);
        RuleFor(x => x.ApprovalStatus).InclusiveBetween(1, 3);
        RuleFor(x => x.GiftDescription).MaximumLength(500);
    }
}
