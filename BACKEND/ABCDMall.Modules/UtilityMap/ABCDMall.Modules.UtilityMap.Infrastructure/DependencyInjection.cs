using Microsoft.Extensions.DependencyInjection;
using ABCDMall.Modules.UtilityMap.Domain.Interfaces;
using ABCDMall.Modules.UtilityMap.Infrastructure.Repositories;
using ABCDMall.Modules.UtilityMap.Application.Services; 

namespace ABCDMall.Modules.UtilityMap.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddUtilityMapModule(this IServiceCollection services)
    {
        // 1. Đăng ký Repository (Đã làm)
        services.AddScoped<IMapRepository, MapRepository>();
        
        // 2. Đăng ký Service (MỚI THÊM)
        services.AddScoped<IMapService, MapService>();

        return services;
    }
}