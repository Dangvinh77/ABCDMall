using ABCDMall.Modules.FoodCourt.Application;
using ABCDMall.Modules.FoodCourt.Infrastructure;
using ABCDMall.Modules.FoodCourt.Infrastructure.Persistence.FoodCourt;
using ABCDMall.Modules.FoodCourt.Infrastructure.Seed;
using ABCDMall.Modules.Movies.Application;
using ABCDMall.Modules.Movies.Infrastructure;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Catalog;
using ABCDMall.Modules.Movies.Infrastructure.Seed;
using ABCDMall.Modules.Shops.Application;
using ABCDMall.Modules.Shops.Infrastructure;
using ABCDMall.Modules.Shops.Infrastructure.Persistence.Shops;
using ABCDMall.Modules.Shops.Infrastructure.Seed;
using ABCDMall.Modules.Users.Application;
using ABCDMall.Modules.Users.Infrastructure;
using ABCDMall.Modules.Users.Infrastructure.Seed;
using ABCDMall.Modules.UtilityMap.Application;
using ABCDMall.Modules.UtilityMap.Infrastructure;
using ABCDMall.Modules.UtilityMap.Infrastructure.Persistence.UtilityMap;
using ABCDMall.Modules.UtilityMap.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ABCDMall.Modules.Events.Application;
using ABCDMall.Modules.Events.Infrastructure;
using ABCDMall.Modules.Events.Infrastructure.Persistence.Events;
using ABCDMall.Modules.Events.Infrastructure.Seed;
 

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter token using format: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDevelopment", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var autoMapperLicenseKey = builder.Configuration["AutoMapper:LicenseKey"];
var resetMoviesDatabase = builder.Configuration.GetValue<bool>("DatabaseStartup:ResetMoviesDatabase");
var seedMoviesData = builder.Configuration.GetValue("DatabaseStartup:SeedMoviesData", true);
var resetUsersDatabase = builder.Configuration.GetValue<bool>("DatabaseStartup:ResetUsersDatabase");
var seedUsersData = builder.Configuration.GetValue("DatabaseStartup:SeedUsersData", true);
var resetFoodCourtDatabase = builder.Configuration.GetValue<bool>("DatabaseStartup:ResetFoodCourtDatabase");
var seedFoodCourtData = builder.Configuration.GetValue("DatabaseStartup:SeedFoodCourtData", true);
var resetShopsDatabase = builder.Configuration.GetValue<bool>("DatabaseStartup:ResetShopsDatabase");
var seedShopsData = builder.Configuration.GetValue("DatabaseStartup:SeedShopsData", true);
var resetEventsDatabase = builder.Configuration.GetValue<bool>("DatabaseStartup:ResetEventsDatabase");
var seedEventsData = builder.Configuration.GetValue("DatabaseStartup:SeedEventsData", true);
var resetUtilityMapDatabase = builder.Configuration.GetValue<bool>("DatabaseStartup:ResetUtilityMapDatabase");
var seedUtilityMapData = builder.Configuration.GetValue("DatabaseStartup:SeedUtilityMapData", true);

