using ABCDMall.Modules.Movies.Application.DTOs.Bookings;
using FluentValidation;

namespace ABCDMall.Modules.Movies.Application.Services.Bookings.Validators;

public sealed class BookingQuoteRequestDtoValidator : AbstractValidator<BookingQuoteRequestDto>
{
    public BookingQuoteRequestDtoValidator()
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
