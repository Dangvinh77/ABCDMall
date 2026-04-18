using ABCDMall.Modules.Shops.Application.Services.Catalog;
using Microsoft.Extensions.DependencyInjection;

namespace ABCDMall.Modules.Shops.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddShopsApplication(this IServiceCollection services)
    {
        services.AddScoped<IShopCatalogQueryService, ShopCatalogQueryService>();
        return services;
    }
}
