using ABCDMall.Modules.Movies.Application.DTOs.Promotions;
using ABCDMall.Modules.Movies.Application.Mappings;
using ABCDMall.Modules.Movies.Application.Services.Movies;
using ABCDMall.Modules.Movies.Application.Services.Promotions;
using ABCDMall.Modules.Movies.Application.Services.Promotions.Validators;
using ABCDMall.Modules.Movies.Application.Services.Showtimes;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ABCDMall.Modules.Movies.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddMoviesApplication(
        this IServiceCollection services,
        string? autoMapperLicenseKey)
    {
        services.AddAutoMapper(cfg =>
        {
            cfg.LicenseKey = autoMapperLicenseKey;
        }, typeof(MovieProfile), typeof(PromotionProfile));

        services.AddScoped<IValidator<EvaluatePromotionRequestDto>, EvaluatePromotionRequestDtoValidator>();
        services.AddScoped<IMovieQueryService, MovieQueryService>();
        services.AddScoped<IPromotionQueryService, PromotionQueryService>();
        services.AddScoped<IPromotionEvaluationService, PromotionEvaluationService>();
        services.AddScoped<ISeatMapQueryService, SeatMapQueryService>();
        services.AddScoped<ISnackComboQueryService, SnackComboQueryService>();
        services.AddScoped<IShowtimeQueryService, ShowtimeQueryService>();

        return services;
    }
}
