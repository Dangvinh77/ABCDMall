# Movie Design Document

## 1. Mục tiêu tài liệu

Tài liệu này thiết kế backend cho module `Movies` dựa trên source code hiện có của frontend tại `FRONTEND/src/features/movies`.

Phạm vi nghiệp vụ frontend hiện đang cần:

- Danh sách phim đang chiếu và sắp chiếu
- Trang chi tiết phim
- Lịch chiếu theo ngày, rạp, định dạng phòng, ngôn ngữ
- Chọn ghế
- Chọn combo bắp nước
- Checkout không cần login
- Áp dụng promotion theo rule
- Thanh toán
- Phát hành booking code và e-ticket
- Khu vực admin quản trị movie/showtime/seat/bookings/payments/promotions/logs/users

Mục tiêu backend:

- Thiết kế endpoint đủ để thay mock data ở frontend
- Thiết kế bảng đủ mở rộng cho các module khác dùng chung DB
- Giảm conflict khi nhiều nhóm cùng thêm bảng hoặc migration
- Chia rõ thứ tự code endpoint để làm dần nhưng không vỡ luồng booking

## 2. Nghiệp vụ rút ra từ frontend

### 2.1 Luồng user chính

1. User vào trang `movies`
2. Xem phim đang chiếu, sắp chiếu, promotion
3. Chọn phim hoặc vào trang showtimes
4. Lọc theo ngày, rạp, hall type, language
5. Chọn 1 showtime
6. Chọn ghế
7. Có thể chọn snack combo
8. Sang checkout, nhập thông tin guest
9. Chọn payment method
10. Backend evaluate promotion
11. Tạo booking, giữ ghế, xử lý payment
12. Payment thành công thì phát hành vé điện tử

### 2.2 Dữ liệu frontend đang dùng

Frontend hiện đang cần các nhóm dữ liệu sau:

- `Movie`
- `Cinema`
- `HallType`
- `Language`
- `Showtime`
- `Seat`
- `SeatType`
- `SnackCombo`
- `Promotion`
- `Booking`
- `Payment`
- `GuestCustomer`

### 2.3 Rule nghiệp vụ nổi bật từ frontend

- Có phim `now showing` và `coming soon`
- Showtime gắn với rạp, phòng, hall type, ngôn ngữ, giá từ
- Ghế có loại `regular`, `vip`, `couple`
- Couple seat đi theo cặp
- Có phí dịch vụ theo số vé
- Có combo đồ ăn
- Promotion có nhiều kiểu:
  - Cuối tuần tặng combo
  - Giảm theo payment method
  - Date night với couple seat
  - Group booking
  - Combo Gold
  - Sinh nhật
  - Early bird
- Checkout là guest checkout, không bắt buộc account
- Sau thanh toán cần email e-ticket

## 3. Định hướng kiến trúc backend

Khuyến nghị chia module `Movies` thành các phần:

- `ABCDMall.Modules.Movies.Domain`
- `ABCDMall.Modules.Movies.Application`
- `ABCDMall.Modules.Movies.Infrastructure`
- `ABCDMall.WebAPI`

Khuyến nghị chia subdomain trong Movies:

- Catalog: movie, genre, cast, crew, media
- Screening: cinema, hall, seat template, showtime
- Booking: booking session, seat hold, ticket, guest info
- Promotion: promotion definition, rule, redemption
- Concession: snack combo, order item
- Payment: transaction, gateway callback
- Admin: dashboard, logs, audit

## 4. Danh sách endpoint đề xuất

## 4.1 Public endpoints

### 4.1.1 Movies catalog

- `GET /api/movies`
  - Lấy danh sách phim
  - Query:
    - `status=now-showing|coming-soon|all`
    - `genre`
    - `search`
    - `page`
    - `pageSize`
    - `sort=releaseDate|rating|title`
- `GET /api/movies/home`
  - Dữ liệu trang home movies
  - Trả về:
    - hero movies
    - now showing
    - coming soon
    - featured promotions
- `GET /api/movies/{movieId}`
  - Chi tiết phim
- `GET /api/movies/{movieId}/showtimes`
  - Lịch chiếu cho 1 phim theo ngày
  - Query:
    - `date`
    - `cinemaId`

### 4.1.2 Showtimes

- `GET /api/showtimes`
  - Lấy lịch chiếu tổng hợp
  - Query:
    - `date`
    - `movieId`
    - `cinemaId`
    - `hallType`
    - `language`
- `GET /api/showtimes/{showtimeId}`
  - Chi tiết suất chiếu
- `GET /api/showtimes/{showtimeId}/seat-map`
  - Trả về seat map và trạng thái ghế realtime

### 4.1.3 Promotions

- `GET /api/movies-promotions`
  - Danh sách promotion
  - Query:
    - `category=all|ticket|combo|member|bank|weekend`
    - `activeOnly=true|false`
- `GET /api/movies-promotions/{promotionId}`
  - Chi tiết promotion
