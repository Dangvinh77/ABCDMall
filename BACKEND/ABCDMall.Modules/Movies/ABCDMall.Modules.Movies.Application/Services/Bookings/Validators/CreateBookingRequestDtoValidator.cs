using ABCDMall.Modules.Movies.Application.DTOs.Bookings;
using FluentValidation;

namespace ABCDMall.Modules.Movies.Application.Services.Bookings.Validators;

public sealed class CreateBookingRequestDtoValidator : AbstractValidator<CreateBookingRequestDto>
{
    public CreateBookingRequestDtoValidator()
    {
        RuleFor(x => x.HoldIds)
            .NotEmpty()
            .Must(holdIds => holdIds.Distinct().Count() == holdIds.Count)
            .WithMessage("HoldIds must contain at least one unique hold id.");

        RuleFor(x => x.CustomerName)
            .NotEmpty()
            .WithMessage("Customer name is required.")
            .MaximumLength(200);

        RuleFor(x => x.CustomerEmail)
            .NotEmpty()
            .WithMessage("Customer email is required.")
            .EmailAddress()
            .WithMessage("Customer email is invalid.")
            .MaximumLength(200);

        RuleFor(x => x.CustomerPhoneNumber)
            .NotEmpty()
            .WithMessage("Customer phone number is required.")
            .MaximumLength(20);
    }
}
