using Microsoft.Extensions.DependencyInjection;
using ABCDMall.Modules.FoodCourt.Domain.Interfaces;
using ABCDMall.Modules.FoodCourt.Application.Interfaces;
using ABCDMall.Modules.FoodCourt.Application.Services;
using ABCDMall.Modules.FoodCourt.Infrastructure.Repositories;

namespace ABCDMall.Modules.FoodCourt.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFoodCourtModule(this IServiceCollection services)
    {
        services.AddScoped<IFoodRepository, FoodRepository>();
        services.AddScoped<IFoodService, FoodService>();
        return services;
    }
}