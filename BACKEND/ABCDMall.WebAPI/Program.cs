using ABCDMall.Shared.MongoDB;
using ABCDMall.Modules.FoodCourt.Domain.Interfaces;
using ABCDMall.Modules.FoodCourt.Application.Services;
using ABCDMall.Modules.FoodCourt.Infrastructure.Repositories;
using MongoDB.Driver;
using ABCDMall.Modules.UtilityMap.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Register MongoDB context
builder.Services.AddSingleton<MongoContext>();

// Load MongoDB settings
builder.Services.Configure<MongoDbSetting>(
    builder.Configuration.GetSection("ConnectDB"));

// Food Court
builder.Services.AddScoped<IFoodRepository, FoodRepository>();
builder.Services.AddScoped<IFoodService, FoodService>();

builder.Services.AddUtilityMapModule();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

app.UseCors("AllowFrontend");
app.UseCors("AllowReact");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
