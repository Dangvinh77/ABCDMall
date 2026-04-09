# Movies Module Collection Relationship

## 1. Purpose

This file rewrites the MongoDB collection design of the `Movies` module into a format that is easier to read like a SQL ERD.

Important note:

- The backend still uses MongoDB
- These are **collections**, not real SQL tables
- This document presents them in a `table-like` structure so developers can visualize:
  - fields
  - pseudo primary keys
  - pseudo foreign keys
  - enum values
  - one-to-many relationships

Database:

- `ABCDMall`

Module prefix:

- `movies_`

---

## 2. Relationship Overview

```text
movies_movies
    1 --- n movies_showtimes

movies_cinemas
    1 --- n movies_halls
    1 --- n movies_showtimes

movies_halls
    1 --- n movies_showtimes

movies_showtimes
    1 --- n movies_bookings
    1 --- n movies_seat_locks

movies_bookings
    1 --- n movies_payments
    1 --- n movies_ticket_deliveries
    1 --- n movies_refunds

movies_promotions
    1 --- n movies_bookings

movies_bookings
    n --- 1 movies_guests (by guest email projection)

movies_admin_users
    1 --- n movies_activity_logs
```

---

## 3. ERD Style Collection Definitions

## 3.1 `movies_movies`

Purpose:

- Master movie catalog
- Used by customer FE and admin FE

| Field | Type | Required | Key | Description |
|---|---|---:|---|---|
| `_id` | string / ObjectId | yes | PK | Internal movie id |
| `slug` | string | yes | UQ | Public route slug |
| `title.default` | string | yes |  | Main title |
| `title.vi` | string | no |  | Vietnamese title |
| `description.default` | string | yes |  | Main description |
| `description.vi` | string | no |  | Vietnamese description |
| `genreIds` | array<string> | no |  | Genre references |
| `genresText` | array<string> | yes |  | Denormalized display genres |
| `durationMinutes` | int | yes |  | Runtime in minutes |
| `director` | string | yes |  | Director name |
| `cast` | array<string> | no |  | Cast names |
| `language.audio` | string | yes |  | Audio language |
| `language.subtitle` | string | no |  | Subtitle language |
| `language.display` | string | yes |  | FE display text |
| `ageRating` | string | yes |  | `P`, `T13`, `T16`, `T18` |
| `status` | string | yes | IDX | `draft`, `coming_soon`, `now_showing`, `archived` |
| `releaseDate` | datetime | no | IDX | Release date |
| `posterUrl` | string | yes |  | Poster image |
| `backdropUrl` | string | no |  | Backdrop image |
| `trailerUrl` | string | no |  | Trailer URL |
| `rating.score` | decimal | no |  | Public score |
| `rating.votes` | int | no |  | Vote count |
| `isActive` | bool | yes | IDX | Soft active flag |
| `tags` | array<string> | no |  | Search/filter tags |
| `seo.metaTitle` | string | no |  | SEO title |
| `seo.metaDescription` | string | no |  | SEO description |
| `createdAt` | datetime | yes |  | Audit |
| `updatedAt` | datetime | yes |  | Audit |
| `createdBy` | string | no | FK -> movies_admin_users.username | Creator |
| `updatedBy` | string | no | FK -> movies_admin_users.username | Last updater |

Indexes:

- unique: `slug`
- index: `status, isActive`
- index: `releaseDate`
- text index: `title.default`, `title.vi`

---

## 3.2 `movies_cinemas`

Purpose:

- Cinema branches for the Movies module

