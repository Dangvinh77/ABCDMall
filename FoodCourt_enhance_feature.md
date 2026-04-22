# FoodCourt Enhance Feature

## Muc tieu

Bo sung manager ownership cho FoodCourt de manager chi co the CRUD du lieu FoodCourt thuoc pham vi cua minh.

## Ket luan nhanh

Phuong an hop nhat nhat voi codebase hien tai la:

- Tai su dung `shopId` da co trong `User` va JWT claim.
- Them truong ownership vao `FoodItem` thay vi tao he thong `FoodCourtManager` moi.
- Tach endpoint manager-scoped cho FoodCourt theo cung mau voi `ShopsController`.

Phuong an nhe nhat de implement la:

1. Them `OwnerShopId` vao `FoodItem`.
2. Khi manager tao FoodCourt item, backend tu dong gan `OwnerShopId` bang claim `shopId`.
3. Tao cac endpoint `GET/POST/PUT/DELETE /api/food/manager...` chi thao tac tren cac item co `OwnerShopId` trung voi manager dang dang nhap.
4. Giu cac endpoint public hien tai cho trang public `FoodPage` va `FoodDetailPage`.

## Vi sao day la phuong an hop nhat nhat

Codebase hien tai da co pattern manager ownership cho Shops:

- `User` da co truong `ShopId`.
- JWT da phat claim `shopId`.
- `ShopsController` da doc claim `shopId` de chi cho manager thao tac tren du lieu cua minh.

FoodCourt hien tai chua co relation voi `User` hay `Manager`, nhung van da dung role `Admin,Manager` cho CRUD. Vi vay cach it xao tron nhat la noi FoodCourt vao he thong ownership da ton tai, khong tao them bang manager rieng.

Neu tao moi mot he thong `FoodCourtManager` hoac `FoodCourtStore` day du ngay luc nay, pham vi se lon hon nhieu:

- them entity moi
- them relation moi
- them seed moi
- them migration phuc tap hon
- them logic auth moi
- them UI mapping moi

Trong khi codebase hien tai da coi manager la mot tai khoan gan voi `ShopId`, nen gan FoodCourt ownership vao `ShopId` la cach nhat quan nhat.

## Thiet ke de xuat

### 1. Domain

Them vao `FoodItem`:

- `string? OwnerShopId`

Y nghia:

- `null`: item do Admin seed hoac item cong khai chua gan manager cu the
- co gia tri: item do manager so huu va chi manager co `shopId` trung moi duoc sua/xoa

Khong can them `ManagerUserId` o giai doan nay vi:

- manager identity trong codebase hien tai van xoay quanh `ShopId`
- JWT da co claim `shopId`
- manager co the doi `UserId` hoac duoc cap nhat profile, nhung `ShopId` van la business anchor on dinh hon

### 2. Database

Cap nhat bang `FoodItems`:

- them cot `OwnerShopId nvarchar(64) null`
- them index cho `OwnerShopId`

Khong can foreign key sang bang `Users` vi `ShopId` hien tai mang tinh business ownership, khong phai relationship EF da duoc map san trong module FoodCourt.

Neu muon chac chan hon trong tuong lai, co the them validation runtime kiem tra `OwnerShopId` ton tai trong module Users.

### 3. Authorization va API

Giu cac endpoint public hien tai:

- `GET /api/food`
- `GET /api/food/slug/{slug}`
- `GET /api/food/search`

Khong nen tiep tuc de manager dung chung endpoint tong:

- `POST /api/food`
- `PUT /api/food/{id}`
- `DELETE /api/food/{id}`

Vi cach nay kho enforce ownership ro rang.

Nen them manager-scoped endpoint, giong pattern cua Shops:

- `GET /api/food/manager`
- `POST /api/food/manager`
- `PUT /api/food/manager/{id}`
- `DELETE /api/food/manager/{id}`

Rule:

