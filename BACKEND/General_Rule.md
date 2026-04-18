# BACKEND General Rule

Tài liệu này mô tả pattern backend đang được dùng thực tế trong source `BACKEND` để AI/dev khác có thể tiếp tục phát triển module mới theo cùng cách làm.

## 1. Kiến trúc đang dùng

Backend hiện tại không dùng `AppDbContext` chung cho toàn hệ thống.

Pattern thực tế là:

- `ABCDMall.WebAPI` là host project, chỉ chứa `Program.cs` và các `Controller`.
- Mỗi module nằm trong `BACKEND/ABCDMall.Modules/<ModuleName>`.
- Mỗi module tách tối thiểu thành 3 project:
  - `*.Domain`
  - `*.Application`
  - `*.Infrastructure`
- Một module có thể có hơn 1 `DbContext` nếu domain đủ lớn.
  - Ví dụ `Movies` tách:
    - `MoviesCatalogDbContext`
    - `MoviesBookingDbContext`

Luồng phụ thuộc bắt buộc:

- `WebAPI` -> gọi `Application`
- `Application` -> phụ thuộc interface/repository abstraction
- `Infrastructure` -> implement abstraction và EF Core
- `Domain` -> chỉ chứa entity/enum/domain model

Không để `Controller` gọi trực tiếp `DbContext`.
Không để `Application` phụ thuộc trực tiếp `Infrastructure`.

## 2. Cấu trúc chuẩn của một module

Ví dụ module đang đúng pattern: `Movies`, `Users`, `FoodCourt`.

Folder/project chuẩn nên có:

```text
ABCDMall.Modules/<ModuleName>/
├── ABCDMall.Modules.<ModuleName>.Domain/
│   ├── Entities/
│   └── Enums/                  # nếu cần
│
├── ABCDMall.Modules.<ModuleName>.Application/
│   ├── DTOs/
│   ├── Mappings/
│   ├── Services/
│   │   └── <Feature>/
│   │       ├── I...Service.cs
│   │       ├── I...Repository.cs
│   │       ├── ...Service.cs
│   │       └── Validators/     # nếu có FluentValidation
│   └── DependencyInjection.cs
│
└── ABCDMall.Modules.<ModuleName>.Infrastructure/
    ├── Persistence/
    │   └── <BoundedContext>/
    │       ├── Configurations/
    │       ├── Migrations/
    │       ├── <Module>DbContext.cs
    │       └── <Module>DbContextFactory.cs
    ├── Repositories/
    ├── Services/               # external service implementations nếu có
    ├── Seed/                   # nếu module có seed local/frontend
    └── DependencyInjection.cs
```

## 3. Rule theo từng layer

### 3.1 Domain

Chỉ chứa dữ liệu lõi:

- `Entities`
- `Enums`
- value-like model nếu thực sự cần

Không đặt ở đây:

- `DTO`
- EF query logic
- service gọi DB
- logic web/API

Entity hiện tại trong repo đa số là POCO đơn giản, ví dụ `FoodItem`, `User`, `Movie`, `Showtime`.

### 3.2 Application

Đây là nơi chứa use case của module.

Đặt ở đây:

- `DTOs`
- `AutoMapper Profile`
- service interface: `I...Service`
- repository abstraction: `I...Repository`, `I...ReadRepository`, `I...CommandRepository`
- validator dùng `FluentValidation`
- business logic/use case orchestration

Pattern đang dùng:

- Query và Command thường tách riêng nếu module phức tạp.
  - Ví dụ `IUserQueryService`, `IUserCommandService`
  - Ví dụ `IRentalAreaQueryService`, `IRentalAreaCommandService`
- Module đơn giản có thể chỉ dùng `IFoodQueryService`, `IFoodCommandService`
- `Application` có `DependencyInjection.cs` để đăng ký:
  - AutoMapper
  - validators
  - application services

### 3.3 Infrastructure

Đây là nơi triển khai hạ tầng kỹ thuật.

Đặt ở đây:

- `DbContext`
- `IEntityTypeConfiguration<>`
- migrations
- repository implementation
- external services implementation
  - JWT
  - file storage
  - email
  - background service
  - seed

Pattern đang dùng:

- Mỗi `DbContext` có folder `Persistence/<BoundedContext>/`
- Mỗi entity có 1 file configuration riêng trong `Configurations/`
- Repository implement interface khai báo ở `Application`
- `Infrastructure/DependencyInjection.cs` đăng ký:
  - `DbContext`
  - repository implementation
  - external service implementation
  - hosted service nếu có

### 3.4 WebAPI

Controller chỉ làm 4 việc:

1. nhận request
2. validate input
3. gọi application service
4. map kết quả sang HTTP response

Không để business logic lớn trong controller.

Pattern hiện có:

- route rõ ràng theo resource, ví dụ:
  - `api/movies`
  - `api/bookings`
  - `api/auth`
