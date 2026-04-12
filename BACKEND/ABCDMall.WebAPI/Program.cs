using ABCDMall.Modules.Movies.Infrastructure;
using ABCDMall.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Movies module infrastructure
builder.Services.AddMoviesInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseCors("AllowFrontend");
app.UseCors("AllowReact");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseAuthorization();
app.MapControllers();

app.Run();
