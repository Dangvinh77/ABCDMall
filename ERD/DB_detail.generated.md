# Database Detail

Tài liệu này liệt kê chi tiết các bảng hiện có theo `ModelSnapshot` của dự án.

## Danh sách bảng chi tiết

## Table: dbo.ForgotPasswordOtps

- Mapped Entity: `ABCDMall.Modules.Users.Domain.Entities.ForgotPasswordOtp`
- Schema: `dbo`
- Table Name: `ForgotPasswordOtps`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | nvarchar(64) | Yes | PK | Primary identifier for this record. |
| CreatedAt | datetime2 | No | - | Stores when the record was created. |
| Email | nvarchar(256) | No | - | Email address associated with this record. |
| ExpiresAt | datetime2 | No | - | Stores the expiration timestamp. |
| IsUsed | bit | No | - | Boolean flag describing the state of this record. |
| NewPasswordHash | nvarchar(max) | No | - | Stores password-related secure data. |
| Otp | nvarchar(10) | No | - | One-time password or verification code. |
| UsedAt | datetime2 | Yes | - | Stores the used at value for this table. |
| UserId | nvarchar(64) | No | - | References a user record. |

## Table: dbo.PasswordResetOtps

- Mapped Entity: `ABCDMall.Modules.Users.Domain.Entities.PasswordResetOtp`
- Schema: `dbo`
- Table Name: `PasswordResetOtps`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | nvarchar(64) | Yes | PK | Primary identifier for this record. |
| CreatedAt | datetime2 | No | - | Stores when the record was created. |
| ExpiresAt | datetime2 | No | - | Stores the expiration timestamp. |
| IsUsed | bit | No | - | Boolean flag describing the state of this record. |
| NewPasswordHash | nvarchar(max) | No | - | Stores password-related secure data. |
| Otp | nvarchar(10) | No | - | One-time password or verification code. |
| UsedAt | datetime2 | Yes | - | Stores the used at value for this table. |
| UserId | nvarchar(64) | No | - | References a user record. |

## Table: dbo.ProfileUpdateHistories

- Mapped Entity: `ABCDMall.Modules.Users.Domain.Entities.ProfileUpdateHistory`
- Schema: `dbo`
- Table Name: `ProfileUpdateHistories`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | nvarchar(64) | Yes | PK | Primary identifier for this record. |
| Email | nvarchar(256) | No | - | Email address associated with this record. |
| PreviousAddress | nvarchar(500) | Yes | - | Stores the previous address value for this table. |
| PreviousCCCD | nvarchar(50) | Yes | - | Stores the previous cccd value for this table. |
| PreviousFullName | nvarchar(200) | Yes | - | Display name used for this record. |
| PreviousImage | nvarchar(500) | Yes | - | Path or URL to an image asset. |
| UpdatedAddress | nvarchar(500) | Yes | - | Stores the updated address value for this table. |
| UpdatedAt | datetime2 | No | - | Stores the most recent update time. |
| UpdatedCCCD | nvarchar(50) | Yes | - | Stores the updated cccd value for this table. |
| UpdatedFullName | nvarchar(200) | Yes | - | Display name used for this record. |
| UpdatedImage | nvarchar(500) | Yes | - | Path or URL to an image asset. |
| UserId | nvarchar(64) | No | - | References a user record. |

## Table: dbo.RefreshTokens

- Mapped Entity: `ABCDMall.Modules.Users.Domain.Entities.RefreshToken`
- Schema: `dbo`
- Table Name: `RefreshTokens`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | nvarchar(64) | Yes | PK | Primary identifier for this record. |
| CreatedAt | datetime2 | No | - | Stores when the record was created. |
| ExpiresAt | datetime2 | No | - | Stores the expiration timestamp. |
| IsRevoked | bit | No | - | Boolean flag describing the state of this record. |
| RevokedAt | datetime2 | Yes | - | Stores the revoked at value for this table. |
| Token | nvarchar(500) | No | UQ | Stores the token value for this table. |
| UserId | nvarchar(64) | No | - | References a user record. |

## Table: dbo.RentalAreas

- Mapped Entity: `ABCDMall.Modules.Users.Domain.Entities.RentalArea`
- Schema: `dbo`
- Table Name: `RentalAreas`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | nvarchar(64) | Yes | PK | Primary identifier for this record. |
| AreaCode | nvarchar(50) | No | UQ | Stores the area code value for this table. |
| AreaName | nvarchar(200) | No | - | Display name used for this record. |
| CreatedAt | datetime2 | No | - | Stores when the record was created. |
| Floor | nvarchar(20) | No | - | Stores the floor value for this table. |
| MonthlyRent | decimal(18,2) | No | - | Numeric monetary amount used by this record. |
| ShopInfoId | nvarchar(64) | Yes | IX | References a shop-related record. |
| Size | nvarchar(50) | No | - | Stores the size value for this table. |
| Status | nvarchar(50) | No | - | Status value used by the workflow for this record. |
| TenantName | nvarchar(200) | Yes | - | Display name used for this record. |

## Table: dbo.ShopInfos