- `Admin` co the dung endpoint tong va tac dong moi item
- `Manager` chi dung endpoint `/manager`
- backend lay `shopId` tu claim
- query/update/delete phai loc theo `OwnerShopId == claimShopId`

## Hanh vi nghiep vu

### Admin

- xem toan bo FoodCourt item
- tao item cho bat ky thuong hieu nao
- co the de `OwnerShopId = null` hoac chu dong gan `OwnerShopId`

### Manager

- chi xem danh sach FoodCourt item cua minh
- chi tao item voi `OwnerShopId` bang `shopId` cua minh
- khong duoc sua/xoa item cua manager khac
- khong duoc sua/xoa item seed public khac ownership

## Anh huong seed data

Seed hien tai cua FoodCourt la public showcase.

De giu tuong thich:

- cac item seed hien tai co the de `OwnerShopId = null`
- UI public van doc duoc binh thuong
- manager CRUD page chi hien item cua manager, khong can hien item public neu khong muon

Neu can manager nhin thay seed public de clone/chinh sua, co the bo sung co che `template`, nhung khong nen lam trong buoc dau.

## Phuong an toi uu hon trong tuong lai

Phuong an day du hon nhung nang hon la tach:

- `FoodStore`
- `FoodStoreManager`
- `FoodMenuItem`

Khi do:

- manager so huu `FoodStore`
- nhieu `FoodItem` thuoc `FoodStore`
- co the co logo, cover, gallery, opening hours, floor, promotion rieng

Day la huong dung neu FoodCourt duoc nang cap thanh mot module doc lap day du. Tuy nhien no khong phai phuong an nhe nhat.

## Phuong an nen chon

Nen chon:

- **Them `OwnerShopId` vao `FoodItem` va dung claim `shopId` de enforce manager ownership**

Day la phuong an:

- hop voi model `User.ShopId` hien tai
- hop voi JWT claim hien tai
- hop voi pattern `ShopsController`
- it doi domain nhat
- it migration nhat
- it rui ro nhat

## Ke hoach implement nhe nhat

### Backend

1. Them `OwnerShopId` vao `FoodItem`.
2. Cap nhat `FoodItemConfiguration`.
3. Tao migration.
4. Mo rong DTO neu can.
5. Bo sung repository method:
   - lay list theo `OwnerShopId`
   - lay item theo `id + OwnerShopId`
6. Them service method manager-scoped.
7. Them endpoint `/api/food/manager`.
8. Doc claim `shopId` trong `FoodController`.
9. Enforce:
   - manager create => gan `OwnerShopId` tu claim
   - manager update/delete => loc theo `OwnerShopId`

### Frontend

1. Tao `FoodAdminPage`.
2. Tao `FoodForm`.
3. Them API:
   - `getMyFoods`
   - `createMyFood`
   - `updateMyFood`
   - `deleteMyFood`
4. Them route guard cho manager.
5. Tai su dung upload image hien tai.

## Quy uoc response va loi

- Manager khong co `shopId` claim => `400 BadRequest`
- Item khong thuoc manager => `404 NotFound` hoac `403 Forbidden`

Khuyen nghi:

- dung `404` de tranh lo ownership cua item
- chi dung `403` khi user khong co role hop le

## Test can co

### Backend

- manager tao item => item duoc gan `OwnerShopId`
- manager chi lay duoc item cua minh
- manager khong update duoc item cua manager khac
- manager khong delete duoc item cua manager khac
- admin van thao tac full access

### Frontend

- manager page load du lieu cua minh
- create/update/delete goi dung endpoint `/manager`
- upload image van hoat dong

## Chot

Neu muc tieu la dua FoodCourt vao manager ownership nhanh, dung huong it rui ro va it xao tron codebase, thi nen implement:

- `OwnerShopId` trong `FoodItem`
- manager-scoped FoodCourt endpoints theo `shopId` claim
- frontend CRUD rieng cho manager

Day la phuong an nhe nhat va hop nhat nhat voi codebase hien tai.
