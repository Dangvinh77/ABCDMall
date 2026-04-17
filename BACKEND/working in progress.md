# Working In Progress - Movies Backend

> Updated from actual source state on 2026-04-16.
> Inputs checked: backend source, `Movie_backend_work_split.md`, `Movie_dev1_execution_plan.md`, `Movie_dev2_execution_plan.md`.

## 1. Overall status

### Build status

- `dotnet build BACKEND/ABCDMall.sln` passed
- current warnings are outside Movies module (`FoodCourt`)

### Shared foundation

- `Option B` is in place with 2 DbContext:
  - `MoviesCatalogDbContext`
  - `MoviesBookingDbContext`
- both catalog and booking migrations already exist
- startup already runs:
  - catalog migration
  - catalog seed
  - booking migration
  - promotion seed

## 2. Dev 1 status: Catalog + Screening

### Done in source

- domain entities and enums for catalog/screening are present
- `MoviesCatalogDbContext` and configurations are present
- catalog migration is present
- catalog seed is present: `Seed/CatalogSeed.cs`
- read repositories are present:
  - `Repositories/Catalog/MovieRepository.cs`
  - `Repositories/Screening/ShowtimeRepository.cs`
- application services are present:
  - `MovieQueryService`
  - `ShowtimeQueryService`
  - `SeatMapQueryService`
- AutoMapper profile is present: `Mappings/MovieProfile.cs`
- API controllers are present:
  - `MoviesController`
  - `ShowtimesController`
- Dev 1 ownership APIs are already opened:
  - `GET /api/movies/home`
  - `GET /api/movies`
  - `GET /api/movies/{movieId}`
  - `GET /api/movies/{movieId}/showtimes`
  - `GET /api/showtimes`
  - `GET /api/showtimes/{showtimeId}`
  - `GET /api/showtimes/{showtimeId}/seat-map`

### Remaining for Dev 1

- add validation for movie/showtime query inputs
- add stronger logging around read flows and seed
- review query/index tuning for `Showtimes` and `ShowtimeSeatInventory`
- verify `seat-map` contract with Dev 2 on real integration cases
- add endpoint-level tests/integration tests for movies/showtimes/seat-map

## 3. Dev 2 status: Booking + Promotion + Payment

### Done in source

- booking-side entities and enums are present
- `MoviesBookingDbContext` and booking configurations are present
- booking migration is present
- promotion seed is present: `Seed/MoviesPromotionSeed.cs`
- promotion repository is present: `Repositories/Promotions/PromotionRepository.cs`
- promotion services are present:
  - `PromotionQueryService`
  - `PromotionEvaluationService`
  - `SnackComboQueryService`
- promotion endpoints are already opened:
  - `GET /api/movies-promotions`
  - `GET /api/movies-promotions/{promotionId}`
  - `POST /api/movies-promotions/evaluate`
  - `GET /api/snack-combos`
- quote DTOs, validator, and `BookingQuoteService` already exist in application layer

### Not done yet in source

- no `BookingsController`
- no `PaymentsController`
- no booking hold service/repository
- no create booking service/repository
- no payment intent/callback service
- no ticket issuing flow
- no outbox/email/background service
- quote flow is not wired end-to-end yet

### Current blocker for Dev 2

- `BookingQuoteService` depends on:
  - `IShowtimeReadRepository`
  - `ISeatInventoryReadRepository`
- current infrastructure does not provide concrete implementations for these interfaces
- DI also does not register:
  - `IBookingQuoteService`
  - booking validators
  - booking read repositories for quote flow

## 4. Recommended next order

### Next for Dev 1

1. add validation + tests for movies/showtimes APIs
2. confirm stable `seat-map` payload for booking integration
3. review query/index performance

### Next for Dev 2

1. wire quote flow end-to-end
2. expose `POST /api/bookings/quote`
3. implement hold flow:
   - `POST /api/bookings/holds`
   - `GET /api/bookings/holds/{holdId}`
   - `DELETE /api/bookings/holds/{holdId}`
4. implement create booking
5. implement payment flow
6. implement ticket/outbox/email

## 5. Short conclusion

- Dev 1 is mostly complete for the planned read boundary and has usable APIs in source now.
- Dev 2 has completed promotion flow and part of the quote application layer, but booking/payment execution flow is still not implemented at API/infrastructure level.