- `POST /api/movies-promotions/evaluate`
  - Evaluate promotion theo booking context

Payload gợi ý:

```json
{
  "promotionId": "f2",
  "showtimeId": "st_001",
  "seatIds": ["D5", "D6"],
  "paymentMethod": "momo",
  "birthday": "2002-04-15",
  "snackCombos": [
    { "comboId": "combo-gold", "quantity": 1 }
  ]
}
```

### 4.1.4 Snack combos

- `GET /api/snack-combos`
  - Danh sách combo áp dụng cho movies

### 4.1.5 Booking flow

- `POST /api/bookings/holds`
  - Giữ ghế tạm thời trước khi checkout
- `GET /api/bookings/holds/{holdId}`
  - Lấy trạng thái giữ ghế
- `DELETE /api/bookings/holds/{holdId}`
  - Hủy giữ ghế
- `POST /api/bookings/quote`
  - Tính tiền preview
  - Bao gồm seat price, service fee, combo, discount, total
- `POST /api/bookings`
  - Tạo booking từ hold
- `GET /api/bookings/{bookingCode}`
  - Xem booking public bằng booking code + email hoặc token
- `POST /api/bookings/{bookingId}/resend-ticket`
  - Gửi lại e-ticket

### 4.1.6 Payments

- `POST /api/payments/intents`
  - Tạo payment intent cho booking
- `POST /api/payments/callback/{provider}`
  - Callback từ MoMo / VNPay / ATM / Visa provider
- `GET /api/payments/{paymentId}`
  - Trạng thái payment

## 4.2 Admin endpoints

### 4.2.1 Movies admin

- `GET /api/admin/movies`
- `POST /api/admin/movies`
- `GET /api/admin/movies/{movieId}`
- `PUT /api/admin/movies/{movieId}`
- `PATCH /api/admin/movies/{movieId}/publish`
- `PATCH /api/admin/movies/{movieId}/archive`

### 4.2.2 Cinema and hall admin

- `GET /api/admin/cinemas`
- `POST /api/admin/cinemas`
- `PUT /api/admin/cinemas/{cinemaId}`
- `GET /api/admin/halls`
- `POST /api/admin/halls`
- `PUT /api/admin/halls/{hallId}`
- `GET /api/admin/halls/{hallId}/seats`
- `PUT /api/admin/halls/{hallId}/seats`

### 4.2.3 Showtimes admin

- `GET /api/admin/showtimes`
- `POST /api/admin/showtimes`
- `PUT /api/admin/showtimes/{showtimeId}`
- `DELETE /api/admin/showtimes/{showtimeId}`
- `POST /api/admin/showtimes/bulk-generate`

### 4.2.4 Promotion admin

- `GET /api/admin/movies-promotions`
- `POST /api/admin/movies-promotions`
- `PUT /api/admin/movies-promotions/{promotionId}`
- `PATCH /api/admin/movies-promotions/{promotionId}/activate`
- `PATCH /api/admin/movies-promotions/{promotionId}/deactivate`

### 4.2.5 Booking and payment admin

- `GET /api/admin/bookings`
- `GET /api/admin/bookings/{bookingId}`
- `PATCH /api/admin/bookings/{bookingId}/cancel`
- `GET /api/admin/payments`
- `GET /api/admin/payments/{paymentId}`
- `POST /api/admin/bookings/{bookingId}/refund`

### 4.2.6 Dashboard and logs

- `GET /api/admin/movies-dashboard/summary`
- `GET /api/admin/movies-dashboard/revenue`
- `GET /api/admin/movies-dashboard/occupancy`
- `GET /api/admin/movies-logs`

## 5. Thứ tự code endpoint nên làm

Đây là thứ tự tối ưu để frontend có thể chuyển dần từ mock sang API.

### Phase 1: đọc dữ liệu trước

1. `GET /api/movies/home`
2. `GET /api/movies`
3. `GET /api/movies/{movieId}`
4. `GET /api/showtimes`
5. `GET /api/showtimes/{showtimeId}`
6. `GET /api/showtimes/{showtimeId}/seat-map`
7. `GET /api/movies-promotions`
8. `GET /api/movies-promotions/{promotionId}`
9. `GET /api/snack-combos`

### Phase 2: quote và promotion

10. `POST /api/movies-promotions/evaluate`
11. `POST /api/bookings/quote`

### Phase 3: booking core

12. `POST /api/bookings/holds`
13. `GET /api/bookings/holds/{holdId}`
14. `DELETE /api/bookings/holds/{holdId}`
15. `POST /api/bookings`
16. `GET /api/bookings/{bookingCode}`

### Phase 4: payment

17. `POST /api/payments/intents`
18. `POST /api/payments/callback/{provider}`
19. `GET /api/payments/{paymentId}`
20. `POST /api/bookings/{bookingId}/resend-ticket`

### Phase 5: admin