| Field | Type | Required | Key | Description |
|---|---|---:|---|---|
| `_id` | string / ObjectId | yes | PK | Internal cinema id |
| `code` | string | yes | UQ | Public/short branch code |
| `name` | string | yes |  | Full cinema name |
| `mallBranch` | string | yes |  | Mall branch name |
| `address.line1` | string | yes |  | Address line |
| `address.street` | string | no |  | Street |
| `address.district` | string | no |  | District |
| `address.city` | string | yes |  | City |
| `address.country` | string | yes |  | Country |
| `contact.phone` | string | no |  | Support phone |
| `contact.email` | string | no |  | Branch email |
| `location.type` | string | no |  | Usually `Point` |
| `location.coordinates` | array<number> | no | GEO | `[lng, lat]` |
| `openingHours.open` | string | no |  | `HH:mm` |
| `openingHours.close` | string | no |  | `HH:mm` |
| `isActive` | bool | yes | IDX | Soft active flag |
| `createdAt` | datetime | yes |  | Audit |
| `updatedAt` | datetime | yes |  | Audit |

Indexes:

- unique: `code`
- index: `isActive`
- geospatial: `location`

---

## 3.3 `movies_halls`

Purpose:

- Screening rooms and seat layout definitions

| Field | Type | Required | Key | Description |
|---|---|---:|---|---|
| `_id` | string / ObjectId | yes | PK | Hall id |
| `cinemaId` | string / ObjectId | yes | FK -> movies_cinemas._id | Cinema owner |
| `code` | string | yes | UQ per cinema | Hall code |
| `name` | string | yes |  | Display hall name |
| `hallType` | string | yes | IDX | `2D`, `3D`, `IMAX`, `4DX` |
| `capacity` | int | yes |  | Total seats |
| `seatLayout.rows` | int | yes |  | Number of rows |
| `seatLayout.cols` | int | yes |  | Number of columns |
| `seatLayout.aisleAfter` | array<int> | no |  | Aisle positions |
| `seatLayout.screenLabel` | string | no |  | FE label |
| `seatMap` | array<object> | yes |  | Embedded seat definitions |
| `seatMap[].seatCode` | string | yes |  | `A1`, `D5` |
| `seatMap[].row` | string | yes |  | Row code |
| `seatMap[].number` | int | yes |  | Seat number |
| `seatMap[].seatType` | string | yes |  | `regular`, `vip`, `couple` |
| `seatMap[].pairCode` | string | no |  | For couple seats |
| `seatMap[].isActive` | bool | yes |  | Seat usable flag |
| `seatTypePricing.regular` | decimal | yes |  | Extra charge or 0 |
| `seatTypePricing.vip` | decimal | yes |  | Extra charge |
| `seatTypePricing.couple` | decimal | yes |  | Extra charge |
| `maintenanceStatus` | string | yes | IDX | `ready`, `maintenance`, `blocked` |
| `isActive` | bool | yes | IDX | Hall active flag |
| `createdAt` | datetime | yes |  | Audit |
| `updatedAt` | datetime | yes |  | Audit |

Indexes:

- unique compound: `cinemaId + code`
- index: `cinemaId + hallType`
- index: `isActive`

---

## 3.4 `movies_showtimes`

Purpose:

- Every bookable movie session
- Main operational collection

