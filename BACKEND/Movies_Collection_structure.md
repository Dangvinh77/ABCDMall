# Movies Module Collection Structure for MongoDB

## 1. Purpose

This document proposes a MongoDB collection structure for the `Movies` module inside the shared `ABCDMall` database.

Scope of this module:

- Public movie browsing
- Movie detail pages
- Showtimes
- Seat selection
- Guest booking flow
- Payment tracking
- Ticket delivery by email
- Admin operations for movies

This proposal is based on the current frontend behavior in:

- `FRONTEND/src/features/movies`
- `FRONTEND/src/features/movies-admin`

The goal is to keep the module isolated enough to scale independently, while still living inside the same MongoDB database as the rest of the mall project.

---

## 2. Recommended Database Strategy

Use one shared database:

- Database name: `ABCDMall`

Inside that database, group collections by module naming prefix:

- `movies_movies`
- `movies_cinemas`
- `movies_halls`
- `movies_showtimes`
- `movies_bookings`
- `movies_payments`
- `movies_ticket_deliveries`
- `movies_guests`
- `movies_promotions`
- `movies_admin_users`
- `movies_activity_logs`
- `movies_seat_locks`
- `movies_refunds`

This approach is better than putting everything into generic collection names like `movies`, `bookings`, `payments` because:

- It avoids collision with other modules in the mall
- It makes ownership clear
- It simplifies backup, debugging, and reporting per module

---

## 3. Domain Split

The Movies module should be split into these logical domains:

1. Catalog domain
2. Cinema scheduling domain
3. Booking domain
4. Payment domain
5. Ticket delivery domain
6. Guest support domain
7. Promotions domain
8. Admin and audit domain

MongoDB collections should follow this split.

---

## 4. Recommended Collections

## 4.1 `movies_movies`

Purpose:

- Store movie master data
- Feed customer home page, detail page, and admin movie management

Recommended document:

```json
{
  "_id": "movie_cosmic_odyssey",
  "slug": "cosmic-odyssey",
  "title": {
    "default": "Cosmic Odyssey",
    "vi": "Hanh Trinh Vu Tru"
  },
  "description": {
    "default": "A daring crew of astronauts...",
    "vi": "Mot phi hanh doan..."
  },
  "genreIds": ["genre_scifi", "genre_adventure"],
  "genresText": ["Sci-Fi", "Adventure"],
  "durationMinutes": 148,
  "director": "Christopher Wright",
  "cast": ["Tom Holland", "Zoe Saldana", "Oscar Isaac"],
  "language": {
    "audio": "English",
    "subtitle": "Vietnamese",
    "display": "English with Vietnamese subtitles"
  },
  "ageRating": "T13",
  "status": "now_showing",
  "releaseDate": "2026-04-10T00:00:00.000Z",
  "posterUrl": "https://...",
  "backdropUrl": "https://...",
  "trailerUrl": "https://youtube.com/...",
  "rating": {
    "score": 8.5,
    "votes": 1240
  },
  "isActive": true,
  "tags": ["featured", "imax-ready"],
  "seo": {
    "metaTitle": "Cosmic Odyssey | ABCD Cinema",
    "metaDescription": "Book tickets for Cosmic Odyssey..."
  },
  "createdAt": "2026-04-01T08:00:00.000Z",
  "updatedAt": "2026-04-05T09:30:00.000Z",
  "createdBy": "admin.mall",
  "updatedBy": "movie.ops"
}
```

Notes:

- `status` should be enum:
  - `draft`
  - `coming_soon`
  - `now_showing`
  - `archived`
- Keep `durationMinutes` as number, not text like `148 min`
- `slug` should be unique

Suggested indexes:

- `{ slug: 1 }` unique
- `{ status: 1, isActive: 1 }`
- `{ releaseDate: -1 }`
- `{ "title.default": "text", "title.vi": "text" }`

---

## 4.2 `movies_cinemas`

Purpose:

- Store cinema branch information used by showtimes and admin

Recommended document:

