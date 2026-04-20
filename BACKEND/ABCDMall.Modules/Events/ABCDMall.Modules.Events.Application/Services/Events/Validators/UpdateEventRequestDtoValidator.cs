using ABCDMall.Modules.Events.Application.DTOs.Events;
using FluentValidation;

namespace ABCDMall.Modules.Events.Application.Services.Events.Validators;

public sealed class UpdateEventRequestDtoValidator : AbstractValidator<UpdateEventRequestDto>
{
    public UpdateEventRequestDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(300);

        RuleFor(x => x.Description)
            .MaximumLength(4000);

        RuleFor(x => x.CoverImageUrl)
            .MaximumLength(1000);

        RuleFor(x => x.Location)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.StartDate)
            .NotEmpty();

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .GreaterThan(x => x.StartDate)
            .WithMessage("EndDate phải sau StartDate.");

        RuleFor(x => x.EventType)
            .InclusiveBetween(1, 2)
            .WithMessage("EventType phải là 1 (MallEvent) hoặc 2 (BrandEvent).");

        RuleFor(x => x.ShopId)
            .NotEmpty()
            .When(x => x.EventType == 2)
            .WithMessage("ShopId là bắt buộc khi EventType là BrandEvent.");

        RuleFor(x => x.ShopName)
            .MaximumLength(300);
    }
}
