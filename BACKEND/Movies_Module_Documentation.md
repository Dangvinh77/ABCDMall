# Movies Module Documentation

## 1. Muc tieu tai lieu

Tai lieu nay mo ta module `Movies` cua du an `ABCDMall` theo cach de doc, de trinh bay va de thong nhat nghiep vu.

Tai lieu nay phu hop cho:

- team backend
- team frontend
- BA/PM
- nguoi review he thong

No tra loi 4 cau hoi chinh:

1. Module phim dung de lam gi
2. Du lieu duoc chia thanh nhung nhom nao
3. Luong dat ve van hanh ra sao
4. Can nhung collection nao trong MongoDB

---

## 2. Pham vi module

Module `Movies` phuc vu toan bo nghiep vu dat ve phim tai ABCD Mall:

- hien thi danh sach phim
- xem chi tiet phim
- xem lich chieu
- chon ghe
- dat ve voi guest
- thanh toan
- gui ve qua email
- quan tri phim va lich chieu o trang admin

Module nay dung chung database `ABCDMall`, nhung du lieu duoc tach rieng bang prefix `movies_` de:

- tranh trung ten voi module khac
- de quan ly ownership
- de backup, debug va bao cao

---

## 3. Chien luoc database

Database de xuat:

- `ABCDMall`

Quy uoc dat ten collection:

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

Y nghia:

- `movies_` la prefix theo module
- phan sau the hien vai tro collection

---

## 4. Cach chia du lieu

Du lieu cua module phim duoc chia thanh 4 nhom de de hieu:

### 4.1 Du lieu danh muc

Dung de mo ta tai nguyen co ban cua he thong:

- phim
- rap
- phong chieu
- khuyen mai

Collection:

- `movies_movies`
- `movies_cinemas`
- `movies_halls`
- `movies_promotions`

### 4.2 Du lieu van hanh lich chieu

Dung de cho biet phim nao dang chieu, o dau, luc nao, gia bao nhieu:

- `movies_showtimes`

### 4.3 Du lieu giao dich dat ve

Dung cho qua trinh chon ghe, tao don, thanh toan, hoan tien:

- `movies_bookings`
- `movies_seat_locks`
- `movies_payments`
- `movies_refunds`

### 4.4 Du lieu ho tro va quan tri

Dung cho email, support, admin va audit:

- `movies_ticket_deliveries`
- `movies_guests`
- `movies_admin_users`
- `movies_activity_logs`

---

## 5. Collection chinh va vai tro

### `movies_movies`

Luu thong tin phim:

- ten phim
- mo ta
- the loai
- thoi luong
- dao dien, dien vien
- ngon ngu
- do tuoi
- anh poster, backdrop, trailer
- trang thai phim: sap chieu, dang chieu, luu tru

### `movies_cinemas`

Luu thong tin chi nhanh rap:

- ten rap
- dia chi
- thong tin lien he
- toa do
- gio mo cua

### `movies_halls`

Luu thong tin phong chieu:

- thuoc rap nao
- loai phong
- suc chua
- so do ghe
- cau hinh gia theo loai ghe

### `movies_showtimes`

Day la collection trung tam cua van hanh lich chieu.

No cho biet:

- phim nao
- tai rap nao
- phong nao
- bat dau luc nao
- ket thuc luc nao
- gia co ban
- ton kho ghe con lai
- co dang mo ban hay khong

### `movies_bookings`

Day la collection quan trong nhat ve nghiep vu.

No luu:

- ma dat ve
- thong tin guest
- suat chieu
- danh sach ghe
- combo
- tong tien
- khuyen mai da ap dung
- trang thai don
- lien ket sang payment va ticket

### `movies_seat_locks`

Dung de giu ghe tam thoi trong luc guest dang thanh toan.

Muc dich:

- tranh 2 nguoi dat cung 1 ghe
- tu dong het han sau mot khoang thoi gian

### `movies_payments`

Luu tung lan thanh toan:

- don nao
- cong thanh toan nao
- so tien
- trang thai
- callback tra ve gi