21. Movies admin
22. Hall/seat admin
23. Showtime admin
24. Promotion admin
25. Booking/payment admin
26. Dashboard/logs

## 6. Phân bố bảng

Khuyến nghị không để tất cả table trộn lẫn tên chung chung. Nên dùng prefix hoặc schema riêng cho module Movies.

### Phương án khuyến nghị

- Schema: `movies`
- Ví dụ:
  - `movies.Movies`
  - `movies.Showtimes`
  - `movies.Bookings`
  - `movies.Payments`

Nếu team chưa dùng multiple schema ổn định, dùng prefix table:

- `Movies_Movies`
- `Movies_Showtimes`
- `Movies_Bookings`

Khuyến nghị mạnh hơn là dùng schema `movies`.

## 7. Thiết kế bảng chi tiết

## 7.1 Catalog tables

### `movies.Movies`

Mục đích: lưu thông tin phim.

| Column | Type | Notes |
|---|---|---|
| `Id` | `uuid` | PK |
| `Code` | `varchar(50)` | unique, public code |
| `Slug` | `varchar(255)` | unique |
| `Title` | `varchar(255)` | not null |
| `OriginalTitle` | `varchar(255)` | nullable |
| `Description` | `text` | synopsis |
| `DurationMinutes` | `int` | not null |
| `ReleaseDate` | `date` | nullable for draft |
| `LanguageDisplay` | `varchar(150)` | ví dụ `English with Vietnamese subtitles` |
| `AgeRatingCode` | `varchar(10)` | `P`, `T13`, `T16`, `T18` |
| `Director` | `varchar(255)` | |
| `CastDisplay` | `text` | fallback nhanh cho UI |
| `TrailerUrl` | `varchar(1000)` | nullable |
| `PosterUrl` | `varchar(1000)` | |
| `BackdropUrl` | `varchar(1000)` | nullable |
| `Status` | `varchar(30)` | `Draft`, `NowShowing`, `ComingSoon`, `Archived` |
| `RatingAverage` | `decimal(3,1)` | nullable |
| `IsPublished` | `bit` | |
| `CreatedAtUtc` | `datetime2` | |
| `UpdatedAtUtc` | `datetime2` | |
| `CreatedBy` | `uuid` | nullable |
| `UpdatedBy` | `uuid` | nullable |

Index:

- unique `Code`
- unique `Slug`
- index `(Status, IsPublished, ReleaseDate desc)`

### `movies.Genres`

| Column | Type | Notes |
|---|---|---|
| `Id` | `uuid` | PK |
| `Code` | `varchar(50)` | unique |
| `Name` | `varchar(100)` | |
| `DisplayOrder` | `int` | |

### `movies.MovieGenres`

| Column | Type | Notes |
|---|---|---|
| `MovieId` | `uuid` | FK -> Movies |
| `GenreId` | `uuid` | FK -> Genres |

PK: `(MovieId, GenreId)`

### `movies.People`

Mục đích: diễn viên, đạo diễn, crew.

| Column | Type | Notes |
|---|---|---|
| `Id` | `uuid` | PK |
| `FullName` | `varchar(255)` | |
| `Slug` | `varchar(255)` | unique |
| `AvatarUrl` | `varchar(1000)` | nullable |
| `Bio` | `text` | nullable |

### `movies.MovieCredits`

| Column | Type | Notes |
|---|---|---|
| `Id` | `uuid` | PK |
| `MovieId` | `uuid` | FK |
| `PersonId` | `uuid` | FK |
| `CreditType` | `varchar(30)` | `Director`, `Actor`, `Producer` |
| `CharacterName` | `varchar(255)` | nullable |
| `DisplayOrder` | `int` | |

## 7.2 Cinema and hall tables

### `movies.Cinemas`

| Column | Type | Notes |
|---|---|---|
| `Id` | `uuid` | PK |
| `Code` | `varchar(50)` | unique |
| `Name` | `varchar(255)` | |
| `AddressLine` | `varchar(500)` | |
| `City` | `varchar(100)` | |
| `District` | `varchar(100)` | nullable |
| `Latitude` | `decimal(9,6)` | nullable |
| `Longitude` | `decimal(9,6)` | nullable |
| `IsActive` | `bit` | |
| `CreatedAtUtc` | `datetime2` | |

### `movies.Halls`

| Column | Type | Notes |
|---|---|---|
| `Id` | `uuid` | PK |
| `CinemaId` | `uuid` | FK -> Cinemas |
| `Code` | `varchar(50)` | unique within cinema |
| `Name` | `varchar(100)` | ví dụ `Room 01` |
| `HallType` | `varchar(20)` | `2D`, `3D`, `IMAX`, `4DX` |
| `Capacity` | `int` | |
| `IsActive` | `bit` | |

Unique:

- `(CinemaId, Code)`

### `movies.HallSeats`

Mục đích: seat template của phòng.

