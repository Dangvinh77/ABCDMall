# 🚀 .NET CLI & EF Core Cheat Sheet

## 🗄️ Package Console Manager (CLI)

### Tạo migration

```bash
Add-Migration <Name-Migrations> -Context <Name-DBcontext> -OutputDir <Name-output/Migrations>
```

### Xóa migration

```bash
Remove-Migration -Context <Name-DBcontext>
```

### Update database

```bash
Update-Database -Context <Name-DBContext>

Update-Database -Context MoviesCatalogDbContext
Update-Database -Context MoviesBookingDbContext
Update-Database -Context MallDbContext
Update-Database -Context FoodCourtDbContext
Update-Database -Context ShopDbContext
Update-Database -Context UtilityMapDbContext

```

### Xoá database

```bash
Drop-Database -Context <Name-DBContext>
```
