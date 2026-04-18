## ⚙️ Thiết lập kết nối SQL Server(Local)

Tạo một file đổi tên appsettings.**Development**.json (thêm Development) bằng cách copy và paste **appsettings.json** ,đặt cùng đường dẫn với **appsettings.json**

Thay thế value của **<server_name>** phù hợp với Servername của máy

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

### 💡 Tip test kết nối với DB

Vào **Package Console Manager**, nhập

```bash
Update-database
```

Hoặc **Terminal**, nhập

```bash
dotnet ef database update
```

#### Chạy dự án ABCDMall.API

Test Swagger UI tại endpoint

![GET](https://img.shields.io/badge/GET-blue)
`/Db/test-db`

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
