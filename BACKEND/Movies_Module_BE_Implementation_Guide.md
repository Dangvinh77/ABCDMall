# Movies Module BE Implementation Guide

## 1. Muc tieu tai lieu

Tai lieu nay dung cho backend implementation.

Muc dich:

- chot pham vi v1
- chot collection nao la core
- chot relationship, index va quy tac modeling
- giup implement API, service, repository va MongoDB migration/index setup

Day la tai lieu ky thuat, uu tien tinh thuc dung hon tinh trinh bay.

---

## 2. Chot pham vi implementation

### V1 nen gom 10 collection

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

### Phase sau

- `movies_guests`
- `movies_refunds`
- `movies_admin_users`

Ly do:

- V1 tap trung vao browse, showtimes, booking, payment, ticket email, admin monitoring
- 3 collection de sau chu yeu phuc vu support va permission chi tiet

---

## 3. Chot 3 collection core

3 collection core cua module:

### `movies_showtimes`

La source chinh cho:

- public showtime listing
- seat inventory
- admin scheduling

### `movies_bookings`

La source chinh cho:

- order lifecycle
- trang thai dat ve
- tong tien cuoi cung
- lien ket ticket va payment

### `movies_payments`

La source chinh cho:

- payment attempt
- callback tracking
- reconciliation

Rule quan trong:

- `showtimes` quan ly nang luc ban
- `bookings` quan ly nghiep vu don
- `payments` quan ly giao dich thanh toan

---

## 4. Aggregate boundary de code

De chia service/backend boundary cho ro:

### Aggregate: Catalog

Collections:

- `movies_movies`
- `movies_cinemas`
- `movies_halls`
- `movies_promotions`

Phu trach:

- CRUD danh muc
- query public catalog
- query admin catalog

### Aggregate: Showtime Operations

Collections:

- `movies_showtimes`

Phu trach:

- CRUD suat chieu
- validate trung lich
- publish/unpublish showtime
- cap nhat seatInventory

### Aggregate: Booking Lifecycle

Collections:

- `movies_bookings`
- `movies_seat_locks`
- `movies_ticket_deliveries`
- `movies_refunds`

Phu trach:

- lock ghe
- tao booking
- cap nhat booking status
- issue ticket
- resend email
- refund business state

### Aggregate: Payment Lifecycle

Collections:

- `movies_payments`

Phu trach:

- create payment
- save payment request
- process callback
- idempotency payment provider

### Aggregate: Support and Audit

Collections:

- `movies_guests`
- `movies_admin_users`
- `movies_activity_logs`

Phu trach:

- admin support
- guest projections
- audit log

---

## 5. Relationship chot de implement

```text
movies_movies      1 -> n movies_showtimes
movies_cinemas     1 -> n movies_halls
movies_cinemas     1 -> n movies_showtimes
movies_halls       1 -> n movies_showtimes

movies_showtimes   1 -> n movies_bookings
movies_showtimes   1 -> n movies_seat_locks

movies_bookings    1 -> n movies_payments
movies_bookings    1 -> n movies_ticket_deliveries
movies_bookings    1 -> n movies_refunds

movies_promotions  1 -> n movies_bookings
```

Implementation note:

- MongoDB khong co foreign key that su
- can validate reference bang code/service layer
- khong duoc phu thuoc 100% vao schema document de dam bao tinh dung

---

## 6. Schema rule bat buoc

### 6.1 Quy tac id

Can chot som 1 trong 2 huong:

- dung `ObjectId` cho `_id`
- hoac dung string business id co format ro rang

De de thuc hien backend va mapping, khuyen nghi:

- `_id`: dung `ObjectId`
- business code rieng cho field can hien thi ra ngoai

Vi du:

- booking `_id`: `ObjectId`
- `bookingCode`: `ABCD-20260410-000123`

### 6.2 Quy tac snapshot

Bat buoc snapshot vao document giao dich:

- showtime snapshot movie/cinema/hall
- booking snapshot thong tin gia, ghe, promo
- payment snapshot bookingCode
- ticket delivery snapshot guestEmail, ticketCode

Khong duoc query lai movie/hall/promotion hien tai de tinh toan cho don da tao.

### 6.3 Quy tac enum

Can centralize enum trong code backend, khong hardcode moi noi.

Khuyen nghi tach:

- `MovieStatus`
- `ShowtimeStatus`
- `BookingStatus`
- `PaymentStatus`
- `DeliveryStatus`
- `RefundStatus`

### 6.4 Quy tac money

Tat ca gia tri tien phai:

- thong nhat don vi `VND`
- luu so nguyen theo VND neu co the
- tranh float khi tinh tong tien