| Field | Type | Required | Key | Description |
|---|---|---:|---|---|
| `_id` | string / ObjectId | yes | PK | Showtime id |
| `movieId` | string / ObjectId | yes | FK -> movies_movies._id | Movie reference |
| `movieSnapshot.title` | string | yes |  | Snapshot at creation time |
| `movieSnapshot.posterUrl` | string | no |  | Snapshot |
| `movieSnapshot.durationMinutes` | int | yes |  | Snapshot |
| `movieSnapshot.ageRating` | string | yes |  | Snapshot |
| `cinemaId` | string / ObjectId | yes | FK -> movies_cinemas._id | Cinema reference |
| `cinemaSnapshot.name` | string | yes |  | Snapshot |
| `cinemaSnapshot.address` | string | yes |  | Snapshot |
| `hallId` | string / ObjectId | yes | FK -> movies_halls._id | Hall reference |
| `hallSnapshot.name` | string | yes |  | Snapshot |
| `hallSnapshot.hallType` | string | yes |  | Snapshot |
| `startsAt` | datetime | yes | IDX | Session start |
| `endsAt` | datetime | yes |  | Session end |
| `bookingDate` | string | yes | IDX | `YYYY-MM-DD` for query speed |
| `language` | string | yes |  | `sub`, `dub` |
| `pricing.basePrice` | decimal | yes |  | Base ticket price |
| `pricing.seatTypeOverrides.regular` | decimal | yes |  | Final regular seat price |
| `pricing.seatTypeOverrides.vip` | decimal | yes |  | Final VIP seat price |
| `pricing.seatTypeOverrides.couple` | decimal | yes |  | Final couple seat price |
| `pricing.pricingBand` | string | yes |  | `normal`, `peak`, `off_peak` |
| `seatInventory.totalSeats` | int | yes |  | Total capacity |
| `seatInventory.availableSeats` | int | yes |  | Remaining seats |
| `seatInventory.soldSeats` | int | yes |  | Paid/booked seats |
| `seatInventory.blockedSeats` | int | yes |  | Blocked seats |
| `bookingStatus` | string | yes | IDX | `draft`, `open`, `closed`, `cancelled`, `completed` |
| `isPublished` | bool | yes | IDX | Customer-facing visibility |
| `createdAt` | datetime | yes |  | Audit |
| `updatedAt` | datetime | yes |  | Audit |
| `createdBy` | string | no | FK -> movies_admin_users.username | Creator |
| `updatedBy` | string | no | FK -> movies_admin_users.username | Last updater |

Indexes:

- index: `movieId + startsAt`
- index: `cinemaId + startsAt`
- index: `hallId + startsAt`
- index: `bookingDate + movieId`
- index: `bookingStatus + isPublished + startsAt`
- unique compound: `hallId + startsAt`

---

## 3.5 `movies_bookings`

Purpose:

- Guest booking orders
- Core transaction collection

| Field | Type | Required | Key | Description |
|---|---|---:|---|---|
| `_id` | string / ObjectId | yes | PK | Booking id |
| `bookingCode` | string | yes | UQ | Human-readable order code |
| `showtimeId` | string / ObjectId | yes | FK -> movies_showtimes._id | Showtime reference |
| `movieId` | string / ObjectId | yes | FK -> movies_movies._id | Movie reference |
| `cinemaId` | string / ObjectId | yes | FK -> movies_cinemas._id | Cinema reference |
| `hallId` | string / ObjectId | yes | FK -> movies_halls._id | Hall reference |
| `guest.email` | string | yes | IDX | Guest email |
| `guest.fullName` | string | yes |  | Guest name |
| `guest.phone` | string | yes |  | Guest phone |
| `guest.birthday` | date | no |  | Promotion support |
| `seats` | array<object> | yes |  | Selected seats |
| `seats[].seatCode` | string | yes |  | Seat code |
| `seats[].seatType` | string | yes |  | `regular`, `vip`, `couple` |
| `seats[].price` | decimal | yes |  | Locked seat price |
| `combos` | array<object> | no |  | Snack combos |
| `combos[].comboId` | string | yes |  | Combo code |
| `combos[].name` | string | yes |  | Snapshot |
| `combos[].quantity` | int | yes |  | Quantity |
| `combos[].unitPrice` | decimal | yes |  | Unit price |
| `combos[].lineTotal` | decimal | yes |  | Total line |
| `pricing.subtotal` | decimal | yes |  | Seat subtotal |
| `pricing.serviceFee` | decimal | yes |  | Service fee |
| `pricing.comboSubtotal` | decimal | yes |  | Combo subtotal |
| `pricing.discount` | decimal | yes |  | Applied discount |
| `pricing.total` | decimal | yes |  | Final total |
| `pricing.currency` | string | yes |  | Usually `VND` |
| `promotion.promotionId` | string / ObjectId | no | FK -> movies_promotions._id | Applied promo |
| `promotion.code` | string | no |  | Promo code |
| `promotion.title` | string | no |  | Promo title snapshot |
| `promotion.discount` | decimal | no |  | Discount value |
| `promotion.status` | string | no |  | Promo resolution status |
| `payment.paymentId` | string / ObjectId | no | FK -> movies_payments._id | Current payment link |
| `payment.status` | string | no |  | Payment summary state |
| `ticket.ticketCode` | string | no |  | Issued ticket code |
| `ticket.pdfUrl` | string | no |  | Generated ticket PDF |
| `ticket.emailStatus` | string | no |  | Delivery state |
| `status` | string | yes | IDX | `draft`, `pending_payment`, `paid`, `ticket_issued`, `cancelled`, `expired`, `refund_pending`, `refunded`, `failed` |
| `seatLockExpiresAt` | datetime | no | IDX | Reservation expiration |
| `cancelReason` | string | no |  | Why cancelled |
| `refundedAt` | datetime | no |  | Refund completion time |
| `notes` | array<object> | no |  | Support notes |
| `createdAt` | datetime | yes |  | Audit |
| `updatedAt` | datetime | yes |  | Audit |
| `source.channel` | string | no |  | `web`, `mobile`, etc. |
| `source.device` | string | no |  | Device type |
| `source.ip` | string | no |  | Source ip |

