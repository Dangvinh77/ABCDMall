using ABCDMall.Modules.Movies.Application.DTOs.Payments;
using FluentValidation;

namespace ABCDMall.Modules.Movies.Application.Services.Payments.Validators;

public sealed class CreateStripeCheckoutSessionRequestDtoValidator : AbstractValidator<CreateStripeCheckoutSessionRequestDto>
{
    public CreateStripeCheckoutSessionRequestDtoValidator()
    {
        RuleFor(x => x.BookingId)
            .NotEmpty();
    }
}
