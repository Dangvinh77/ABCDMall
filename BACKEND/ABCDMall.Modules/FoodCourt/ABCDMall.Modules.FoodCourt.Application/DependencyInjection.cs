using ABCDMall.Modules.FoodCourt.Application.DTOs.Foods;
using ABCDMall.Modules.FoodCourt.Application.Mappings;
using ABCDMall.Modules.FoodCourt.Application.Services.Foods;
using ABCDMall.Modules.FoodCourt.Application.Services.Foods.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ABCDMall.Modules.FoodCourt.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddFoodCourtApplication(this IServiceCollection services, string? autoMapperLicenseKey)
    {
        services.AddAutoMapper(cfg =>
        {
            cfg.LicenseKey = autoMapperLicenseKey;
        }, typeof(FoodProfile));

        services.AddScoped<IValidator<FoodListQueryDto>, FoodListQueryDtoValidator>();
        services.AddScoped<IValidator<CreateFoodRequestDto>, CreateFoodRequestDtoValidator>();
        services.AddScoped<IValidator<UpdateFoodRequestDto>, UpdateFoodRequestDtoValidator>();

        services.AddScoped<IFoodQueryService, FoodQueryService>();
        services.AddScoped<IFoodCommandService, FoodCommandService>();
        return services;
    }
}
