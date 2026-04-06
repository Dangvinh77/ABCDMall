# Movies Module ERD Mermaid

## Purpose

This file provides a Mermaid ER diagram for the `Movies` module collections inside the `ABCDMall` MongoDB database.

Even though MongoDB is non-relational, this ERD is useful for:

- system design discussion
- backend planning
- API design
- admin/reporting logic

---

## Mermaid ER Diagram

```mermaid
erDiagram
    movies_movies ||--o{ movies_showtimes : "has"
    movies_cinemas ||--o{ movies_halls : "contains"
    movies_cinemas ||--o{ movies_showtimes : "hosts"
    movies_halls ||--o{ movies_showtimes : "runs"

    movies_showtimes ||--o{ movies_bookings : "receives"
    movies_showtimes ||--o{ movies_seat_locks : "locks"

    movies_bookings ||--o{ movies_payments : "paid_by"
    movies_bookings ||--o{ movies_ticket_deliveries : "delivered_by"
    movies_bookings ||--o{ movies_refunds : "refunded_by"

    movies_promotions ||--o{ movies_bookings : "applied_to"
    movies_guests ||--o{ movies_bookings : "owns_projection"

    movies_admin_users ||--o{ movies_activity_logs : "creates"

    movies_movies {
        string _id PK
        string slug UK
        string title_default
        string title_vi
        int duration_minutes
        string age_rating
        string status
        datetime release_date
        bool is_active
    }

    movies_cinemas {
        string _id PK
        string code UK
        string name
        string mall_branch
        string city
        bool is_active
    }

    movies_halls {
        string _id PK
        string cinema_id FK
        string code
        string hall_type
        int capacity
        string maintenance_status
        bool is_active
    }

    movies_showtimes {
        string _id PK
        string movie_id FK
        string cinema_id FK
        string hall_id FK
        datetime starts_at
        datetime ends_at
        string booking_date
        string language
        decimal base_price
        int total_seats
        int available_seats
        string booking_status
        bool is_published
    }

    movies_bookings {
        string _id PK
        string booking_code UK
        string showtime_id FK
        string movie_id FK
        string cinema_id FK
        string hall_id FK
        string guest_email
        decimal total
        string status
        datetime seat_lock_expires_at
        datetime created_at
    }

    movies_seat_locks {
        string _id PK
        string showtime_id FK
        string booking_id FK
        string guest_email
        string status
        datetime expires_at
    }

    movies_payments {
        string _id PK
        string booking_id FK
        string booking_code
        string provider
        string provider_transaction_id
        decimal amount
        string status
        bool callback_received
        datetime created_at
    }

    movies_ticket_deliveries {
        string _id PK
        string booking_id FK
        string booking_code
        string guest_email
        string ticket_code
        string status
        int attempt
        datetime created_at
    }

    movies_promotions {
        string _id PK
        string code UK
        string title
        string type
        decimal value
        string status
        datetime starts_at
        datetime ends_at
    }

    movies_guests {
        string _id PK
        string email UK
        string full_name
        string phone
        int booking_count
        decimal total_spent
        datetime last_booking_at
    }

    movies_refunds {
        string _id PK
        string booking_id FK
        string payment_id FK
        decimal amount
        string status
        datetime requested_at
    }

    movies_admin_users {
        string _id PK
        string username UK
        string email UK
        string status
    }

    movies_activity_logs {
        string _id PK
        string category
        string level
        string booking_id FK
        string payment_id FK
        string showtime_id FK
        string admin_username FK
        datetime created_at
    }
```

---

## Notes

- `movies_guests -> movies_bookings` is a support/read-model relationship, not a strict transactional ownership
- `movies_activity_logs` may reference one or more entities depending on event type
- `movies_ticket_deliveries` and `movies_refunds` are operational support collections