Indexes:

- unique: `bookingCode`
- index: `guest.email + createdAt desc`
- index: `showtimeId + status`
- index: `payment.paymentId`
- index: `status + createdAt desc`
- index: `seatLockExpiresAt`

---

## 3.6 `movies_seat_locks`

Purpose:

- Temporary seat reservation before payment success

| Field | Type | Required | Key | Description |
|---|---|---:|---|---|
| `_id` | string / ObjectId | yes | PK | Lock id |
| `showtimeId` | string / ObjectId | yes | FK -> movies_showtimes._id | Showtime reference |
| `bookingId` | string / ObjectId | yes | FK -> movies_bookings._id | Booking reference |
| `seatCodes` | array<string> | yes |  | Locked seats |
| `guestEmail` | string | yes | IDX | For support/debug |
| `status` | string | yes | IDX | `locked`, `released`, `expired`, `converted` |
| `expiresAt` | datetime | yes | TTL | Auto-expire lock |
| `createdAt` | datetime | yes |  | Creation time |

Indexes:

- index: `showtimeId + seatCodes`
- index: `bookingId`
- TTL: `expiresAt`

---

## 3.7 `movies_payments`

Purpose:

- Store payment attempts and callback data

| Field | Type | Required | Key | Description |
|---|---|---:|---|---|
| `_id` | string / ObjectId | yes | PK | Payment id |
| `bookingId` | string / ObjectId | yes | FK -> movies_bookings._id | Booking reference |
| `bookingCode` | string | yes | IDX | Booking code snapshot |
| `provider` | string | yes | IDX | `momo`, `vnpay`, `stripe`, `visa`, etc. |
| `providerTransactionId` | string | no | UQ sparse | Gateway transaction id |
| `amount` | decimal | yes |  | Amount paid |
| `currency` | string | yes |  | Currency |
| `status` | string | yes | IDX | `pending`, `success`, `failed`, `cancelled`, `expired`, `refunded` |
| `paymentUrl` | string | no |  | Redirect URL |
| `callback.received` | bool | yes | IDX | Callback status |
| `callback.receivedAt` | datetime | no |  | Callback time |
| `callback.httpStatus` | int | no |  | Callback response status |
| `callback.rawPayload` | object | no |  | Provider payload |
| `requestPayload` | object | no |  | Provider request payload |
| `error` | object | no |  | Error info |
| `createdAt` | datetime | yes |  | Audit |
| `updatedAt` | datetime | yes |  | Audit |

