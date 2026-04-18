using ABCDMall.Modules.UtilityMap.Application.DTOs.Maps;
using ABCDMall.Modules.UtilityMap.Application.Mappings;
using ABCDMall.Modules.UtilityMap.Application.Services.Maps;
using ABCDMall.Modules.UtilityMap.Application.Services.Maps.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ABCDMall.Modules.UtilityMap.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddUtilityMapApplication(this IServiceCollection services, string? autoMapperLicenseKey)
    {
        services.AddAutoMapper(cfg => { cfg.LicenseKey = autoMapperLicenseKey; }, typeof(MapProfile));

        services.AddScoped<IValidator<CreateFloorPlanRequestDto>, CreateFloorPlanRequestDtoValidator>();
        services.AddScoped<IValidator<CreateMapLocationRequestDto>, CreateMapLocationRequestDtoValidator>();

        services.AddScoped<IMapQueryService, MapQueryService>();
        services.AddScoped<IMapCommandService, MapCommandService>();
        
        return services;
    }
}