```json
{
  "_id": "cinema_abcd_mall",
  "code": "abcd-mall",
  "name": "ABCD Cinema - ABCD Mall",
  "mallBranch": "ABCD Mall",
  "address": {
    "line1": "Level 5, ABCD Mall",
    "street": "123 Le Van Viet Street",
    "district": "Thu Duc",
    "city": "Ho Chi Minh City",
    "country": "Vietnam"
  },
  "contact": {
    "phone": "1900xxxx",
    "email": "cinema.mall@abcd.vn"
  },
  "location": {
    "type": "Point",
    "coordinates": [106.79, 10.84]
  },
  "isActive": true,
  "openingHours": {
    "open": "08:00",
    "close": "23:59"
  },
  "createdAt": "2026-04-01T08:00:00.000Z",
  "updatedAt": "2026-04-05T09:30:00.000Z"
}
```

Suggested indexes:

- `{ code: 1 }` unique
- `{ isActive: 1 }`
- `{ location: "2dsphere" }`

---

## 4.3 `movies_halls`

Purpose:

- Store screening room definitions and seat layout metadata

Recommended document:

```json
{
  "_id": "hall_abcd_mall_imax_01",
  "cinemaId": "cinema_abcd_mall",
  "code": "IMAX-01",
  "name": "Hall A - IMAX",
  "hallType": "IMAX",
  "capacity": 240,
  "seatLayout": {
    "rows": 8,
    "cols": 12,
    "aisleAfter": [6],
    "screenLabel": "Screen"
  },
  "seatMap": [
    { "seatCode": "A1", "row": "A", "number": 1, "seatType": "regular", "isActive": true },
    { "seatCode": "C4", "row": "C", "number": 4, "seatType": "vip", "isActive": true },
    { "seatCode": "H7", "row": "H", "number": 7, "seatType": "couple", "pairCode": "H8", "isActive": true }
  ],
  "seatTypePricing": {
    "regular": 0,
    "vip": 30000,
    "couple": 50000
  },
  "maintenanceStatus": "ready",
  "isActive": true,
  "createdAt": "2026-04-01T08:00:00.000Z",
  "updatedAt": "2026-04-05T09:30:00.000Z"
}
```

Notes:

- `seatMap` can stay embedded because a hall layout is bounded and relatively small
- If halls become very large or frequently edited, split seat definitions into a separate collection later

Suggested indexes:

- `{ cinemaId: 1, code: 1 }` unique
- `{ cinemaId: 1, hallType: 1 }`
- `{ isActive: 1 }`

---

## 4.4 `movies_showtimes`

Purpose:

- Store each bookable showtime
- Central operational collection for the whole module

Recommended document:

```json
{
  "_id": "showtime_cosmic_odyssey_abcdmall_20260410_1930",
  "movieId": "movie_cosmic_odyssey",
  "movieSnapshot": {
    "title": "Cosmic Odyssey",
    "posterUrl": "https://...",
    "durationMinutes": 148,
    "ageRating": "T13"
  },
  "cinemaId": "cinema_abcd_mall",
  "cinemaSnapshot": {
    "name": "ABCD Cinema - ABCD Mall",
    "address": "Level 5, ABCD Mall, 123 Le Van Viet Street, Ho Chi Minh City"
  },
  "hallId": "hall_abcd_mall_imax_01",
  "hallSnapshot": {
    "name": "Hall A - IMAX",
    "hallType": "IMAX"
  },
  "startsAt": "2026-04-10T19:30:00.000Z",
  "endsAt": "2026-04-10T21:58:00.000Z",
  "bookingDate": "2026-04-10",
  "language": "sub",
  "pricing": {
    "basePrice": 140000,
    "seatTypeOverrides": {
      "regular": 140000,
      "vip": 170000,
      "couple": 190000
    },
    "pricingBand": "peak"
  },
  "seatInventory": {
    "totalSeats": 240,
    "availableSeats": 9,
    "soldSeats": 231,
    "blockedSeats": 0
  },
  "bookingStatus": "open",
  "isPublished": true,
  "createdAt": "2026-04-01T08:00:00.000Z",
  "updatedAt": "2026-04-10T12:15:00.000Z",
  "createdBy": "movie.ops",
  "updatedBy": "movie.ops"
}
```

Notes:

- Keep `movieSnapshot`, `cinemaSnapshot`, `hallSnapshot` for read performance and history safety
- `bookingStatus` enum:
  - `draft`
  - `open`
  - `closed`
  - `cancelled`
  - `completed`
