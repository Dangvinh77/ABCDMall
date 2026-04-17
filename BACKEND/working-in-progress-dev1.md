# Working In Progress - Dev 1

> Updated from real source state on 2026-04-17.
> Scope: `Catalog + Screening`.

## 1. Current progress

### Completed

- `MoviesCatalogDbContext` is done
- catalog/screening entities are done:
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
- Dev 1 enums are done:
  - `MovieStatus`
  - `HallType`
  - `LanguageType`
  - `SeatType`
  - `ShowtimeStatus`
  - `SeatInventoryStatus`
- EF Core configurations are done
- catalog migration is done
- catalog seed is done and consolidated into: `Seed/FrontendMoviesSeed.cs`
- movie/showtime repositories are done
- movie/showtime/seat-map services are done
- AutoMapper movie/showtime mapping is done
- Dev 1 controllers are done:
  - `MoviesController`
  - `ShowtimesController`
- query input validation is now present for Dev 1 read APIs:
  - `MovieListQueryDtoValidator`
  - `MovieShowtimesQueryDtoValidator`
  - `ShowtimeListQueryDtoValidator`
- validation is wired in WebAPI controllers for:
  - `GET /api/movies`
  - `GET /api/movies/{movieId}/showtimes`
  - `GET /api/showtimes`
- logging is now added for:
  - movie home
  - movie list
  - movie detail
  - movie showtimes
  - showtime list
  - showtime detail
  - seat-map
  - startup migration/seed flow
- shared contract mapping is now hardened:
  - `hallType` response uses `2D | 3D | IMAX | 4DX`
  - `language` response uses `Sub | Dub`
  - filters also accept shared contract aliases
- Dev 1 test coverage now exists in `ABCDMall.Modules.Movies.Tests`:
  - query validation tests
  - showtime/seat-map contract tests

## 2. APIs already available

- `GET /api/movies/home`
- `GET /api/movies`
- `GET /api/movies/{movieId}`
- `GET /api/movies/{movieId}/showtimes`
- `GET /api/showtimes`
- `GET /api/showtimes/{showtimeId}`
- `GET /api/showtimes/{showtimeId}/seat-map`

## 3. Boundary delivered to Dev 2

### Showtime boundary

- `showtimeId`
- `movieId`
- `cinemaId`
- `hallId`
- `hallType`
- `businessDate`
- `startAt`
- `basePrice`
- `status`
- `language`

### Seat-map boundary

- `seatInventoryId`
- `seatCode`
- `row`
- `col`
- `seatType`
- `status`
- `price`
- `coupleGroupCode`

### Current contract note

- `seat-map` now also overlays active booking holds as `Held`
- this is already consumed from booking-side hold repository and is visible to frontend/demo flow

## 4. Verification status

### Confirmed from source

- WebAPI build passes
- Movies test project passes
- current Movies tests cover:
  - invalid query filters
  - showtime detail boundary fields
  - seat-map required fields
  - couple-seat group consistency
  - hall type / language / date filtering behavior

## 5. Remaining work for Dev 1

### Priority 1

- review seed behavior and startup seeding logs more deeply if demo data changes again
- review response payloads with frontend on actual screen bindings
- confirm whether FE wants extra local-time fields or keeps using UTC fields as-is

### Priority 2

- review performance/index tuning for:
  - `Showtimes`
  - `ShowtimeSeatInventory`
  - `HallSeats`
- tune projections if homepage/showtime filtering gets heavier with real data

### Priority 3

- admin CRUD for `Catalog + Screening` is now assigned to Dev 1:
  - movies
  - cinemas
  - halls
  - showtimes
- create separate admin endpoints instead of expanding current public read endpoints

## 6. Notes

- Dev 1 read boundary is no longer just "implemented"; it is already hardened for frontend integration
- `SeatMapQueryService` now depends on booking hold data to reflect active holds as `Held`
- shared contract values are no longer raw enum names in the response

## 7. Conclusion

Dev 1 is effectively complete for the planned `Catalog + Screening` read boundary. The remaining work is now mostly:

- performance review
- demo-data maintenance
- frontend follow-up adjustments
- future admin CRUD for `Catalog + Screening`
