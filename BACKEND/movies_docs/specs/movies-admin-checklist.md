# Movies Admin Checklist

## 1. Ket luan nhanh ve Modules.Users

### Co the tai su dung auth cua Modules.Users

- Backend da co login/refresh/profile va JWT role-based auth trong `BACKEND/ABCDMall.WebAPI/Controllers/AuthController.cs`.
- JWT da mang role claim trong `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Infrastructure/Services/JwtService.cs`.
- Frontend login dang luu `token`, `refreshToken`, `role` vao `localStorage` trong `FRONTEND/src/features/auth/pages/Login.jsx`.

### Nhung chua du de tao 3 tai khoan Admin cho movies admin bang API hien tai

- `POST /api/Auth/register` la endpoint chi Admin moi goi duoc.
- `RegisterDto` khong co field `Role` trong `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/DTOs/RegisterDto.cs`.
- `UpdateUserAccountDto` cung khong co field `Role` trong `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/DTOs/UpdateUserAccountDto.cs`.
- `UserCommandService.RegisterAsync(...)` dang hardcode `Role = "Manager"` trong `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/Services/Auth/UserCommandService.cs`.
- `UpdateUserAccountAsync(...)` khong phai luong doi role, va con chan sua account co role `Admin`.

## 2. Tra loi cau hoi "co lay API auth ben Users de vao movies admin duoc khong?"

### Co, nhung theo 2 lop

- Lop 1: Tai su dung toan bo login, JWT, refresh token, profile cua `Modules.Users`.
- Lop 2: Bo sung phan role/permission cho movies admin, vi movies admin hien chua co guard rieng o frontend va chua co luong cap tai khoan admin movies o backend.

### Cach lam dung huong

- Khong lam auth rieng cho `Movies`.
- Tiep tuc dung `AuthController` va JWT hien tai.
- Bo sung mot role ro rang cho movies admin. Co 2 lua chon:
  - Don gian: dung chung role `Admin`.
  - Tot hon: them role nhu `MoviesAdmin` hoac permission theo module, de khong mo qua rong quyen cua admin tong.

## 3. Thuc trang Movies Admin hien tai

### Frontend

- Da co bo route/page prototype cho movies admin:
  - `FRONTEND/src/features/movies-admin/routes/MoviesAdminRoutes.tsx`
  - `FRONTEND/src/features/movies-admin/pages/MoviesAdminDashboardPage.tsx`
  - `FRONTEND/src/features/movies-admin/pages/MoviesAdminSectionPage.tsx`
- Phan nay hien la mock UI, chua noi API that. `MoviesAdminSectionPage.tsx` dang hien badge `Frontend admin prototype`.
- Route `/movies/admin/*` dang mount truc tiep trong `FRONTEND/src/routes/AppRoutes.tsx`, chua thay guard role cho movies admin.

### Backend

- Chua thay movie CRUD admin endpoints trong `MoviesController`.
- Chua thay create/update/delete showtime trong `ShowtimesController`.
- Chua thay API lich su ve cho admin, thong ke doanh thu chi tiet cho movie module.
- Stripe payment da co huong tich hop, nhung day moi la mot phan cua movie operations, chua thanh bo movies admin dashboard.

## 4. Checklist trien khai de co 3 tai khoan truy cap Movies Admin

### A. Chot mo hinh phan quyen

- [ ] Quyet dinh dung `Admin` chung hay tao role rieng `MoviesAdmin`.
- [ ] Neu can tach quyen chi tiet, them permission/module scope cho `Movies`.
- [ ] Quy uoc ro trang nao, API nao yeu cau role nao.

### B. Bo sung backend cho Users/Auth

- [ ] Mo rong `RegisterDto` de ho tro `Role`, hoac tao endpoint rieng `POST /api/Auth/admin-users`.
- [ ] Gioi han role co the cap phat, khong cho client tu gan role nguy hiem neu khong qua kiem soat.
- [ ] Neu giu role `Admin` la role he thong, can co bootstrap path de tao admin dau tien:
  - migration seed,
  - script SQL,
  - hoac command/dev seed chi chay local.
- [ ] Them API liet ke user theo role de tim nhanh nhom movies admin.
- [ ] Them validation tranh tu cap role sai.
- [ ] Log audit cho tao/sua/khoa tai khoan admin.

### C. Tao 3 tai khoan quan tri movies

- [ ] Chon email/username cho 3 tai khoan.
- [ ] Xac dinh role cua 3 tai khoan.
- [ ] Tao bang 1 trong 3 cach:
  - seed data,
  - endpoint admin tao user,
  - update truc tiep DB trong giai doan bootstrap.
- [ ] Dang nhap thu tung tai khoan va kiem tra JWT co role dung.

