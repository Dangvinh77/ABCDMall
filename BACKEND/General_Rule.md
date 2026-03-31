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
