# Movie Backend Work Split

## 1. Mục tiêu

Tài liệu này chia việc cho 2 dev backend triển khai module `Movies` dựa trên:

- [Movie_design_document.md](D:\HK3\API+Cloud\Eprojectsem3\ABCDMall\BACKEND\Movie_design_document.md)
- Hướng triển khai `Option B` ở mục 12:
  - nhiều `DbContext`
  - dùng chung một database
  - schema riêng `movies`
  - migration tách riêng để giảm conflict

Mục tiêu chia việc:

- mỗi dev có ownership rõ ràng
- giảm conflict khi code song song
- frontend có thể tích hợp dần
- booking flow vẫn ghép được với showtime flow
- feedbacks/reviews cho từng phim có owner rõ ràng

## 2. Nguyên tắc chia việc

### 2.1 Chia theo nghiệp vụ, không chia theo file lẻ

Không nên chia kiểu:

- mỗi người sửa vài controller
- mỗi người sửa chung `AppDbContext`
- mỗi người tạo migration trong cùng một chỗ

Nên chia theo chiều dọc:

- Dev 1: `Catalog + Screening`
- Dev 2: `Booking + Promotion + Payment + Feedback`

### 2.2 Dùng Option B

Hai `DbContext` dùng chung 1 database:

- `MoviesCatalogDbContext`
- `MoviesBookingDbContext`

Khuyến nghị:

- cả 2 cùng dùng schema `movies`
- migration tách folder riêng
- không sửa migration của nhau

### 2.3 Shared contract phải chốt sớm

Hai dev phải thống nhất trước:

- enum dùng chung
- format ID
- response model public
- quy ước trạng thái ghế
- quy ước trạng thái booking
- quy ước promotion status

## 3. Cấu trúc đề xuất

## 3.1 DbContext

### Dev 1 quản

- `MoviesCatalogDbContext`

### Dev 2 quản

- `MoviesBookingDbContext`

## 3.2 Gợi ý folder

### Dev 1

- `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Domain`
  - `Entities/Catalog/*`
  - `Entities/Screening/*`
  - `Enums/*`
- `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application`
  - `Movies/Queries/*`
  - `Showtimes/Queries/*`
- `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure`
  - `Persistence/Catalog/MoviesCatalogDbContext.cs`
  - `Persistence/Catalog/Configurations/*`
  - `Persistence/Catalog/Migrations/*`
  - `Repositories/Catalog/*`
  - `Repositories/Screening/*`

### Dev 2

- `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Domain`
  - `Entities/Booking/*`
  - `Entities/Promotion/*`
  - `Entities/Payment/*`
  - `Enums/*`
- `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application`
  - `Bookings/Commands/*`
  - `Bookings/Queries/*`
  - `Promotions/*`
  - `Payments/*`
  - `Feedbacks/*`
- `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure`
  - `Persistence/Booking/MoviesBookingDbContext.cs`
  - `Persistence/Booking/Configurations/*`
  - `Persistence/Booking/Migrations/*`
  - `Repositories/Booking/*`
  - `Repositories/Promotions/*`
  - `Repositories/Payments/*`
  - `Repositories/Feedbacks/*`
  - `Integrations/Payments/*`
  - `Integrations/Email/*`

## 4. Chia việc cụ thể cho 2 dev

## 4.1 Dev 1: Catalog + Screening Owner

### 4.1.1 Responsibility chính

Dev 1 sở hữu toàn bộ phần dữ liệu để frontend có thể:

- load homepage movies
- xem chi tiết phim
- xem showtimes
- xem seat map

Dev 1 không sở hữu:

- booking hold
- booking quote
- payment
- promotion redemption
- booking admin operations
- payment admin operations

### 4.1.2 Bảng Dev 1 chịu trách nhiệm

- `movies.Movies`
- `movies.Genres`
- `movies.MovieGenres`
- `movies.People`
- `movies.MovieCredits`
- `movies.Cinemas`
- `movies.Halls`
- `movies.HallSeats`
- `movies.Showtimes`
- `movies.ShowtimeSeatInventory`

### 4.1.3 Entity Dev 1 code

- `Movie`
- `Genre`
- `MovieGenre`
- `Person`
- `MovieCredit`
- `Cinema`
- `Hall`
- `HallSeat`
- `Showtime`
- `ShowtimeSeatInventory`

### 4.1.4 Enum Dev 1 code

- `MovieStatus`
- `HallType`
- `LanguageType`
- `SeatType`
- `ShowtimeStatus`
- `SeatInventoryStatus`

