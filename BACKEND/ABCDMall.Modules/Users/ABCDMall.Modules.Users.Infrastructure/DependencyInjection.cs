using ABCDMall.Modules.Users.Application.Services.Auth;
using ABCDMall.Modules.Users.Application.Services.RentalAreas;
using ABCDMall.Modules.Users.Application.Services.ShopInfos;
using ABCDMall.Modules.Users.Application.Services;
using ABCDMall.Modules.Users.Infrastructure.Services;
using ABCDMall.Modules.Users.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ABCDMall.Modules.Users.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddUsersInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ABCDMallConnection")
            ?? configuration.GetConnectionString("ABCDMallUsersDBConnection")
            ?? throw new InvalidOperationException("Connection string 'ABCDMallConnection' or 'ABCDMallUsersDBConnection' was not found.");

        services.AddDbContext<MallDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        services.AddSingleton<JwtService>();
        services.AddSingleton<EmailService>();
        services.AddScoped<IUserReadRepository, UserReadRepository>();
        services.AddScoped<IUserCommandRepository, UserCommandRepository>();
        services.AddScoped<IRentalAreaReadRepository, RentalAreaReadRepository>();
        services.AddScoped<IRentalAreaCommandRepository, RentalAreaCommandRepository>();
        services.AddScoped<IShopMonthlyBillReadRepository, ShopMonthlyBillReadRepository>();
        services.AddScoped<IEmailNotificationService, EmailNotificationService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<ITokenService, TokenService>();

        var jwtKey = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Configuration value 'Jwt:Key' was not found.");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };
        });

        services.AddAuthorization();

        return services;
    }
}
