using ABCDMall.Modules.Events.Application.DTOs.Events;
using FluentValidation;

namespace ABCDMall.Modules.Events.Application.Services.Events.Validators;

public sealed class EventListQueryDtoValidator : AbstractValidator<EventListQueryDto>
{
    private static readonly HashSet<string> ValidFilters = new(StringComparer.OrdinalIgnoreCase) { "ongoing", "upcoming" };

    public EventListQueryDtoValidator()
    {
        RuleFor(x => x.Keyword)
            .MaximumLength(200);

        RuleFor(x => x.ApprovalStatus)
            .InclusiveBetween(1, 3)
            .When(x => x.ApprovalStatus.HasValue);

        RuleFor(x => x.TimeFilter)
            .Must(x => x is null || ValidFilters.Contains(x))
            .WithMessage("TimeFilter must be either ongoing or upcoming.");
    }
}