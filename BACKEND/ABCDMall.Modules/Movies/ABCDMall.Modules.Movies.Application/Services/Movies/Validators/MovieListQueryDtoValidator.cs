using ABCDMall.Modules.Movies.Application.DTOs.Movies;
using ABCDMall.Modules.Movies.Domain.Enums;
using FluentValidation;

namespace ABCDMall.Modules.Movies.Application.Services.Movies.Validators;

public sealed class MovieListQueryDtoValidator : AbstractValidator<MovieListQueryDto>
{
    public MovieListQueryDtoValidator()
    {
        RuleFor(x => x.Status)
            .Must(BeValidMovieStatus)
            .When(x => !string.IsNullOrWhiteSpace(x.Status))
            .WithMessage($"status must be one of: {string.Join(", ", Enum.GetNames<MovieStatus>())}.");
    }

    private static bool BeValidMovieStatus(string? status)
    {
        return Enum.TryParse<MovieStatus>(status, true, out _);
    }
}
