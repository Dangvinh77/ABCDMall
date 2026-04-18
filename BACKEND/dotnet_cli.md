# .NET CLI Guide for VS Code and Visual Studio 2022

## Muc dich

File nay dung cho dev lam viec bang:

- VS Code: dung `Terminal`
- Visual Studio 2022: dung `Terminal` / `Developer PowerShell`

Neu muon dung `Package Manager Console`, xem them file `package_console_cli.md`.

## Thu muc dung lenh

Chay tat ca lenh ben duoi tai thu muc:

```powershell
E:\DEV\Coding_Resource\Project\e_PROJECT\Semester3\eProjectSem3_Group2_ABCDMall_T2.2410.E0\CODE\BACKEND
```

## Cai dat can thiet

### Cai EF CLI

```bash
dotnet tool install --global dotnet-ef --version 8.0.4
```

### Kiem tra EF CLI

```bash
dotnet ef
```

### Restore solution

```bash
dotnet restore ABCDMall.sln
```

## Build va run

### Build solution

```bash
dotnet build ABCDMall.sln
```

### Run WebAPI

```bash
dotnet run --project .\ABCDMall.WebAPI\ABCDMall.WebAPI.csproj
```

### Run WebAPI voi hot reload

```bash
dotnet watch --project .\ABCDMall.WebAPI\ABCDMall.WebAPI.csproj run
```

## HTTPS dev certificate

### Trust certificate

```bash
dotnet dev-certs https --trust
```

### Kiem tra certificate

```bash
dotnet dev-certs https --check
```

## EF Core cho du an nay

## Nguyen tac

Trong solution nay, moi module co infrastructure project rieng. Khi dung `dotnet ef`, nen chi ro:

- `--project`: project chua migrations / DbContext
- `--startup-project`: project khoi dong app, o day la `ABCDMall.WebAPI`
- `--context`: DbContext can thao tac

Mau chung:

```bash
dotnet ef migrations add <MigrationName> --project <Infrastructure.csproj> --startup-project .\ABCDMall.WebAPI\ABCDMall.WebAPI.csproj --context <DbContext> --output-dir <MigrationsFolder>
```

## Mapping voi Package Manager Console

- `Add-Migration`  -> `dotnet ef migrations add`
- `Remove-Migration` -> `dotnet ef migrations remove`
- `Update-Database` -> `dotnet ef database update`
- `Drop-Database` -> `dotnet ef database drop`

Visual Studio 2022 van dung duoc file nay neu ban mo `Terminal` trong VS va chay y nhu tren.

## Cac DbContext hien co

- `MoviesCatalogDbContext`
- `MoviesBookingDbContext`
- `MallDbContext`
- `FoodCourtDbContext`
- `ShopsDbContext`
- `UtilityMapDbContext`

Luu y: context dung trong code la `ShopsDbContext`, khong phai `ShopDbContext`.

## Tao migration

### Movies Catalog

```bash
dotnet ef migrations add <MigrationName> `
  --project .\ABCDMall.Modules\Movies\ABCDMall.Modules.Movies.Infrastructure\ABCDMall.Modules.Movies.Infrastructure.csproj `
  --startup-project .\ABCDMall.WebAPI\ABCDMall.WebAPI.csproj `
  --context MoviesCatalogDbContext `
  --output-dir Persistence\Catalog\Migrations
```

### Movies Booking

```bash
dotnet ef migrations add <MigrationName> `
  --project .\ABCDMall.Modules\Movies\ABCDMall.Modules.Movies.Infrastructure\ABCDMall.Modules.Movies.Infrastructure.csproj `
  --startup-project .\ABCDMall.WebAPI\ABCDMall.WebAPI.csproj `
  --context MoviesBookingDbContext `
  --output-dir Persistence\Booking\Migrations
```

### Mall / Users

```bash
dotnet ef migrations add <MigrationName> `
  --project .\ABCDMall.Modules\Users\ABCDMall.Modules.Users.Infrastructure\ABCDMall.Modules.Users.Infrastructure.csproj `
  --startup-project .\ABCDMall.WebAPI\ABCDMall.WebAPI.csproj `
  --context MallDbContext `
  --output-dir Persistence\Migrations
```

### FoodCourt

```bash
dotnet ef migrations add <MigrationName> `
  --project .\ABCDMall.Modules\FoodCourt\ABCDMall.Modules.FoodCourt.Infrastructure\ABCDMall.Modules.FoodCourt.Infrastructure.csproj `
  --startup-project .\ABCDMall.WebAPI\ABCDMall.WebAPI.csproj `
  --context FoodCourtDbContext `
  --output-dir Persistence\FoodCourt\Migrations
```