| Column | Type | Notes |
|---|---|---|
| `Id` | `uuid` | PK |
| `HallId` | `uuid` | FK |
| `SeatCode` | `varchar(10)` | ví dụ `D5` |
| `RowLabel` | `varchar(5)` | |
| `ColumnNumber` | `int` | |
| `SeatType` | `varchar(20)` | `Regular`, `VIP`, `Couple` |
| `CoupleGroupCode` | `varchar(20)` | nullable, ghế đôi cùng group |
| `IsActive` | `bit` | |
| `DisplayOrder` | `int` | |

Unique:

- `(HallId, SeatCode)`

## 7.3 Showtime tables

### `movies.Showtimes`

Đây là bảng trung tâm của module.

| Column | Type | Notes |
|---|---|---|
| `Id` | `uuid` | PK |
| `MovieId` | `uuid` | FK |
| `CinemaId` | `uuid` | FK |
| `HallId` | `uuid` | FK |
| `BusinessDate` | `date` | ngày chiếu local |
| `StartAtLocal` | `datetime2` | giờ local |
| `EndAtLocal` | `datetime2` | giờ local |
| `LanguageType` | `varchar(20)` | `Sub`, `Dub` |
| `SubtitleLanguage` | `varchar(50)` | nullable |
| `AudioLanguage` | `varchar(50)` | nullable |
| `BasePrice` | `decimal(18,2)` | giá khởi điểm |
| `Status` | `varchar(20)` | `Scheduled`, `Open`, `Closed`, `Cancelled` |
| `BookingOpenAtUtc` | `datetime2` | |
| `BookingCloseAtUtc` | `datetime2` | |
| `IsPublished` | `bit` | |
| `Version` | `rowversion` | chống race condition |
| `CreatedAtUtc` | `datetime2` | |
| `UpdatedAtUtc` | `datetime2` | |

Index:

- `(BusinessDate, CinemaId)`
- `(MovieId, BusinessDate)`
- `(HallId, StartAtLocal)`

Rule:

- Không cho 2 showtime trùng khoảng thời gian trong cùng `HallId`

### `movies.ShowtimeSeatInventory`

Snapshot trạng thái ghế theo từng showtime.

| Column | Type | Notes |
|---|---|---|
| `Id` | `uuid` | PK |
| `ShowtimeId` | `uuid` | FK |
| `HallSeatId` | `uuid` | FK |
| `SeatCode` | `varchar(10)` | denormalized |
| `SeatType` | `varchar(20)` | denormalized |
| `Price` | `decimal(18,2)` | giá ghế cuối cùng |
| `Status` | `varchar(20)` | `Available`, `Held`, `Booked`, `Blocked` |
| `HoldId` | `uuid` | nullable |
| `BookedTicketId` | `uuid` | nullable |
| `Version` | `rowversion` | chống double-book |
| `UpdatedAtUtc` | `datetime2` | |

Unique:

- `(ShowtimeId, HallSeatId)`

Index:

- `(ShowtimeId, Status)`

## 7.4 Booking tables

### `movies.GuestCustomers`

Không ép phụ thuộc module Users.

| Column | Type | Notes |
|---|---|---|
| `Id` | `uuid` | PK |
| `NormalizedEmail` | `varchar(320)` | index |
| `FullName` | `varchar(255)` | |
| `Email` | `varchar(320)` | |
| `Phone` | `varchar(30)` | |
| `Birthday` | `date` | nullable |
| `LinkedUserId` | `uuid` | nullable, nếu sau này map sang module Users |
| `CreatedAtUtc` | `datetime2` | |
| `UpdatedAtUtc` | `datetime2` | |

### `movies.BookingHolds`

Giữ ghế tạm thời.

| Column | Type | Notes |
|---|---|---|
| `Id` | `uuid` | PK |
| `ShowtimeId` | `uuid` | FK |
| `GuestCustomerId` | `uuid` | nullable |
| `HoldCode` | `varchar(50)` | unique |
| `Status` | `varchar(20)` | `Active`, `Expired`, `Released`, `Converted` |
| `ExpiresAtUtc` | `datetime2` | |
| `CreatedAtUtc` | `datetime2` | |

### `movies.BookingHoldSeats`

| Column | Type | Notes |
|---|---|---|
| `Id` | `uuid` | PK |
| `HoldId` | `uuid` | FK |
| `ShowtimeSeatInventoryId` | `uuid` | FK |
| `SeatCode` | `varchar(10)` | denormalized |
| `SeatPrice` | `decimal(18,2)` | |

### `movies.Bookings`

