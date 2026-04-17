# Movie Dev2 Execution Plan

## 1. Vai trò và phạm vi

- Tôi đảm nhận `Dev 2` cho backend module `Movies`
- Phần tôi phụ trách:
  - `Booking`
  - `Promotion`
  - `Payment`
  - `Ticket`
  - `Feedback / Review`
  - `Outbox / Email`
  - `Movies admin management` cho `Promotion + SnackCombo + Booking + Payment + Feedback`
- Tôi không phụ trách:
  - `Movie Catalog`
  - `Cinema / Hall / Seat template`
  - `Showtime generation`
  - `Seat map read API`
  - admin CRUD cho movie/cinema/hall/showtime

Tài liệu này dùng làm file handoff để mở cuộc trò chuyện mới vẫn tiếp tục làm việc được ngay.

## 2. Tài liệu nền đã chốt

Các tài liệu liên quan:

- [Movie_design_document.md](D:\HK3\API+Cloud\Eprojectsem3\ABCDMall\BACKEND\Movie_design_document.md)
- [Movie_backend_work_split.md](D:\HK3\API+Cloud\Eprojectsem3\ABCDMall\BACKEND\Movie_backend_work_split.md)

## 3. Kiến trúc đã chốt

### 3.1 Kiến trúc module

Áp dụng:

- `DI`
- `DTO`
- `EF Core Code First`
- `Migration`
- `AutoMapper`
- `FluentValidation`

### 3.2 Flow code

Chốt cách làm:

- code theo `flow nghiệp vụ`
- không code theo kiểu rải đều từng lớp kỹ thuật trước

Lý do:

- dễ hiểu bài hơn
- dễ bám frontend hơn
- dễ test từng use case
- dễ biết endpoint nào đang thiếu gì

### 3.3 Option B

Áp dụng `Option B` từ tài liệu thiết kế:

- nhiều `DbContext`
- dùng chung một database
- dùng schema `movies`
- migration tách riêng để giảm conflict

Tôi phụ trách:

- `MoviesBookingDbContext`

Dev 1 phụ trách:

- `MoviesCatalogDbContext`

### 3.4 Shared contract đã chốt

Contract giữa Dev 1 và Dev 2 đã chốt xong.

Các key dùng chung:

- `movieId`
- `showtimeId`
- `cinemaId`
- `hallId`
- `seatInventoryId`
- `promotionId`
- `bookingId`
- `paymentId`

Các enum/shared concept đã chốt:

- `HallType`
- `SeatType`
- `LanguageType`
- `seat status`
- `booking status`
- `promotion evaluation status`

### 3.5 Contract chi tiết với Dev 1

Dev 1 sở hữu `Catalog + Screening`, Dev 2 chỉ tiêu thụ dữ liệu từ Dev 1 cho flow booking.

#### Dữ liệu Dev 2 cần từ Dev 1

- `showtimeId` là khóa chuẩn cho flow booking
- `seatInventoryId` là khóa chuẩn cho ghế theo từng showtime
- `seatCode` chỉ dùng để hiển thị
- `seatType` dùng chung enum đã chốt
- `status` ghế dùng chung tập giá trị đã chốt
- `price` là source of truth để tính quote
- `coupleGroupCode` phải có nếu ghế thuộc nhóm couple

#### Dữ liệu tối thiểu từ showtime

Dev 2 cần đọc được:

- `showtimeId`
- `movieId`
- `cinemaId`
- `hallId`
- `hallType`
- `businessDate`
- `startAt`
- `basePrice`
- `status`

#### Dữ liệu tối thiểu từ seat-map

Mỗi ghế trong `seat-map` phải có:

- `seatInventoryId`
- `seatCode`
- `row`
- `col`
- `seatType`
- `status`
- `price`
- `coupleGroupCode`

#### Boundary API

Dev 1 cung cấp:

- `GET /api/showtimes`
- `GET /api/showtimes/{showtimeId}`
- `GET /api/showtimes/{showtimeId}/seat-map`