- `pricingBand` enum:
  - `normal`
  - `peak`
  - `off_peak`

Suggested indexes:

- `{ movieId: 1, startsAt: 1 }`
- `{ cinemaId: 1, startsAt: 1 }`
- `{ hallId: 1, startsAt: 1 }`
- `{ bookingDate: 1, movieId: 1 }`
- `{ bookingStatus: 1, isPublished: 1, startsAt: 1 }`
- unique composite for schedule conflict prevention:
  - `{ hallId: 1, startsAt: 1 }`

---

## 4.5 `movies_bookings`

Purpose:

- Store the guest booking order
- This is the most important business collection

Recommended document:

```json
{
  "_id": "booking_20260410_000123",
  "bookingCode": "ABCD-20260410-000123",
  "showtimeId": "showtime_cosmic_odyssey_abcdmall_20260410_1930",
  "movieId": "movie_cosmic_odyssey",
  "cinemaId": "cinema_abcd_mall",
  "hallId": "hall_abcd_mall_imax_01",
  "guest": {
    "email": "linh.nguyen@gmail.com",
    "fullName": "Nguyen Thi Linh",
    "phone": "0901234567",
    "birthday": "2001-04-10"
  },
  "seats": [
    { "seatCode": "D5", "seatType": "vip", "price": 170000 },
    { "seatCode": "D6", "seatType": "vip", "price": 170000 }
  ],
  "combos": [
    {
      "comboId": "combo-gold",
      "name": "Combo Gold",
      "quantity": 1,
      "unitPrice": 85000,
      "lineTotal": 85000
    }
  ],
  "pricing": {
    "subtotal": 340000,
    "serviceFee": 16000,
    "comboSubtotal": 85000,
    "discount": 60000,
    "total": 381000,
    "currency": "VND"
  },
  "promotion": {
    "promotionId": "promo_momo30",
    "code": "MOMO30",
    "title": "Pay with MoMo - Get 30% Off",
    "discount": 60000,
    "status": "applied"
  },
  "payment": {
    "paymentId": "payment_20260410_8891",
    "status": "pending"
  },
  "ticket": {
    "ticketCode": "TCK-20260410-123",
    "pdfUrl": null,
    "emailStatus": "queued"
  },
  "status": "pending_payment",
  "seatLockExpiresAt": "2026-04-10T19:12:00.000Z",
  "cancelReason": null,
  "refundedAt": null,
  "notes": [],
  "createdAt": "2026-04-10T19:02:00.000Z",
  "updatedAt": "2026-04-10T19:05:00.000Z",
  "source": {
    "channel": "web",
    "device": "desktop",
    "ip": "127.0.0.1"
  }
}
```

Recommended booking status enum:

- `draft`
- `pending_payment`
- `paid`
- `ticket_issued`
- `cancelled`
- `expired`
- `refund_pending`
- `refunded`
- `failed`

Suggested indexes:

- `{ bookingCode: 1 }` unique
- `{ "guest.email": 1, createdAt: -1 }`
- `{ showtimeId: 1, status: 1 }`
- `{ "payment.paymentId": 1 }`
- `{ status: 1, createdAt: -1 }`
- `{ seatLockExpiresAt: 1 }`

---

## 4.6 `movies_seat_locks`

Purpose:

- Temporarily lock seats before payment is completed
- Avoid race conditions in guest booking

Recommended document:

```json
{
  "_id": "lock_showtime_cosmic_odyssey_abcdmall_20260410_1930_D5_D6",
  "showtimeId": "showtime_cosmic_odyssey_abcdmall_20260410_1930",
  "bookingId": "booking_20260410_000123",
  "seatCodes": ["D5", "D6"],
  "guestEmail": "linh.nguyen@gmail.com",
  "status": "locked",
  "expiresAt": "2026-04-10T19:12:00.000Z",
  "createdAt": "2026-04-10T19:02:00.000Z"
}
```

Notes:

- This collection should use TTL index on `expiresAt`
- Do not rely only on in-memory seat locks

Suggested indexes:

- `{ showtimeId: 1, seatCodes: 1 }`
- `{ bookingId: 1 }`
- TTL index on `{ expiresAt: 1 }`

