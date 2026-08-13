# Movies Per-Seat Hold Design

Date: 2026-04-22

## Summary

Replace the current seat-page shared countdown with per-seat 10 minute holds that start as soon as a seat is selected on the seat map.

Each selected seat must:

- create a real backend hold immediately on selection
- display its own countdown timer in the `Selected seats` UI
- release its own hold when the same seat is unselected
- be treated as held by someone else after a page refresh because the client does not restore prior ownership

The current backend already supports booking holds, hold expiry cleanup, and held-seat projection into the seat map. The main change is moving hold creation earlier in the flow and changing checkout from single-hold consumption to multi-hold consumption.

## Goals

- Start a real 10 minute hold when a user selects a seat on the seat map.
- Show one timer per selected seat in the `Selected seats` section.
- Release only the specific seat hold when the user unselects that seat.
- Preserve existing backend-driven held-seat visibility for other users.
- Make refresh behave like a fresh visitor so previously selected seats appear as externally held.
- Allow checkout to create one booking from multiple independent seat holds.

## Non-Goals

- Restoring ownership of held seats after refresh.
- Introducing hold renewal or silent extension.
- Showing ownership-aware held seats in seat map after reload.
- Reworking promotion logic beyond what is necessary to decouple it from hold creation.
- Changing the existing 10 minute backend hold duration.

## Current State

### Frontend

- `SeatSelectionPage.tsx` starts a shared `timeLeft` countdown locally when the page loads.
- Seat holds are not created during selection.
- `createBookingHold()` is only called inside the seat-page checkout transition.
- Seat selection state is purely client-managed until checkout.

### Backend

- `BookingHoldService` creates real holds with a fixed 10 minute expiration.
- `BookingHoldCleanupBackgroundService` expires active holds in the background.
- `SeatMapQueryService` overlays `Held` status using active booking holds.
- `CreateBooking` currently assumes one `holdId`, not many.

## Recommended Approach

Use one backend `BookingHold` per selected seat.

This matches the requested behavior directly:

- each seat gets an independent expiration time
- each seat can be released independently
- no partial-release semantics are needed inside a multi-seat hold
- refresh naturally loses client ownership while backend still reports the seat as held

The main cost is more API calls when selecting several seats, but the domain behavior remains clear and close to the current backend model.

## Detailed Design

### 1. Seat Selection Flow

When the user clicks an available seat:

1. Frontend sends `POST /api/bookings/hold` with exactly one `seatInventoryId`.
2. Backend creates a normal `BookingHold` for that seat.
3. Frontend stores the returned `holdId`, `expiresAtUtc`, and `remainingSeconds` on that selected seat record.
4. The seat becomes `selected` only after hold creation succeeds.

If hold creation fails because the seat was already held or booked:

- the frontend marks the seat as `held`
- the selected-seat entry is not created
- the frontend refreshes the seat map to reconcile state

### 2. Seat Unselect Flow

When the user clicks a selected seat again:

1. Frontend looks up the `holdId` associated with that seat.
2. Frontend calls `DELETE /api/bookings/holds/{holdId}`.
3. On success, frontend removes the seat from selected state.
4. The seat returns to `available` on the next local render and remains consistent with the next seat-map refresh.

If release fails:

- the seat remains selected locally
- the user receives a recoverable error
- the frontend may refresh seat map to reconcile if needed

### 3. Per-Seat Timer UI

`Selected seats` renders one line or chip per seat with:

- seat code
- seat type
- price
- independent countdown derived from backend `expiresAtUtc`

Timer behavior:

- countdown is derived from wall-clock comparison against `expiresAtUtc`
- there is no shared seat-page countdown anymore
- each seat expires independently
- when a timer reaches zero, the frontend removes that seat from the local selected list and refreshes the seat map

The frontend must not display a timer for seats that it no longer owns locally after refresh.

### 4. Refresh Behavior

On full page refresh:

- all local selected-seat state is discarded
- the client does not reload previous `holdId` ownership
- the seat map is fetched from backend as normal
- any still-active prior holds appear as `Held`, not `Selected`

