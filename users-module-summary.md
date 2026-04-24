# Tong hop thay doi module Users

## 1. Account onboarding va password flow cho manager

- Tao manager bang mat khau dung 1 lan va bat buoc doi mat khau sau lan dang nhap dau.
- Them luong `change initial password` va resend link.
- Login response, profile va user summary DTO duoc dieu chinh theo flow moi.
- File lien quan:
  - `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/Services/Auth/UserCommandService.cs`
  - `BACKEND/ABCDMall.WebAPI/Controllers/AuthController.cs`
  - `FRONTEND/src/features/auth/pages/Login.jsx`
  - `FRONTEND/src/features/auth/pages/ChangeInitialPassword.jsx`
  - `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/DTOs/Auth/CompleteInitialPasswordChangeDto.cs`

## 2. Profile approval workflow

- Manager khong update profile truc tiep nua ma gui request de admin duyet.
- Them `ProfileUpdateRequest`, approve/reject, pending request va history sau khi duyet.
- Giao dien profile da doi sang modal va co khu vuc pending approval.
- File chinh:
  - `FRONTEND/src/features/auth/pages/Profile.jsx`
  - `FRONTEND/src/features/auth/pages/UserManagement.jsx`
  - `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Domain/Entities/ProfileUpdateRequest.cs`
  - `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/DTOs/Auth/ProfileUpdateRequestResponseDto.cs`
  - `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/DTOs/Auth/ProfileUpdateRequestDecisionDto.cs`

## 3. Admin management va inactive account

- `User Management` duoc tach thanh 3 tab: active accounts, inactive accounts, profile approval.
- Inactive account khong xoa cung, chi set trang thai.
- Them nut kich hoat lai account.
- Them rule chan inactive khi manager van dang thue mat bang.
- File chinh:
  - `FRONTEND/src/features/auth/pages/UserManagement.jsx`
  - `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Domain/Entities/User.cs`
  - `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Infrastructure/Repositories/UserReadRepository.cs`
  - `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Infrastructure/Repositories/UserCommandRepository.cs`

## 4. Rental area, shop info va manager shop creation

- Rental area dung chung nguon du lieu voi `MapLocations`.
- Admin va manager deu xem chi tiet qua modal.
- Manager tao shop chi duoc chon slot da duoc thue nhung chua tao shop.
- Neu chi con 1 slot thi he thong tu chon.
- `ShopInfo` cua manager tach ro `Rental Information` va `Monthly Bills`.
- Khi tao account manager da co cac thong tin:
  - `Location`
  - `Floor`
  - `Start Date`
  - `Lease Term`
  - `Electricity Fee`
  - `Water Fee`
  - `Fee`
  - upload avatar, CCCD front/back, contract
- File chinh:
  - `FRONTEND/src/features/auth/pages/ManagerShops.jsx`
  - `FRONTEND/src/features/auth/pages/Register.jsx`
  - `FRONTEND/src/features/auth/pages/ShopInfo.jsx`
  - `FRONTEND/src/features/auth/pages/RentalAreasAdmin.jsx`
  - `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/Services/PublicCatalog/ShopInfoPublicManagerService.cs`
  - `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/Services/ShopInfos/ShopInfoQueryService.cs`
  - `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/Services/RentalAreas/RentalAreaCommandService.cs`

## 5. Rental payment va revenue

- Them rental payment flow rieng, khong dung chung module movie.
- Revenue cua admin da tach paid/unpaid bills.
- Luong xem theo 3 muc: shop -> monthly bills -> bill detail.
- `Location Details` tinh lai:
  - `Electricity` = tong `usage * electricity fee`
  - `Water` = tong `usage * water fee`
  - `Fee` = tong `service fee`
  - `Revenue/Outstanding` = tong `totalDue`
- File chinh:
  - `BACKEND/ABCDMall.WebAPI/Controllers/RentalPaymentsController.cs`
  - `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/Services/RentalPayments/`
  - `FRONTEND/src/features/auth/pages/RevenueStatistics.jsx`

## 6. SQL Server, migration, seed va file storage

- Users module da gom migration ve baseline moi.
- Xoa migration cu `20260421162312_InitialUsersSchema`.
- Dung migration moi `20260423093111_InitialUsersSchema`.
- Seed Users, ShopInfo va Monthly Bills da duoc cap nhat:
  - dong bo shop theo map
  - seed bill thang 1 den thang 4
  - them du lieu rental, profile va payment
- Them local file storage cho:
  - profile images
  - CCCD images
  - contract images
- File chinh:
  - `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Infrastructure/Seed/FrontendUsersSeed.cs`
  - `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Infrastructure/Services/LocalFileStorageService.cs`
  - `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Infrastructure/Persistence/MallDbContext.cs`
  - `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Infrastructure/Persistence/Migrations/`
