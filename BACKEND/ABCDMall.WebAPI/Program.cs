using ABCDMall.Shared.Persistence;
using ABCDMall.Modules.FoodCourt.Infrastructure;
using ABCDMall.Modules.UtilityMap.Infrastructure;
using Microsoft.EntityFrameworkCore;
using ABCDMall.Modules.Shops.Infrastructure.Data;
using ABCDMall.Modules.Shops.Application.Interfaces;
using ABCDMall.Modules.Shops.Application.Services;
using ABCDMall.Modules.Shops.Domain.Interfaces;
using ABCDMall.Modules.Shops.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Database
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectDB"));
});

// 2. Đăng ký các Module (Luật chơi của nhóm)
builder.Services.AddFoodCourtModule();
builder.Services.AddUtilityMapModule(); 

// 3. Đăng ký Service cho Shop
builder.Services.AddScoped<IShopRepository, ShopRepository>();
builder.Services.AddScoped<IShopService, ShopService>();

// 4. Cấu hình API & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 5. Cấu hình CORS (Mở cửa cho React FE)
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

// =========================================================
// PHẦN SEED DỮ LIỆU (THEO THỨ TỰ LOGIC)
// =========================================================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();       
    
    // 1. Seed Map trước (để lấy tọa độ và ShopName gốc)
    await ABCDMall.WebAPI.MapDataSeeder.SeedAsync(db); 
    
    // 2. Seed Shop thật (Dữ liệu 23 shop "hàng real")
    await ShopDataSeeder.SeedAsync(db);     
    
    // 3. Tự động lấp đầy (Clone data từ shop thật sang các shop trống trên bản đồ)
    // Lưu ý: Nhớ thêm phương thức này vào ShopDataSeeder như tôi hướng dẫn ở bài trước
    await ShopDataSeeder.AutoFillMissingShopsAsync(db); 
}

// =========================================================
// MIDDLEWARE PIPELINE
// =========================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles(); // Để load được ảnh từ wwwroot (Logo, Cover...)

app.UseRouting();

// QUAN TRỌNG: UseCors phải nằm sau UseRouting và trước UseAuthorization
app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers(); // <--- Đưa các API ra ngoài

app.Run();