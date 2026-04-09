using ABCDMall.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Register MongoDB context
builder.Services.AddSingleton<MongoDbContext>();
// Load MongoDB settings
builder.Services.Configure<MongoDbSetting>(
    builder.Configuration.GetSection("ConnectDB"));
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