### 4.1.5 API Dev 1 code

- `GET /api/movies/home`
- `GET /api/movies`
- `GET /api/movies/{movieId}`
- `GET /api/movies/{movieId}/showtimes`
- `GET /api/showtimes`
- `GET /api/showtimes/{showtimeId}`
- `GET /api/showtimes/{showtimeId}/seat-map`

### 4.1.6 Output Dev 1 phải bàn giao cho Dev 2

- `showtimeId` là key chuẩn cho booking flow
- `seat inventory` trả về danh sách ghế theo showtime
- rule seat map:
  - seat code
  - seat type
  - current status
  - price
  - couple group
- query chuẩn để Dev 2 lấy:
  - showtime có tồn tại không
  - ghế nào đang available
  - giá ghế theo showtime

### 4.1.7 Phase Dev 1

#### Phase A

- tạo `MoviesCatalogDbContext`
- tạo schema catalog + screening
- seed movie/cinema/hall/seat/showtime

#### Phase B

- làm read API cho movies
- làm read API cho showtimes

#### Phase C

- làm `seat-map`
- đảm bảo seat inventory đủ để Dev 2 giữ ghế

#### Phase D

- làm admin CRUD cho:
  - movies
  - cinemas
  - halls
  - hall seats
  - showtimes
- làm admin read/search/filter cho catalog + screening
- giữ riêng admin endpoint, không trộn vào public customer API

## 4.2 Dev 2: Booking + Promotion + Payment + Feedback Owner

### 4.2.1 Responsibility chính

Dev 2 sở hữu toàn bộ phần:

- booking hold
- quote
- promotion evaluation
- checkout
- payment
- ticket issue
- feedback/review của khách cho từng movie

Dev 2 không sở hữu:

- movie catalog
- cinema/hall setup
- showtime generation
- catalog admin CRUD
- screening admin CRUD

### 4.2.2 Bảng Dev 2 chịu trách nhiệm

- `movies.GuestCustomers`
- `movies.BookingHolds`
- `movies.BookingHoldSeats`
- `movies.Bookings`
- `movies.BookingItems`
- `movies.Tickets`
- `movies.Promotions`
- `movies.PromotionRules`
- `movies.PromotionRedemptions`
- `movies.SnackCombos`
- `movies.Payments`
- `movies.OutboxEvents`
- `movies.AuditLogs`
- `movies.MovieFeedbacks`

### 4.2.3 Entity Dev 2 code

- `GuestCustomer`
- `BookingHold`
- `BookingHoldSeat`
- `Booking`
- `BookingItem`
- `Ticket`
- `Promotion`
- `PromotionRule`
- `PromotionRedemption`
- `SnackCombo`
- `Payment`
- `OutboxEvent`
- `AuditLog`
- `MovieFeedback`

### 4.2.4 Enum Dev 2 code

- `BookingHoldStatus`
- `BookingStatus`
- `PaymentStatus`
- `PaymentProvider`
- `PromotionStatus`
- `PromotionRuleType`
- `PromotionEvaluationStatus`

### 4.2.5 API Dev 2 code

- `GET /api/movies-promotions`
- `GET /api/movies-promotions/{promotionId}`
- `POST /api/movies-promotions/evaluate`
- `GET /api/snack-combos`
- `GET /api/movies/{movieId}/feedbacks`
- `POST /api/movies/{movieId}/feedbacks`
- `POST /api/bookings/holds`
- `GET /api/bookings/holds/{holdId}`
- `DELETE /api/bookings/holds/{holdId}`
- `POST /api/bookings/quote`
- `POST /api/bookings`
- `GET /api/bookings/{bookingCode}`
- `POST /api/bookings/{bookingId}/resend-ticket`
- `POST /api/payments/intents`
- `POST /api/payments/callback/{provider}`
- `GET /api/payments/{paymentId}`

### 4.2.6 Input Dev 2 cần lấy từ Dev 1

- `showtimeId`
- `Showtime`
- `ShowtimeSeatInventory`
- hall type
- seat type
- seat price
- seat availability

### 4.2.7 Phase Dev 2

#### Phase A

- tạo `MoviesBookingDbContext`
- tạo bảng promotion, snack combo, booking, payment
- seed promotion + snack combo

#### Phase B

- làm `promotion evaluate`
- làm `booking quote`

#### Phase C

- làm `booking hold`
- làm expire hold

#### Phase D

- làm create booking
- làm payment intent
- làm callback
- phát hành ticket

#### Phase E

