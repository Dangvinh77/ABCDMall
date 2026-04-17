# Dev1 Note For Dev2 - Quote / Hold Boundary

> Updated from real source state on 2026-04-17.

## Boundary confirmation

Dev 1 has confirmed the booking-side quote and hold flow can rely on these catalog/screening fields.

### Showtime

- `Id`
- `MovieId`
- `CinemaId`
- `HallId`
- `BusinessDate`
- `StartAtUtc`
- `BasePrice`
- `Status`
- `Language`
- `Hall.HallType`

### ShowtimeSeatInventory

- `Id`
- `ShowtimeId`
- `SeatCode`
- `RowLabel`
- `ColumnNumber`
- `SeatType`
- `Status`
- `Price`
- `CoupleGroupCode`

## Source of truth

- seat price for quote: `ShowtimeSeatInventory.Price`
- seat availability for quote: `ShowtimeSeatInventory.Status`
- couple-seat grouping for quote: `ShowtimeSeatInventory.CoupleGroupCode`

## API response contract now exposed by Dev 1

### Showtime contract

- `hallType` is exposed as shared contract values:
  - `2D`
  - `3D`
  - `IMAX`
  - `4DX`
- `language` is exposed as shared contract values:
  - `Sub`
  - `Dub`

### Seat-map contract

Each seat in `GET /api/showtimes/{showtimeId}/seat-map` exposes:

- `seatInventoryId`
- `seatCode`
- `row`
- `col`
- `seatType`
- `status`
- `price`
- `coupleGroupCode`

## Important current behavior

- seat-map no longer reflects only catalog seat inventory status
- active booking holds are now overlaid into seat-map response as:
  - `Held`
- this overlay comes from booking-side hold data, not from direct mutation of `ShowtimeSeatInventory.Status`

## Stability note

These fields and meanings should now be treated as stable for current quote + hold integration.
If Dev 1 changes schema, enum names, or seat-map shape, Dev 2 must be informed first.

## Previous seed issue status

The older note about inconsistent couple-seat seed status from `CatalogSeed` is no longer the active concern:

- `CatalogSeed` has been removed from runtime/test seed flow
- runtime and tests now use `FrontendMoviesSeed`
- current Movies tests already verify couple-seat group consistency in seat-map output

## Recommendation for Dev 2

- rely on `seat-map` + quote flow using the current Dev 1 response contract
- treat `Held` as a temporary booking-side overlay state
- do not assume raw enum names like `Standard2D` or `Subtitle` will appear in public API responses
- use shared contract values from API output instead:
  - `2D/3D/IMAX/4DX`
  - `Sub/Dub`