---

## 4.7 `movies_payments`

Purpose:

- Track payment attempts and callback results independently from bookings

Recommended document:

```json
{
  "_id": "payment_20260410_8891",
  "bookingId": "booking_20260410_000123",
  "bookingCode": "ABCD-20260410-000123",
  "provider": "momo",
  "providerTransactionId": "MOMO-998877",
  "amount": 381000,
  "currency": "VND",
  "status": "success",
  "paymentUrl": "https://...",
  "callback": {
    "received": true,
    "receivedAt": "2026-04-10T19:07:00.000Z",
    "httpStatus": 200,
    "rawPayload": {
      "resultCode": 0,
      "message": "Success"
    }
  },
  "requestPayload": {
    "orderId": "ABCD-20260410-000123"
  },
  "error": null,
  "createdAt": "2026-04-10T19:05:00.000Z",
  "updatedAt": "2026-04-10T19:07:00.000Z"
}
```

Recommended payment status enum:

- `pending`
- `success`
- `failed`
- `cancelled`
- `expired`
- `refunded`

Suggested indexes:

- `{ bookingId: 1 }`
- `{ bookingCode: 1 }`
- `{ provider: 1, providerTransactionId: 1 }` unique sparse
- `{ status: 1, createdAt: -1 }`
- `{ "callback.received": 1, status: 1 }`

---

## 4.8 `movies_ticket_deliveries`

Purpose:

- Log ticket email sending attempts
- Support resend and delivery debugging

Recommended document:

```json
{
  "_id": "delivery_20260410_1001",
  "bookingId": "booking_20260410_000123",
  "bookingCode": "ABCD-20260410-000123",
  "guestEmail": "linh.nguyen@gmail.com",
  "ticketCode": "TCK-20260410-123",
  "channel": "email",
  "status": "sent",
  "attempt": 1,
  "templateCode": "ticket-v3",
  "subject": "Your ABCD Cinema ticket",
  "pdfUrl": "https://...",
  "provider": {
    "name": "smtp",
    "messageId": "<abc-123@abcd.vn>"
  },
  "error": null,
  "sentAt": "2026-04-10T19:08:00.000Z",
  "openedAt": null,
  "createdAt": "2026-04-10T19:08:00.000Z"
}
```

Recommended delivery status enum:

- `queued`
- `sending`
- `sent`
- `failed`
- `bounced`
- `resent`

Suggested indexes:

- `{ bookingId: 1, createdAt: -1 }`
- `{ guestEmail: 1, createdAt: -1 }`
- `{ status: 1, createdAt: -1 }`

---

## 4.9 `movies_guests`

Purpose:

- Lightweight customer support view for guest buyers
- Not an account collection

Recommended document:

```json
{
  "_id": "guest_linh_nguyen_gmail_com",
  "email": "linh.nguyen@gmail.com",
  "fullName": "Nguyen Thi Linh",
  "phone": "0901234567",
  "stats": {
    "bookingCount": 12,
    "paidBookingCount": 11,
    "refundCount": 1,
    "totalSpent": 3245000,
    "lastBookingAt": "2026-04-10T19:02:00.000Z"
  },
  "riskFlags": ["none"],
  "notes": [
    {
      "type": "support",
      "message": "Requested resend due to mail delay",
      "createdBy": "support.cs",
      "createdAt": "2026-04-10T19:20:00.000Z"
    }
  ],
  "createdAt": "2026-03-20T10:00:00.000Z",
  "updatedAt": "2026-04-10T19:20:00.000Z"
}
```

Notes:

- This is a support projection, not a source of truth
- Can be updated asynchronously from bookings

Suggested indexes:

- `{ email: 1 }` unique
- `{ "stats.lastBookingAt": -1 }`
- `{ riskFlags: 1 }`

---

## 4.10 `movies_promotions`

Purpose:

- Store promotions used by guest checkout and admin portal

Recommended document:

