# Working In Progress - Dev 2

> Updated from real source state on 2026-04-17.
> Scope: `Booking + Promotion + Payment`.

## 1. Current progress

### Completed

- `MoviesBookingDbContext` is done
- booking-side entities are present:
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
- Dev 2 enums are present:
  - `BookingHoldStatus`
  - `BookingStatus`
  - `PaymentProvider`
  - `PaymentStatus`
  - `PromotionStatus`
  - `PromotionRuleType`
  - `PromotionEvaluationStatus`
- booking configurations are present
- booking migration is present
- promotion/snack combo seed is present in consolidated runtime seed: `Seed/FrontendMoviesSeed.cs`
- promotion query/evaluation services are present
- snack combo query service is present
- promotion repository is present
- quote services are present:
  - `IBookingQuoteService`
  - `BookingQuoteService`
- hold services are present:
  - `IBookingHoldService`
  - `BookingHoldService`
- hold persistence is present:
  - `IBookingHoldRepository`
  - `BookingHoldRepository`
- hold cleanup background service is present:
  - `BookingHoldCleanupBackgroundService`
- booking validators are present:
  - `BookingQuoteRequestDtoValidator`
  - `CreateBookingHoldRequestDtoValidator`
- DI registration is already wired for:
  - booking quote service
  - booking hold service
  - booking validators

## 2. APIs already available

- `GET /api/movies-promotions`
- `GET /api/movies-promotions/{promotionId}`
- `POST /api/movies-promotions/evaluate`
- `GET /api/snack-combos`
- `POST /api/bookings/quote`
- `POST /api/bookings/hold`
- `GET /api/bookings/holds/{holdId}`
- `DELETE /api/bookings/holds/{holdId}`
- `POST /api/bookings/holds/{holdId}/confirm`

## 3. Quote flow status

### Already executable end-to-end

- `BookingsController` now exists
- `POST /api/bookings/quote` is exposed in WebAPI
- quote validation is executed before service call
- quote service is registered in DI
- quote flow consumes Dev 1 screening boundary through application/infrastructure wiring already present in source

### Current behavior note

- quote is now usable for demo/frontend integration
- invalid quote cases are handled by returning `400 BadRequest` with problem details

## 4. Hold flow status

### Already implemented for demo/test flow

- `POST /api/bookings/hold`
- `GET /api/bookings/holds/{holdId}`
- `DELETE /api/bookings/holds/{holdId}`
- `POST /api/bookings/holds/{holdId}/confirm`
- active hold seats are reflected back into Dev 1 `seat-map` as `Held`
- hold cleanup background service exists

### Important note

- `confirm` is explicitly marked in source as `DAY5 TEST-ONLY CONFIRM FLOW`
- this is not the final booking/payment flow
- it is only a temporary bridge so frontend can demo hold -> confirm seat behavior before full checkout is built

## 5. Remaining work for Dev 2

### Priority 1: replace demo confirm with real booking flow

- create real booking create use case
- expose:
  - `POST /api/bookings`
  - `GET /api/bookings/{bookingCode}`
- persist booking snapshot and booking items
- stop relying on test-only confirm endpoint for real checkout

### Priority 2: payment

- create payment DTOs/services
- expose:
  - `POST /api/payments/intents`
  - `POST /api/payments/callback/{provider}`
  - `GET /api/payments/{paymentId}`
- add idempotent callback handling

### Priority 3: ticket/outbox/email

- implement ticket issuing flow
- implement resend ticket endpoint
- implement outbox/email/background processing

### Priority 4: admin management

- admin CRUD for promotions
- admin CRUD for snack combos
- admin read/support actions for bookings
- admin read/support actions for payments

## 6. Current blockers / gaps

- no real booking creation endpoint yet
- no real payment integration yet
- no ticket issuing flow yet
- `confirm hold` is only a temporary demo endpoint and should not be treated as final domain behavior
- no real admin APIs yet for promotion/snack combo/booking/payment management

## 7. Notes

- Dev 1 boundary APIs already exist and are stable enough for current quote/hold integration:
  - `GET /api/showtimes`
  - `GET /api/showtimes/{showtimeId}`
  - `GET /api/showtimes/{showtimeId}/seat-map`
- frontend can already demo:
  - promotion list/evaluate
  - snack combos
  - quote
  - hold seat
  - release hold
  - temporary confirm path

## 8. Conclusion

Dev 2 is no longer only at "promotion + quote skeleton" stage. Source now has a usable demo slice for `quote + hold`, including controller, DI wiring, repository, cleanup background service, and temporary confirm flow. The next real milestone is to replace the demo confirm path with full booking, payment, and ticket issuance.
