# Movies Admin MVP Design

**Date:** 2026-04-21

## 1. Muc tieu

Trien khai `movies admin` o muc MVP de co the van hanh that trong he thong hien tai, voi cac nang luc sau:

- Dung chung `login`, `refresh token`, `profile`, `JWT` cua `Modules.Users`
- Tach role rieng `MoviesAdmin`
- Co seed tai khoan `MoviesAdmin` cho moi truong dev
- Co API van hanh de tao/cap nhat/khoa tai khoan `MoviesAdmin`
- Co route guard frontend cho `/movies/admin/*`
- Co admin backend/frontend cho:
  - dashboard KPI co ban
  - movie CRUD
  - showtime CRUD
  - booking list/detail co ban

MVP nay khong bao gom payment monitor chi tiet, email resend, reporting nang cao, promotions, guests, logs, va user management UI day du trong movies admin.

## 2. Nguyen tac kien truc

### 2.1 Auth va identity

- `Modules.Users` van la noi cap identity va JWT duy nhat.
- `Movies` khong tao he thong auth rieng.
- Role moi `MoviesAdmin` duoc dua vao cung co che role claim hien co.
- Backend `Movies admin` su dung `[Authorize(Roles = "MoviesAdmin,Admin")]` de:
  - cho phep `MoviesAdmin` truy cap
  - giu kha nang `Admin` he thong truy cap nhu super-admin

### 2.2 Tach biet public va admin APIs

- Public movie APIs hien tai giu nguyen.
- Tao nhom admin controller rieng cho movie operations.
- DTO, service method, repository method cua admin flow duoc tach khoi public flow de tranh lan nghiep vu.

### 2.3 Lam toi thieu nhung dung pattern hien co

- Tuan theo pattern controller -> application service -> repository cua source hien tai.
- Uu tien tan dung entity, repository, query logic da co.
- Khong refactor rong ngoai pham vi MVP.

## 3. Pham vi backend

### 3.1 Users module

Can mo rong `Modules.Users` de quan ly role `MoviesAdmin`.

#### Yeu cau

- Bo sung role `MoviesAdmin` vao luong user management.
- Tao seed dev cho 3 tai khoan movies admin.
- Tao API van hanh de quan tri tai khoan movies admin.

#### Huong thiet ke

- Mo rong DTO tao/cap nhat user de co the chon role hop le trong nhom role cho phep.
- Rang buoc role cap phat duoc phep boi backend, khong tin hoan toan input tu client.
- Tao endpoint rieng trong `AuthController` hoac controller admin-related de:
  - tao movies admin
  - danh sach movies admin
  - cap nhat thong tin movies admin
  - khoa/xoa movies admin
- Seed dev 3 tai khoan `MoviesAdmin` bang cau hinh an toan cho local/dev.

### 3.2 Movies admin APIs

#### Dashboard

Them endpoint dashboard tong quan, tra ve:

- tong so phim dang active
- tong so suat chieu sap toi
- tong so booking trong khoang thoi gian mac dinh
- tong doanh thu booking paid trong khoang thoi gian mac dinh

#### Movie management

Them admin APIs cho:

- lay danh sach phim co filter co ban
- lay chi tiet phim theo id
- tao phim
- cap nhat phim
- an/xoa phim theo kha nang schema hien co

#### Showtime management

Them admin APIs cho:

- lay danh sach showtime co filter theo phim/ngay
- tao showtime
- cap nhat showtime
- huy/xoa showtime

Can validate:

- showtime phai nam trong khoang thoi gian hop le
- khong duoc trung lich trong cung phong
- thong tin movie/showroom phai ton tai

#### Booking management

Them admin APIs cho:

- danh sach booking co filter co ban
- xem chi tiet booking

Thong tin can hien thi:

- booking code
- phim
- showtime
- ghe
- customer name/email/phone neu co
- payment status
- tong tien
- thoi diem tao

## 4. Pham vi frontend

### 4.1 Route va guard

- Giu route `/movies/admin/*`
- Them guard o frontend:
  - chua login -> redirect `/login`
  - da login nhung role khong dung -> redirect `403` hoac `login`
- Guard doc `token` va `role` tu local storage hien co

### 4.2 Movies admin UI MVP

Noi bo khung prototype san co voi backend that cho 4 khu:

- `dashboard`
- `movies`
- `showtimes`
- `bookings`

#### Dashboard

- KPI cards
- bang/tom tat lich chieu sap toi
- bang/tom tat booking moi nhat

#### Movies

- table danh sach phim
- filter co ban
- create/edit form
- action an/xoa phim

#### Showtimes

- table showtime
- filter theo phim/ngay
- create/edit form
- action huy/xoa showtime

#### Bookings

- table booking
- filter theo ngay/trang thai/phim
- booking detail view

### 4.3 Section ngoai MVP

Cac section sau khong noi backend that trong dot nay:

- payments
- emails
- guests
- promotions
- logs
- users
- settings
- seats neu can phu thuoc vao CRUD phong/ghe chua co

Chung se giu giao dien prototype va thong bao rang chua kich hoat.

## 5. Data flow

### 5.1 Dang nhap va vao movies admin

1. User login bang `Modules.Users`
2. Frontend nhan `accessToken`, `refreshToken`
3. Frontend lay profile va luu `role`
4. User vao `/movies/admin/*`
5. Guard kiem tra `role`
6. Neu hop le moi cho render admin shell

### 5.2 CRUD phim/lich chieu

1. Frontend goi admin API bang bearer token
2. Backend authorize role
3. Application service validate nghiep vu
4. Repository doc/ghi DB
5. Frontend reload list hoac cap nhat optimistic state co kiem soat

### 5.3 Dashboard

1. Frontend tai dashboard summary
2. Backend tong hop tu movie/showtime/booking/payment tables
3. Frontend hien KPI va snapshot van hanh

## 6. Error handling

### Backend

- Tra `400` cho validation/business rule fail
- Tra `401/403` cho auth/role fail
- Tra `404` neu movie/showtime/booking khong ton tai
- Tra response ro rang de frontend show duoc thong diep

### Frontend

- Co loading state
- Co empty state
- Co error state/toast/message
- Khong de section mock va real data bi nham lan

## 7. Testing va xac minh

### Backend

- Verify role `MoviesAdmin` duoc dua vao JWT va authorize dung
- Verify seed 3 tai khoan dev thanh cong
- Verify movie CRUD
- Verify showtime CRUD va rule trung lich
- Verify booking list/detail

### Frontend

- Verify guard route
- Verify movies admin login flow
- Verify CRUD forms goi dung API
- Verify dashboard/load/filter states

## 8. Rui ro va gioi han

- Schema DB hien tai co the chua day du cot/trang thai cho soft delete hay dashboard KPI, can ra soat khi implement.
- User management hien tai dang nghieng ve `Manager`, nen mo rong role can lam can than de khong lam vo luong cu.
- Prototype movies admin co nhieu section mock, can gioi han ro MVP de tranh sua qua rong trong mot dot.

## 9. Ket qua mong doi sau MVP

Sau dot nay, he thong dat duoc:

- Co 3 tai khoan `MoviesAdmin` de dang nhap va truy cap dashboard phim
- Co movies admin route guard that
- Co dashboard van hanh co ban
- Co movie CRUD that
- Co showtime CRUD that
- Co booking list/detail that
- Co nen tang role/auth dung de mo rong sang payments, emails, reports o dot sau