| Column | Type | Notes |
|---|---|---|
| `Id` | `uuid` | PK |
| `BookingCode` | `varchar(50)` | unique, ví dụ `ABCD-XXXX-XXXX` |
| `ShowtimeId` | `uuid` | FK |
| `GuestCustomerId` | `uuid` | FK |
| `HoldId` | `uuid` | nullable |
| `Status` | `varchar(20)` | `PendingPayment`, `Confirmed`, `Cancelled`, `Expired`, `Refunded` |
| `SeatSubtotal` | `decimal(18,2)` | |
| `ServiceFeeTotal` | `decimal(18,2)` | |
| `ComboSubtotal` | `decimal(18,2)` | |
| `DiscountTotal` | `decimal(18,2)` | |
| `GrandTotal` | `decimal(18,2)` | |
| `PromotionId` | `uuid` | nullable |
| `PromotionSnapshotJson` | `nvarchar(max)` | snapshot rule tại thời điểm booking |
| `PaymentStatus` | `varchar(20)` | `Unpaid`, `Paid`, `Failed`, `Refunded` |
| `ConfirmedAtUtc` | `datetime2` | nullable |
| `CreatedAtUtc` | `datetime2` | |
| `UpdatedAtUtc` | `datetime2` | |

### `movies.BookingItems`

Dùng cho seat và combo trong một cấu trúc thống nhất.

| Column | Type | Notes |
|---|---|---|
| `Id` | `uuid` | PK |
| `BookingId` | `uuid` | FK |
| `ItemType` | `varchar(20)` | `Seat`, `Combo`, `Fee`, `Discount` |
| `ReferenceId` | `uuid` | nullable |
| `Code` | `varchar(50)` | seat code hoặc combo code |
| `Name` | `varchar(255)` | |
| `Quantity` | `int` | |
| `UnitPrice` | `decimal(18,2)` | |
| `LineTotal` | `decimal(18,2)` | |
| `MetadataJson` | `nvarchar(max)` | |

### `movies.Tickets`

| Column | Type | Notes |
|---|---|---|
| `Id` | `uuid` | PK |
| `BookingId` | `uuid` | FK |
| `ShowtimeSeatInventoryId` | `uuid` | FK |
| `TicketCode` | `varchar(50)` | unique |
| `SeatCode` | `varchar(10)` | |
| `QrCodePayload` | `varchar(1000)` | |
| `IssuedAtUtc` | `datetime2` | |
| `Status` | `varchar(20)` | `Issued`, `Used`, `Cancelled`, `Refunded` |

## 7.5 Promotion tables

### `movies.Promotions`

| Column | Type | Notes |
|---|---|---|
| `Id` | `uuid` | PK |
| `Code` | `varchar(50)` | unique |
| `Title` | `varchar(255)` | |
| `Description` | `text` | |
| `Category` | `varchar(30)` | `Ticket`, `Combo`, `Member`, `Bank`, `Weekend` |
| `Status` | `varchar(20)` | `Draft`, `Active`, `Inactive`, `Expired` |
| `Priority` | `int` | |
| `StartAtUtc` | `datetime2` | |
| `EndAtUtc` | `datetime2` | nullable |
| `BannerImageUrl` | `varchar(1000)` | nullable |
| `BadgeLabel` | `varchar(100)` | |
| `TermsText` | `text` | |
| `CreatedAtUtc` | `datetime2` | |
| `UpdatedAtUtc` | `datetime2` | |

### `movies.PromotionRules`

Lưu rule linh hoạt, tránh hard-code hết vào DB design.

| Column | Type | Notes |
|---|---|---|
| `Id` | `uuid` | PK |
| `PromotionId` | `uuid` | FK |
| `RuleType` | `varchar(50)` | ví dụ `MinSeatCount`, `WeekendOnly`, `PaymentMethod`, `MorningShowtime`, `BirthdayMonth`, `ComboRequired`, `ExcludeHallType` |
| `Operator` | `varchar(20)` | `Equals`, `In`, `Gte`, `Lte` |
| `RuleValue` | `varchar(500)` | |
| `DisplayMessage` | `varchar(500)` | |

### `movies.PromotionRedemptions`

| Column | Type | Notes |
|---|---|---|
| `Id` | `uuid` | PK |
| `PromotionId` | `uuid` | FK |
| `BookingId` | `uuid` | FK |
| `GuestCustomerId` | `uuid` | nullable |
| `DiscountAmount` | `decimal(18,2)` | |
| `BonusSnapshotJson` | `nvarchar(max)` | |
| `RedeemedAtUtc` | `datetime2` | |

## 7.6 Combo and payment tables

### `movies.SnackCombos`

| Column | Type | Notes |
|---|---|---|
| `Id` | `uuid` | PK |
| `Code` | `varchar(50)` | unique |
| `Name` | `varchar(255)` | |
| `Description` | `text` | |
| `BasePrice` | `decimal(18,2)` | |
| `PromoPrice` | `decimal(18,2)` | nullable |
| `IsActive` | `bit` | |
| `DisplayOrder` | `int` | |

### `movies.Payments`