- inject service interface từ `Application`
- inject `IValidator<T>` khi endpoint cần validate request/query
- trả:
  - `Ok`
  - `CreatedAtAction`
  - `BadRequest`
  - `NotFound`
  - `Unauthorized`
  - `ValidationProblem`

## 4. Naming convention cần giữ

### 4.1 Project/module

- `ABCDMall.Modules.<ModuleName>.Domain`
- `ABCDMall.Modules.<ModuleName>.Application`
- `ABCDMall.Modules.<ModuleName>.Infrastructure`

### 4.2 Service

- Query:
  - `IFoodQueryService` / `FoodQueryService`
  - `IMovieQueryService` / `MovieQueryService`
- Command:
  - `IUserCommandService` / `UserCommandService`
  - `IRentalAreaCommandService` / `RentalAreaCommandService`

### 4.3 Repository

- Nếu đơn giản:
  - `IFoodRepository` / `FoodRepository`
- Nếu tách read/write:
  - `IUserReadRepository` / `UserReadRepository`
  - `IUserCommandRepository` / `UserCommandRepository`

### 4.4 DTO

- Request:
  - `CreateFoodRequestDto`
  - `BookingQuoteRequestDto`
- Response:
  - `FoodItemDto`
  - `MovieDetailResponseDto`
  - `LoginResponseDto`
- Query DTO:
  - `FoodListQueryDto`
  - `MovieListQueryDto`

### 4.5 EF Core

- `...DbContext`
- `...Configuration`
- `...DbContextFactory`

Nếu module có schema riêng thì khai báo:

```csharp
public const string DefaultSchema = "movies";
```

## 5. Quy tắc viết service

Service trong `Application` là nơi chứa business rule.

Phải làm:

- nhận dependency qua constructor
- xử lý validate nghiệp vụ
- gọi repository abstraction
- dùng `CancellationToken`
- log ở những điểm quan trọng nếu cần
- map Domain -> DTO bằng AutoMapper hoặc map tay khi response đặc thù

Không nên làm:

- query SQL/EF trực tiếp trong controller
- phụ thuộc class ở `Infrastructure`
- nhồi quá nhiều use case không liên quan vào 1 service

Rule thực tế từ repo:

- Query service trả DTO/read model.
- Command service xử lý mutation, save dữ liệu, phát sinh token/email/file nếu cần.
- Với module cần trả status rõ hơn HTTP đơn thuần, có thể dùng wrapper kiểu `ApplicationResult<T>` như `Users`.

## 6. Quy tắc viết repository

Repository ở `Infrastructure` chỉ xử lý persistence.

Phải làm:

- implement interface từ `Application`
- dùng `AsNoTracking()` cho read-only query nếu phù hợp
- chỉ chứa query/update liên quan lưu trữ dữ liệu
- `SaveChangesAsync` ở repository hoặc ngay sau mutation theo pattern module hiện có

Không nên làm:

- chứa business rule phức tạp
- trả `IQueryable` ra ngoài service trừ khi có lý do rất rõ

Pattern hiện tại chấp nhận 2 kiểu:

- repository tự `SaveChangesAsync` trong từng hàm mutation, ví dụ `FoodRepository`
- repository expose `SaveChangesAsync`, service điều phối nhiều thao tác rồi commit, ví dụ `Users`

Khi làm module mới, chọn 1 kiểu nhất quán trong chính module đó.

## 7. Quy tắc viết DbContext và EF configuration

### 7.1 DbContext

Mỗi module hoặc bounded context có `DbSet<>` riêng, ví dụ:

```csharp
public DbSet<FoodItem> FoodItems => Set<FoodItem>();
```

Trong `OnModelCreating`:

- đặt schema nếu module cần
- `ApplyConfiguration(...)` cho từng entity

Không nhét Fluent API dài trực tiếp vào `DbContext` nếu đã có `Configurations/`.

### 7.2 Configuration

Mỗi entity nên có 1 file `IEntityTypeConfiguration<TEntity>`.

Tại đây cấu hình:

- table name
- key
- column length
- required
- relationship
- index
- default value

### 7.3 Migration

Migration đặt trong chính `Infrastructure` của module/bounded context tương ứng.

Module nào có nhiều `DbContext` thì mỗi `DbContext` có migration history riêng.

Ví dụ `Movies` đang làm đúng:

- `__EFMigrationsHistory_MoviesCatalog`
- `__EFMigrationsHistory_MoviesBooking`

## 8. Dependency Injection rule

Mỗi module phải có 2 điểm đăng ký rõ ràng:

- `Application/DependencyInjection.cs`
- `Infrastructure/DependencyInjection.cs`

`Program.cs` chỉ gọi extension method, không đăng ký lẻ tẻ toàn bộ class.

Pattern thực tế:

```csharp
builder.Services.AddMoviesApplication(autoMapperLicenseKey);
builder.Services.AddMoviesInfrastructure(builder.Configuration);

builder.Services.AddFoodCourtApplication(autoMapperLicenseKey);
builder.Services.AddFoodCourtInfrastructure(builder.Configuration);

builder.Services.AddUsersApplication(autoMapperLicenseKey);
builder.Services.AddUsersInfrastructure(builder.Configuration);
```