### `movies_ticket_deliveries`

Luu lich su gui ve:

- gui cho ai
- gui lan thu may
- da gui thanh cong chua
- loi gi neu that bai

### `movies_guests`

La view ho tro guest, khong phai tai khoan dang nhap.

Dung de:

- xem lich su dat ve theo email
- tong hop tan suat mua
- luu note support

### `movies_promotions`

Luu quy tac khuyen mai:

- ma code
- loai giam gia
- gia tri giam
- dieu kien ap dung
- thoi gian hieu luc

### `movies_admin_users`

Luu quyen admin rieng cho module phim:

- tai khoan admin
- role
- permission

### `movies_activity_logs`

Luu log van hanh va audit:

- su kien booking
- payment callback
- gui email
- thao tac admin

### `movies_refunds`

Luu nghiep vu hoan tien:

- booking nao
- payment nao
- ly do
- so tien
- trang thai xu ly

---

## 6. Quan he giua cac collection

Nhin o muc nghiep vu, module nay van hanh theo quan he:

- Mot phim co nhieu suat chieu
- Mot rap co nhieu phong
- Mot phong co nhieu suat chieu
- Mot suat chieu co nhieu booking
- Mot suat chieu co nhieu seat lock
- Mot booking co the co nhieu payment lan thu
- Mot booking co nhieu lan gui ve
- Mot booking co the phat sinh refund
- Mot promotion co the duoc ap dung cho nhieu booking

Tom gon:

```text
Movie -> Showtime -> Booking -> Payment / Ticket Delivery / Refund
Cinema -> Hall -> Showtime
Showtime -> Seat Lock
Booking -> Guest projection
Admin User -> Activity Log
```

---

## 7. Luong nghiep vu dat ve

### Buoc 1. Xem phim va lich chieu

He thong doc:

- `movies_movies`
- `movies_showtimes`
- `movies_cinemas`
- `movies_halls`

### Buoc 2. Chon ghe

He thong doc:

- `movies_showtimes`
- `movies_halls`

He thong ghi:

- `movies_seat_locks`

### Buoc 3. Nhap thong tin guest va tao don

He thong ghi:

- `movies_bookings`

### Buoc 4. Tao giao dich thanh toan

He thong ghi:

- `movies_payments`

He thong cap nhat:

- `movies_bookings`

### Buoc 5. Nhan callback thanh toan

He thong cap nhat:

- `movies_payments`
- `movies_bookings`
- `movies_showtimes`

He thong xu ly ghe tam:

- xoa hoac de TTL tu het han `movies_seat_locks`

He thong ghi log:

- `movies_activity_logs`

### Buoc 6. Gui ve qua email

He thong ghi:

- `movies_ticket_deliveries`

He thong cap nhat:

- `movies_bookings`
- `movies_guests`

---

## 8. Nguyen tac modeling

Module nay nen di theo cac nguyen tac sau:

1. Du lieu danh muc giu tuong doi chuan hoa
2. Du lieu giao dich cho phep snapshot de truy van nhanh va giu lich su
3. Booking phai luu tong tien cuoi cung, khong tinh lai tu du lieu hien tai
4. Seat lock chi la tam thoi va phai co co che tu dong het han
5. Log nen ghi thanh su kien, khong duoc overwrite mat lich su

---

## 9. Bo collection nen co trong phien ban dau

Neu can mot phien ban thuc te de bat dau nhanh, co the uu tien 10 collection:

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

Co the bo sung sau:

- `movies_guests`
- `movies_refunds`
- `movies_admin_users`

---

## 10. Ket luan

Module `Movies` nen duoc hieu theo 3 truc chinh:

- Catalog: phim, rap, phong, khuyen mai
- Operation: lich chieu
- Transaction: booking, payment, ticket

Trong do:

- `movies_showtimes` la trung tam van hanh
- `movies_bookings` la trung tam nghiep vu
- `movies_payments` la trung tam doi soat thanh toan

Tai lieu nay la ban tong quan de viet doc va thong nhat cach hieu trong team.