```json
{
  "_id": "promo_momo30",
  "code": "MOMO30",
  "title": "Pay with MoMo - Get 30% Off",
  "description": "Save 30% on ticket value, up to 60,000 VND",
  "type": "percentage",
  "value": 30,
  "maxDiscount": 60000,
  "status": "active",
  "conditions": {
    "paymentMethods": ["momo"],
    "minTickets": 1,
    "validDays": ["mon", "tue", "wed", "thu", "fri", "sat", "sun"],
    "validShowtimeRange": null,
    "movieIds": [],
    "hallTypes": [],
    "birthdayMonthOnly": false
  },
  "usageLimit": {
    "total": 5000,
    "perGuest": 1
  },
  "usageStats": {
    "usedCount": 1344
  },
  "startsAt": "2026-04-01T00:00:00.000Z",
  "endsAt": "2026-05-15T23:59:59.999Z",
  "isStackable": false,
  "createdAt": "2026-03-28T08:00:00.000Z",
  "updatedAt": "2026-04-05T09:30:00.000Z"
}
```

Recommended promotion status enum:

- `draft`
- `scheduled`
- `active`
- `inactive`
- `expired`
- `archived`

Suggested indexes:

- `{ code: 1 }` unique
- `{ status: 1, startsAt: 1, endsAt: 1 }`

---

## 4.11 `movies_refunds`

Purpose:

- Track refund requests and completed refund operations

Recommended document:

```json
{
  "_id": "refund_20260410_001",
  "bookingId": "booking_20260410_000123",
  "paymentId": "payment_20260410_8891",
  "bookingCode": "ABCD-20260410-000123",
  "amount": 381000,
  "reason": "Customer selected wrong showtime",
  "status": "approved",
  "providerRefundId": "MOMO-RFD-001",
  "requestedBy": "support.cs",
  "requestedAt": "2026-04-10T19:30:00.000Z",
  "processedAt": "2026-04-10T19:45:00.000Z",
  "notes": []
}
```

Suggested indexes:

- `{ bookingId: 1 }`
- `{ paymentId: 1 }`
- `{ status: 1, requestedAt: -1 }`

---

## 4.12 `movies_admin_users`

Purpose:

- Keep movie-module-specific admin access and role mapping

Recommended document:

```json
{
  "_id": "admin_admin.mall",
  "username": "admin.mall",
  "email": "admin.mall@abcd.vn",
  "displayName": "Mall Administrator",
  "status": "active",
  "roles": ["super_admin"],
  "permissions": [
    "movies.manage",
    "showtimes.manage",
    "bookings.manage",
    "payments.read",
    "logs.read"
  ],
  "lastLoginAt": "2026-04-10T08:00:00.000Z",
  "createdAt": "2026-03-20T10:00:00.000Z",
  "updatedAt": "2026-04-05T09:30:00.000Z"
}
```

Suggested indexes:

- `{ username: 1 }` unique
- `{ email: 1 }` unique
- `{ status: 1 }`

---

## 4.13 `movies_activity_logs`

Purpose:

- Store operational logs and audit logs for the module

Recommended document:

```json
{
  "_id": "log_20260410_10001",
  "module": "movies",
  "category": "payment_callback",
  "level": "warning",
  "reference": {
    "bookingId": "booking_20260410_000123",
    "paymentId": "payment_20260410_8891",
    "showtimeId": "showtime_cosmic_odyssey_abcdmall_20260410_1930"
  },
  "message": "VNPay callback delayed more than 30 seconds",
  "payload": {
    "provider": "vnpay",
    "delayMs": 32000
  },
  "createdAt": "2026-04-10T19:07:32.000Z"
}
```

Recommended categories:

- `booking`
- `payment`
- `payment_callback`
- `ticket_delivery`
- `admin_action`
- `system`

Suggested indexes:

- `{ category: 1, createdAt: -1 }`
- `{ level: 1, createdAt: -1 }`
- `{ "reference.bookingId": 1 }`
- `{ "reference.paymentId": 1 }`

---

## 5. Optional Supporting Collections

You may add these later if the module grows:

- `movies_genres`
- `movies_snack_combos`
- `movies_email_templates`
- `movies_payment_callbacks_raw`
- `movies_reports_daily`

For current scope, they are optional.

---

## 6. Collection Relationship Summary

Main relationship map:

