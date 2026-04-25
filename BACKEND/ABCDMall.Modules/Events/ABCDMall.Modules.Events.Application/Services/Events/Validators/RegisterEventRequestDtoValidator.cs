using ABCDMall.Modules.Events.Application.DTOs.Events;
using FluentValidation;

namespace ABCDMall.Modules.Events.Application.Services.Events.Validators;

public sealed class RegisterEventRequestDtoValidator : AbstractValidator<RegisterEventRequestDto>
{
    public RegisterEventRequestDtoValidator()
    {
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.CustomerEmail).NotEmpty().EmailAddress().MaximumLength(160);
        RuleFor(x => x.CustomerPhone).NotEmpty().MaximumLength(30);
    }
}
