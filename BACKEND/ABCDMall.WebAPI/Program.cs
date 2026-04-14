using ABCDMall.Modules.Movies.Application;
using ABCDMall.Modules.Movies.Infrastructure;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Catalog;
using ABCDMall.Modules.Movies.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDevelopment", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddMoviesApplication();
builder.Services.AddMoviesInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("FrontendDevelopment");
app.UseStaticFiles();
app.UseAuthorization();
app.MapControllers();

await using (var scope = app.Services.CreateAsyncScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

    try
    {
        var catalogDbContext = scope.ServiceProvider.GetRequiredService<MoviesCatalogDbContext>();
        await catalogDbContext.Database.MigrateAsync();

        var bookingDbContext = scope.ServiceProvider.GetRequiredService<MoviesBookingDbContext>();
        await bookingDbContext.Database.MigrateAsync();
        await MoviesPromotionSeed.SeedAsync(bookingDbContext);
    }
    catch (Exception ex)
    {
        // Không chặn app khởi động nếu local DB chưa cấu hình xong.
        // Điều này giúp team vẫn vào được Swagger/logs để kiểm tra wiring trước.
        logger.LogWarning(ex, "Movies database migration/seed was skipped because startup database initialization failed.");
    }
}

app.Run();