## 5. Checklist trien khai Movies Admin module

### A. Bao ve truy cap admin

- [ ] Them frontend route guard cho `/movies/admin/*`.
- [ ] Neu khong du role thi redirect ve `/login` hoac trang `403`.
- [ ] Dong bo token attach cho cac request admin API.
- [ ] Backend admin endpoints phai co `[Authorize(Roles = \"...\")]`.

### B. Quan ly phim

- [ ] Them API tao phim.
- [ ] Them API cap nhat phim.
- [ ] Them API xoa/an phim.
- [ ] Them upload poster/banner/trailer metadata.
- [ ] Them rule validation: do dai phim, do tuoi, ngay khoi chieu, status.

### C. Quan ly lich chieu va phong chieu

- [ ] Them API tao showtime theo ngay-gio cu the.
- [ ] Them API sua showtime.
- [ ] Them API huy/xoa showtime.
- [ ] Validate trung lich trong cung phong.
- [ ] Validate khoang dem giua 2 suat chieu.
- [ ] Hien thi seat map va seat availability cho tung showtime.

### D. Lich su ve va booking operations

- [ ] Them admin API xem danh sach booking.
- [ ] Loc theo ngay, phim, showtime, trang thai thanh toan.
- [ ] Xem chi tiet booking, ghe, khach hang, giao dich, email da gui.
- [ ] Ho tro re-send ticket email.
- [ ] Ho tro huy booking theo rule nghiep vu.

### E. Thanh toan

- [ ] Chot payment status flow cho booking: `Pending`, `Paid`, `Failed`, `Cancelled`, `Refunded`.
- [ ] Them admin page theo doi giao dich Stripe.
- [ ] Neu se lam PayPal sau, thiet ke abstraction `IPaymentGateway`/`PaymentProvider` nhat quan ngay tu dau.
- [ ] Co trang doi soat webhook/payment logs de debug.

### F. Email ticketing

- [ ] Hoan thien `EmailSettings` production.
- [ ] Co page/admin action xem lich su gui mail.
- [ ] Co trang thai `sent/failed/retry`.
- [ ] Co mau email ve va file PDF nhat quan.

### G. Dashboard va thong ke

- [ ] KPI tong quan: so ve ban, doanh thu ngay/tuan/thang, ti le lap day, ty le thanh toan thanh cong.
- [ ] Bao cao theo phim.
- [ ] Bao cao theo khung gio.
- [ ] Bao cao theo phong chieu/showtime.
- [ ] Bao cao theo provider thanh toan.
- [ ] Export CSV/Excel neu can nop van hanh.

### H. Frontend movies-admin

- [ ] Thay mock data trong `movies-admin/data/adminData.ts` bang data that tu API.
- [ ] Tach tung section thanh page/service/form that.
- [ ] Them form tao/sua phim.
- [ ] Them form tao/sua showtime.
- [ ] Them booking table co filter, sort, paging.
- [ ] Them payment status badges, timeline, empty state, error state.

### I. Database va migration

- [ ] Ra soat toan bo columns moi cua Movies module da co migration chua.
- [ ] Chay migration truoc khi test webhook/payment.
- [ ] Kiem tra cac bang booking/payment/email-ticket da map dung schema.

### J. Logging va van hanh

- [ ] Tach log payment webhook de debug nhanh.
- [ ] Ghi audit log cho thao tac admin quan trong.
- [ ] Co error page/toast ro rang cho frontend admin.
- [ ] Co checklist run local: frontend, backend, Stripe CLI, DB migration.

## 6. De xuat cach trien khai thuc te nhat

### Phase 1: Mo khoa truy cap admin

- [ ] Them role `MoviesAdmin` hoac cho phep tao user role `Admin/MoviesAdmin`.
- [ ] Tao 3 tai khoan admin.
- [ ] Them route guard frontend cho `/movies/admin/*`.

### Phase 2: Bien prototype thanh admin CRUD that

- [ ] Movie CRUD.
- [ ] Showtime CRUD.
- [ ] Booking history list/detail.
- [ ] Payment monitoring.

### Phase 3: Van hanh

- [ ] Ticket email resend.
- [ ] Revenue dashboard.
- [ ] Audit log.
- [ ] Export bao cao.

## 7. Ket luan

- Co the dung auth API cua `Modules.Users` lam nen tang cho movies admin.
- Khong the dung ngay API hien tai de tao 3 tai khoan `Admin`, vi register flow dang khoa role ve `Manager`.
- Muon movies admin chay dung, can lam them 2 viec truoc tien:
  - mo rong Users/Auth de tao duoc tai khoan co role dung,
  - them route guard va admin APIs that cho Movies module.
