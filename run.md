# Hướng dẫn chạy dự án ABCDMall (Dành cho máy mới clone)

Tài liệu này hướng dẫn cách thiết lập và chạy dự án cả Backend và Frontend.

---

## 1. Yêu cầu hệ thống

- **.NET SDK 9.0** hoặc mới hơn.
- **Node.js** (khuyên dùng v18+).
- **SQL Server** (LocalDB hoặc SQL Server Instance).
- **Stripe Account** (để lấy key test).

---

## 2. Thiết lập Backend (.NET 9 Web API)

### 2.1. Cấu hình File `appsettings.Development.json`

Vào thư mục `BACKEND/ABCDMall.WebAPI`, tạo file `appsettings.Development.json` từ file mẫu:

1. Copy `appsettings.json` thành `appsettings.Development.json`.
2. Cập nhật chuỗi kết nối SQL Server tại mục `ConnectionStrings`:
   ```json
   "ConnectionStrings": {
     "ABCDMallConnection": "Server=YOUR_SERVER_NAME;Database=ABCDMall;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
   }
   ```
   _Lưu ý: Thay `YOUR_SERVER_NAME` bằng tên SQL Server của bạn (thường là `.` hoặc `localhost` hoặc `(localdb)\MSSQLLocalDB`)._

### 2.2. Lấy Key Stripe (Thanh toán)

Dự án tích hợp Stripe để thanh toán vé phim. Bạn cần thực hiện:

1. Truy cập [Stripe Dashboard](https://dashboard.stripe.com/test/apikeys) (chế độ Test).
2. Lấy **Publishable key** (`pk_test_...`) và **Secret key** (`sk_test_...`).
3. Dán vào file `appsettings.Development.json`.
4. **Lấy Webhook Secret (Cần thiết để xác nhận thanh toán):**
   - Đảm bảo bạn đã cài đặt [Stripe CLI](https://stripe.com/docs/stripe-cli).
   - Đăng nhập Stripe CLI: `stripe login`.
   - Mở một Terminal mới tại thư mục gốc của dự án và chạy:
     ```bash
     npm run stripe
     ```
   - Sau khi chạy, Stripe CLI sẽ hiển thị một chuỗi bắt đầu bằng `whsec_...` (Webhook signing secret).
   - Copy chuỗi này và dán vào mục `WebhookSecret` trong `appsettings.Development.json`.

   ```json
   "StripeSettings": {
     "SecretKey": "sk_test_...",
     "PublishableKey": "pk_test_...",
     "WebhookSecret": "whsec_...",
     "SuccessUrl": "http://localhost:5173/movies/payment/success",
     "CancelUrl": "http://localhost:5173/movies/payment/cancel",
     "Currency": "usd"
   }
   ```

### 2.4. Chạy Migration (Khởi tạo Database)

Bạn có thể dùng một trong hai cách:

#### Cách 1: Sử dụng .NET CLI (Khuyên dùng cho VS Code)

Mở Terminal tại thư mục `BACKEND` và chạy lần lượt:

```bash
# Cài đặt công cụ EF nếu chưa có
dotnet tool install --global dotnet-ef

# Cập nhật Database cho các Module (Dự án dùng Modular Monolith)
dotnet ef database update --project .\ABCDMall.Modules\Movies\ABCDMall.Modules.Movies.Infrastructure\ABCDMall.Modules.Movies.Infrastructure.csproj --startup-project .\ABCDMall.WebAPI\ABCDMall.WebAPI.csproj --context MoviesCatalogDbContext

dotnet ef database update --project .\ABCDMall.Modules\Movies\ABCDMall.Modules.Movies.Infrastructure\ABCDMall.Modules.Movies.Infrastructure.csproj --startup-project .\ABCDMall.WebAPI\ABCDMall.WebAPI.csproj --context MoviesBookingDbContext

dotnet ef database update --project .\ABCDMall.Modules\Users\ABCDMall.Modules.Users.Infrastructure\ABCDMall.Modules.Users.Infrastructure.csproj --startup-project .\ABCDMall.WebAPI\ABCDMall.WebAPI.csproj --context MallDbContext

dotnet ef database update --project .\ABCDMall.Modules\FoodCourt\ABCDMall.Modules.FoodCourt.Infrastructure\ABCDMall.Modules.FoodCourt.Infrastructure.csproj --startup-project .\ABCDMall.WebAPI\ABCDMall.WebAPI.csproj --context FoodCourtDbContext

dotnet ef database update --project .\ABCDMall.Modules\Shops\ABCDMall.Modules.Shops.Infrastructure\ABCDMall.Modules.Shops.Infrastructure.csproj --startup-project .\ABCDMall.WebAPI\ABCDMall.WebAPI.csproj --context ShopsDbContext

dotnet ef database update --project .\ABCDMall.Modules\Events\ABCDMall.Modules.Events.Infrastructure\ABCDMall.Modules.Events.Infrastructure.csproj --startup-project .\ABCDMall.WebAPI\ABCDMall.WebAPI.csproj --context EventsDbContext

dotnet ef database update --project .\ABCDMall.Modules\UtilityMap\ABCDMall.Modules.UtilityMap.Infrastructure\ABCDMall.Modules.UtilityMap.Infrastructure.csproj --startup-project .\ABCDMall.WebAPI\ABCDMall.WebAPI.csproj --context UtilityMapDbContext
```

#### Cách 2: Sử dụng Package Manager Console (Trong Visual Studio)

Mở **Package Manager Console** và chọn Default Project tương ứng cho mỗi lệnh:

1. Chọn Default Project: `ABCDMall.Modules.Movies.Infrastructure`
   - Chạy: `Update-Database -Context MoviesCatalogDbContext`
   - Chạy: `Update-Database -Context MoviesBookingDbContext`
2. Chọn Default Project: `ABCDMall.Modules.Users.Infrastructure`
   - Chạy: `Update-Database -Context MallDbContext`
3. Chọn Default Project: `ABCDMall.Modules.FoodCourt.Infrastructure`
   - Chạy: `Update-Database -Context FoodCourtDbContext`
4. Chọn Default Project: `ABCDMall.Modules.Shops.Infrastructure`
   - Chạy: `Update-Database -Context ShopsDbContext`
5. Chọn Default Project: `ABCDMall.Modules.Events.Infrastructure`
   - Chạy: `Update-Database -Context EventsDbContext`
6. Chọn Default Project: `ABCDMall.Modules.UtilityMap.Infrastructure`
   - Chạy: `Update-Database -Context UtilityMapDbContext`

### 2.5. Tùy chọn nhanh cho môi trường dev: dùng `DatabaseStartup`

Ngoài cách migrate thủ công, project hiện hỗ trợ reset, migrate và seed qua các flag trong `BACKEND/ABCDMall.WebAPI/appsettings.Development.json`.

Ví dụ:

```json
"DatabaseStartup": {
  "ResetMoviesDatabase": false,
  "SeedMoviesData": true,
  "ResetFoodCourtDatabase": false,
  "SeedFoodCourtData": true,
  "ResetUsersDatabase": false,
  "SeedUsersData": true,
  "ResetShopsDatabase": false,
  "SeedShopsData": true,
  "ResetEventsDatabase": false,
  "SeedEventsData": true,
  "ResetUtilityMapDatabase": false,
  "SeedUtilityMapData": true
}
```

Luồng startup hiện tại:

1. Nếu có `Reset... = true`, shared database sẽ chỉ bị xóa đúng một lần.
2. Sau đó WebAPI sẽ migrate tất cả `DbContext`.
3. Cuối cùng seed dữ liệu cho các module có `Seed... = true`.

Cách dùng an toàn trong dev:

1. Chỉ bật các `Reset...` bạn thực sự muốn reset.
2. Chạy backend một lần để reset + migrate + seed.
3. Đổi các `Reset...` đó về `false` để tránh lần sau khởi động bị xóa database lại.

### 2.6. Chạy Backend

Tại thư mục `BACKEND`:

```bash
dotnet run --project .\ABCDMall.WebAPI\ABCDMall.WebAPI.csproj
```

Backend sẽ chạy tại: `https://localhost:5184` (hoặc cổng cấu hình trong `launchSettings.json`). Truy cập `/swagger` để xem API.

---

## 3. Thiết lập Frontend (React + Vite + Tailwind)

### 3.1. Cài đặt thư viện

Mở Terminal tại thư mục `FRONTEND`:

```bash
npm install
```

### 3.2. Chạy Frontend

```bash
npm run dev
```

Ứng dụng sẽ chạy tại: `http://localhost:5173`.

---

## 4. Tổng kết quy trình chạy nhanh

1. Mở SQL Server.
2. Thiết lập `appsettings.Development.json` (DB + Stripe Key).
3. Tạo `BACKEND/ABCDMall.WebAPI/wwwroot` nếu chưa có.
4. Chọn một trong hai cách:
   - Chạy `Update-Database` cho tất cả các Context.
   - Hoặc dùng `DatabaseStartup` flags để reset/migrate/seed tự động khi chạy backend.
5. Terminal 1 (Backend): `dotnet run`.
6. Terminal 2 (Frontend): `npm run dev`.

---

_Lưu ý: Nếu gặp lỗi về Webhook Stripe, bạn cần cài đặt [Stripe CLI](https://stripe.com/docs/stripe-cli) và chạy lệnh `stripe listen --forward-to https://localhost:5184/api/payments/webhooks/stripe` để nhận thông báo thanh toán._
