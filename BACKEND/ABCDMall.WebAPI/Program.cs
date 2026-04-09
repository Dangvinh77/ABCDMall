using ABCDMall.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using ABCDMall.Modules.FoodCourt.Domain.Interfaces;
using ABCDMall.Modules.FoodCourt.Application.Services;
using ABCDMall.Modules.FoodCourt.Infrastructure.Repositories;
using ABCDMall.Modules.UtilityMap.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Database (SQL Server thay vì MongoDB)
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectDB"));
});

// 2. Register Services & Repositories (Đảm bảo các Repo này đã được code lại để dùng DbContext)
builder.Services.AddScoped<IFoodRepository, FoodRepository>();
builder.Services.AddScoped<IFoodService, FoodService>();

// 3. Register Modules khác
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

// 6. Cấu hình Middleware
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