### 6.5 Quy tac time

Bat buoc:

- luu datetime theo UTC
- `bookingDate` co the la chuoi `YYYY-MM-DD` de filter nhanh

---

## 7. Collection specification cho V1

## 7.1 `movies_movies`

Field can co:

- `_id`
- `slug`
- `title`
- `description`
- `genreIds` hoac `genresText`
- `durationMinutes`
- `director`
- `cast`
- `language`
- `ageRating`
- `status`
- `releaseDate`
- `posterUrl`
- `backdropUrl`
- `trailerUrl`
- `isActive`
- `createdAt`
- `updatedAt`

Indexes:

- unique `slug`
- `status + isActive`
- `releaseDate`

## 7.2 `movies_cinemas`

Field can co:

- `_id`
- `code`
- `name`
- `mallBranch`
- `address`
- `contact`
- `location`
- `openingHours`
- `isActive`
- `createdAt`
- `updatedAt`

Indexes:

- unique `code`
- `isActive`
- `location 2dsphere` neu co map

## 7.3 `movies_halls`

Field can co:

- `_id`
- `cinemaId`
- `code`
- `name`
- `hallType`
- `capacity`
- `seatLayout`
- `seatMap`
- `seatTypePricing`
- `maintenanceStatus`
- `isActive`
- `createdAt`
- `updatedAt`

Indexes:

- unique `cinemaId + code`
- `cinemaId + hallType`
- `isActive`

## 7.4 `movies_showtimes`

Field can co:

- `_id`
- `movieId`
- `movieSnapshot`
- `cinemaId`
- `cinemaSnapshot`
- `hallId`
- `hallSnapshot`
- `startsAt`
- `endsAt`
- `bookingDate`
- `language`
- `pricing`
- `seatInventory`
- `bookingStatus`
- `isPublished`
- `createdAt`
- `updatedAt`
- `createdBy`
- `updatedBy`

Indexes:

- `movieId + startsAt`
- `cinemaId + startsAt`
- `hallId + startsAt`
- `bookingDate + movieId`
- `bookingStatus + isPublished + startsAt`
- unique `hallId + startsAt`

Rule:

- truoc khi tao showtime phai check trung lich cung hall
- `seatInventory.totalSeats` phai khop hall capacity tai thoi diem tao

## 7.5 `movies_bookings`

Field can co:

- `_id`
- `bookingCode`
- `showtimeId`
- `movieId`
- `cinemaId`
- `hallId`
- `guest`
- `seats`
- `combos`
- `pricing`
- `promotion`
- `payment`
- `ticket`
- `status`
- `seatLockExpiresAt`
- `cancelReason`
- `refundedAt`
- `notes`
- `createdAt`
- `updatedAt`
- `source`

Indexes:

- unique `bookingCode`
- `guest.email + createdAt desc`
- `showtimeId + status`
- `payment.paymentId`
- `status + createdAt desc`
- `seatLockExpiresAt`

Rule:

- booking la source of truth cua tong tien cuoi cung
- chi duoc chuyen `pending_payment -> paid/ticket_issued/cancelled/expired/...` theo state machine

## 7.6 `movies_seat_locks`

Field can co:

- `_id`
- `showtimeId`
- `bookingId`
- `seatCodes`
- `guestEmail`
- `status`
- `expiresAt`
- `createdAt`

Indexes:

- `showtimeId + seatCodes`
- `bookingId`
- TTL `expiresAt`

Rule:

- lock chi co hieu luc tam thoi
- booking logic van phai verify lock con han, khong duoc chi tin TTL

## 7.7 `movies_payments`

Field can co:

- `_id`
- `bookingId`
- `bookingCode`
- `provider`
- `providerTransactionId`
- `amount`
- `currency`
- `status`
- `paymentUrl`
- `callback`
- `requestPayload`
- `error`
- `createdAt`
- `updatedAt`

Indexes:

- `bookingId`
- `bookingCode`
- `status + createdAt desc`
- `callback.received + status`
- unique sparse `provider + providerTransactionId`

Rule:

- callback phai xu ly idempotent
- khong duoc tao 2 payment success cho cung mot booking

## 7.8 `movies_ticket_deliveries`

Field can co:

- `_id`
- `bookingId`
- `bookingCode`
- `guestEmail`
- `ticketCode`
- `channel`
- `status`
- `attempt`
- `templateCode`
- `subject`
- `pdfUrl`
- `provider`
- `error`
- `sentAt`
- `openedAt`
- `createdAt`

Indexes:

- `bookingId + createdAt desc`
- `guestEmail + createdAt desc`
- `status + createdAt desc`

## 7.9 `movies_promotions`