- Mapped Entity: `ABCDMall.Modules.Users.Domain.Entities.ShopInfo`
- Schema: `dbo`
- Table Name: `ShopInfos`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | nvarchar(64) | Yes | PK | Primary identifier for this record. |
| Badge | nvarchar(120) | Yes | - | Stores the badge value for this table. |
| CCCD | nvarchar(50) | Yes | UQ | Stores the cccd value for this table. |
| Category | nvarchar(120) | No | - | Stores the category value for this table. |
| ContractImage | nvarchar(500) | Yes | - | Path or URL to an image asset. |
| ContractImages | nvarchar(500) | Yes | - | Stores the contract images value for this table. |
| CoverImageUrl | nvarchar(500) | No | - | Path or URL to an image asset. |
| CreatedAt | datetime2 | No | - | Stores when the record was created. |
| Description | nvarchar(2000) | No | - | Long-form descriptive content. |
| ElectricityFee | decimal(18,2) | No | - | Numeric monetary amount used by this record. |
| ElectricityUsage | nvarchar(100) | No | - | Stores the electricity usage value for this table. |
| Floor | nvarchar(80) | No | - | Stores the floor value for this table. |
| IsPublicVisible | bit | No | - | Boolean flag describing the state of this record. |
| LeaseStartDate | datetime2 | No | - | Stores the lease start date value for this table. |
| LeaseTermDays | int | No | - | Stores the lease term days value for this table. |
| LocationSlot | nvarchar(100) | No | - | Stores the location slot value for this table. |
| LogoUrl | nvarchar(500) | No | - | Path or URL to an image asset. |
| ManagerName | nvarchar(200) | Yes | - | Display name used for this record. |
| Month | nvarchar(50) | No | - | Stores the month value for this table. |
| Offer | nvarchar(250) | Yes | - | Stores the offer value for this table. |
| OpenHours | nvarchar(80) | No | - | Stores the open hours value for this table. |
| OwnerShopInfoId | nvarchar(64) | Yes | IX | References a shop-related record. |
| RentalLocation | nvarchar(100) | No | - | Numeric monetary amount used by this record. |
| ServiceFee | decimal(18,2) | No | - | Numeric monetary amount used by this record. |
| ShopName | nvarchar(200) | No | - | Display name used for this record. |
| Slug | nvarchar(160) | No | UQ | SEO-friendly route segment or public identifier. |
| Summary | nvarchar(500) | No | - | Short summary used in listings or previews. |
| Tags | nvarchar(500) | No | - | Stores the tags value for this table. |
| TotalDue | decimal(18,2) | No | - | Numeric monetary amount used by this record. |
| WaterFee | decimal(18,2) | No | - | Numeric monetary amount used by this record. |
| WaterUsage | nvarchar(100) | No | - | Stores the water usage value for this table. |

## Table: dbo.ShopMonthlyBills

- Mapped Entity: `ABCDMall.Modules.Users.Domain.Entities.ShopMonthlyBill`
- Schema: `dbo`
- Table Name: `ShopMonthlyBills`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | nvarchar(64) | Yes | PK | Primary identifier for this record. |
| BillKey | nvarchar(220) | No | UQ | Stores the bill key value for this table. |
| BillingMonthKey | nvarchar(20) | No | - | Stores the billing month key value for this table. |
| CCCD | nvarchar(50) | Yes | - | Stores the cccd value for this table. |
| ContractImage | nvarchar(500) | Yes | - | Path or URL to an image asset. |
| ContractImages | nvarchar(500) | Yes | - | Stores the contract images value for this table. |
| CreatedAt | datetime2 | No | - | Stores when the record was created. |
| ElectricityFee | decimal(18,2) | No | - | Numeric monetary amount used by this record. |
| ElectricityUsage | nvarchar(100) | No | - | Stores the electricity usage value for this table. |
| LeaseStartDate | datetime2 | No | - | Stores the lease start date value for this table. |
| LeaseTermDays | int | No | - | Stores the lease term days value for this table. |
| ManagerName | nvarchar(200) | Yes | - | Display name used for this record. |
| Month | nvarchar(50) | No | - | Stores the month value for this table. |
| RentalLocation | nvarchar(100) | No | - | Numeric monetary amount used by this record. |
| ServiceFee | decimal(18,2) | No | - | Numeric monetary amount used by this record. |
| ShopInfoId | nvarchar(64) | No | - | References a shop-related record. |
| ShopName | nvarchar(200) | No | - | Display name used for this record. |
| TotalDue | decimal(18,2) | No | - | Numeric monetary amount used by this record. |
| UpdatedAt | datetime2 | Yes | - | Stores the most recent update time. |
| UsageMonth | nvarchar(50) | No | - | Stores the usage month value for this table. |
| UsageMonthKey | nvarchar(20) | No | - | Stores the usage month key value for this table. |
| WaterFee | decimal(18,2) | No | - | Numeric monetary amount used by this record. |
| WaterUsage | nvarchar(100) | No | - | Stores the water usage value for this table. |

## Table: dbo.Users

- Mapped Entity: `ABCDMall.Modules.Users.Domain.Entities.User`
- Schema: `dbo`
- Table Name: `Users`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | nvarchar(64) | Yes | PK | Primary identifier for this record. |
| Address | nvarchar(500) | Yes | - | Stores the address value for this table. |
| CCCD | nvarchar(50) | Yes | UQ | Stores the cccd value for this table. |
| CreatedAt | datetime2 | Yes | - | Stores when the record was created. |
| Email | nvarchar(256) | No | UQ | Email address associated with this record. |
| FailedLoginAttempts | int | No | - | Stores the failed login attempts value for this table. |
| FullName | nvarchar(200) | Yes | - | Display name used for this record. |
| Image | nvarchar(500) | Yes | - | Path or URL to an image asset. |
| IsActive | bit | No | - | Boolean flag describing the state of this record. |
| LoginOtpCode | nvarchar(10) | Yes | - | One-time password or verification code. |
| LoginOtpExpiresAt | datetime2 | Yes | - | Stores the expiration timestamp. |
| Password | nvarchar(max) | No | - | Stores password-related secure data. |
| Role | nvarchar(50) | No | - | Stores the role value for this table. |
| ShopId | nvarchar(64) | Yes | - | References a shop-related record. |
| UpdatedAt | datetime2 | Yes | - | Stores the most recent update time. |

