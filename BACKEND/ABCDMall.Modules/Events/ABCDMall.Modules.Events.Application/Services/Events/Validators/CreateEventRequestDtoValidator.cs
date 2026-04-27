using ABCDMall.Modules.Events.Application.DTOs.Events;
using FluentValidation;

namespace ABCDMall.Modules.Events.Application.Services.Events.Validators;

public sealed class CreateEventRequestDtoValidator : AbstractValidator<CreateEventRequestDto>
{
    public CreateEventRequestDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.ImageUrl).MaximumLength(1000);
        RuleFor(x => x.StartDateTime).NotEmpty();
        RuleFor(x => x.EndDateTime)
            .GreaterThan(x => x.StartDateTime)
            .WithMessage("Ngày kết thúc phải sau ngày bắt đầu");
        RuleFor(x => x.LocationType).InclusiveBetween(1, 5);
        RuleFor(x => x.GiftDescription).MaximumLength(500);
        RuleFor(x => x.GiftDescription)
            .NotEmpty()
            .When(x => x.HasGiftRegistration)
            .WithMessage("Mô tả quà tặng là bắt buộc khi bật đăng ký quà tặng.");
    }
}