Rule:

- `Application DI` đăng ký service + validator + mapping
- `Infrastructure DI` đăng ký DbContext + repository + infra service

## 9. Validation rule

Nếu request/query có rule kiểm tra rõ ràng thì tạo `Validator` riêng trong `Application`.

Đặt file ở:

- `Application/Services/<Feature>/Validators/`

Controller pattern:

1. tạo request/query DTO
2. gọi `ValidateAsync`
3. nếu fail thì trả `ValidationProblem(...)`
4. nếu pass thì gọi service

Không để validation request rải rác bằng `if` trong nhiều endpoint nếu đã có thể gom vào FluentValidation.

## 10. Controller rule

Controller nên mỏng và nhất quán.

Checklist:

- route rõ
- inject đúng service interface
- validate DTO/query trước khi gọi service
- truyền `CancellationToken`
- map `ApplicationResult` hoặc exception sang HTTP response

Nếu service có thể fail do nghiệp vụ:

- ưu tiên trả result có status rõ ràng
- hoặc ném `InvalidOperationException` và controller map sang `BadRequest`

Giữ cách làm nhất quán trong từng module.

## 11. Config và connection string

`ABCDMall.WebAPI/appsettings.json` hiện đang dùng:

```json
"ConnectionStrings": {
  "ABCDMallConnection": "Server=<server_name>;Database=ABCDMall;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

Ngoài key chung `ABCDMallConnection`, một số module đã hỗ trợ fallback key riêng:

- `ABCDMallMoviesDBConnection`
- `ABCDMallFoodCourtDBConnection`
- `ABCDMallUsersDBConnection`

Rule khi thêm module mới:

- ưu tiên dùng `ABCDMallConnection`
- nếu module cần tách DB/key riêng thì cho phép fallback tương tự pattern hiện tại

## 12. Rule khi tạo module mới

Làm theo thứ tự này:

1. Tạo folder `BACKEND/ABCDMall.Modules/<ModuleName>`.
2. Tạo 3 project `Domain`, `Application`, `Infrastructure`.
3. Tạo entity trong `Domain/Entities`.
4. Tạo DTO + service interface + repository abstraction trong `Application`.
5. Tạo validator nếu endpoint có input/query cần kiểm tra.
6. Tạo AutoMapper profile nếu module dùng mapping.
7. Tạo `DbContext`, `Configurations`, `Repositories`, `DependencyInjection` trong `Infrastructure`.
8. Tạo migration cho `DbContext`.
9. Đăng ký module trong `ABCDMall.WebAPI/Program.cs`.
10. Tạo controller mới trong `ABCDMall.WebAPI/Controllers`.
11. Nếu cần dữ liệu demo thì thêm `Seed/`.
12. Chạy migration + test endpoint trên Swagger.

## 13. Checklist review trước khi merge

- Có tách đúng `Domain / Application / Infrastructure / WebAPI` chưa.
- `Application` không reference implementation từ `Infrastructure`.
- `Controller` không gọi trực tiếp `DbContext`.
- Mọi dependency đã được đăng ký ở `DependencyInjection.cs`.
- Query DTO / Request DTO / Response DTO đã tách rõ.
- Validator nằm trong `Application`, không nhét vào controller.
- `DbContext` có `Configuration` riêng cho entity.
- Migration nằm đúng project chứa `DbContext`.
- Endpoint có `CancellationToken`.
- Role/Authorize được đặt ở controller action nếu cần.

## 14. Những điểm cần giữ nhất quán với source hiện tại

- Dùng module-level DI extension method, không phình `Program.cs`.
- Ưu tiên `DbContext` riêng theo module hoặc bounded context.
- Tách query service và command service khi domain bắt đầu phức tạp.
- Repository interface đặt ở `Application`, implementation đặt ở `Infrastructure`.
- Controller chỉ là lớp adapter HTTP.
- Có thể dùng `ApplicationResult<T>` cho module cần kiểm soát response status chi tiết.

## 15. Một số file tham chiếu tốt trong repo

- `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Application/DependencyInjection.cs`
- `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Infrastructure/DependencyInjection.cs`
- `BACKEND/ABCDMall.Modules/FoodCourt/ABCDMall.Modules.FoodCourt.Infrastructure/Persistence/FoodCourt/FoodCourtDbContext.cs`
- `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/Services/Auth/UserCommandService.cs`
- `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Infrastructure/Repositories/UserCommandRepository.cs`
- `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/DependencyInjection.cs`
- `BACKEND/ABCDMall.WebAPI/Program.cs`

Nếu cần tạo module mới, hãy copy pattern từ `FoodCourt` trước cho module đơn giản, và copy pattern từ `Users` hoặc `Movies` cho module phức tạp hơn.