## Table: events.Events

- Mapped Entity: `ABCDMall.Modules.Events.Domain.Entities.Event`
- Schema: `events`
- Table Name: `Events`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | uniqueidentifier | No | PK | Primary identifier for this record. |
| CoverImageUrl | nvarchar(1000) | No | - | Path or URL to an image asset. |
| CreatedAt | datetime2 | No | - | Stores when the record was created. |
| Description | nvarchar(4000) | No | - | Long-form descriptive content. |
| EndDate | datetime2 | No | IX | Stores the end date value for this table. |
| EventType | int | No | IX | Stores the event type value for this table. |
| IsHot | bit | No | IX | Boolean flag describing the state of this record. |
| Location | nvarchar(500) | No | - | Stores the location value for this table. |
| ShopId | nvarchar(64) | Yes | - | References a shop-related record. |
| ShopName | nvarchar(300) | Yes | - | Display name used for this record. |
| StartDate | datetime2 | No | IX | Stores the start date value for this table. |
| Title | nvarchar(300) | No | - | Title or headline shown for this record. |

## Table: foodcourt.FoodItems

- Mapped Entity: `ABCDMall.Modules.FoodCourt.Domain.Entities.FoodItem`
- Schema: `foodcourt`
- Table Name: `FoodItems`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | nvarchar(64) | Yes | PK | Primary identifier for this record. |
| CategorySlug | nvarchar(200) | No | - | SEO-friendly route segment or public identifier. |
| Description | nvarchar(4000) | No | - | Long-form descriptive content. |
| ImageUrl | nvarchar(1000) | No | - | Path or URL to an image asset. |
| MallSlug | nvarchar(200) | No | - | SEO-friendly route segment or public identifier. |
| Name | nvarchar(200) | No | - | Display name used for this record. |
| Slug | nvarchar(200) | No | UQ | SEO-friendly route segment or public identifier. |

## Table: movies.AuditLogs

- Mapped Entity: `ABCDMall.Modules.Movies.Domain.Entities.AuditLog`
- Schema: `movies`
- Table Name: `AuditLogs`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | uniqueidentifier | No | PK | Primary identifier for this record. |
| Action | nvarchar(50) | No | - | Stores the action value for this table. |
| ActorId | nvarchar(100) | Yes | - | Stores the actor id value for this table. |
| ChangesJson | nvarchar(max) | Yes | - | Serialized JSON payload for this record. |
| CreatedAtUtc | datetime2 | No | IX | Stores when the record was created. |
| EntityId | nvarchar(100) | No | IX | Stores the entity id value for this table. |
| EntityName | nvarchar(100) | No | IX | Display name used for this record. |

## Table: movies.BookingHolds

- Mapped Entity: `ABCDMall.Modules.Movies.Domain.Entities.BookingHold`
- Schema: `movies`
- Table Name: `BookingHolds`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | uniqueidentifier | No | PK | Primary identifier for this record. |
| ComboSnapshotJson | nvarchar(max) | Yes | - | Serialized JSON payload for this record. |
| ComboSubtotal | decimal(18,2) | No | - | Numeric monetary amount used by this record. |
| CreatedAtUtc | datetime2 | No | - | Stores when the record was created. |
| DiscountAmount | decimal(18,2) | No | - | Numeric monetary amount used by this record. |
| ExpiresAtUtc | datetime2 | No | IX | Stores the expiration timestamp. |
| GrandTotal | decimal(18,2) | No | - | Numeric monetary amount used by this record. |
| HoldCode | nvarchar(32) | No | UQ | Stores the hold code value for this table. |
| PromotionId | uniqueidentifier | Yes | IX | References a promotion record. |
| PromotionSnapshotJson | nvarchar(max) | Yes | - | Serialized JSON payload for this record. |
| SeatSubtotal | decimal(18,2) | No | - | Numeric monetary amount used by this record. |
| ServiceFee | decimal(18,2) | No | - | Numeric monetary amount used by this record. |
| SessionId | nvarchar(100) | Yes | - | Stores the session id value for this table. |
| ShowtimeId | uniqueidentifier | No | IX | References a showtime record. |
| Status | int | No | IX | Status value used by the workflow for this record. |
| UpdatedAtUtc | datetime2 | No | - | Stores the most recent update time. |

## Table: movies.BookingHoldSeats

- Mapped Entity: `ABCDMall.Modules.Movies.Domain.Entities.BookingHoldSeat`
- Schema: `movies`
- Table Name: `BookingHoldSeats`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | uniqueidentifier | No | PK | Primary identifier for this record. |
| BookingHoldId | uniqueidentifier | No | UQ | References a booking-related record. |
| CoupleGroupCode | nvarchar(50) | Yes | - | Stores the couple group code value for this table. |
| SeatCode | nvarchar(20) | No | - | Stores the seat code value for this table. |
| SeatInventoryId | uniqueidentifier | No | UQ, IX | References a location or seating record. |
| SeatType | nvarchar(30) | No | - | Stores the seat type value for this table. |
| UnitPrice | decimal(18,2) | No | - | Numeric monetary amount used by this record. |

