using ABCDMall.Modules.Movies.Application.DTOs.Promotions;
using ABCDMall.Modules.Movies.Application.Services.Promotions;
using ABCDMall.Modules.Movies.Application.Services.Promotions.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ABCDMall.Modules.Movies.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddMoviesApplication(this IServiceCollection services)
    {
        // Day 3 chi dang ky nhung service can cho promotion + snack combo read flow.
        services.AddScoped<IValidator<EvaluatePromotionRequestDto>, EvaluatePromotionRequestDtoValidator>();
        services.AddScoped<IPromotionQueryService, PromotionQueryService>();
        services.AddScoped<IPromotionEvaluationService, PromotionEvaluationService>();
        services.AddScoped<ISnackComboQueryService, SnackComboQueryService>();

        return services;
    }
}
