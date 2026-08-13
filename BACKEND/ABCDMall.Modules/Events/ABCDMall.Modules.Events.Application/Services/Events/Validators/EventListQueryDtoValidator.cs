using ABCDMall.Modules.Events.Application.DTOs.Events;
using FluentValidation;

namespace ABCDMall.Modules.Events.Application.Services.Events.Validators;

public sealed class EventListQueryDtoValidator : AbstractValidator<EventListQueryDto>
{
    private static readonly HashSet<string> ValidStatuses =
        new(StringComparer.OrdinalIgnoreCase) { "upcoming", "ongoing", "ended" };

    public EventListQueryDtoValidator()
    {
        RuleFor(x => x.Keyword)
            .MaximumLength(200);

        RuleFor(x => x.EventType)
            .InclusiveBetween(1, 2)
            .When(x => x.EventType.HasValue)
            .WithMessage("EventType phải là 1 (MallEvent) hoặc 2 (BrandEvent).");

        RuleFor(x => x.Status)
            .Must(s => s is null || ValidStatuses.Contains(s))
            .WithMessage("Status phải là 'upcoming', 'ongoing' hoặc 'ended'.");
    }
}