## Table: movies.BookingItems

- Mapped Entity: `ABCDMall.Modules.Movies.Domain.Entities.BookingItem`
- Schema: `movies`
- Table Name: `BookingItems`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | uniqueidentifier | No | PK | Primary identifier for this record. |
| BookingId | uniqueidentifier | No | IX | References a booking-related record. |
| Description | nvarchar(500) | No | - | Long-form descriptive content. |
| ItemCode | nvarchar(50) | No | - | Stores the item code value for this table. |
| ItemType | nvarchar(30) | No | - | Stores the item type value for this table. |
| LineTotal | decimal(18,2) | No | - | Numeric monetary amount used by this record. |
| Quantity | int | No | - | Stores the quantity value for this table. |
| SeatInventoryId | uniqueidentifier | Yes | IX | References a location or seating record. |
| UnitPrice | decimal(18,2) | No | - | Numeric monetary amount used by this record. |

## Table: movies.Bookings

- Mapped Entity: `ABCDMall.Modules.Movies.Domain.Entities.Bookingg`
- Schema: `movies`
- Table Name: `Bookings`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | uniqueidentifier | No | PK | Primary identifier for this record. |
| BookingCode | nvarchar(32) | No | UQ | Stores the booking code value for this table. |
| BookingHoldId | uniqueidentifier | Yes | UQ | References a booking-related record. |
| ComboSubtotal | decimal(18,2) | No | - | Numeric monetary amount used by this record. |
| CreatedAtUtc | datetime2 | No | - | Stores when the record was created. |
| Currency | nvarchar(10) | No | - | Stores the currency value for this table. |
| CustomerEmail | nvarchar(200) | No | - | Email address associated with this record. |
| CustomerName | nvarchar(200) | No | - | Display name used for this record. |
| CustomerPhoneNumber | nvarchar(20) | No | - | Stores the customer phone number value for this table. |
| DiscountAmount | decimal(18,2) | No | - | Numeric monetary amount used by this record. |
| GrandTotal | decimal(18,2) | No | - | Numeric monetary amount used by this record. |
| GuestCustomerId | uniqueidentifier | Yes | IX | Stores the guest customer id value for this table. |
| PromotionId | uniqueidentifier | Yes | - | References a promotion record. |
| PromotionSnapshotJson | nvarchar(max) | Yes | - | Serialized JSON payload for this record. |
| SeatSubtotal | decimal(18,2) | No | - | Numeric monetary amount used by this record. |
| ServiceFee | decimal(18,2) | No | - | Numeric monetary amount used by this record. |
| ShowtimeId | uniqueidentifier | No | IX | References a showtime record. |
| Status | int | No | - | Status value used by the workflow for this record. |
| UpdatedAtUtc | datetime2 | No | - | Stores the most recent update time. |

## Table: movies.Cinemas

- Mapped Entity: `ABCDMall.Modules.Movies.Domain.Entities.Cinema`
- Schema: `movies`
- Table Name: `Cinemas`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | uniqueidentifier | No | PK | Primary identifier for this record. |
| AddressLine1 | nvarchar(300) | No | - | Stores the address line 1 value for this table. |
| AddressLine2 | nvarchar(300) | Yes | - | Stores the address line 2 value for this table. |
| City | nvarchar(120) | No | IX | Stores the city value for this table. |
| Code | nvarchar(50) | No | UQ | Stores the code value for this table. |
| CreatedAtUtc | datetime2 | No | - | Stores when the record was created. |
| IsActive | bit | No | IX | Boolean flag describing the state of this record. |
| Name | nvarchar(200) | No | - | Display name used for this record. |
| UpdatedAtUtc | datetime2 | No | - | Stores the most recent update time. |

## Table: movies.Genres

- Mapped Entity: `ABCDMall.Modules.Movies.Domain.Entities.Genre`
- Schema: `movies`
- Table Name: `Genres`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | uniqueidentifier | No | PK | Primary identifier for this record. |
| Description | nvarchar(500) | Yes | - | Long-form descriptive content. |
| Name | nvarchar(100) | No | UQ | Display name used for this record. |

## Table: movies.GuestCustomers

- Mapped Entity: `ABCDMall.Modules.Movies.Domain.Entities.GuestCustomer`
- Schema: `movies`
- Table Name: `GuestCustomers`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | uniqueidentifier | No | PK | Primary identifier for this record. |
| CreatedAtUtc | datetime2 | No | - | Stores when the record was created. |
| Email | nvarchar(200) | No | IX | Email address associated with this record. |
| FullName | nvarchar(200) | No | - | Display name used for this record. |
| PhoneNumber | nvarchar(20) | No | IX | Stores the phone number value for this table. |
| UpdatedAtUtc | datetime2 | No | - | Stores the most recent update time. |

## Table: movies.Halls

- Mapped Entity: `ABCDMall.Modules.Movies.Domain.Entities.Hall`
- Schema: `movies`
- Table Name: `Halls`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | uniqueidentifier | No | PK | Primary identifier for this record. |
| CinemaId | uniqueidentifier | No | UQ | References a location or seating record. |
| HallCode | nvarchar(50) | No | UQ | Stores the hall code value for this table. |
| HallType | int | No | - | Stores the hall type value for this table. |
| IsActive | bit | No | - | Boolean flag describing the state of this record. |
| Name | nvarchar(150) | No | - | Display name used for this record. |
| SeatCapacity | int | No | - | Stores the seat capacity value for this table. |

