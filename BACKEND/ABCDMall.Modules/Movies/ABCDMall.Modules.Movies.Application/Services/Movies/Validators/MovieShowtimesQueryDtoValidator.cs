using ABCDMall.Modules.Movies.Application.DTOs.Movies;
using FluentValidation;

namespace ABCDMall.Modules.Movies.Application.Services.Movies.Validators;

public sealed class MovieShowtimesQueryDtoValidator : AbstractValidator<MovieShowtimesQueryDto>
{
    public MovieShowtimesQueryDtoValidator()
    {
        RuleFor(x => x.BusinessDate)
            .Must(date => !date.HasValue || date.Value != default)
            .WithMessage("businessDate is invalid.");
    }
}