Indexes:

- index: `bookingId`
- index: `bookingCode`
- unique sparse: `provider + providerTransactionId`
- index: `status + createdAt desc`
- index: `callback.received + status`

---

## 3.8 `movies_ticket_deliveries`

Purpose:

- Ticket email sending history

| Field | Type | Required | Key | Description |
|---|---|---:|---|---|
| `_id` | string / ObjectId | yes | PK | Delivery id |
| `bookingId` | string / ObjectId | yes | FK -> movies_bookings._id | Booking reference |
| `bookingCode` | string | yes | IDX | Booking code |
| `guestEmail` | string | yes | IDX | Recipient |
| `ticketCode` | string | no |  | Ticket code |
| `channel` | string | yes |  | Usually `email` |
| `status` | string | yes | IDX | `queued`, `sending`, `sent`, `failed`, `bounced`, `resent` |
| `attempt` | int | yes |  | Send attempt number |
| `templateCode` | string | no |  | Email template version |
| `subject` | string | no |  | Email subject |
| `pdfUrl` | string | no |  | Ticket PDF |
| `provider.name` | string | no |  | SMTP provider |
| `provider.messageId` | string | no |  | Mail provider message id |
| `error` | object | no |  | Failure details |
| `sentAt` | datetime | no |  | Sent time |
| `openedAt` | datetime | no |  | Optional tracking |
| `createdAt` | datetime | yes |  | Audit |

Indexes:

- index: `bookingId + createdAt desc`
- index: `guestEmail + createdAt desc`
- index: `status + createdAt desc`

---

## 3.9 `movies_guests`

Purpose:

- Guest customer support projection
- Not an authentication table

| Field | Type | Required | Key | Description |
|---|---|---:|---|---|
| `_id` | string / ObjectId | yes | PK | Guest profile id |
| `email` | string | yes | UQ | Guest unique key |
| `fullName` | string | no |  | Latest known name |
| `phone` | string | no |  | Latest known phone |
| `stats.bookingCount` | int | yes |  | Total bookings |
| `stats.paidBookingCount` | int | yes |  | Paid bookings |
| `stats.refundCount` | int | yes |  | Refund count |
| `stats.totalSpent` | decimal | yes |  | Lifetime spending |
| `stats.lastBookingAt` | datetime | no | IDX | Last order time |
| `riskFlags` | array<string> | no | IDX | Fraud/support markers |
| `notes` | array<object> | no |  | Support notes |
| `notes[].type` | string | yes |  | `support`, `risk`, `refund` |
| `notes[].message` | string | yes |  | Note message |
| `notes[].createdBy` | string | no | FK -> movies_admin_users.username | Author |
| `notes[].createdAt` | datetime | yes |  | Timestamp |
| `createdAt` | datetime | yes |  | Audit |
| `updatedAt` | datetime | yes |  | Audit |

Indexes:

- unique: `email`
- index: `stats.lastBookingAt desc`
- index: `riskFlags`

---

## 3.10 `movies_promotions`

Purpose:

- Promotions and discount logic definitions

| Field | Type | Required | Key | Description |
|---|---|---:|---|---|
| `_id` | string / ObjectId | yes | PK | Promotion id |
| `code` | string | yes | UQ | Promo code |
| `title` | string | yes |  | Promotion title |
| `description` | string | yes |  | Promotion description |
| `type` | string | yes |  | `percentage`, `amount`, `bundle`, `gift` |
| `value` | decimal | yes |  | Discount value |
| `maxDiscount` | decimal | no |  | Optional cap |
| `status` | string | yes | IDX | `draft`, `scheduled`, `active`, `inactive`, `expired`, `archived` |
| `conditions.paymentMethods` | array<string> | no |  | Allowed payment methods |
| `conditions.minTickets` | int | no |  | Minimum seat count |
| `conditions.validDays` | array<string> | no |  | `mon`..`sun` |
| `conditions.validShowtimeRange` | object | no |  | Time window |
| `conditions.movieIds` | array<string> | no | FK -> movies_movies._id | Eligible movies |
| `conditions.hallTypes` | array<string> | no |  | Hall restrictions |
| `conditions.birthdayMonthOnly` | bool | no |  | Birthday promo flag |
| `usageLimit.total` | int | no |  | Total uses allowed |
| `usageLimit.perGuest` | int | no |  | Uses per guest |
| `usageStats.usedCount` | int | yes |  | Current use count |
| `startsAt` | datetime | yes | IDX | Start time |
| `endsAt` | datetime | no | IDX | End time |
| `isStackable` | bool | yes |  | Can combine with others or not |
| `createdAt` | datetime | yes |  | Audit |
| `updatedAt` | datetime | yes |  | Audit |