## Table: movies.HallSeats

- Mapped Entity: `ABCDMall.Modules.Movies.Domain.Entities.HallSeat`
- Schema: `movies`
- Table Name: `HallSeats`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | uniqueidentifier | No | PK | Primary identifier for this record. |
| ColumnNumber | int | No | UQ | Stores the column number value for this table. |
| CoupleGroupCode | nvarchar(50) | Yes | - | Stores the couple group code value for this table. |
| HallId | uniqueidentifier | No | UQ | References a location or seating record. |
| IsActive | bit | No | - | Boolean flag describing the state of this record. |
| RowLabel | nvarchar(10) | No | UQ | Stores the row label value for this table. |
| SeatCode | nvarchar(20) | No | UQ | Stores the seat code value for this table. |
| SeatType | int | No | - | Stores the seat type value for this table. |

## Table: movies.MovieCredits

- Mapped Entity: `ABCDMall.Modules.Movies.Domain.Entities.MovieCredit`
- Schema: `movies`
- Table Name: `MovieCredits`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | uniqueidentifier | No | PK | Primary identifier for this record. |
| CreditType | nvarchar(50) | No | IX | Stores the credit type value for this table. |
| DisplayOrder | int | No | - | Stores the display order value for this table. |
| MovieId | uniqueidentifier | No | IX | References a movie record. |
| PersonId | uniqueidentifier | No | IX | Stores the person id value for this table. |
| RoleName | nvarchar(150) | No | - | Display name used for this record. |

## Table: movies.MovieFeedbackRequests

- Mapped Entity: `ABCDMall.Modules.Movies.Domain.Entities.MovieFeedbackRequest`
- Schema: `movies`
- Table Name: `MovieFeedbackRequests`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | uniqueidentifier | No | PK | Primary identifier for this record. |
| AvailableAtUtc | datetime2 | No | IX | Stores the available at utc value for this table. |
| BookingId | uniqueidentifier | No | UQ | References a booking-related record. |
| CreatedAtUtc | datetime2 | No | - | Stores when the record was created. |
| EmailRetryCount | int | No | - | Email address associated with this record. |
| ExpiresAtUtc | datetime2 | Yes | IX | Stores the expiration timestamp. |
| InvalidatedAtUtc | datetime2 | Yes | - | Stores the invalidated at utc value for this table. |
| LastEmailError | nvarchar(max) | Yes | - | Email address associated with this record. |
| MovieId | uniqueidentifier | No | IX | References a movie record. |
| PurchaserEmail | nvarchar(200) | No | - | Email address associated with this record. |
| SentAtUtc | datetime2 | Yes | - | Stores the sent at utc value for this table. |
| ShowtimeId | uniqueidentifier | No | UQ | References a showtime record. |
| Status | int | No | IX | Status value used by the workflow for this record. |
| SubmittedAtUtc | datetime2 | Yes | - | Stores the submitted at utc value for this table. |
| TokenHash | nvarchar(128) | Yes | UQ | Stores the token hash value for this table. |
| UpdatedAtUtc | datetime2 | No | - | Stores the most recent update time. |

## Table: movies.MovieFeedbacks

- Mapped Entity: `ABCDMall.Modules.Movies.Domain.Entities.MovieFeedback`
- Schema: `movies`
- Table Name: `MovieFeedbacks`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | uniqueidentifier | No | PK | Primary identifier for this record. |
| BookingId | uniqueidentifier | Yes | IX | References a booking-related record. |
| Comment | nvarchar(2000) | No | - | Stores the comment value for this table. |
| CreatedByEmail | nvarchar(200) | Yes | - | Email address associated with this record. |
| CreatedAtUtc | datetime2 | No | IX | Stores when the record was created. |
| DisplayName | nvarchar(120) | Yes | - | Display name used for this record. |
| FeedbackRequestId | uniqueidentifier | Yes | UQ | Numeric monetary amount used by this record. |
| IsVisible | bit | No | IX | Boolean flag describing the state of this record. |
| ModeratedAtUtc | datetime2 | Yes | - | Stores the moderated at utc value for this table. |
| ModeratedBy | nvarchar(120) | Yes | - | Stores the moderated by value for this table. |
| ModerationReason | nvarchar(500) | Yes | - | Stores the moderation reason value for this table. |
| ModerationStatus | int | No | IX | Status value used by the workflow for this record. |
| MovieId | uniqueidentifier | No | IX | References a movie record. |
| Rating | int | No | IX | Stores the rating value for this table. |
| ShowtimeId | uniqueidentifier | Yes | IX | References a showtime record. |
| TagsJson | nvarchar(max) | Yes | - | Serialized JSON payload for this record. |

## Table: movies.MovieGenres

- Mapped Entity: `ABCDMall.Modules.Movies.Domain.Entities.MovieGenre`
- Schema: `movies`
- Table Name: `MovieGenres`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| MovieId | uniqueidentifier | No | PK | References a movie record. |
| GenreId | uniqueidentifier | No | PK, IX | Stores the genre id value for this table. |

## Table: movies.Movies

