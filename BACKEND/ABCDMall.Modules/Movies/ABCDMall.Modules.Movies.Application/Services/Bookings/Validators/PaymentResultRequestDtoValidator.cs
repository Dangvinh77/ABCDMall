using ABCDMall.Modules.Movies.Application.DTOs.Payments;
using FluentValidation;

namespace ABCDMall.Modules.Movies.Application.Services.Bookings.Validators;

public sealed class PaymentResultRequestDtoValidator : AbstractValidator<PaymentResultRequestDto>
{
    public PaymentResultRequestDtoValidator()
    {
        RuleFor(x => x.Provider)
            .NotEmpty()
            .MaximumLength(30);

        RuleFor(x => x.ProviderTransactionId)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Status)
            .NotEmpty()
            .MaximumLength(30);

        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .MaximumLength(10);

        RuleFor(x => x.FailureReason)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.FailureReason));
    }
}
