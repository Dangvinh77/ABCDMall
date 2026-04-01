## ⚙️ Thiết lập kết nối MongoDB

Tạo một file tên appsettings.**Development**.json (thêm Development) đặt cùng đường dẫn với appsettings.json

copy

```bash
{
  "ConnectDB": {
    "ConnectionString": "mongodb+srv://<username>:<password>@<cluster-url>/<database>?retryWrites=true&w=majority",
    "DatabaseName": "ABCDMall"
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

Thay thế value của key **"ConnectionString"** được ghim trong **Zalo**

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

### 2. Tại dự án ABCD.Modules.Movies.Infrastructure (Nơi "Bảng" Movies thực sự tồn tại)

Ví dụ đường dẫn chứa Repository/Services/Interface là:
**ABCD.Modules.Movies.Infrastructure/Repository/MovieRepository.cs**

```bash
public class MovieRepository : IMovieRepository {
    private readonly IMongoCollection<Movie> _movies;

    public MovieRepository(MongoDbContext context) {
        // Đây chính là nơi bạn xác định Collection "Movies" cho module này
        _movies = context.GetCollection<Movie>("Movies");
    }

    public async Task<List<Movie>> GetAllAsync() {
        return await _movies.Find(_ => true).ToListAsync();
    }
}
```

### 3. Tóm tắt quy tắc đứng ở đâu làm gì:

1. **Muốn định nghĩa các cột/trường (Entity)**: Đứng ở dự án Module.Domain.
2. **Muốn tạo bảng/truy vấn dữ liệu (Collection/Repository)**: Đứng ở dự án Module.Infrastructure.
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