Indexes:

- unique: `code`
- index: `status + startsAt + endsAt`

---

## 3.11 `movies_refunds`

Purpose:

- Refund management and audit

| Field | Type | Required | Key | Description |
|---|---|---:|---|---|
| `_id` | string / ObjectId | yes | PK | Refund id |
| `bookingId` | string / ObjectId | yes | FK -> movies_bookings._id | Booking reference |
| `paymentId` | string / ObjectId | yes | FK -> movies_payments._id | Payment reference |
| `bookingCode` | string | yes | IDX | Booking code |
| `amount` | decimal | yes |  | Refund amount |
| `reason` | string | yes |  | Refund reason |
| `status` | string | yes | IDX | `requested`, `approved`, `rejected`, `processing`, `completed`, `failed` |
| `providerRefundId` | string | no |  | Gateway refund id |
| `requestedBy` | string | no | FK -> movies_admin_users.username | Initiator |
| `requestedAt` | datetime | yes |  | Request time |
| `processedAt` | datetime | no |  | Completion time |
| `notes` | array<object> | no |  | Internal notes |

Indexes:

- index: `bookingId`
- index: `paymentId`
- index: `status + requestedAt desc`

---

## 3.12 `movies_admin_users`

Purpose:

- Admin users and permission assignment for Movies module

| Field | Type | Required | Key | Description |
|---|---|---:|---|---|
| `_id` | string / ObjectId | yes | PK | Admin user id |
| `username` | string | yes | UQ | Login/display code |
| `email` | string | yes | UQ | Admin email |
| `displayName` | string | yes |  | Full name |
| `status` | string | yes | IDX | `active`, `inactive`, `locked`, `invited` |
| `roles` | array<string> | yes |  | Role names |
| `permissions` | array<string> | yes |  | Permission keys |
| `lastLoginAt` | datetime | no |  | Last login |
| `createdAt` | datetime | yes |  | Audit |
| `updatedAt` | datetime | yes |  | Audit |

Indexes:

- unique: `username`
- unique: `email`
- index: `status`

---

## 3.13 `movies_activity_logs`

Purpose:

- Operational logs and admin audit events

| Field | Type | Required | Key | Description |
|---|---|---:|---|---|
| `_id` | string / ObjectId | yes | PK | Log id |
| `module` | string | yes |  | Always `movies` |
| `category` | string | yes | IDX | `booking`, `payment`, `payment_callback`, `ticket_delivery`, `admin_action`, `system` |
| `level` | string | yes | IDX | `info`, `warning`, `error`, `critical` |
| `reference.bookingId` | string / ObjectId | no | FK -> movies_bookings._id | Related booking |
| `reference.paymentId` | string / ObjectId | no | FK -> movies_payments._id | Related payment |
| `reference.showtimeId` | string / ObjectId | no | FK -> movies_showtimes._id | Related showtime |
| `reference.adminUsername` | string | no | FK -> movies_admin_users.username | Related admin |
| `message` | string | yes |  | Log summary |
| `payload` | object | no |  | Extended log details |
| `createdAt` | datetime | yes | IDX | Event timestamp |

