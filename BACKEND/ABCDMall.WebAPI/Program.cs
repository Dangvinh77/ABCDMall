using ABCDMall.Modules.Movies.Application;
using ABCDMall.Modules.Movies.Infrastructure;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Catalog;
using ABCDMall.Modules.Movies.Infrastructure.Seed;
using ABCDMall.Modules.Users.Application;
using ABCDMall.Modules.Users.Infrastructure;
using ABCDMall.Modules.Users.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

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

builder.Services.AddMoviesApplication(autoMapperLicenseKey);
builder.Services.AddMoviesInfrastructure(builder.Configuration);
builder.Services.AddUsersApplication(autoMapperLicenseKey);
builder.Services.AddUsersInfrastructure(builder.Configuration);

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
}

app.Run();