- Mapped Entity: `ABCDMall.Modules.Movies.Domain.Entities.Movie`
- Schema: `movies`
- Table Name: `Movies`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | uniqueidentifier | No | PK | Primary identifier for this record. |
| CreatedAtUtc | datetime2 | No | - | Stores when the record was created. |
| DefaultLanguage | int | No | - | Stores the default language value for this table. |
| DurationMinutes | int | No | - | Stores the duration minutes value for this table. |
| PosterUrl | nvarchar(1000) | Yes | - | Path or URL to an image asset. |
| RatingLabel | nvarchar(20) | Yes | - | Stores the rating label value for this table. |
| ReleaseDate | date | Yes | IX | Stores the release date value for this table. |
| Slug | nvarchar(250) | No | UQ | SEO-friendly route segment or public identifier. |
| Status | int | No | IX | Status value used by the workflow for this record. |
| Synopsis | nvarchar(4000) | Yes | - | Stores the synopsis value for this table. |
| Title | nvarchar(250) | No | - | Title or headline shown for this record. |
| TrailerUrl | nvarchar(1000) | Yes | - | Stores the trailer url value for this table. |
| UpdatedAtUtc | datetime2 | No | - | Stores the most recent update time. |

## Table: movies.OutboxEvents

- Mapped Entity: `ABCDMall.Modules.Movies.Domain.Entities.OutboxEvent`
- Schema: `movies`
- Table Name: `OutboxEvents`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | uniqueidentifier | No | PK | Primary identifier for this record. |
| EventType | nvarchar(200) | No | - | Stores the event type value for this table. |
| LastError | nvarchar(max) | Yes | - | Stores the last error value for this table. |
| OccurredAtUtc | datetime2 | No | IX | Stores the occurred at utc value for this table. |
| PayloadJson | nvarchar(max) | No | - | Serialized JSON payload for this record. |
| ProcessedAtUtc | datetime2 | Yes | - | Stores the processed at utc value for this table. |
| RetryCount | int | No | - | Stores the retry count value for this table. |
| Status | nvarchar(30) | No | IX | Status value used by the workflow for this record. |

## Table: movies.Payments

- Mapped Entity: `ABCDMall.Modules.Movies.Domain.Entities.Payment`
- Schema: `movies`
- Table Name: `Payments`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | uniqueidentifier | No | PK | Primary identifier for this record. |
| Amount | decimal(18,2) | No | - | Numeric monetary amount used by this record. |
| BookingId | uniqueidentifier | No | IX | References a booking-related record. |
| CallbackPayloadJson | nvarchar(max) | Yes | - | Serialized JSON payload for this record. |
| CompletedAtUtc | datetime2 | Yes | - | Stores the completed at utc value for this table. |
| CreatedAtUtc | datetime2 | No | - | Stores when the record was created. |
| Currency | nvarchar(10) | No | - | Stores the currency value for this table. |
| FailureReason | nvarchar(500) | Yes | - | Stores the failure reason value for this table. |
| PaymentIntentId | nvarchar(100) | Yes | - | Stores the payment intent id value for this table. |
| Provider | int | No | - | Stores the provider value for this table. |
| ProviderTransactionId | nvarchar(100) | Yes | UQ | Stores the provider transaction id value for this table. |
| Status | int | No | IX | Status value used by the workflow for this record. |
| UpdatedAtUtc | datetime2 | No | - | Stores the most recent update time. |

## Table: movies.People

- Mapped Entity: `ABCDMall.Modules.Movies.Domain.Entities.Person`
- Schema: `movies`
- Table Name: `People`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | uniqueidentifier | No | PK | Primary identifier for this record. |
| Biography | nvarchar(4000) | Yes | - | Stores the biography value for this table. |
| FullName | nvarchar(200) | No | - | Display name used for this record. |
| ProfileImageUrl | nvarchar(1000) | Yes | - | Path or URL to an image asset. |

## Table: movies.PromotionRedemptions

- Mapped Entity: `ABCDMall.Modules.Movies.Domain.Entities.PromotionRedemption`
- Schema: `movies`
- Table Name: `PromotionRedemptions`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | uniqueidentifier | No | PK | Primary identifier for this record. |
| BookingId | uniqueidentifier | Yes | IX | References a booking-related record. |
| CouponCode | nvarchar(50) | Yes | IX | Stores the coupon code value for this table. |
| DiscountAmount | decimal(18,2) | No | - | Numeric monetary amount used by this record. |
| GuestCustomerId | uniqueidentifier | Yes | IX | Stores the guest customer id value for this table. |
| PromotionId | uniqueidentifier | No | IX | References a promotion record. |
| RedeemedAtUtc | datetime2 | No | - | Stores the redeemed at utc value for this table. |

## Table: movies.PromotionRules

- Mapped Entity: `ABCDMall.Modules.Movies.Domain.Entities.PromotionRule`
- Schema: `movies`
- Table Name: `PromotionRules`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | uniqueidentifier | No | PK | Primary identifier for this record. |
| IsRequired | bit | No | - | Boolean flag describing the state of this record. |
| PromotionId | uniqueidentifier | No | IX | References a promotion record. |
| RuleType | int | No | - | Stores the rule type value for this table. |
| RuleValue | nvarchar(200) | No | - | Stores the rule value value for this table. |
| SortOrder | int | No | IX | Stores the sort order value for this table. |
| ThresholdValue | decimal(18,2) | Yes | - | Stores the threshold value value for this table. |

## Table: movies.Promotions