### Shops

```bash
dotnet ef migrations add <MigrationName> `
  --project .\ABCDMall.Modules\Shops\ABCDMall.Modules.Shops.Infrastructure\ABCDMall.Modules.Shops.Infrastructure.csproj `
  --startup-project .\ABCDMall.WebAPI\ABCDMall.WebAPI.csproj `
  --context ShopsDbContext `
  --output-dir Persistence\Shops\Migrations
```

### UtilityMap

```bash
dotnet ef migrations add <MigrationName> `
  --project .\ABCDMall.Modules\UtilityMap\ABCDMall.Modules.UtilityMap.Infrastructure\ABCDMall.Modules.UtilityMap.Infrastructure.csproj `
  --startup-project .\ABCDMall.WebAPI\ABCDMall.WebAPI.csproj `
  --context UtilityMapDbContext `
  --output-dir Persistence\UtilityMap\Migrations
```

## Xoa migration cuoi cung

Mau chung:

```bash
dotnet ef migrations remove --project <Infrastructure.csproj> --startup-project .\ABCDMall.WebAPI\ABCDMall.WebAPI.csproj --context <DbContext>
```

Vi du voi `MallDbContext`:

```bash
dotnet ef migrations remove `
  --project .\ABCDMall.Modules\Users\ABCDMall.Modules.Users.Infrastructure\ABCDMall.Modules.Users.Infrastructure.csproj `
  --startup-project .\ABCDMall.WebAPI\ABCDMall.WebAPI.csproj `
  --context MallDbContext
```

## Update database

### Cap nhat theo tung context

```bash
dotnet ef database update --project .\ABCDMall.Modules\Movies\ABCDMall.Modules.Movies.Infrastructure\ABCDMall.Modules.Movies.Infrastructure.csproj --startup-project .\ABCDMall.WebAPI\ABCDMall.WebAPI.csproj --context MoviesCatalogDbContext

dotnet ef database update --project .\ABCDMall.Modules\Movies\ABCDMall.Modules.Movies.Infrastructure\ABCDMall.Modules.Movies.Infrastructure.csproj --startup-project .\ABCDMall.WebAPI\ABCDMall.WebAPI.csproj --context MoviesBookingDbContext

dotnet ef database update --project .\ABCDMall.Modules\Users\ABCDMall.Modules.Users.Infrastructure\ABCDMall.Modules.Users.Infrastructure.csproj --startup-project .\ABCDMall.WebAPI\ABCDMall.WebAPI.csproj --context MallDbContext

dotnet ef database update --project .\ABCDMall.Modules\FoodCourt\ABCDMall.Modules.FoodCourt.Infrastructure\ABCDMall.Modules.FoodCourt.Infrastructure.csproj --startup-project .\ABCDMall.WebAPI\ABCDMall.WebAPI.csproj --context FoodCourtDbContext

dotnet ef database update --project .\ABCDMall.Modules\Shops\ABCDMall.Modules.Shops.Infrastructure\ABCDMall.Modules.Shops.Infrastructure.csproj --startup-project .\ABCDMall.WebAPI\ABCDMall.WebAPI.csproj --context ShopsDbContext

dotnet ef database update --project .\ABCDMall.Modules\UtilityMap\ABCDMall.Modules.UtilityMap.Infrastructure\ABCDMall.Modules.UtilityMap.Infrastructure.csproj --startup-project .\ABCDMall.WebAPI\ABCDMall.WebAPI.csproj --context UtilityMapDbContext
```

## Drop database

Mau chung:

```bash
dotnet ef database drop --project <Infrastructure.csproj> --startup-project .\ABCDMall.WebAPI\ABCDMall.WebAPI.csproj --context <DbContext>
```

Vi du:

```bash
dotnet ef database drop `
  --project .\ABCDMall.Modules\FoodCourt\ABCDMall.Modules.FoodCourt.Infrastructure\ABCDMall.Modules.FoodCourt.Infrastructure.csproj `
  --startup-project .\ABCDMall.WebAPI\ABCDMall.WebAPI.csproj `
  --context FoodCourtDbContext
```

## Cach dung trong tung IDE

### VS Code

1. Mo folder `BACKEND`.
2. Mo `Terminal`.
3. Chay cac lenh `dotnet` trong file nay.

### Visual Studio 2022

1. Mo file `ABCDMall.sln`.
2. Vao `View > Terminal` hoac mo `Developer PowerShell`.
3. Chuyen ve thu muc `BACKEND` neu can.
4. Chay cac lenh `dotnet` trong file nay.

Neu muon thao tac bang `Package Manager Console` trong Visual Studio 2022, dung file `package_console_cli.md`.