This intentionally makes refresh behave like a new visitor opening the seat map.

### 5. Checkout Flow

Checkout must switch from single-hold to multi-hold input.

Frontend changes:

- pass `holdIds: string[]` for all currently selected seats
- do not create new holds during `goToCheckout`

Backend changes:

- update booking creation request DTO to accept multiple `holdIds`
- validate all holds before creating the booking

Validation rules:

- every hold must exist
- every hold must still be `Active`
- every hold must be unexpired
- every hold must belong to the same `showtimeId`
- no duplicate `holdIds`

Booking creation behavior:

- create one booking from all held seats
- aggregate prices from the validated holds and checkout quote
- mark all consumed holds as converted in the same transaction as booking creation

If any hold is expired or invalid, booking creation must fail clearly and no partial booking may be created.

### 6. Promotions and Combos

Per-seat hold creation should not duplicate checkout-level pricing state across many holds.

Recommended behavior:

- seat hold creation uses only seat data needed to reserve inventory
- promo, combo, birthday, and payment-provider-sensitive quote logic remains at quote/checkout time

Implication:

- remove the current dependency that packs snack combos and promotion data into every seat hold created from the seat page
- keep `quoteBooking()` as the source of dynamic totals while the user is still composing the cart

This avoids repeated promo/combo snapshots across independent one-seat holds.

### 7. Backend Persistence and Querying

The existing backend data model is mostly reusable.

Expected changes:

- no schema change is required for per-seat holds if each hold simply contains one `BookingHoldSeat`
- repository and service logic can continue using current hold expiration semantics
- booking creation logic must be updated to consume many holds atomically

`SeatMapQueryService` can remain conceptually unchanged because it already projects active hold seat IDs as `Held`.

## Error Handling

### Seat hold creation errors

- If the seat is already held or booked, show the seat as `held` and refresh seat map.
- If the request fails unexpectedly, show an inline or alert-level error and keep the seat unselected.

### Seat release errors

- Keep the seat selected until release is confirmed.
- Show a recoverable error and allow retry.

### Expiry during seat selection

- Remove expired seat from selected list when local timer reaches zero or when seat-map refresh shows the hold is gone.

### Expiry during checkout

- Booking creation must fail with a specific error indicating one or more holds expired or became invalid.
- Frontend should send the user back to seat selection or prompt them to reselect seats.

## Testing Strategy

### Backend tests

- Creating one-seat holds succeeds for available seats.
- Creating a hold for an already-held seat fails with a conflict-style validation error.
- Releasing one hold does not affect other held seats.
- Creating one booking from multiple hold IDs succeeds when all holds are valid and same-showtime.
- Creating one booking from multiple hold IDs fails when any hold is expired, released, converted, duplicated, or belongs to another showtime.

### Frontend tests

- Selecting a seat calls hold API immediately and only marks the seat selected on success.
- Unselecting a selected seat calls release API for the correct hold.
- `Selected seats` renders one timer per selected seat.
- Refresh does not restore prior selected-seat ownership.
- Checkout submits multiple hold IDs instead of a single hold ID.

### Contract/integration checks

- Seat map shows externally held seats after a fresh reload.
- Expired holds disappear from selected state and from backend held-seat projection after cleanup.

## Risks

- More network calls during seat selection can increase UI latency when selecting many seats quickly.
- Existing checkout and payment flow assume one hold; this change touches a central path and must be validated carefully.
- Promotion and combo pricing can drift if hold payloads continue to duplicate pricing context instead of moving that logic to quote/checkout only.

## Open Decisions Resolved

- Timer model: one timer per seat.
- Hold granularity: one hold per seat.
- Refresh ownership: do not restore ownership after refresh.

## Implementation Outline

1. Update seat-page state model to track per-seat hold metadata.
2. Move hold creation from checkout transition into seat selection.
3. Remove shared seat-page timer and render per-seat timers.
4. Release seat-specific holds on unselect.
5. Change checkout request/booking API from single `holdId` to multiple `holdIds`.
6. Update backend booking service and tests for multi-hold consumption.
7. Verify seat map, checkout, and expiry behavior end to end.
