using ABCDMall.Shared.Persistence;
using ABCDMall.Modules.FoodCourt.Infrastructure;
using ABCDMall.Modules.UtilityMap.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectDB"));
});

builder.Services.AddFoodCourtModule();
builder.Services.AddUtilityMapModule();

// 4. Cấu hình Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 5. Cấu hình CORS (Giữ từ nhánh fe-shop)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();       
    await ABCDMall.WebAPI.MapDataSeeder.SeedAsync(db);      
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

// Sử dụng đúng Policy CORS đã khai báo ở trên
app.UseCors("AllowFrontend");

app.UseAuthorization();
app.MapControllers();

app.Run();