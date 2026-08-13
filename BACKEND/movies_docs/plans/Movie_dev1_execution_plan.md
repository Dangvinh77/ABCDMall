# Movie Dev1 Execution Plan

## 1. Vai trò và phạm vi

- Tôi đảm nhận `Dev 1` cho backend module `Movies`
- Phần tôi phụ trách:
  - `Movie Catalog`
  - `Cinema`
  - `Hall`
  - `Seat template`
  - `Screening / Showtime`
- `Seat map read model`
- `Movies admin CRUD` cho `Catalog + Screening`
- Tôi không phụ trách:
- `Booking hold`
- `Booking quote`
- `Promotion`
- `Payment`
- `Feedback / Review`
- `Ticket issue`
- `Outbox / Email`
- admin management cho booking/promotion/payment

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
- dễ test từng màn hình thật
- dễ biết boundary nào cần bàn giao cho Dev 2

### 3.3 Option B

Áp dụng `Option B` từ tài liệu thiết kế:

- nhiều `DbContext`
- dùng chung một database
- dùng schema `movies`
- migration tách riêng để giảm conflict

Tôi phụ trách:

- `MoviesCatalogDbContext`

Dev 2 phụ trách:

- `MoviesBookingDbContext`

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

### 3.5 Contract chi tiết với Dev 2

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

- `GET /api/movies/home`
- `GET /api/movies`
- `GET /api/movies/{movieId}`
- `GET /api/movies/{movieId}/showtimes`
- `GET /api/showtimes`
- `GET /api/showtimes/{showtimeId}`
- `GET /api/showtimes/{showtimeId}/seat-map`

Dev 2 dựa vào đó để làm:

- `POST /api/movies-promotions/evaluate`
- `POST /api/bookings/quote`
- `POST /api/bookings/holds`
- `POST /api/bookings`
- `POST /api/payments/intents`
- `GET /api/movies/{movieId}/feedbacks`
- `POST /api/movies/{movieId}/feedbacks`

#### Rule ownership

- Dev 1 owner của:
  - `Movie`
  - `Cinema`
  - `Hall`
  - `HallSeat`
  - `Showtime`
  - `ShowtimeSeatInventory`
  - response shape của `seat-map`
- Dev 2 owner của:
  - `BookingHold`
  - `Booking`
  - `Payment`
  - `Promotion evaluation`
  - `Quote`

Nếu cần thêm field vào `showtime` hoặc đổi contract ở `seat-map`, phải sync lại với Dev 1 trước khi code.

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
- Dapper

### Chưa dùng ngay

- Redis
- Kafka
- RabbitMQ
- Hangfire

Lý do:

- hệ thống hiện chưa cần event bus nặng
- phần Dev 1 chủ yếu là read API và seed dữ liệu
- tránh tăng độ phức tạp quá sớm

### Dapper

Trạng thái:

- cài package từ đầu
- chưa bắt buộc dùng cho version đầu

