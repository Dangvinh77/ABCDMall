using ABCDMall.Modules.Movies.Application.DTOs.Bookings;
using FluentValidation;

namespace ABCDMall.Modules.Movies.Application.Services.Bookings.Validators;

public sealed class CreateBookingHoldRequestDtoValidator : AbstractValidator<CreateBookingHoldRequestDto>
{
    public CreateBookingHoldRequestDtoValidator()
    {
        RuleFor(x => x.ShowtimeId)
            .NotEmpty()
            .WithMessage("ShowtimeId is required.");

        RuleFor(x => x.SeatInventoryIds)
            .NotEmpty()
            .WithMessage("At least one seat must be selected.");

        RuleFor(x => x.SeatInventoryIds)
            .Must(x => x.Distinct().Count() == x.Count)
            .WithMessage("SeatInventoryIds must not contain duplicates.");

        RuleFor(x => x.SessionId)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.SessionId));

        RuleForEach(x => x.SnackCombos)
            .ChildRules(combo =>
            {
                combo.RuleFor(x => x.ComboId)
                    .NotEmpty()
                    .WithMessage("ComboId is required.");

                combo.RuleFor(x => x.Quantity)
                    .GreaterThan(0)
                    .WithMessage("Combo quantity must be greater than zero.");
            });
    }
}
