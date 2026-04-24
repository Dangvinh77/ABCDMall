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
using ABCDMall.Modules.Events.Application;
using ABCDMall.Modules.Events.Infrastructure;
using ABCDMall.Modules.Events.Infrastructure.Persistence.Events;
using ABCDMall.Modules.Events.Infrastructure.Seed;
using ABCDMall.Modules.UtilityMap.Application;
using ABCDMall.Modules.UtilityMap.Infrastructure;
using ABCDMall.Modules.UtilityMap.Infrastructure.Persistence.UtilityMap;
using ABCDMall.Modules.UtilityMap.Infrastructure.Seed;
using ABCDMall.WebAPI.Services.Chatbot;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

var envFilePath = Path.Combine(builder.Environment.ContentRootPath, ".env");
if (File.Exists(envFilePath))
{
    Env.Load(envFilePath);
}

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

builder.Services.AddHttpClient("Gemini", client =>
{
    client.Timeout = TimeSpan.FromMinutes(2);
});
builder.Services.AddSingleton<IGeminiMallAssistantClient, GeminiMallAssistantClient>();
builder.Services.AddScoped<IMallRagContextProvider, MallRagContextProvider>();
builder.Services.AddScoped<IChatbotAskService, ChatbotAskService>();

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

static string GetResetFlagName(string taskName)
    => $"Reset{taskName}Database";

static StartupDbTask CreateStartupDbTask(
    string name,
    DbContext context,
    bool shouldReset,
    bool shouldSeed,
    Func<CancellationToken, Task> seedAsync)
    => new(
        Name: name,
        Context: context,
        ShouldReset: shouldReset,
        ShouldSeed: shouldSeed,
        MigrateAsync: cancellationToken => context.Database.MigrateAsync(cancellationToken),
        SeedAsync: seedAsync);

static async Task ResetDatabasesOnceAsync(
    IReadOnlyList<StartupDbTask> tasks,
    ILogger logger,
    CancellationToken cancellationToken = default)
{
    var groupedTasks = tasks
        .Where(task => task.ShouldReset)
        .GroupBy(task => task.Context.Database.GetConnectionString(), StringComparer.OrdinalIgnoreCase);

    foreach (var group in groupedTasks)
    {
        var connectionString = group.Key;
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            logger.LogWarning(
                "Skipping database reset for contexts {Contexts} because the connection string could not be resolved.",
                string.Join(", ", group.Select(task => task.Name)));
            continue;
        }

        var representativeTask = group.First();
        var triggeredBy = string.Join(", ", group.Select(task => GetResetFlagName(task.Name)));
        var groupedNames = string.Join(", ", group.Select(task => task.Name));

        logger.LogWarning(
            "Reset requested by {Flags}. Deleting shared database once for contexts: {Contexts}.",
            triggeredBy,
            groupedNames);

        await representativeTask.Context.Database.EnsureDeletedAsync(cancellationToken);
    }
}

static async Task MigrateAndSeedAsync(
    StartupDbTask task,
    ILogger logger,
    CancellationToken cancellationToken = default)
{
    try
    {
        logger.LogInformation("Starting {Name} database initialization.", task.Name);

        await task.MigrateAsync(cancellationToken);

        if (task.ShouldSeed)
        {
            await task.SeedAsync(cancellationToken);
        }

        logger.LogInformation("{Name} database initialization completed.", task.Name);
    }
    catch (Exception ex)
    {
        logger.LogWarning(
            ex,
            "{Name} database migration/seed was skipped because startup database initialization failed.",
            task.Name);
    }
}

await using (var scope = app.Services.CreateAsyncScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    var catalogDbContext = scope.ServiceProvider.GetRequiredService<MoviesCatalogDbContext>();
    var bookingDbContext = scope.ServiceProvider.GetRequiredService<MoviesBookingDbContext>();
    var foodCourtDbContext = scope.ServiceProvider.GetRequiredService<FoodCourtDbContext>();
    var shopsDbContext = scope.ServiceProvider.GetRequiredService<ShopsDbContext>();
    var eventsDbContext = scope.ServiceProvider.GetRequiredService<EventsDbContext>();
    var usersDbContext = scope.ServiceProvider.GetRequiredService<MallDbContext>();
    var utilityMapDbContext = scope.ServiceProvider.GetRequiredService<UtilityMapDbContext>();

    var startupTasks = new List<StartupDbTask>
    {
        CreateStartupDbTask(
            name: "MoviesCatalog",
            context: catalogDbContext,
            shouldReset: resetMoviesDatabase,
            shouldSeed: seedMoviesData,
            seedAsync: cancellationToken => FrontendMoviesSeed.SeedCatalogAsync(catalogDbContext)),
        CreateStartupDbTask(
            name: "MoviesBooking",
            context: bookingDbContext,
            shouldReset: resetMoviesDatabase,
            shouldSeed: seedMoviesData,
            seedAsync: cancellationToken => FrontendMoviesSeed.SeedBookingAsync(bookingDbContext)),
        CreateStartupDbTask(
            name: "FoodCourt",
            context: foodCourtDbContext,
            shouldReset: resetFoodCourtDatabase,
            shouldSeed: seedFoodCourtData,
            seedAsync: cancellationToken => FrontendFoodCourtSeed.SeedAsync(foodCourtDbContext)),
        CreateStartupDbTask(
            name: "Shops",
            context: shopsDbContext,
            shouldReset: resetShopsDatabase,
            shouldSeed: seedShopsData,
            seedAsync: cancellationToken => FrontendShopsSeed.SeedAsync(shopsDbContext)),
        CreateStartupDbTask(
            name: "Events",
            context: eventsDbContext,
            shouldReset: resetEventsDatabase,
            shouldSeed: seedEventsData,
            seedAsync: cancellationToken => FrontendEventsSeed.SeedAsync(eventsDbContext)),
        CreateStartupDbTask(
            name: "Users",
            context: usersDbContext,
            shouldReset: resetUsersDatabase,
            shouldSeed: seedUsersData,
            seedAsync: cancellationToken => FrontendUsersSeed.SeedAsync(usersDbContext, cancellationToken)),
        CreateStartupDbTask(
            name: "UtilityMap",
            context: utilityMapDbContext,
            shouldReset: resetUtilityMapDatabase,
            shouldSeed: seedUtilityMapData,
            seedAsync: cancellationToken => FrontendUtilityMapSeed.SeedAsync(utilityMapDbContext))
    };

    try
    {
        await ResetDatabasesOnceAsync(startupTasks, logger);
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Shared database reset failed during startup initialization.");
    }

    foreach (var task in startupTasks)
    {
        await MigrateAndSeedAsync(task, logger);
    }
}

app.Run();

sealed record StartupDbTask(
    string Name,
    DbContext Context,
    bool ShouldReset,
    bool ShouldSeed,
    Func<CancellationToken, Task> MigrateAsync,
    Func<CancellationToken, Task> SeedAsync);