| Column | Type | Notes |
|---|---|---|
| `Id` | `uuid` | PK |
| `BookingId` | `uuid` | FK |
| `Provider` | `varchar(20)` | `MoMo`, `VNPay`, `Visa`, `ATM` |
| `ProviderTransactionId` | `varchar(100)` | nullable |
| `Amount` | `decimal(18,2)` | |
| `Currency` | `varchar(10)` | `VND` |
| `Status` | `varchar(20)` | `Pending`, `Succeeded`, `Failed`, `Cancelled`, `Refunded` |
| `RequestedAtUtc` | `datetime2` | |
| `CompletedAtUtc` | `datetime2` | nullable |
| `RawRequestJson` | `nvarchar(max)` | nullable |
| `RawResponseJson` | `nvarchar(max)` | nullable |

## 7.7 Audit and integration tables

### `movies.OutboxEvents`

Phục vụ gửi email, callback, sync analytics mà không mất event.

| Column | Type | Notes |
|---|---|---|
| `Id` | `uuid` | PK |
| `AggregateType` | `varchar(100)` | |
| `AggregateId` | `uuid` | |
| `EventType` | `varchar(100)` | |
| `PayloadJson` | `nvarchar(max)` | |
| `Status` | `varchar(20)` | `Pending`, `Processed`, `Failed` |
| `CreatedAtUtc` | `datetime2` | |
| `ProcessedAtUtc` | `datetime2` | nullable |

### `movies.AuditLogs`

| Column | Type | Notes |
|---|---|---|
| `Id` | `uuid` | PK |
| `Module` | `varchar(50)` | `Movies` |
| `Action` | `varchar(100)` | |
| `EntityType` | `varchar(100)` | |
| `EntityId` | `uuid` | nullable |
| `ActorUserId` | `uuid` | nullable |
| `MetadataJson` | `nvarchar(max)` | |
| `CreatedAtUtc` | `datetime2` | |

## 8. Quan hệ dữ liệu

### 8.1 Quan hệ chính

- `Movies 1 - n Showtimes`
- `Cinemas 1 - n Halls`
- `Halls 1 - n HallSeats`
- `Showtimes 1 - n ShowtimeSeatInventory`
- `GuestCustomers 1 - n Bookings`
- `Showtimes 1 - n Bookings`
- `Bookings 1 - n BookingItems`
- `Bookings 1 - n Tickets`
- `Bookings 1 - n Payments`
- `Promotions 1 - n PromotionRules`
- `Promotions 1 - n PromotionRedemptions`
- `Movies n - n Genres`
- `Movies n - n People` qua `MovieCredits`

### 8.2 Quan hệ booking thực tế

1. User chọn `Showtime`
2. Backend đọc `ShowtimeSeatInventory`
3. Tạo `BookingHold`
4. Lock các ghế bằng cách cập nhật `ShowtimeSeatInventory.Status = Held`
5. User checkout thành công
6. Tạo `Booking`
7. Tạo `BookingItems`
8. Tạo `Payment`
9. Payment success thì:
   - `Booking.Status = Confirmed`
   - `ShowtimeSeatInventory.Status = Booked`
   - Tạo `Tickets`
   - Ghi `PromotionRedemptions`
   - Đẩy `OutboxEvents` gửi email vé

## 9. Gợi ý payload response cho frontend

### 9.1 `GET /api/showtimes`

Frontend hiện cần cấu trúc gần như:

```json
[
  {
    "movie": {
      "id": "movie_01",
      "title": "Cosmic Odyssey",
      "genre": "Sci-Fi, Adventure",
      "rating": 8.5,
      "duration": "148 min",
      "language": "English with Vietnamese subtitles",
      "ageRating": "T13",
      "imageUrl": "https://..."
    },
    "cinemaSchedules": [
      {
        "cinemaId": "cinema_01",
        "cinemaName": "ABCD Cinema - ABCD Mall",
        "cinemaAddress": "Level 5, ABCD Mall...",
        "showtimes": [
          {
            "id": "showtime_01",
            "time": "19:30",
            "hallType": "IMAX",
            "language": "sub",
            "availableSeats": 9,
            "totalSeats": 240,
            "priceFrom": 140000
          }
        ]
      }
    ]
  }
]
```

### 9.2 `GET /api/showtimes/{showtimeId}/seat-map`

```json
{
  "showtime": {
    "id": "showtime_01",
    "movieId": "movie_01",
    "cinemaId": "cinema_01",
    "hallType": "IMAX",
    "startAt": "2026-04-09T19:30:00+07:00"
  },
  "pricing": {
    "basePrice": 140000,
    "serviceFeePerSeat": 10000,
    "seatSurcharges": {
      "regular": 0,
      "vip": 35000,
      "couple": 10000
    }
  },
  "layout": {
    "rows": ["A","B","C","D","E","F","G","H"],
    "columns": 12,
    "aisleAfter": 6
  },
  "seats": [
    {
      "seatId": "seat_inv_01",
      "seatCode": "D5",
      "row": "D",
      "col": 5,
      "seatType": "vip",
      "status": "available",
      "price": 175000,
      "coupleGroupCode": null
    }
  ]
}
```

