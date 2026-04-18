using ABCDMall.Modules.Users.Application.Mappings;
using ABCDMall.Modules.Users.Application.Services.Auth;
using ABCDMall.Modules.Users.Application.Services.RentalAreas;
using ABCDMall.Modules.Users.Application.Services.ShopInfos;
using Microsoft.Extensions.DependencyInjection;

namespace ABCDMall.Modules.Users.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddUsersApplication(this IServiceCollection services, string? autoMapperLicenseKey)
    {
        services.AddAutoMapper(cfg =>
        {
            cfg.LicenseKey = autoMapperLicenseKey;
        }, typeof(UsersProfile));

        services.AddScoped<IUserQueryService, UserQueryService>();
        services.AddScoped<IUserCommandService, UserCommandService>();
        services.AddScoped<IRentalAreaQueryService, RentalAreaQueryService>();
        services.AddScoped<IRentalAreaCommandService, RentalAreaCommandService>();
        services.AddScoped<IShopInfoQueryService, ShopInfoQueryService>();

        return services;
    }
}
