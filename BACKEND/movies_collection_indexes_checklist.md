# Movies Module MongoDB Indexes Checklist

## Purpose

This file lists recommended indexes for the `Movies` module collections in `ABCDMall`.

Goal:

- fast customer page queries
- safe booking flow
- faster admin filtering
- simpler payment and email debugging

---

## 1. `movies_movies`

Required:

- unique index: `{ slug: 1 }`
- index: `{ status: 1, isActive: 1 }`
- index: `{ releaseDate: -1 }`

Recommended:

- text index:
  - `{ "title.default": "text", "title.vi": "text" }`

Reason:

- support movie page routing
- show now showing / coming soon quickly
- support admin search

---

## 2. `movies_cinemas`

Required:

- unique index: `{ code: 1 }`
- index: `{ isActive: 1 }`

Recommended:

- `2dsphere` index on:
  - `{ location: "2dsphere" }`

Reason:

- unique branch lookup
- active branch filtering
- future map/nearby feature support

---

## 3. `movies_halls`

Required:

- unique compound index: `{ cinemaId: 1, code: 1 }`
- index: `{ cinemaId: 1, hallType: 1 }`
- index: `{ isActive: 1 }`

Reason:

- avoid duplicate hall codes in one cinema
- query all IMAX/4DX halls in one branch

---

## 4. `movies_showtimes`

Required:

- index: `{ movieId: 1, startsAt: 1 }`
- index: `{ cinemaId: 1, startsAt: 1 }`
- index: `{ hallId: 1, startsAt: 1 }`
- index: `{ bookingDate: 1, movieId: 1 }`
- index: `{ bookingStatus: 1, isPublished: 1, startsAt: 1 }`

Strongly recommended:

- unique compound index: `{ hallId: 1, startsAt: 1 }`

Optional:

- index: `{ bookingDate: 1, cinemaId: 1, bookingStatus: 1 }`

Reason:

- customer showtime listing
- admin showtime filtering
- prevent hall schedule collision

---

## 5. `movies_bookings`

Required:

- unique index: `{ bookingCode: 1 }`
- index: `{ "guest.email": 1, createdAt: -1 }`
- index: `{ showtimeId: 1, status: 1 }`
- index: `{ "payment.paymentId": 1 }`
- index: `{ status: 1, createdAt: -1 }`
- index: `{ seatLockExpiresAt: 1 }`

Recommended:

- index: `{ bookingCode: 1, status: 1 }`
- index: `{ movieId: 1, createdAt: -1 }`
- index: `{ cinemaId: 1, createdAt: -1 }`

Reason:

- support guest lookup by email
- support admin order management
- release expired bookings efficiently

---

## 6. `movies_seat_locks`

Required:

- index: `{ showtimeId: 1, seatCodes: 1 }`
- index: `{ bookingId: 1 }`
- TTL index: `{ expiresAt: 1 }`

Recommended:

- index: `{ guestEmail: 1, createdAt: -1 }`

Reason:

- prevent double-lock
- auto-clean expired seat reservations

Note:

- TTL cleanup is not immediate to the second, so booking logic must still verify lock validity in code

---

## 7. `movies_payments`

Required:

- index: `{ bookingId: 1 }`
- index: `{ bookingCode: 1 }`
- index: `{ status: 1, createdAt: -1 }`
- index: `{ "callback.received": 1, status: 1 }`

Recommended:

- unique sparse index: `{ provider: 1, providerTransactionId: 1 }`

Optional:

- index: `{ provider: 1, createdAt: -1 }`

Reason:

- payment reconciliation
- callback debugging
- prevent duplicate external transaction ids

---

## 8. `movies_ticket_deliveries`

Required:

- index: `{ bookingId: 1, createdAt: -1 }`
- index: `{ guestEmail: 1, createdAt: -1 }`
- index: `{ status: 1, createdAt: -1 }`

Recommended:

- index: `{ ticketCode: 1 }`

Reason:

- resend ticket support
- audit delivery failures
- inspect email history by booking or email

---

## 9. `movies_guests`

Required:

- unique index: `{ email: 1 }`
- index: `{ "stats.lastBookingAt": -1 }`

Recommended:

- index: `{ riskFlags: 1 }`

Reason:

- support guest history lookup
- detect suspicious cases faster

---

## 10. `movies_promotions`

Required:

- unique index: `{ code: 1 }`
- index: `{ status: 1, startsAt: 1, endsAt: 1 }`

Optional:

- index: `{ "conditions.paymentMethods": 1, status: 1 }`

Reason:

- promo lookup by code
- active campaign filtering

---

## 11. `movies_refunds`

Required:

- index: `{ bookingId: 1 }`
- index: `{ paymentId: 1 }`
- index: `{ status: 1, requestedAt: -1 }`

Reason:

- refund tracking
- support and finance workflows

---

## 12. `movies_admin_users`

Required:

- unique index: `{ username: 1 }`
- unique index: `{ email: 1 }`
- index: `{ status: 1 }`

Reason:

- admin account lookup
- permission management

---

## 13. `movies_activity_logs`

Required:

- index: `{ category: 1, createdAt: -1 }`
- index: `{ level: 1, createdAt: -1 }`
- index: `{ "reference.bookingId": 1 }`
- index: `{ "reference.paymentId": 1 }`

Recommended:

- index: `{ "reference.showtimeId": 1 }`
- index: `{ "reference.adminUsername": 1, createdAt: -1 }`

Reason:

- operational debugging
- support investigation
- admin audit review

---

## 14. Priority Rollout

If the backend team wants staged rollout, apply indexes in this order:

### Phase 1: Must-have for booking flow

- `movies_showtimes`
- `movies_bookings`
- `movies_seat_locks`
- `movies_payments`

### Phase 2: Support and admin

- `movies_ticket_deliveries`
- `movies_promotions`
- `movies_activity_logs`

### Phase 3: Extended support

- `movies_guests`
- `movies_refunds`
- `movies_admin_users`

---

## 15. Final Recommendation

The most critical indexes for the module are:

1. `movies_showtimes.hallId + startsAt`
2. `movies_bookings.bookingCode`
3. `movies_bookings.guest.email + createdAt`
4. `movies_seat_locks.expiresAt` TTL
5. `movies_payments.provider + providerTransactionId`

These 5 indexes protect the most important flows:

- schedule correctness
- booking retrieval
- guest support
- seat lock cleanup
- payment reconciliation
