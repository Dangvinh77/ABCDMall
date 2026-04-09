## ⚙️ Thiết lập kết nối SQL Server(Local)

Tạo một file tên appsettings.**Development**.json (thêm Development) đặt cùng đường dẫn với appsettings.json

Thay thế value của **<server_name>** phù hợp với Servername của máy

```bash
{
  "ConnectionStrings": {
    "ConnectDB": "Server=<server_name>;Database=ABCDMall;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### 💡 Tip tìm Server name của máy

Vào **Terminal**, nhập

```bash
sc query type= service | findstr /I "SQL"
```

Kết quả trả về sẽ như sau:

```bash
SERVICE_NAME: MSSQL$HARORISQLSERVER
DISPLAY_NAME: SQL Server (HARORISQLSERVER)
SERVICE_NAME: SQLAgent$HARORISQLSERVER
DISPLAY_NAME: SQL Server Agent (HARORISQLSERVER)
SERVICE_NAME: SQLBrowser
DISPLAY_NAME: SQL Server Browser
SERVICE_NAME: SQLTELEMETRY$HARORISQLSERVER
DISPLAY_NAME: SQL Server CEIP service (HARORISQLSERVER)
SERVICE_NAME: SQLWriter
DISPLAY_NAME: SQL Server VSS Writer
```

#### Phần đi kèm prefix <code>$</code> (E.g. HARORISQLSERVER) chính là **{Server Name}**

```bash
<server_name> = .\\{Server Name}
```

## ⚙️ Gọi/Tạo bảng trong Modules

### 1. Tại dự án ABCD.Modules.Movies.Domain (Định nghĩa thực thể)

Ví dụ đường dẫn chứa Entity(Model) là:
**./ABCD.Modules.Movies.Domain/Entities/Movie.cs**

```bash
public class Movie {
    public ObjectId Id { get; set; }
    public string Title { get; set; }
    // ... các trường khác
}
```

### 2. Tại dự án ABCD.Shared

#### Đường dẫn chứa Entity(Model) là: **./ABCDMall.Shared/Persistence/AppDbContext.cs**

**Bổ dung Entity(Model) tương ứng tạo bên Domain:**

```bash
public DbSet<T> <Entity> {get;set;}
```

**Minh họa:**

```bash
namespace ABCD.Shared.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // --- Module Movies ---
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Showtime> Showtimes { get; set; }
        public DbSet<Seat> Seats { get; set; }

        // --- Module Shops ---
        public DbSet<Shop> Shops { get; set; }
        public DbSet<Product> Products { get; set; }

        // --- Module Feedbacks ---
        public DbSet<Feedback> Feedbacks { get; set; }

        // --- Module Users ---
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Mẹo: Nên chia cấu hình Fluent API theo module để file này gọn hơn
            // Ví dụ: modelBuilder.ApplyConfiguration(new MovieConfiguration());
        }
    }
}
```

### 3. Tóm tắt quy tắc đứng ở đâu làm gì:

1. **Muốn định nghĩa các cột/trường (Entity)**: Đứng ở dự án Module.Domain.
2. **Muốn tương tác dữ liệu và thực thể (Migration/Interface/Repository)**: Đứng ở dự án Module.Infrastructure.
3. **Muốn xử lý logic hay tính toán**: Đứng ở dự án Module.Application.
4. **Muốn tạo API Endpoint cho Front-end gọi**: Đứng ở dự án WebAPI.

## ⚙️ Đăng ký Dependency Injection (DI)

Vì bạn có rất nhiều Project, nếu viết hết vào Program.cs của WebAPI sẽ rất rối.

**Cách làm chuẩn:** Trong mỗi Project Infrastructure của từng Module, bạn tạo một file tên là **DependencyInjection.cs**.

Ví dụ trong Movies.Infrastructure:

```bash
public static class DependencyInjection {
    public static IServiceCollection AddMoviesModule(this IServiceCollection services) {
        services.AddScoped<IMovieRepository, MovieRepository>();
        services.AddScoped<IMovieService, MovieService>();
        return services;
    }
}
```

Sau đó trong **Program.cs** của WebAPI, bạn chỉ cần gọi:

```bash
builder.Services.AddMoviesModule();
builder.Services.AddShopsModule();
```

... tương tự cho các module khác