Field can co:

- `_id`
- `code`
- `title`
- `description`
- `type`
- `value`
- `maxDiscount`
- `status`
- `conditions`
- `usageLimit`
- `usageStats`
- `startsAt`
- `endsAt`
- `isStackable`
- `createdAt`
- `updatedAt`

Indexes:

- unique `code`
- `status + startsAt + endsAt`

## 7.10 `movies_activity_logs`

Field can co:

- `_id`
- `module`
- `category`
- `level`
- `reference`
- `message`
- `payload`
- `createdAt`

Indexes:

- `category + createdAt desc`
- `level + createdAt desc`
- `reference.bookingId`
- `reference.paymentId`
- optional `reference.showtimeId`

---

## 8. Booking flow de implement

## 8.1 Open showtime

Read:

- `movies_movies`
- `movies_showtimes`
- `movies_cinemas`
- `movies_halls`

## 8.2 Lock seats

Input:

- showtime id
- seat codes
- guest email

Logic:

1. Validate showtime dang `open`
2. Validate seat ton tai trong hall
3. Validate seat chua bi paid booking chiem
4. Validate seat chua bi lock con han
5. Tao `movies_seat_locks`

## 8.3 Create booking

Logic:

1. Verify seat lock van con han
2. Snapshot showtime/movie/cinema/hall neu can
3. Tinh pricing final
4. Apply promotion
5. Tao `movies_bookings` status `pending_payment`

## 8.4 Create payment

Logic:

1. Tao record `movies_payments` status `pending`
2. Gan `payment.paymentId` vao booking
3. Goi payment provider
4. Luu request payload / paymentUrl

## 8.5 Handle callback

Logic:

1. Xac minh callback
2. Tim payment theo providerTransactionId hoac bookingCode
3. Xu ly idempotent
4. Update `movies_payments`
5. Update `movies_bookings`
6. Update `movies_showtimes.seatInventory`
7. Release/chuyen trang thai seat lock
8. Ghi `movies_activity_logs`

## 8.6 Issue ticket

Logic:

1. Tao ticket code
2. Update booking ticket info
3. Tao `movies_ticket_deliveries`
4. Gui email
5. Update email status

---

## 9. State machine can chot trong code

### Booking status

De xuat:

- `draft`
- `pending_payment`
- `paid`
- `ticket_issued`
- `cancelled`
- `expired`
- `refund_pending`
- `refunded`
- `failed`

Flow toi thieu:

```text
draft -> pending_payment
pending_payment -> paid
pending_payment -> expired
pending_payment -> failed
paid -> ticket_issued
paid -> refund_pending
refund_pending -> refunded
pending_payment -> cancelled
```

### Payment status

- `pending`
- `success`
- `failed`
- `cancelled`
- `expired`
- `refunded`

### Showtime status

- `draft`
- `open`
- `closed`
- `cancelled`
- `completed`

---

## 10. Index rollout uu tien

### Phase 1 bat buoc

- `movies_showtimes.hallId + startsAt`
- `movies_bookings.bookingCode`
- `movies_bookings.guest.email + createdAt`
- `movies_seat_locks.expiresAt` TTL
- `movies_payments.provider + providerTransactionId`

### Phase 2 nen co

- `movies_showtimes.bookingStatus + isPublished + startsAt`
- `movies_payments.status + createdAt`
- `movies_ticket_deliveries.bookingId + createdAt`
- `movies_promotions.code`
- `movies_activity_logs.category + createdAt`

---

## 11. Quy tac tranh loi khi implement

1. Khong tinh lai gia booking tu movie/hall/promotion hien tai sau khi da tao booking
2. Khong rely vao TTL alone de mo khoa ghe
3. Khong cap nhat seatInventory theo kieu read-modify-write khong bao ve tranh race condition
4. Khong cho phep callback thanh cong xu ly lap lai lam tang soldSeats nhieu lan
5. Khong dung activity log thay cho transaction state chinh

---

## 12. Goi y API ownership

### Public APIs

- movies catalog
- movie detail
- showtimes
- seat availability
- create booking
- payment redirect/init

### Admin APIs

- CRUD movies
- CRUD showtimes
- booking search
- payment inspection
- resend ticket
- promotion CRUD
- audit log query

---

## 13. Ket luan implementation

Neu chot de implement nhanh va dung huong, backend nen di theo thu tu:

1. Chot schema 10 collection v1
2. Chot enum + state machine
3. Chot indexes phase 1
4. Implement showtime read + seat lock
5. Implement booking + payment callback
6. Implement ticket delivery + admin monitoring

Tai lieu nay la ban ky thuat de minh va ban bam vao khi code BE.