- Mapped Entity: `ABCDMall.Modules.Movies.Domain.Entities.Promotion`
- Schema: `movies`
- Table Name: `Promotions`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | uniqueidentifier | No | PK | Primary identifier for this record. |
| Code | nvarchar(50) | No | UQ | Stores the code value for this table. |
| CreatedAtUtc | datetime2 | No | - | Stores when the record was created. |
| Description | nvarchar(1000) | No | - | Long-form descriptive content. |
| FlatDiscountValue | decimal(18,2) | Yes | - | Stores the flat discount value value for this table. |
| IsAutoApplied | bit | No | - | Boolean flag describing the state of this record. |
| MaxRedemptions | int | Yes | - | Stores the max redemptions value for this table. |
| MaxRedemptionsPerCustomer | int | Yes | - | Stores the max redemptions per customer value for this table. |
| MaximumDiscountAmount | decimal(18,2) | Yes | - | Numeric monetary amount used by this record. |
| MetadataJson | nvarchar(max) | Yes | - | Serialized JSON payload for this record. |
| MinimumSpendAmount | decimal(18,2) | Yes | - | Numeric monetary amount used by this record. |
| Name | nvarchar(200) | No | - | Display name used for this record. |
| PercentageValue | decimal(5,2) | Yes | - | Stores the percentage value value for this table. |
| Status | int | No | IX | Status value used by the workflow for this record. |
| UpdatedAtUtc | datetime2 | No | - | Stores the most recent update time. |
| ValidFromUtc | datetimeoffset | Yes | IX | Stores the valid from utc value for this table. |
| ValidToUtc | datetimeoffset | Yes | IX | Stores the valid to utc value for this table. |

## Table: movies.Showtimes

- Mapped Entity: `ABCDMall.Modules.Movies.Domain.Entities.Showtime`
- Schema: `movies`
- Table Name: `Showtimes`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | uniqueidentifier | No | PK | Primary identifier for this record. |
| BasePrice | decimal(18,2) | No | - | Numeric monetary amount used by this record. |
| BusinessDate | date | No | IX | Stores the business date value for this table. |
| CinemaId | uniqueidentifier | No | IX | References a location or seating record. |
| CreatedAtUtc | datetime2 | No | - | Stores when the record was created. |
| EndAtUtc | datetime2 | Yes | - | Stores the end at utc value for this table. |
| HallId | uniqueidentifier | No | IX | References a location or seating record. |
| Language | int | No | - | Stores the language value for this table. |
| MovieId | uniqueidentifier | No | IX | References a movie record. |
| StartAtUtc | datetime2 | No | IX | Stores the start at utc value for this table. |
| Status | int | No | IX | Status value used by the workflow for this record. |
| UpdatedAtUtc | datetime2 | No | - | Stores the most recent update time. |

## Table: movies.ShowtimeSeatInventory

- Mapped Entity: `ABCDMall.Modules.Movies.Domain.Entities.ShowtimeSeatInventory`
- Schema: `movies`
- Table Name: `ShowtimeSeatInventory`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | uniqueidentifier | No | PK | Primary identifier for this record. |
| ColumnNumber | int | No | - | Stores the column number value for this table. |
| CoupleGroupCode | nvarchar(50) | Yes | - | Stores the couple group code value for this table. |
| HallSeatId | uniqueidentifier | No | IX | Stores the hall seat id value for this table. |
| Price | decimal(18,2) | No | - | Numeric monetary amount used by this record. |
| RowLabel | nvarchar(10) | No | - | Stores the row label value for this table. |
| SeatCode | nvarchar(20) | No | UQ | Stores the seat code value for this table. |
| SeatType | int | No | - | Stores the seat type value for this table. |
| ShowtimeId | uniqueidentifier | No | UQ, IX | References a showtime record. |
| Status | int | No | IX | Status value used by the workflow for this record. |
| UpdatedAtUtc | datetime2 | No | - | Stores the most recent update time. |

## Table: movies.SnackCombos

- Mapped Entity: `ABCDMall.Modules.Movies.Domain.Entities.SnackCombo`
- Schema: `movies`
- Table Name: `SnackCombos`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | uniqueidentifier | No | PK | Primary identifier for this record. |
| Code | nvarchar(50) | No | UQ | Stores the code value for this table. |
| CreatedAtUtc | datetime2 | No | - | Stores when the record was created. |
| Description | nvarchar(1000) | No | - | Long-form descriptive content. |
| ImageUrl | nvarchar(1000) | Yes | - | Path or URL to an image asset. |
| IsActive | bit | No | IX | Boolean flag describing the state of this record. |
| Name | nvarchar(200) | No | - | Display name used for this record. |
| Price | decimal(18,2) | No | - | Numeric monetary amount used by this record. |
| UpdatedAtUtc | datetime2 | No | - | Stores the most recent update time. |

## Table: movies.Tickets

- Mapped Entity: `ABCDMall.Modules.Movies.Domain.Entities.Ticket`
- Schema: `movies`
- Table Name: `Tickets`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | uniqueidentifier | No | PK | Primary identifier for this record. |
| BookingId | uniqueidentifier | No | IX | References a booking-related record. |
| BookingItemId | uniqueidentifier | Yes | - | Stores the booking item id value for this table. |
| DeliveryStatus | nvarchar(30) | No | - | Status value used by the workflow for this record. |
| EmailSendError | nvarchar(max) | Yes | - | Email address associated with this record. |
| EmailSentAtUtc | datetime2 | Yes | - | Email address associated with this record. |
| IssuedAtUtc | datetime2 | No | - | Boolean flag describing the state of this record. |
| PdfFileName | nvarchar(255) | Yes | - | Display name used for this record. |
| QrCodeContent | nvarchar(max) | Yes | - | Stores the qr code content value for this table. |
| SeatCode | nvarchar(20) | Yes | - | Stores the seat code value for this table. |
| SeatInventoryId | uniqueidentifier | Yes | IX | References a location or seating record. |
| TicketCode | nvarchar(50) | No | UQ | Stores the ticket code value for this table. |
| UpdatedAtUtc | datetime2 | Yes | - | Stores the most recent update time. |

