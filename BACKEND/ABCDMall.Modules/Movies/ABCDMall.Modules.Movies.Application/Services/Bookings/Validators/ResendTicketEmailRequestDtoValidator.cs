using ABCDMall.Modules.Movies.Application.DTOs.Bookings;
using FluentValidation;

namespace ABCDMall.Modules.Movies.Application.Services.Bookings.Validators;

public sealed class ResendTicketEmailRequestDtoValidator : AbstractValidator<ResendTicketEmailRequestDto>
{
    public ResendTicketEmailRequestDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.BookingCode)
            .NotEmpty()
            .MaximumLength(64);
    }
}