### 9.3 `POST /api/bookings/quote`

```json
{
  "holdId": "hold_01",
  "seatSubtotal": 350000,
  "serviceFeeTotal": 20000,
  "comboSubtotal": 85000,
  "discountTotal": 60000,
  "grandTotal": 395000,
  "promotion": {
    "promotionId": "f2",
    "status": "applied",
    "message": "30% MoMo discount has been applied."
  },
  "lines": [
    { "type": "Seat", "code": "D5", "amount": 175000 },
    { "type": "Seat", "code": "D6", "amount": 175000 },
    { "type": "Fee", "code": "SERVICE_FEE", "amount": 20000 },
    { "type": "Combo", "code": "combo-gold", "amount": 85000 },
    { "type": "Discount", "code": "f2", "amount": -60000 }
  ]
}
```

## 10. Thứ tự code trong backend theo layer

## 10.1 Domain trước

1. Enum:
   - `MovieStatus`
   - `ShowtimeStatus`
   - `SeatType`
   - `SeatInventoryStatus`
   - `BookingStatus`
   - `PaymentStatus`
   - `PaymentProvider`
   - `PromotionStatus`
   - `PromotionRuleType`
2. Entities:
   - Movie
   - Genre
   - Cinema
   - Hall
   - HallSeat
   - Showtime
   - ShowtimeSeatInventory
   - GuestCustomer
   - BookingHold
   - Booking
   - BookingItem
   - Ticket
   - Promotion
   - PromotionRule
   - PromotionRedemption
   - SnackCombo
   - Payment
   - OutboxEvent

## 10.2 Application sau

1. Query services:
   - movie catalog
   - showtime query
   - seat map query
   - promotions query
2. Command services:
   - create hold
   - release hold
   - evaluate promotion
   - create quote
   - create booking
   - create payment intent
   - confirm payment
   - resend ticket

## 10.3 Infrastructure

1. EF Core entity configurations
2. Repositories
3. Payment gateway adapters
4. Email sender
5. Background worker:
   - expire holds
   - process outbox

## 10.4 API controllers

1. `MoviesController`
2. `ShowtimesController`
3. `PromotionsController`
4. `SnackCombosController`
5. `BookingsController`
6. `PaymentsController`
7. `AdminMoviesController`
8. `AdminShowtimesController`
9. `AdminPromotionsController`
10. `AdminBookingsController`

## 11. Kế hoạch dự phòng để các module khác cùng tạo table trên chung DB mà không conflict

Đây là phần quan trọng nhất cho repo hiện tại vì đang có nhiều module như `Users`, `Shops`, `FoodCourt`, `Feedbacks`, `Marketing`, `UtilityMap`, `Movies`.

### 11.1 Nguyên tắc 1: mỗi module có schema riêng

Ví dụ:

- `users.*`
- `shops.*`
- `foodcourt.*`
- `feedbacks.*`
- `marketing.*`
- `movies.*`

Lợi ích:

- Tránh trùng tên bảng
- Dễ backup theo module
- Dễ phân quyền DB
- Dễ đọc migration

### 11.2 Nguyên tắc 2: không cho module này tạo FK cứng sang table nội bộ module khác nếu chưa ổn định

Ví dụ:

- `movies.GuestCustomers.LinkedUserId` có thể lưu `Users.Id`
- Nhưng giai đoạn đầu không nên bắt buộc FK cứng sang `users.Users`

Khuyến nghị:

- Với integration chéo module, ưu tiên:
  - lưu `ExternalId`
  - lưu snapshot dữ liệu cần thiết
  - hoặc dùng event/outbox để sync

### 11.3 Nguyên tắc 3: chia migration theo module

Khuyến nghị:

- Mỗi module có folder migration riêng
- Ví dụ:
  - `BACKEND/ABCDMall.Modules/Movies/.../Persistence/Migrations`
  - `BACKEND/ABCDMall.Modules/Users/.../Persistence/Migrations`

Nếu vẫn dùng một `AppDbContext`, ít nhất phải tách namespace migration:

- `Migrations/Movies`
- `Migrations/Users`

### 11.4 Nguyên tắc 4: đặt convention tên index, FK, unique key theo module

Ví dụ:

- `PK_movies_Movies`
- `FK_movies_Showtimes_movies_Movies_MovieId`
- `IX_movies_Showtimes_BusinessDate_CinemaId`

Lợi ích:

- Tránh tên constraint bị trùng
- Dễ đọc lỗi migration

### 11.5 Nguyên tắc 5: mỗi module chỉ expose configuration của chính nó

