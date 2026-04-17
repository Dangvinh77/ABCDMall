using ABCDMall.Modules.Movies.Application.Contracts;
using ABCDMall.Modules.Movies.Application.DTOs.Showtimes;
using FluentValidation;

namespace ABCDMall.Modules.Movies.Application.Services.Showtimes.Validators;

public sealed class ShowtimeListQueryDtoValidator : AbstractValidator<ShowtimeListQueryDto>
{
    public ShowtimeListQueryDtoValidator()
    {
        RuleFor(x => x.BusinessDate)
            .Must(date => !date.HasValue || date.Value != default)
            .WithMessage("businessDate is invalid.");

        RuleFor(x => x.HallType)
            .Must(BeValidHallType)
            .When(x => !string.IsNullOrWhiteSpace(x.HallType))
            .WithMessage("hallType must be one of: 2D, 3D, IMAX, 4DX.");

        RuleFor(x => x.Language)
            .Must(BeValidLanguageType)
            .When(x => !string.IsNullOrWhiteSpace(x.Language))
            .WithMessage("language must be one of: Sub, Dub.");
    }

    private static bool BeValidHallType(string? hallType)
    {
        return MoviesContractValueMapper.TryParseHallType(hallType, out _);
    }

    private static bool BeValidLanguageType(string? language)
    {
        return MoviesContractValueMapper.TryParseLanguageType(language, out _);
    }
}