builder.Services.AddMoviesApplication(autoMapperLicenseKey);
builder.Services.AddMoviesInfrastructure(builder.Configuration);
builder.Services.AddFoodCourtApplication(autoMapperLicenseKey);
builder.Services.AddFoodCourtInfrastructure(builder.Configuration);
builder.Services.AddShopsApplication();
builder.Services.AddShopsInfrastructure(builder.Configuration);
builder.Services.AddEventsApplication(autoMapperLicenseKey);
builder.Services.AddEventsInfrastructure(builder.Configuration);
builder.Services.AddUsersApplication(autoMapperLicenseKey);
builder.Services.AddUsersInfrastructure(builder.Configuration);
builder.Services.AddUtilityMapApplication(autoMapperLicenseKey);
builder.Services.AddUtilityMapInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("FrontendDevelopment");
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await using (var scope = app.Services.CreateAsyncScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

    try
    {
        logger.LogInformation("Starting Movies catalog database initialization.");
        var catalogDbContext = scope.ServiceProvider.GetRequiredService<MoviesCatalogDbContext>();
        if (resetMoviesDatabase)
        {
            logger.LogWarning("ResetMoviesDatabase is enabled. Deleting Movies catalog database.");
            await catalogDbContext.Database.EnsureDeletedAsync();
        }

        await catalogDbContext.Database.MigrateAsync();
        if (seedMoviesData)
        {
            await FrontendMoviesSeed.SeedCatalogAsync(catalogDbContext);
        }

        logger.LogInformation("Movies catalog database initialization completed.");

        logger.LogInformation("Starting Movies booking database initialization.");
        var bookingDbContext = scope.ServiceProvider.GetRequiredService<MoviesBookingDbContext>();
        if (resetMoviesDatabase)
        {
            logger.LogWarning("ResetMoviesDatabase is enabled. Deleting Movies booking database.");
            await bookingDbContext.Database.EnsureDeletedAsync();
        }

        await bookingDbContext.Database.MigrateAsync();
        if (seedMoviesData)
        {
            await FrontendMoviesSeed.SeedBookingAsync(bookingDbContext);
        }

        logger.LogInformation("Movies booking database initialization completed.");
    }
    catch (Exception ex)
    {
        // Không chặn app khởi động nếu local DB chưa cấu hình xong.
        // Điều này giúp team vẫn vào được Swagger/logs để kiểm tra wiring trước.
        logger.LogWarning(ex, "Movies database migration/seed was skipped because startup database initialization failed.");
    }

    try
    {
        logger.LogInformation("Starting FoodCourt database initialization.");
        var foodCourtDbContext = scope.ServiceProvider.GetRequiredService<FoodCourtDbContext>();
        if (resetFoodCourtDatabase)
        {
            logger.LogWarning("ResetFoodCourtDatabase is enabled. Deleting FoodCourt database.");
            await foodCourtDbContext.Database.EnsureDeletedAsync();
        }

        await foodCourtDbContext.Database.MigrateAsync();
        if (seedFoodCourtData)
        {
            await FrontendFoodCourtSeed.SeedAsync(foodCourtDbContext);
        }

        logger.LogInformation("FoodCourt database initialization completed.");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "FoodCourt database migration/seed was skipped because startup database initialization failed.");
    }

    try
    {
        logger.LogInformation("Starting Shops database initialization.");
        var shopsDbContext = scope.ServiceProvider.GetRequiredService<ShopsDbContext>();
        if (resetShopsDatabase)
        {
            logger.LogWarning("ResetShopsDatabase is enabled. Deleting Shops database.");
            await shopsDbContext.Database.EnsureDeletedAsync();
        }

        await shopsDbContext.Database.MigrateAsync();
        if (seedShopsData)
        {
            await FrontendShopsSeed.SeedAsync(shopsDbContext);
        }

        logger.LogInformation("Shops database initialization completed.");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Shops database migration/seed was skipped because startup database initialization failed.");
    }

    try
    {
        logger.LogInformation("Starting Events database initialization.");
        var eventsDbContext = scope.ServiceProvider.GetRequiredService<EventsDbContext>();
        if (resetEventsDatabase)
        {
            logger.LogWarning("ResetEventsDatabase is enabled. Deleting Events database.");
            await eventsDbContext.Database.EnsureDeletedAsync();
        }

        await eventsDbContext.Database.MigrateAsync();
        if (seedEventsData)
        {
            await FrontendEventsSeed.SeedAsync(eventsDbContext);
        }

        logger.LogInformation("Events database initialization completed.");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Events database migration/seed was skipped because startup database initialization failed.");
    }

    try
    {
        logger.LogInformation("Starting Users database initialization.");
        var usersDbContext = scope.ServiceProvider.GetRequiredService<MallDbContext>();
        if (resetUsersDatabase)
        {
            logger.LogWarning("ResetUsersDatabase is enabled. Deleting Users database.");
            await usersDbContext.Database.EnsureDeletedAsync();
        }

        await usersDbContext.Database.MigrateAsync();
        if (seedUsersData)
        {
            await FrontendUsersSeed.SeedAsync(usersDbContext);
        }

        logger.LogInformation("Users database initialization completed.");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Users database migration/seed was skipped because startup database initialization failed.");
    }

    try
    {
        logger.LogInformation("Starting UtilityMap database initialization.");
        var mapDbContext = scope.ServiceProvider.GetRequiredService<UtilityMapDbContext>();
        if (resetUtilityMapDatabase)
        {
            logger.LogWarning("ResetUtilityMapDatabase is enabled. Deleting UtilityMap database.");
            await mapDbContext.Database.EnsureDeletedAsync();
        }

        await mapDbContext.Database.MigrateAsync();
        if (seedUtilityMapData)
        {
            await FrontendUtilityMapSeed.SeedAsync(mapDbContext);
        }

        logger.LogInformation("UtilityMap database initialization completed.");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "UtilityMap database migration/seed was skipped because startup database initialization failed.");
    }
}

app.Run();