- làm admin management cho:
  - promotions
  - snack combos
  - bookings
  - payments
  - ticket resend
  - audit/log operational views
- làm admin actions cho booking/payment lifecycle nếu business cần

#### Phase F

- làm feedback flow:
  - movie feedback list
  - create feedback
  - validate booking/showtime eligibility nếu business yêu cầu
  - admin moderation/read nếu cần

## 5. Shared items cả 2 cùng chốt trước

## 5.1 Shared enum

Hai dev phải thống nhất một lần rồi không tự đổi riêng:

- `HallType`
  - `2D`
  - `3D`
  - `IMAX`
  - `4DX`
- `SeatType`
  - `Regular`
  - `VIP`
  - `Couple`
- `LanguageType`
  - `Sub`
  - `Dub`

## 5.2 Shared key contract

- `movieId`: `uuid`
- `showtimeId`: `uuid`
- `cinemaId`: `uuid`
- `hallId`: `uuid`
- `seatInventoryId`: `uuid`
- `promotionId`: `uuid`
- `bookingId`: `uuid`
- `paymentId`: `uuid`

## 5.3 Shared API contract

Dev 1 và Dev 2 phải cùng hiểu chính xác:

- `seat-map` response shape
- `quote` request shape
- `promotion evaluate` request shape
- `booking create` request shape

## 5.4 Shared rule kỹ thuật

- timezone mặc định: `Asia/Ho_Chi_Minh`
- money dùng `decimal(18,2)`
- datetime lưu UTC, trả thêm local nếu cần
- mọi status dùng enum string rõ nghĩa
- mọi booking/payment callback phải idempotent

## 6. Việc cần làm chung trước khi code

## 6.1 Task kickoff chung

1. Chốt tên 2 `DbContext`
2. Chốt schema `movies`
3. Chốt naming convention:
   - table
   - index
   - foreign key
4. Chốt enum
5. Chốt DTO public
6. Chốt branch strategy

## 6.2 File/area chỉ nên 1 người sửa chính

### Dev 1 owner

- `MoviesCatalogDbContext`
- catalog entity configs
- screening entity configs
- showtime query handlers
- movies/showtimes controllers
- admin controllers cho:
  - movies
  - cinemas
  - halls
  - showtimes

### Dev 2 owner

- `MoviesBookingDbContext`
- booking/promotion/payment configs
- booking command handlers
- promotions controller
- bookings controller
- payments controller
- feedback services/controllers/repositories
- admin controllers cho:
  - promotions
  - snack combos
  - bookings
  - payments
  - ticket/admin operations

### Cả hai chỉ chạm khi đã thống nhất

- `Program.cs`
- DI registration
- shared enums
- shared DTO project nếu có

## 7. Kế hoạch triển khai theo sprint

## Sprint 1

### Dev 1

- tạo `MoviesCatalogDbContext`
- code bảng catalog/screening
- migration catalog
- seed movie/cinema/hall/seat/showtime
- `GET /api/movies`
- `GET /api/movies/{movieId}`
- `GET /api/showtimes`

### Dev 2

- tạo `MoviesBookingDbContext`
- code bảng promotion/snack/booking/payment
- migration booking
- seed promotions + snack combos
- `GET /api/movies-promotions`
- `GET /api/snack-combos`
- `POST /api/movies-promotions/evaluate`

### Deliverable sprint 1

- frontend có thể gọi movie list, movie detail, showtimes, promotions
- promotion evaluation có thể test độc lập

## Sprint 2

### Dev 1

- `GET /api/movies/home`
- `GET /api/movies/{movieId}/showtimes`
- `GET /api/showtimes/{showtimeId}`
- `GET /api/showtimes/{showtimeId}/seat-map`

### Dev 2

- `POST /api/bookings/quote`
- `POST /api/bookings/holds`
- `GET /api/bookings/holds/{holdId}`
- `DELETE /api/bookings/holds/{holdId}`
- background job expire hold

### Deliverable sprint 2

- seat selection page có thể chạy với API thật
- quote/hold hoạt động được

## Sprint 3

### Dev 1

- fix/filter performance showtime query
- tối ưu index cho `Showtimes` và `ShowtimeSeatInventory`

### Dev 2

- `POST /api/bookings`
- `POST /api/payments/intents`
- `POST /api/payments/callback/{provider}`
- `GET /api/payments/{paymentId}`
- `GET /api/bookings/{bookingCode}`
- issue ticket + outbox email
- `GET /api/movies/{movieId}/feedbacks`
- `POST /api/movies/{movieId}/feedbacks`