Ưu tiên EF Core trước. Nếu query homepage, movie list hoặc showtime filter nặng thì có thể dùng Dapper cho read model.

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
dotnet add BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/ABCDMall.Modules.Movies.Infrastructure.csproj package Dapper
```

## 5.4 `ABCDMall.WebAPI`

Cài:

```powershell
dotnet add BACKEND/ABCDMall.WebAPI/ABCDMall.WebAPI.csproj package Swashbuckle.AspNetCore
dotnet add BACKEND/ABCDMall.WebAPI/ABCDMall.WebAPI.csproj package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add BACKEND/ABCDMall.WebAPI/ABCDMall.WebAPI.csproj package FluentValidation.AspNetCore
```

## 6. Ownership phần Dev 1

## 6.1 DbContext

Tôi phụ trách:

- `MoviesCatalogDbContext`

## 6.2 Bảng tôi sở hữu

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

## 6.3 API tôi sở hữu

- `GET /api/movies/home`
- `GET /api/movies`
- `GET /api/movies/{movieId}`
- `GET /api/movies/{movieId}/showtimes`
- `GET /api/showtimes`
- `GET /api/showtimes/{showtimeId}`
- `GET /api/showtimes/{showtimeId}/seat-map`
- admin APIs cho `Catalog + Screening`:
  - `GET /api/admin/movies`
  - `POST /api/admin/movies`
  - `PUT /api/admin/movies/{movieId}`
  - `DELETE /api/admin/movies/{movieId}`
  - `GET /api/admin/showtimes`
  - `POST /api/admin/showtimes`
  - `PUT /api/admin/showtimes/{showtimeId}`
  - `DELETE /api/admin/showtimes/{showtimeId}`
  - nếu business cần:
    - `GET/POST/PUT/DELETE /api/admin/cinemas`
    - `GET/POST/PUT/DELETE /api/admin/halls`

## 7. Cấu trúc code nên tạo

## 7.1 Domain

Tạo:

- `Enums/MovieStatus.cs`
- `Enums/HallType.cs`
- `Enums/LanguageType.cs`
- `Enums/SeatType.cs`
- `Enums/ShowtimeStatus.cs`
- `Enums/SeatInventoryStatus.cs`

- `Entities/Movie.cs`
- `Entities/Genre.cs`
- `Entities/MovieGenre.cs`
- `Entities/Person.cs`
- `Entities/MovieCredit.cs`
- `Entities/Cinema.cs`
- `Entities/Hall.cs`
- `Entities/HallSeat.cs`
- `Entities/Showtime.cs`
- `Entities/ShowtimeSeatInventory.cs`

## 7.2 Application

Tạo:

- `DTOs/Movies/*`
- `DTOs/Showtimes/*`
- `Mappings/*`
- `Services/Movies/*`
- `Services/Showtimes/*`

## 7.3 Infrastructure

Tạo:

- `Persistence/Catalog/MoviesCatalogDbContext.cs`
- `Persistence/Catalog/Configurations/*`
- `Persistence/Catalog/Migrations/*`
- `Seed/*`
- `Repositories/Catalog/*`
- `Repositories/Screening/*`

## 8. Thứ tự code tổng thể

Code theo flow nghiệp vụ theo thứ tự:

1. `Catalog`
2. `Movies read API`
3. `Showtimes read API`
4. `Seat-map`
5. `Query optimization`
6. `Admin CRUD for Catalog + Screening`

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

- có bộ khung code cho Dev 1
- có package đúng
- có domain cơ bản

Việc cần làm:

1. cài package cho `Application`, `Infrastructure`, `WebAPI`
2. tạo folder structure
3. tạo enums:
   - `MovieStatus`
   - `HallType`
   - `LanguageType`
   - `SeatType`
   - `ShowtimeStatus`
   - `SeatInventoryStatus`
4. tạo entities:
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

Deliverable cuối ngày:

- package cài xong
- compile được domain/application/infrastructure ở mức cơ bản
- tất cả entity và enum đã có file

## Day 2: DbContext + EF Core config + migration đầu tiên

Mục tiêu:

- hoàn tất persistence layer cho phần Dev 1

Việc cần làm:

1. tạo `MoviesCatalogDbContext`
2. thêm `DbSet<>` cho toàn bộ bảng Dev 1
3. tạo `EntityTypeConfiguration` cho từng entity
4. chốt schema `movies`
5. tạo unique/index quan trọng:
   - `Movie.Slug` nếu có
   - `Genre.Name`
   - `Cinema.Code` nếu có
   - `Hall` theo `CinemaId + HallCode`
   - `HallSeat` theo `HallId + SeatCode`
   - `Showtime` theo `HallId + StartAt`
   - `ShowtimeSeatInventory` theo `ShowtimeId + SeatCode`
6. chuẩn bị seed data
7. tạo migration đầu tiên

Deliverable cuối ngày:

- migration đầu tiên chạy được
- DB sinh ra đủ bảng phần Dev 1

## Day 3: Seed dữ liệu catalog và screening

Mục tiêu:

- xong data nền để frontend và Dev 2 tích hợp

Việc cần làm:

1. seed `Genres`
2. seed `Movies`
3. seed `People`
4. seed `MovieCredits`
5. seed `Cinemas`
6. seed `Halls`
7. seed `HallSeats`
8. seed `Showtimes`
9. seed `ShowtimeSeatInventory`

Deliverable cuối ngày:

- frontend có dữ liệu phim, rạp, lịch chiếu thật
- Dev 2 có dữ liệu đầu vào để làm quote/hold

## Day 4: Flow Movies read API

Mục tiêu:

- frontend có thể gọi movie list và movie detail bằng API thật

Việc cần làm:

1. tạo DTO:
   - `MovieHomeItemDto`
   - `MovieListItemDto`
   - `MovieDetailResponseDto`
2. tạo AutoMapper profile cho movies
3. tạo repository cho movies
4. code query service cho movies
5. mở API:
   - `GET /api/movies/home`
   - `GET /api/movies`
   - `GET /api/movies/{movieId}`
6. thêm validation với `FluentValidation` nếu cần

Deliverable cuối ngày:

- frontend có thể gọi movies thật
- movie home, list, detail chạy được

## Day 5: Flow Showtimes read API

Mục tiêu:

- frontend có thể xem lịch chiếu theo phim và theo rạp

Việc cần làm:

1. tạo DTO:
   - `MovieShowtimesResponseDto`
   - `ShowtimeResponseDto`
   - `ShowtimeDetailResponseDto`
2. code repository cho showtimes
3. code `ShowtimeQueryService`
4. xử lý filter:
   - theo `movieId`
   - theo `cinemaId`
   - theo ngày
   - theo `hallType`
   - theo `language`
5. mở API:
   - `GET /api/movies/{movieId}/showtimes`
   - `GET /api/showtimes`
   - `GET /api/showtimes/{showtimeId}`

Deliverable cuối ngày:

- frontend có thể xem showtimes thật
- Dev 2 có thể đọc `showtimeId` và dữ liệu showtime chuẩn

## Day 6: Flow Seat Map

Mục tiêu:

- hoàn tất boundary quan trọng nhất cho booking flow

Việc cần làm:

1. tạo DTO:
   - `SeatMapResponseDto`
   - `SeatMapSeatDto`
2. code `SeatMapQueryService`
3. đảm bảo mỗi seat trả đúng:
   - `seatInventoryId`
   - `seatCode`
   - `row`
   - `col`
   - `seatType`
   - `status`
   - `price`
   - `coupleGroupCode`
4. mở API:
   - `GET /api/showtimes/{showtimeId}/seat-map`
5. sync lại contract với Dev 2

Deliverable cuối ngày:

- seat selection page có thể gọi API thật
- Dev 2 có đủ dữ liệu để làm `quote` và `hold`

## Day 7: Hardening

Mục tiêu:

- làm sạch code và giảm bug runtime

Việc cần làm:

1. bổ sung validation còn thiếu
2. thêm logging
3. rà lại projection và query shape
4. tối ưu unique/index cho query chính
5. rà mapping AutoMapper
6. test các case:
   - movie không tồn tại
   - showtime không tồn tại
   - showtime hết hiệu lực
   - seat-map sai trạng thái ghế
   - filter showtime theo ngày / cinema / movie

Deliverable cuối ngày:

- read API ổn định hơn
- sẵn sàng tích hợp frontend và booking flow

## Day 8: Movies Admin CRUD

Mục tiêu:

- có API thật để frontend admin quản lý movie/showtime thay vì chỉ dùng mock

Việc cần làm:

1. tạo admin DTO cho:
   - movie create/update
   - showtime create/update
   - nếu cần tiếp:
     - cinema create/update
     - hall create/update
2. tạo validator cho admin request
3. code command service/repository cho:
   - create movie
   - update movie
   - delete/soft delete movie
   - create showtime
   - update showtime
   - delete/cancel showtime
4. mở admin controller riêng:
   - không trộn vào public `MoviesController`
   - có route `/api/admin/...`
5. áp JWT/authorization cho admin endpoints

Deliverable cuối ngày:

- admin portal có thể gọi CRUD movie/showtime thật
- boundary public/customer không bị bẩn bởi logic admin

## Day 9: Admin hardening

Mục tiêu:

- làm phần admin đủ ổn để frontend admin bắt đầu nối API

Việc cần làm:

1. thêm search/filter/paging cho admin list
2. chốt soft delete hay hard delete cho movie/showtime
3. thêm guard:
   - không cho xóa movie đang có showtime active
   - không cho sửa hall/showtime phá seat inventory hiện có
4. thêm logging và test cho admin flows
5. sync contract với frontend admin

Deliverable cuối ngày:

- admin CRUD đủ rõ để frontend admin bỏ mock dần
- ownership Dev 1 với phần `Catalog + Screening admin` được khóa rõ

## 10. Những thứ cần nhớ khi code

## 10.1 Showtime và seat map là source of truth cho Dev 2

- Dev 2 sẽ dựa vào `showtime` và `seat-map` để làm quote, hold, booking
- vì vậy dữ liệu này phải ổn định và rõ nghĩa

## 10.2 Seat map contract phải ổn định

- response shape của `seat-map` phải rõ và nhất quán
- không đổi field tùy ý sau khi Dev 2 đã tích hợp

## 10.3 DTO

- không trả entity trực tiếp ra API
- luôn qua DTO + AutoMapper

## 10.4 JWT

- public flow movies/showtimes không cần JWT
- admin endpoints phải có JWT

## 10.5 Không vi phạm ownership với Dev 2

- không tự sửa `MoviesBookingDbContext`
- không tự sửa bảng `Bookings`
- không tự sửa bảng `Payments`
- không tự sửa flow quote/booking/payment
- không tự làm feedback/review flow cho movie
- không tự làm admin CRUD cho promotions/bookings/payments
- mọi thay đổi ở boundary phải chốt với Dev 2 trước

## 10.6 Dapper chỉ dùng đúng chỗ

Dùng Dapper cho:

- query tổng hợp homepage nếu cần
- query showtime/filter nếu read-heavy
- report/query read model không cần tracking

Không lạm dụng Dapper cho:

- toàn bộ persistence
- create/update entity chuẩn
- nơi EF Core đã làm tốt và đủ rõ

## 10.7 Ưu tiên flow chạy đúng trước

Thứ tự ưu tiên khi code:

1. chạy đúng
2. contract rõ
3. validation đủ
4. logging đủ
5. tối ưu sau

## 10.8 Seat inventory phải map chuẩn với hall seat

Phải chú ý:

- mỗi `showtimeId` phải map đúng một hall
- `seatInventoryId` phải ổn định theo từng showtime
- số ghế seat inventory phải khớp seat template của hall
- ghế couple phải có `coupleGroupCode` nhất quán

## 10.9 Data seed phải đủ để frontend test

Phải có tối thiểu:

- movie đang chiếu
- movie sắp chiếu
- nhiều cinema
- nhiều hall type nếu có
- ghế regular / VIP / couple
- nhiều showtime ở các ngày khác nhau

## 10.10 Những gì cần sync nhanh với Dev 2 mỗi ngày

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

- tôi là Dev 1
- đang làm theo `Movie_dev1_execution_plan.md`
- tiếp tục từ Day X hoặc từ flow Y

Model phải hiểu:

- tôi làm phần `Catalog + Screening`
- áp dụng `Option B`
- dùng `MoviesCatalogDbContext`
- code theo flow
- package/stack đã chốt trong file này
