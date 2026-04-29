using ABCDMall.Modules.Events.Application.DTOs.Events;
using FluentValidation;

namespace ABCDMall.Modules.Events.Application.Services.Events.Validators;

public sealed class CreateEventRequestDtoValidator : AbstractValidator<CreateEventRequestDto>
{
    // Number of days ahead that an event must be scheduled
    private const int MinimumDaysAhead = 1;

    public CreateEventRequestDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.ImageUrl).MaximumLength(1000);
        
        RuleFor(x => x.StartDateTime)
            .NotEmpty()
            .WithMessage("Start date/time is required")
            .GreaterThan(DateTime.UtcNow.AddDays(MinimumDaysAhead))
            .WithMessage($"Events must be scheduled at least {MinimumDaysAhead} day(s) in advance. Please select a start date after {DateTime.UtcNow.AddDays(MinimumDaysAhead):MMM dd, yyyy}.");
        
        RuleFor(x => x.EndDateTime)
            .NotEmpty()
            .WithMessage("End date/time is required")
            .GreaterThan(x => x.StartDateTime)
            .WithMessage("End date must be after start date");
        
        RuleFor(x => x.LocationType).InclusiveBetween(1, 5);
        RuleFor(x => x.GiftDescription).MaximumLength(500);
        RuleFor(x => x.GiftDescription)
            .NotEmpty()
            .When(x => x.HasGiftRegistration)
            .WithMessage("Gift description is required when gift registration is enabled.");
    }
}