### Deliverable sprint 3

- checkout hoàn chỉnh
- payment flow hoàn chỉnh
- booking success và có vé
- feedback movie có thể đọc/ghi qua API thật

## Sprint 4

### Dev 1

- admin CRUD:
  - `POST /api/admin/movies`
  - `PUT /api/admin/movies/{movieId}`
  - `DELETE /api/admin/movies/{movieId}`
  - `POST /api/admin/showtimes`
  - `PUT /api/admin/showtimes/{showtimeId}`
  - `DELETE /api/admin/showtimes/{showtimeId}`
- nếu cần, bổ sung admin APIs cho `cinemas`, `halls`, `hall-seats`

### Dev 2

- admin management:
  - `POST /api/admin/movies-promotions`
  - `PUT /api/admin/movies-promotions/{promotionId}`
  - `DELETE /api/admin/movies-promotions/{promotionId}`
  - `POST /api/admin/snack-combos`
  - `PUT /api/admin/snack-combos/{comboId}`
  - `DELETE /api/admin/snack-combos/{comboId}`
  - `GET /api/admin/bookings`
  - `GET /api/admin/payments`
- admin actions cho support/demo:
  - resend ticket
  - view audit/payment/booking status
  - feedback moderation/read nếu cần

### Deliverable sprint 4

- admin portal có API thật cho phần Movies
- ownership vẫn tách rõ theo `Catalog + Screening` và `Booking + Promotion + Payment`

## 8. Quy tắc tránh conflict

## 8.1 Migration

- Dev 1 chỉ tạo migration trong:
  - `Persistence/Catalog/Migrations`
- Dev 2 chỉ tạo migration trong:
  - `Persistence/Booking/Migrations`

Không sửa migration của nhau.

## 8.2 Database ownership

- Dev 1 không sửa bảng booking/payment/promotion
- Dev 2 không sửa bảng movie/cinema/hall/showtime

Nếu cần thêm field chéo:

- mở issue nhỏ
- thống nhất trước
- owner bảng đó là người thêm

## 8.3 Branch strategy

Khuyến nghị:

- Dev 1:
  - `movies/catalog-screening-*`
- Dev 2:
  - `movies/booking-payment-*`

## 8.4 Merge order

Thứ tự merge an toàn:

1. shared enums + shared DTO
2. Dev 1 catalog/screening base
3. Dev 2 promotion/quote base
4. Dev 1 seat map
5. Dev 2 hold/booking/payment

## 9. Checklist giao tiếp giữa 2 dev

Mỗi ngày nên sync nhanh 10-15 phút:

- hôm nay API nào đã xong
- request/response nào đổi
- field nào cần thêm
- migration nào chuẩn bị tạo
- chỗ nào có nguy cơ conflict

Checklist trước khi merge:

- đã rebase/pull nhánh mới nhất chưa
- có sửa file owner của người kia không
- có đổi enum shared không
- API contract có thay đổi không
- migration có đụng schema ngoài vùng ownership không

## 10. Giao việc ngắn gọn

### Dev 1 nhận

- dựng `MoviesCatalogDbContext`
- làm catalog + cinema + hall + seat + showtime + seat inventory
- làm toàn bộ read API cho movies/showtimes/seat-map
- làm admin CRUD cho catalog + screening

### Dev 2 nhận

- dựng `MoviesBookingDbContext`
- làm promotion + snack combo + booking + hold + payment + ticket + feedback
- làm evaluate/quote/booking/payment/feedback flow
- làm admin management cho promotion + snack combo + booking + payment + feedback

## 11. Kết luận

Cách chia hợp lý nhất cho 2 dev backend trong case này là:

- Dev 1 làm phần dữ liệu nền và read API
- Dev 2 làm phần transaction, business flow và feedback

Nếu mở thêm admin portal cho Movies thì chia tiếp như sau:

- Dev 1 làm admin CRUD cho `Catalog + Screening`
- Dev 2 làm admin management cho `Booking + Promotion + Payment + Feedback`

Theo `Option B`, đây là cách ít conflict nhất vì:

- mỗi người có `DbContext` riêng
- mỗi người có migration riêng
- mỗi người sở hữu một cụm bảng riêng
- chỉ cần thống nhất contract ở ranh giới `showtime -> booking`

Nếu muốn mở rộng thêm sau này:

- có thể tách tiếp `Promotion` thành submodule riêng
- hoặc tách `Payment` thành integration module riêng mà không phá phần catalog/screening