Đề xuất trong `AppDbContext`:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(MoviesAssemblyMarker).Assembly);
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(UsersAssemblyMarker).Assembly);
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(FoodCourtAssemblyMarker).Assembly);
}
```

Và mỗi module tự quản entity config của nó.

### 11.6 Nguyên tắc 6: có bảng lock mềm cho seat booking, không dùng suy luận từ booking status

Với Movies, conflict lớn nhất là double-booking.

Phải có:

- `ShowtimeSeatInventory.Version`
- transaction khi hold seat
- update theo optimistic concurrency hoặc `UPDLOCK`

### 11.7 Nguyên tắc 7: dùng Outbox cho nghiệp vụ chéo module

Ví dụ:

- Booking confirmed -> gửi event để module Marketing ghi nhận campaign conversion
- Booking confirmed -> module Feedbacks có thể mở review window
- Booking confirmed -> module Users có thể cộng loyalty point nếu sau này có login

Không nên gọi trực tiếp module khác trong transaction chính của Movies.

### 11.8 Nguyên tắc 8: quy ước ownership rõ ràng

Module nào sở hữu bảng nào phải rõ:

- Movies sở hữu:
  - movies.Movies
  - movies.Showtimes
  - movies.Bookings
  - movies.Payments
  - movies.Promotions
- Users sở hữu:
  - users.Users
  - users.Roles
- FoodCourt sở hữu:
  - foodcourt.*


Movies chỉ tham chiếu `UserId` khi cần, không tự sửa bảng `users`.

## 12. Đề xuất AppDbContext để tránh conflict

Vì hiện tại `AppDbContext` đang gần như rỗng, nên hướng an toàn là:

### Option A: một `AppDbContext` chung, entity config tách theo module

Phù hợp nếu team nhỏ và một DB duy nhất.

Ưu điểm:

- Dễ query join khi cần
- Dễ transaction cross-module

Nhược điểm:

- Migration dễ conflict nếu nhiều người cùng generate

### Option B: mỗi module có DbContext riêng nhưng cùng connection string

Ví dụ:

- `MoviesDbContext`
- `UsersDbContext`
- `FoodCourtDbContext`

Ưu điểm:

- Mỗi module quản migration riêng
- Giảm conflict code-first migration

Nhược điểm:

- Query cross-module khó hơn

Khuyến nghị cho repo này:

- Nếu team còn đang phát triển song song nhiều module, nên dùng `DbContext` riêng cho từng module nhưng cùng DB
- Dùng schema riêng cho từng module

Đây là phương án ít conflict nhất.

## 13. Seed data tối thiểu nên có

Để frontend hoạt động sớm, nên seed:

- 4 cinema
- 4 hall type
- 1 seat template cho mỗi hall
- 6 now showing movies
- 3 coming soon movies
- 7 ngày showtime mẫu
- 3 snack combos
- 8 promotions

## 14. Rủi ro và lưu ý triển khai

### 14.1 Seat hold timeout

- Cần background job giải phóng hold hết hạn
- Nếu không, ghế sẽ bị kẹt `Held`

### 14.2 Promotion không nên chỉ hard-code trong frontend

- Frontend hiện đang mock logic khá nhiều
- Backend nên là source of truth cho:
  - eligibility
  - discount amount
  - bonus item

### 14.3 Payment callback phải idempotent

- Callback provider có thể bắn nhiều lần
- Phải check `ProviderTransactionId`

### 14.4 Booking code và ticket code phải unique

- Có unique index
- Có retry generate nếu trùng

### 14.5 Couple seats

- Không được cho book lẻ nếu business rule yêu cầu đi cặp
- Nên kiểm tra theo `CoupleGroupCode`

## 15. Kết luận triển khai ngắn gọn

Nếu làm theo độ ưu tiên thực tế:

1. Dựng schema bảng cốt lõi:
   - Movies
   - Cinemas
   - Halls
   - HallSeats
   - Showtimes
   - ShowtimeSeatInventory
   - GuestCustomers
   - BookingHolds
   - Bookings
   - BookingItems
   - Tickets
   - Promotions
   - PromotionRules
   - SnackCombos
   - Payments
2. Code read endpoints cho frontend bỏ mock data
3. Code hold seat + quote + evaluate promotion
4. Code create booking + payment flow
5. Code admin CRUD
6. Tách migration theo module và dùng schema `movies` để tránh conflict với các module khác

## 16. Mapping nhanh từ frontend sang backend

| Frontend page | Backend chính cần có |
|---|---|
| `MovieHomePage` | `GET /api/movies/home` |
| `MovieDetailPage` | `GET /api/movies/{movieId}`, `GET /api/movies/{movieId}/showtimes` |
| `SchedulePage` | `GET /api/showtimes` |
| `SeatSelectionPage` | `GET /api/showtimes/{showtimeId}/seat-map`, `POST /api/bookings/holds`, `POST /api/bookings/quote`, `POST /api/movies-promotions/evaluate` |
| `CheckoutPage` | `POST /api/bookings`, `POST /api/payments/intents`, `POST /api/payments/callback/{provider}` |
| `PromotionsPage` | `GET /api/movies-promotions`, `GET /api/movies-promotions/{promotionId}` |