Indexes:

- index: `category + createdAt desc`
- index: `level + createdAt desc`
- index: `reference.bookingId`
- index: `reference.paymentId`

---

## 4. SQL-Like Foreign Key View

This is not real SQL syntax, but it is the easiest way to visualize collection links.

```sql
movies_halls.cinemaId                -> movies_cinemas._id

movies_showtimes.movieId             -> movies_movies._id
movies_showtimes.cinemaId            -> movies_cinemas._id
movies_showtimes.hallId              -> movies_halls._id

movies_bookings.showtimeId           -> movies_showtimes._id
movies_bookings.movieId              -> movies_movies._id
movies_bookings.cinemaId             -> movies_cinemas._id
movies_bookings.hallId               -> movies_halls._id
movies_bookings.promotion.promotionId -> movies_promotions._id
movies_bookings.payment.paymentId    -> movies_payments._id

movies_seat_locks.showtimeId         -> movies_showtimes._id
movies_seat_locks.bookingId          -> movies_bookings._id

movies_payments.bookingId            -> movies_bookings._id

movies_ticket_deliveries.bookingId   -> movies_bookings._id

movies_refunds.bookingId             -> movies_bookings._id
movies_refunds.paymentId             -> movies_payments._id

movies_activity_logs.reference.bookingId  -> movies_bookings._id
movies_activity_logs.reference.paymentId  -> movies_payments._id
movies_activity_logs.reference.showtimeId -> movies_showtimes._id
```

---

## 5. Aggregate Ownership Suggestion

To make backend boundaries clearer:

### Aggregate: Movie Catalog

Collections:

- `movies_movies`
- `movies_cinemas`
- `movies_halls`
- `movies_promotions`

### Aggregate: Showtime Operations

Collections:

- `movies_showtimes`

### Aggregate: Booking Lifecycle

Collections:

- `movies_bookings`
- `movies_seat_locks`
- `movies_ticket_deliveries`
- `movies_refunds`

### Aggregate: Payment Lifecycle

Collections:

- `movies_payments`

### Aggregate: Support and Admin

Collections:

- `movies_guests`
- `movies_admin_users`
- `movies_activity_logs`

---

## 6. Suggested Read Models for API Responses

Because MongoDB is document-oriented, not every API should directly return raw collection documents.

Recommended read models:

### Public home page

Build from:

- `movies_movies`
- optional aggregated showtime count from `movies_showtimes`

### Public showtime page

Build from:

- `movies_showtimes`
- movie snapshot
- cinema snapshot
- hall snapshot

### Seat selection page

Build from:

- `movies_showtimes`
- `movies_halls`
- `movies_seat_locks`
- paid bookings for taken seats

### Admin dashboard

Build from:

- `movies_showtimes`
- `movies_bookings`
- `movies_payments`
- `movies_ticket_deliveries`
- `movies_activity_logs`

### Booking detail admin page

Build from:

- `movies_bookings`
- `movies_payments`
- `movies_ticket_deliveries`
- `movies_refunds`
- `movies_activity_logs`

---

## 7. Recommended Minimal ERD for V1

If the team wants the smallest practical implementation first, these collections are enough:

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

Then later:

11. `movies_guests`
12. `movies_refunds`
13. `movies_admin_users`

---

## 8. Final Recommendation

For implementation clarity, the backend team should treat:

- `movies_showtimes`
- `movies_bookings`
- `movies_payments`

as the 3 core transactional collections.

And treat:

- `movies_movies`
- `movies_cinemas`
- `movies_halls`
- `movies_promotions`

as master collections.

And treat:

- `movies_ticket_deliveries`
- `movies_seat_locks`
- `movies_activity_logs`
- `movies_guests`
- `movies_refunds`

as support and operational collections.

This gives a clear ERD-style mental model while still fitting MongoDB correctly.