## Table: shops.ShopProducts

- Mapped Entity: `ABCDMall.Modules.Shops.Domain.Entities.ShopProduct; ABCDMall.Modules.Users.Domain.Entities.PublicShopProduct`
- Schema: `shops`
- Table Name: `ShopProducts`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | nvarchar(64) | Yes | PK | Primary identifier for this record. |
| DiscountPercent | int | Yes | - | Stores the discount percent value for this table. |
| ImageUrl | nvarchar(1000) | No | - | Path or URL to an image asset. |
| IsDiscounted | bit | No | - | Boolean flag describing the state of this record. |
| IsFeatured | bit | No | - | Boolean flag describing the state of this record. |
| Name | nvarchar(200) | No | - | Display name used for this record. |
| OldPrice | decimal(18,2) | Yes | - | Numeric monetary amount used by this record. |
| Price | decimal(18,2) | No | - | Numeric monetary amount used by this record. |
| ShopId | nvarchar(64) | No | IX | References a shop-related record. |

## Table: shops.Shops

- Mapped Entity: `ABCDMall.Modules.Shops.Domain.Entities.Shop; ABCDMall.Modules.Users.Domain.Entities.PublicShop`
- Schema: `shops`
- Table Name: `Shops`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | nvarchar(64) | Yes | PK | Primary identifier for this record. |
| Badge | nvarchar(120) | Yes | - | Stores the badge value for this table. |
| Category | nvarchar(120) | No | - | Stores the category value for this table. |
| CoverImageUrl | nvarchar(1000) | No | - | Path or URL to an image asset. |
| Description | nvarchar(4000) | No | - | Long-form descriptive content. |
| Floor | nvarchar(80) | No | - | Stores the floor value for this table. |
| LocationSlot | nvarchar(80) | No | - | Stores the location slot value for this table. |
| LogoUrl | nvarchar(1000) | No | - | Path or URL to an image asset. |
| Name | nvarchar(200) | No | - | Display name used for this record. |
| Offer | nvarchar(300) | Yes | - | Stores the offer value for this table. |
| OpenHours | nvarchar(100) | No | - | Stores the open hours value for this table. |
| Slug | nvarchar(200) | No | UQ | SEO-friendly route segment or public identifier. |
| Summary | nvarchar(1000) | No | - | Short summary used in listings or previews. |
| OwnerShopId | nvarchar(64) | Yes | - | References a shop-related record. |

## Table: shops.ShopTags

- Mapped Entity: `ABCDMall.Modules.Shops.Domain.Entities.ShopTag`
- Schema: `shops`
- Table Name: `ShopTags`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | nvarchar(64) | Yes | PK | Primary identifier for this record. |
| ShopId | nvarchar(64) | No | IX | References a shop-related record. |
| Value | nvarchar(120) | No | - | Stores the value value for this table. |

## Table: shops.ShopVouchers

- Mapped Entity: `ABCDMall.Modules.Shops.Domain.Entities.ShopVoucher; ABCDMall.Modules.Users.Domain.Entities.PublicShopVoucher`
- Schema: `shops`
- Table Name: `ShopVouchers`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | nvarchar(64) | Yes | PK | Primary identifier for this record. |
| Code | nvarchar(80) | No | - | Stores the code value for this table. |
| Description | nvarchar(1000) | No | - | Long-form descriptive content. |
| IsActive | bit | No | - | Boolean flag describing the state of this record. |
| ShopId | nvarchar(64) | No | IX | References a shop-related record. |
| Title | nvarchar(200) | No | - | Title or headline shown for this record. |
| ValidUntil | nvarchar(120) | No | - | Stores the valid until value for this table. |

## Table: utility_map.FloorPlans

- Mapped Entity: `ABCDMall.Modules.UtilityMap.Domain.Entities.FloorPlan`
- Schema: `utility_map`
- Table Name: `FloorPlans`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | int | No | PK | Primary identifier for this record. |
| BlueprintImageUrl | nvarchar(500) | No | - | Path or URL to an image asset. |
| Description | nvarchar(500) | No | - | Long-form descriptive content. |
| FloorLevel | nvarchar(50) | No | - | Stores the floor level value for this table. |

## Table: utility_map.MapLocations

- Mapped Entity: `ABCDMall.Modules.UtilityMap.Domain.Entities.MapLocation`
- Schema: `utility_map`
- Table Name: `MapLocations`

| Field Name | Data Type | Null | Keys | Description |
|---|---|---|---|---|
| Id | int | No | PK | Primary identifier for this record. |
| FloorPlanId | int | No | IX | Stores the floor plan id value for this table. |
| LocationSlot | nvarchar(50) | No | - | Stores the location slot value for this table. |
| ShopName | nvarchar(200) | No | - | Display name used for this record. |
| ShopUrl | nvarchar(500) | No | - | Stores the shop url value for this table. |
| StorefrontImageUrl | nvarchar(500) | No | - | Path or URL to an image asset. |
| X | float | No | - | Stores the x value for this table. |
| Y | float | No | - | Stores the y value for this table. |