Dev 2 dựa vào đó để làm:

- `POST /api/movies-promotions/evaluate`
- `POST /api/bookings/quote`
- `POST /api/bookings/holds`
- `POST /api/bookings`
- `POST /api/payments/intents`

#### Rule ownership

- Dev 1 owner của:
  - `Showtime`
  - `ShowtimeSeatInventory`
  - response shape của `seat-map`
- Dev 2 owner của:
  - `BookingHold`
  - `Booking`
  - `Payment`
  - `Promotion evaluation`
  - `Quote`

Nếu cần thêm field vào `seat-map` hoặc đổi contract, phải sync lại với Dev 1 trước khi code.

## 4. Tech stack và package đã chốt

## 4.1 Domain

Không cài package.

Lý do:

- giữ domain sạch
- không phụ thuộc framework

## 4.2 Application

Packages:

- `FluentValidation`
- `AutoMapper`
- `AutoMapper.Extensions.Microsoft.DependencyInjection`

## 4.3 Infrastructure

Packages:

- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.Design`
- `Microsoft.EntityFrameworkCore.Tools`
- `Microsoft.Extensions.Caching.StackExchangeRedis`
- `MailKit`
- `MimeKit`
- `Dapper` (optional nhưng đã quyết định cài từ đầu cho query/report nếu cần)

## 4.4 WebAPI

Packages:

- `Swashbuckle.AspNetCore`
- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `FluentValidation.AspNetCore`

## 4.5 Công nghệ đã phân tích và quyết định

### Dùng ngay

- JWT
- EF Core
- AutoMapper
- FluentValidation
- Swagger
- Redis package
- MailKit

### Chưa dùng ngay

- Kafka
- RabbitMQ
- Hangfire

Lý do:

- hệ thống hiện chưa cần event bus nặng
- booking flow bản đầu có thể chạy tốt với DB + outbox + background service
- tránh tăng độ phức tạp quá sớm

### Redis

Trạng thái:

- cài package từ đầu
- chưa bắt buộc dùng cho booking hold ở version đầu

Source of truth cho booking hold vẫn là SQL + EF Core + concurrency control.

## 5. Project nào cài package nào

## 5.1 `ABCDMall.Modules.Movies.Domain`

Không cài package.

## 5.2 `ABCDMall.Modules.Movies.Application`

Cài:

```powershell
dotnet add BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/ABCDMall.Modules.Movies.Application.csproj package FluentValidation
dotnet add BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/ABCDMall.Modules.Movies.Application.csproj package AutoMapper
dotnet add BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/ABCDMall.Modules.Movies.Application.csproj package AutoMapper.Extensions.Microsoft.DependencyInjection
```

## 5.3 `ABCDMall.Modules.Movies.Infrastructure`

Cài:

```powershell
dotnet add BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/ABCDMall.Modules.Movies.Infrastructure.csproj package Microsoft.EntityFrameworkCore
dotnet add BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/ABCDMall.Modules.Movies.Infrastructure.csproj package Microsoft.EntityFrameworkCore.SqlServer
dotnet add BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/ABCDMall.Modules.Movies.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Design
dotnet add BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/ABCDMall.Modules.Movies.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Tools
dotnet add BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/ABCDMall.Modules.Movies.Infrastructure.csproj package Microsoft.Extensions.Caching.StackExchangeRedis
dotnet add BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/ABCDMall.Modules.Movies.Infrastructure.csproj package MailKit
dotnet add BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/ABCDMall.Modules.Movies.Infrastructure.csproj package MimeKit
dotnet add BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/ABCDMall.Modules.Movies.Infrastructure.csproj package Dapper
```

## 5.4 `ABCDMall.WebAPI`

Cài:

```powershell
dotnet add BACKEND/ABCDMall.WebAPI/ABCDMall.WebAPI.csproj package Swashbuckle.AspNetCore
dotnet add BACKEND/ABCDMall.WebAPI/ABCDMall.WebAPI.csproj package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add BACKEND/ABCDMall.WebAPI/ABCDMall.WebAPI.csproj package FluentValidation.AspNetCore
```

## 6. Ownership phần Dev 2

## 6.1 DbContext

Tôi phụ trách:

- `MoviesBookingDbContext`

## 6.2 Bảng tôi sở hữu

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

## 6.3 API tôi sở hữu

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
- admin APIs cho `Booking + Promotion + Payment + Feedback`:
  - `GET /api/admin/movies-promotions`
  - `POST /api/admin/movies-promotions`
  - `PUT /api/admin/movies-promotions/{promotionId}`
  - `DELETE /api/admin/movies-promotions/{promotionId}`
  - `GET /api/admin/snack-combos`
  - `POST /api/admin/snack-combos`
  - `PUT /api/admin/snack-combos/{comboId}`
  - `DELETE /api/admin/snack-combos/{comboId}`
  - `GET /api/admin/bookings`
  - `GET /api/admin/payments`
  - `GET /api/admin/movie-feedbacks`
  - admin support actions như resend ticket / view audit / feedback moderation

## 7. Cấu trúc code nên tạo

## 7.1 Domain

Tạo:

- `Enums/BookingHoldStatus.cs`
- `Enums/BookingStatus.cs`
- `Enums/PaymentProvider.cs`
- `Enums/PaymentStatus.cs`
- `Enums/PromotionStatus.cs`
- `Enums/PromotionRuleType.cs`

- `Entities/GuestCustomer.cs`
- `Entities/Promotion.cs`
- `Entities/PromotionRule.cs`
- `Entities/PromotionRedemption.cs`
- `Entities/SnackCombo.cs`
- `Entities/BookingHold.cs`
- `Entities/BookingHoldSeat.cs`
- `Entities/Booking.cs`
- `Entities/BookingItem.cs`
- `Entities/Payment.cs`
- `Entities/Ticket.cs`
- `Entities/MovieFeedback.cs`
- `Entities/OutboxEvent.cs`
- `Entities/AuditLog.cs`

## 7.2 Application

Tạo:

- `DTOs/Promotions/*`
- `DTOs/Bookings/*`
- `DTOs/Payments/*`
- `DTOs/Feedbacks/*`
- `Mappings/*`
- `Services/Promotions/*`
- `Services/Bookings/*`
- `Services/Payments/*`
- `Services/Feedbacks/*`

## 7.3 Infrastructure

Tạo:

- `Persistence/Booking/MoviesBookingDbContext.cs`
- `Persistence/Booking/Configurations/*`
- `Persistence/Booking/Migrations/*`
- `Seed/*`
- `Repositories/*`
- `Integrations/Email/*`
- `Integrations/Payments/*`
- `BackgroundServices/*`

## 8. Thứ tự code tổng thể

Code theo flow nghiệp vụ theo thứ tự:

1. `Promotion`
2. `Quote`
3. `Hold`
4. `Create Booking`
5. `Payment`
6. `Ticket + Email + Outbox`
7. `Feedback / Review flow`
8. `Admin management for Booking + Promotion + Payment + Feedback`

Trước khi vào flow phải làm nền:

1. folder structure
2. enums
3. entities
4. dbcontext
5. configurations
6. seed
7. migration
8. DI registration

## 9. Kế hoạch theo ngày

## Day 1: dựng nền project

Mục tiêu:

- có bộ khung code cho Dev 2
- có package đúng
- có domain cơ bản

Việc cần làm:

1. cài package cho `Application`, `Infrastructure`, `WebAPI`
2. tạo folder structure
3. tạo enums:
   - `BookingHoldStatus`
   - `BookingStatus`
   - `PaymentProvider`
   - `PaymentStatus`
   - `PromotionStatus`
   - `PromotionRuleType`
4. tạo entities:
   - `GuestCustomer`
   - `Promotion`
   - `PromotionRule`
   - `PromotionRedemption`
   - `SnackCombo`
   - `BookingHold`
   - `BookingHoldSeat`
   - `Booking`
   - `BookingItem`
   - `Payment`
   - `Ticket`
   - `MovieFeedback`
   - `OutboxEvent`
   - `AuditLog`

Deliverable cuối ngày:

- package cài xong
- compile được domain/application/infrastructure ở mức cơ bản
- tất cả entity và enum đã có file

## Day 2: DbContext + EF Core config + migration đầu tiên

Mục tiêu:

- hoàn tất persistence layer cho phần Dev 2

Việc cần làm:

1. tạo `MoviesBookingDbContext`
2. thêm `DbSet<>` cho toàn bộ bảng Dev 2
3. tạo `EntityTypeConfiguration` cho từng entity
4. chốt schema `movies`
5. tạo unique/index quan trọng:
   - `BookingCode`
   - `Promotion.Code`
   - `SnackCombo.Code`
   - `BookingHold.HoldCode`
   - `Payments.ProviderTransactionId` nếu cần
6. chuẩn bị seed data
7. tạo migration đầu tiên

Deliverable cuối ngày:

- migration đầu tiên chạy được
- DB sinh ra đủ bảng phần Dev 2

## Day 3: Seed dữ liệu và flow Promotion

Mục tiêu:

- xong phần dễ nhất nhưng có giá trị ngay cho frontend

Việc cần làm:

1. seed `SnackCombos`
2. seed `Promotions`
3. seed `PromotionRules`
4. tạo DTO:
   - `PromotionResponseDto`
   - `PromotionDetailResponseDto`
   - `EvaluatePromotionRequestDto`
   - `EvaluatePromotionResponseDto`
5. tạo AutoMapper profile cho promotions
6. tạo repository cho promotions
7. code `PromotionEvaluationService`
8. mở API:
   - `GET /api/movies-promotions`
   - `GET /api/movies-promotions/{promotionId}`
   - `POST /api/movies-promotions/evaluate`
9. thêm validation với `FluentValidation`

Deliverable cuối ngày:

- frontend có thể gọi promotions thật
- logic evaluate promotion chạy được

## Day 4: Flow Quote

Mục tiêu:

- bỏ mock pricing logic bên frontend

Việc cần làm:

1. tạo DTO:
   - `BookingQuoteRequestDto`
   - `BookingQuoteResponseDto`
   - `BookingQuoteLineDto`
2. code `BookingQuoteService`
3. tích hợp contract từ Dev 1:
   - lấy showtime
   - lấy seat inventory
   - đọc seat price / seat type / status
4. gọi lại `PromotionEvaluationService`
5. mở API:
   - `POST /api/bookings/quote`

Deliverable cuối ngày:

- quote tính được:
  - seat subtotal
  - service fee
  - combo subtotal
  - discount
  - grand total

## Day 5: Flow Hold Seat

Mục tiêu:

- có thể giữ ghế trước checkout

Việc cần làm:

1. tạo DTO:
   - `CreateBookingHoldRequestDto`
   - `CreateBookingHoldResponseDto`
2. code repository cho hold
3. code `BookingHoldService`
4. xử lý:
   - showtime tồn tại
   - ghế còn available
   - tạo hold
   - set expiry
5. mở API:
   - `POST /api/bookings/holds`
   - `GET /api/bookings/holds/{holdId}`
   - `DELETE /api/bookings/holds/{holdId}`
6. tạo cleanup background service cho hold expired

Deliverable cuối ngày:

- hold flow chạy được
- có thể release hold

## Day 6: Flow Create Booking

Mục tiêu:

- tạo booking chính thức từ hold

Việc cần làm:

1. tạo DTO:
   - `CreateBookingRequestDto`
   - `CreateBookingResponseDto`
2. code `BookingService`
3. logic:
   - verify hold
   - create or reuse `GuestCustomer`
   - tạo `Booking`
   - tạo `BookingItems`
   - lưu promotion snapshot
   - set trạng thái `PendingPayment`
4. mở API:
   - `POST /api/bookings`
   - `GET /api/bookings/{bookingCode}`

Deliverable cuối ngày:

- booking tạo được
- có booking code

## Day 7: Flow Payment

Mục tiêu:

- nối được payment lifecycle

Việc cần làm:

1. tạo DTO:
   - `CreatePaymentIntentRequestDto`
   - `CreatePaymentIntentResponseDto`
   - `PaymentStatusResponseDto`
2. tạo `IPaymentGateway`
3. tạo payment gateway mock hoặc adapter bước đầu
4. code `PaymentService`
5. mở API:
   - `POST /api/payments/intents`
   - `POST /api/payments/callback/{provider}`
   - `GET /api/payments/{paymentId}`
6. xử lý idempotency cho callback

Deliverable cuối ngày:

- payment record chạy được
- callback update được payment status

## Day 8: Ticket + Email + Outbox

Mục tiêu:

- hoàn tất flow booking success

Việc cần làm:

1. code `TicketService`
2. code `EmailService`
3. code `OutboxProcessor`
4. khi payment success:
   - issue ticket
   - mark booked seats
   - create outbox event
   - send e-ticket
5. mở API:
   - `POST /api/bookings/{bookingId}/resend-ticket`

Deliverable cuối ngày:

- booking success có ticket
- có email resend flow

## Day 9: Hardening

Mục tiêu:

- làm sạch code và giảm bug runtime

Việc cần làm:

1. bổ sung validation còn thiếu
2. thêm logging
3. rà lại transaction boundary
4. rà unique/index/concurrency
5. rà mapping AutoMapper
6. test các case:
   - hold expired
   - callback duplicate
   - promotion invalid
   - quote với seat không hợp lệ
   - booking khi hold không còn

Deliverable cuối ngày:

- flow ổn định hơn
- sẵn sàng tích hợp frontend

## Day 10: Movies Admin Management

Mục tiêu:

- có API thật cho frontend admin quản lý promotions, snack combos, bookings, payments
- có chỗ để quản lý feedback nếu admin UI cần

Việc cần làm:

1. tạo admin DTO cho:
   - promotion create/update
   - snack combo create/update
   - booking admin list/detail
   - payment admin list/detail
   - feedback admin list/moderation nếu cần
2. tạo validator cho admin request
3. code command/query service cho:
   - create/update/delete promotion
   - create/update/delete snack combo
   - booking admin read
   - payment admin read
   - resend ticket action
4. mở admin controller riêng:
   - `/api/admin/movies-promotions`
   - `/api/admin/snack-combos`
   - `/api/admin/bookings`
   - `/api/admin/payments`
   - `/api/admin/movie-feedbacks`
5. áp JWT/authorization cho admin endpoints

Deliverable cuối ngày:

- frontend admin có thể bỏ mock dần ở promotions/combos/bookings/payments

## Day 11: Feedback / Review

Mục tiêu:

- có API thật để mỗi movie có feedback/review từ khách

Việc cần làm:

1. tạo entity + config cho `MovieFeedback`
2. tạo DTO:
   - `MovieFeedbackResponseDto`
   - `CreateMovieFeedbackRequestDto`
3. tạo validator cho feedback:
   - rating hợp lệ
   - nội dung không rỗng
   - movie tồn tại
   - nếu business cần thì kiểm tra booking/ticket eligibility
4. code service/repository feedback
5. mở API:
   - `GET /api/movies/{movieId}/feedbacks`
   - `POST /api/movies/{movieId}/feedbacks`

Deliverable cuối ngày:

- movie detail/frontend có thể load feedback thật
- khách có thể gửi review thật qua API

## Day 12: Admin hardening

Mục tiêu:

- làm phần admin management đủ ổn cho demo nội bộ hoặc handoff frontend admin

Việc cần làm:

1. thêm search/filter/paging cho admin list
2. thêm audit log cần thiết cho action admin
3. chốt support actions:
   - resend ticket
   - xem trạng thái booking/payment
   - override note nếu business cần
4. test các case:
   - promotion inactive
   - combo inactive
   - booking/payment not found
   - resend ticket duplicate
5. sync contract với frontend admin

Deliverable cuối ngày:

- phần admin thuộc ownership Dev 2 có API thật, không chỉ còn mock UI
- feedback flow ổn định đủ để frontend bắt đầu tích hợp

## 10. Những thứ cần nhớ khi code

## 10.1 Booking hold

- source of truth là SQL
- không phụ thuộc Redis để xác định ghế đã giữ hay chưa
- cần có expiry và cleanup

## 10.2 Payment callback

- phải idempotent
- có thể bị gọi nhiều lần

## 10.3 Promotion

- backend là source of truth
- frontend chỉ hiển thị kết quả evaluate

## 10.4 DTO

- không trả entity trực tiếp ra API
- luôn qua DTO + AutoMapper

## 10.5 JWT

- public flow booking không cần JWT
- admin endpoints phải có JWT

## 10.6 Không vi phạm ownership với Dev 1

- không tự sửa `MoviesCatalogDbContext`
- không tự sửa bảng `Showtimes`
- không tự sửa bảng `ShowtimeSeatInventory`
- không tự đổi response `seat-map`
- không tự làm admin CRUD cho movie/cinema/hall/showtime
- feedback phải bám `movieId` và boundary movie của Dev 1, nhưng ownership flow vẫn thuộc Dev 2
- mọi thay đổi ở boundary phải chốt với Dev 1 trước

## 10.7 Redis không phải source of truth ở version đầu

- có thể cài package từ đầu
- có thể dùng cho cache sau này
- nhưng booking hold bản đầu vẫn phải bám SQL

## 10.8 AutoMapper chỉ dùng đúng chỗ

Dùng AutoMapper cho:

- entity -> DTO
- request DTO -> command model nếu cần

Không lạm dụng AutoMapper cho:

- business rule phức tạp
- tạo aggregate có invariant

## 10.9 Ưu tiên flow chạy đúng trước

Thứ tự ưu tiên khi code:

1. chạy đúng
2. contract rõ
3. validation đủ
4. logging đủ
5. tối ưu sau

## 10.10 Idempotency và concurrency là bắt buộc

Phải chú ý:

- payment callback có thể bị gọi nhiều lần
- nhiều request có thể cùng giữ một ghế
- hold có thể hết hạn trước khi create booking
- resend ticket có thể bị gọi lặp

## 10.11 Snapshot dữ liệu khi tạo booking

Khi tạo booking cần snapshot đủ dữ liệu để không lệ thuộc hoàn toàn vào dữ liệu động từ screening:

- seat code
- seat type
- unit price
- combo name/price
- promotion result
- grand total tại thời điểm booking

## 10.12 Những gì cần sync nhanh với Dev 1 mỗi ngày

Mỗi ngày nên xác nhận:

- `showtimeId` có ổn định không
- `seatInventoryId` đã đầy đủ chưa
- `seat-map` có đổi field không
- `status` ghế có đổi tên không
- `price` lấy từ đâu là source of truth

## 11. Trình tự nên làm trong một ngày coding

Nếu bắt đầu một ngày mới, ưu tiên:

1. đọc lại flow đang làm
2. code DTO trước
3. code validator
4. code service
5. code controller
6. test API
7. mới tối ưu sau

## 12. Dùng file này trong cuộc trò chuyện mới

Khi mở cuộc trò chuyện mới, chỉ cần cho model đọc file này và nói:

- tôi là Dev 2
- đang làm theo `Movie_dev2_execution_plan.md`
- tiếp tục từ Day X hoặc từ flow Y

Model phải hiểu:

- tôi làm phần `Booking + Promotion + Payment + Feedback`
- áp dụng `Option B`
- dùng `MoviesBookingDbContext`
- code theo flow
- package/stack đã chốt trong file này