```text
movies_movies
    -> movies_showtimes

movies_cinemas
    -> movies_halls
    -> movies_showtimes

movies_halls
    -> movies_showtimes

movies_showtimes
    -> movies_bookings
    -> movies_seat_locks

movies_bookings
    -> movies_payments
    -> movies_ticket_deliveries
    -> movies_refunds

movies_promotions
    -> movies_bookings

movies_guests
    <- projected from movies_bookings

movies_activity_logs
    <- log events from all of the above
```

---

## 7. Recommended MongoDB Modeling Rules

For this Movies module, use these rules:

1. Keep master data normalized
   - movies, cinemas, halls, promotions, admin users

2. Keep transactional data partially denormalized
   - showtimes, bookings, payments, deliveries

3. Always snapshot critical display fields into booking and payment documents
   - movie title
   - cinema name
   - hall name
   - seat prices
   - promotion title

4. Never calculate booking totals from current movie data after payment
   - the booking document must store final pricing permanently

5. Use TTL for temporary seat locks only

6. Use event logs for debugging instead of overwriting history

---

## 8. Booking Flow Mapping to Collections

### Step 1. User opens showtimes

Read from:

- `movies_movies`
- `movies_showtimes`
- `movies_cinemas`
- `movies_halls`

### Step 2. User chooses seats

Read from:

- `movies_showtimes`
- `movies_halls`

Write to:

- `movies_seat_locks`

### Step 3. User enters guest info and proceeds to payment

Write to:

- `movies_bookings`

### Step 4. Payment is created

Write to:

- `movies_payments`

Update:

- `movies_bookings`

### Step 5. Payment callback returns

Update:

- `movies_payments`
- `movies_bookings`
- `movies_showtimes.seatInventory`

Delete or expire:

- `movies_seat_locks`

Write logs:

- `movies_activity_logs`

### Step 6. Ticket email is sent

Write to:

- `movies_ticket_deliveries`

Update:

- `movies_bookings.ticket`
- `movies_guests`

---

## 9. Recommended Status Enums

### Movie status

- `draft`
- `coming_soon`
- `now_showing`
- `archived`

### Showtime status

- `draft`
- `open`
- `closed`
- `cancelled`
- `completed`

### Booking status

- `draft`
- `pending_payment`
- `paid`
- `ticket_issued`
- `cancelled`
- `expired`
- `refund_pending`
- `refunded`
- `failed`

### Payment status

- `pending`
- `success`
- `failed`
- `cancelled`
- `expired`
- `refunded`

### Ticket delivery status

- `queued`
- `sending`
- `sent`
- `failed`
- `bounced`
- `resent`

### Refund status

- `requested`
- `approved`
- `rejected`
- `processing`
- `completed`
- `failed`

---

## 10. Suggested API Ownership by Collection

### Catalog APIs

- `movies_movies`
- `movies_cinemas`
- `movies_halls`

### Showtime APIs

- `movies_showtimes`

### Booking APIs

- `movies_bookings`
- `movies_seat_locks`

### Payment APIs

- `movies_payments`
- `movies_refunds`

### Delivery APIs

- `movies_ticket_deliveries`

### Admin APIs

- `movies_admin_users`
- `movies_activity_logs`
- projections from all collections above

---

## 11. Recommended Initial Collection Set

If the backend team wants a practical v1 only, start with these 10 collections:

1. `movies_movies`
2. `movies_cinemas`
3. `movies_halls`
4. `movies_showtimes`
5. `movies_bookings`
6. `movies_seat_locks`
7. `movies_payments`
8. `movies_ticket_deliveries`
9. `movies_promotions`
10. `movies_activity_logs`

Then add later:

- `movies_guests`
- `movies_refunds`
- `movies_admin_users`

---

## 12. Final Recommendation

For the `Movies` module inside `ABCDMall`, the best MongoDB approach is:

- One shared database: `ABCDMall`
- Separate collections with `movies_` prefix
- Normalize master data
- Denormalize transaction snapshots
- Treat `bookings`, `showtimes`, and `payments` as the core collections
- Use `seat_locks` and `ticket_deliveries` as operational support collections
- Use `activity_logs` for support and debugging

If the backend grows, this structure can still evolve toward:

- event-driven processing
- read-model projections
- collection sharding by date or cinema

without breaking the module boundary.
