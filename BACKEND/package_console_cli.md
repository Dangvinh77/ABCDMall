# .NET CLI & EF Core Cheat Sheet

## Package Manager Console

### Tạo migration

```powershell
Add-Migration <Name-Migrations> -Context <Name-DBContext> -OutputDir <Name-output/Migrations>
```

### Xóa migration

```powershell
Remove-Migration -Context <Name-DBcontext>
```

### Update database

```powershell
Update-Database -Context <Name-DBContext>
```

### Update theo context trong solution nay

```powershell
Update-Database -Context MoviesCatalogDbContext
Update-Database -Context MoviesBookingDbContext
Update-Database -Context MallDbContext
Update-Database -Context FoodCourtDbContext
Update-Database -Context ShopsDbContext
Update-Database -Context UtilityMapDbContext
```

### Movies Booking: cap nhat den migration cu the

Dung khi DB movies booking thieu schema moi cho ticket email delivery.

```powershell
Update-Database 20260419103000_AddTicketEmailDelivery -Context MoviesBookingDbContext
```

### Movies Booking: cap nhat den migration moi nhat

```powershell
Update-Database -Context MoviesBookingDbContext
```

### Luu y ve Default Project trong Package Manager Console

Khi dung `Package Manager Console`, can chon `Default Project` la:

```text
ABCDMall.Modules.Movies.Infrastructure
```

Neu update `MoviesBookingDbContext` tu sai project, EF co the khong tim thay migration hoac startup assembly.

### Xoá database

```powershell
Drop-Database -Context <Name-DBContext>
```
