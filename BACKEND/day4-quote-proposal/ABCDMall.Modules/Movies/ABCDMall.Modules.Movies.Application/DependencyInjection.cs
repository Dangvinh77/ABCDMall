using ABCDMall.Modules.Movies.Application.DTOs.Bookings;
using ABCDMall.Modules.Movies.Application.DTOs.Promotions;
using ABCDMall.Modules.Movies.Application.Services.Bookings;
using ABCDMall.Modules.Movies.Application.Services.Bookings.Validators;
using ABCDMall.Modules.Movies.Application.Services.Promotions;
using ABCDMall.Modules.Movies.Application.Services.Promotions.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ABCDMall.Modules.Movies.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddMoviesApplication(this IServiceCollection services)
    {
        services.AddScoped<IValidator<EvaluatePromotionRequestDto>, EvaluatePromotionRequestDtoValidator>();
        services.AddScoped<IValidator<BookingQuoteRequestDto>, BookingQuoteRequestDtoValidator>();

        services.AddScoped<IPromotionQueryService, PromotionQueryService>();
        services.AddScoped<IPromotionEvaluationService, PromotionEvaluationService>();
        services.AddScoped<ISnackComboQueryService, SnackComboQueryService>();
        services.AddScoped<IBookingQuoteService, BookingQuoteService>();

        return services;
    }
}
