# 🚀 .NET CLI & EF Core Cheat Sheet

## 📦 Setup & Create Project

### Cài EF CLI

```bash
dotnet tool install --global dotnet-ef --version 8.0.4
```

### Tạo project MVC

```bash
dotnet new mvc -n MyMvcProject -f net8.0
```

### Tạo Web API (tạo thư mục mới)

```bash
dotnet new webapi --framework net8.0 -o <project_name>
```

### Tạo Web API ngay trong thư mục hiện tại

```bash
dotnet new webapi --framework net8.0
```

## 📚 Cài đặt Package

```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.2
```

## 🔐 HTTPS Dev Certificate

### Trust certificate

```bash
dotnet dev-certs https --trust
```

### Kiểm tra certificate

```bash
dotnet dev-certs https --check
```

## ▶️ Build & Run

### Build project

```bash
dotnet build
```

### Run (không auto reload)

```bash
dotnet run
```

### Run có auto reload + chọn profile https

```bash
dotnet watch run --launch-profile https
```

## 💡 Tip:

Nếu không muốn dùng --launch-profile
→ chỉnh file:
**./Properties/launchSettings.json**

### 👉 Đưa profile https lên trên http để mặc định chạy HTTPS

## 🗄️ Entity Framework Core (CLI)

### Tạo migration

```bash
dotnet ef migrations add <MigrationName>
```

### Update database

```bash
dotnet ef database update
```

### Xoá database

```bash
dotnet ef database drop
```
