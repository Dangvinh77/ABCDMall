# General Rule - Frontend

Tài liệu này mô tả pattern frontend hiện tại của `CODE/FRONTEND`.
Mục tiêu là để AI và developer khác tiếp tục code đúng kiến trúc đang chạy, không tạo thêm pattern mới ngoài ý muốn.

## 1. Nguyên tắc tổng quát

- Frontend đi theo hướng `feature-first`.
- Các domain lớn như `movies`, `shops`, `food`, `auth` phải tự quản lý route, page, api và state của chính feature đó.
- `src/routes/AppRoutes.tsx` chỉ giữ route cấp app và nối các feature route lại.
- `src/App.tsx` là app shell chung, hiện đang giữ `Header`, `Footer`, và mount `AppRoutes`.
- Ưu tiên mở rộng cấu trúc đang có, không tạo thêm một style tổ chức thư mục khác song song.

## 2. Cấu trúc hiện tại cần tôn trọng

```bash
src/
├── assets/                 # static asset phía frontend nếu có
├── components/             # shared UI dùng lại nhiều nơi
├── core/
│   ├── api/                # axios client, interceptor, api wrapper
│   ├── layouts/            # Header, Footer, app shell pieces
│   └── utils/              # helper dùng chung toàn app
├── features/               # business module theo domain
├── pages/                  # page wrapper mỏng cho các route đơn
├── routes/                 # AppRoutes và app-level routing
└── store/                  # global state thật sự dùng chung
```

## 3. Phân vai giữa `features` và `pages`

### 3.1 `features/`

Dùng cho logic nghiệp vụ thật sự.

Mỗi feature có thể chứa:

- `api/`: gọi backend cho feature
- `components/`: UI nội bộ của feature
- `hooks/`: orchestration, fetch state, filter state
- `types/`: request/response/view model
- `pages/`: page thật của feature nếu feature đó có nested route
- `routes/`: route nội bộ của feature
- `services/` hoặc `data/`: chỉ dùng khi thực sự cần và đã có precedent trong feature đó

### 3.2 `pages/`

`src/pages/*` chỉ nên là wrapper mỏng cho các trang cấp app không cần nested route lớn.

Ví dụ hiện tại:

- `pages/brands/BrandsPage.tsx`
- `pages/contact/ContactPage.tsx`
- `pages/directory/MapPage.tsx`
- `pages/support/FaqPage.tsx`

Pattern đúng:

```tsx
import { BrandsFeature } from "../../features/brands/BrandsFeature";

export const BrandsPage = () => {
  return <BrandsFeature />;
};
```

Không nhét business logic lớn trực tiếp vào `src/pages` nếu logic đó thuộc một domain rõ ràng.

## 4. Quy tắc route

### 4.1 Route cấp app

`src/routes/AppRoutes.tsx` là nơi:

- khai báo route public của toàn mall
- lazy-load feature/page lớn
- xử lý redirect legacy
- xử lý 404

Hiện tại `AppRoutes` đang dùng `React.lazy` + `Suspense` để giảm bundle đầu vào.

Vì vậy:

- route mới có page đủ lớn nên dùng lazy import
- feature lớn phải mount bằng route riêng, ví dụ `/movies/*`, `/shops/*`, `/food-court/*`
- redirect legacy phải để ở app-level nếu ảnh hưởng URL cũ của toàn hệ thống

### 4.2 Route của feature

Feature lớn phải có file route riêng trong chính feature đó.

Ví dụ đang có:

- `src/features/shops/routes/ShopsRoutes.tsx`
- `src/features/food/routes/FoodRoutes.tsx`
- `src/features/movies/routes/MovieRoutes.tsx`
- `src/features/movies-admin/routes/MoviesAdminRoutes.tsx`

Pattern mong muốn:

```tsx
export function ShopsRoutes() {
  return (
    <Routes>
      <Route index element={<ShopPage />} />
      <Route path=":slug" element={<ShopDetailPage />} />
      <Route path="*" element={<Navigate to="/shops" replace />} />
    </Routes>
  );
}
```

## 5. Quy tắc khi thêm feature mới

### 5.1 Nếu feature là domain lớn

Tạo dưới `src/features/<feature-name>/` với các phần cần thiết:

```bash
src/features/<feature-name>/
├── api/
├── components/
├── hooks/
├── pages/
├── routes/
└── types/
```

Không bắt buộc phải có đủ toàn bộ thư mục nếu feature nhỏ.
Chỉ tạo thư mục nào thực sự dùng.

### 5.2 Nếu chỉ là trang đơn

Nếu trang đơn giản, không có nested route và không có nhiều logic riêng:

- tạo feature component trong `src/features/<feature-name>/`
- tạo wrapper page mỏng trong `src/pages/<feature-name>/`
- khai báo route ở `AppRoutes`

Đây là pattern hiện tại của:

- `brands`
- `contact`
- `faq`
- `feedbacks`
- `amenities`
- `directory`

## 6. Quy tắc API layer

### 6.1 Dùng `core/api/api.ts` làm entry chung

Tất cả API call mới nên đi qua:

- `BASE_URL`
- `http`
- `api.get/post/put/delete`

Không tự tạo axios client mới cho từng feature nếu không có lý do rõ ràng.

`core/api/api.ts` hiện đã có:

- base URL từ `VITE_API_BASE_URL`
- interceptor gắn bearer token
- refresh token flow
- map lỗi API về `Error`

### 6.2 Feature API

Mỗi feature nên có file API riêng trong `features/<feature>/api/`.

Ví dụ:

- `features/shops/api/shopApi.ts`
- `features/food/api/foodApi.ts`
- `features/directory/api/mapApi.ts`

Feature API chỉ nên:

- gọi endpoint
- map kiểu dữ liệu cần thiết
- không render UI

## 7. Quy tắc xử lý ảnh và static resources

### 7.1 Không hard-code origin ảnh trong component

Nếu ảnh backend trả về là relative path như `/img/...` hoặc `/images/...`, phải convert qua helper chung.

Dùng:

- `src/core/utils/image.ts`

Ví dụ:

```tsx
import { getImageUrl } from "../../core/utils/image";
```

Không được giữ kiểu:

```tsx
const API_BASE = "http://localhost:5184";
```

trong component mới.

### 7.2 Quy tắc dùng ảnh

- Nếu backend trả absolute URL thì render thẳng
- Nếu backend trả relative path thì phải đi qua helper
- Nếu dữ liệu mock dùng `/img/...` thì vẫn phải hiển thị qua `getImageUrl`

## 8. Import rule

### 8.1 Ưu tiên nhất quán hơn là giáo điều

Repo hiện đang tồn tại cả 2 kiểu import:

- alias `@/...`
- relative import `../../...`

Rule cho hiện tại:

- khi sửa file cũ, ưu tiên giữ style import đang dùng trong file đó
- khi tạo file mới, có thể dùng relative import nếu ngắn và rõ
- không trộn lung tung trong cùng một file nếu không cần

### 8.2 Cross-feature import

Hạn chế import chéo giữa các feature.

Đúng:

- `routes/AppRoutes.tsx` import route/page của feature
- `pages/*` import feature component của chính nó

Sai:

- `features/shops` import sâu vào implementation của `features/movies`

Nếu có phần dùng chung, đưa về:

- `src/components`
- `src/core`
- hoặc `src/store`

## 9. Bundle và performance rule

Frontend hiện đã tối ưu theo 2 lớp:

- lazy-load route ở `AppRoutes`
- manual chunk trong `vite.config.ts`

Vì vậy khi thêm page hoặc feature lớn:

- ưu tiên lazy-load ở app route
- không import trực tiếp toàn bộ feature nặng vào entry bundle nếu chưa cần

Nếu một feature làm bundle chính tăng mạnh, xem lại:

- route lazy chưa
- có import dataset lớn ở top-level không
- có kéo thư viện nặng vào trang thường xuyên không

## 10. UI và layout rule

- `Header` và `Footer` đang là layout toàn app trong `App.tsx`
- page mới phải chừa không gian hợp lý cho header fixed
- các page public hiện thường bắt đầu với `pt-32`
- ưu tiên bám visual style hiện có của mall: card bo tròn lớn, gradient đỏ-cam, spacing rộng, text đậm

Không tự tạo thêm app shell khác trừ khi có yêu cầu cụ thể cho admin layout riêng.

## 11. TypeScript và file convention

- File `.ts` dùng cho type, api, helper
- File `.tsx` dùng cho React component
- Với `verbatimModuleSyntax`, type import phải dùng `import type`

Ví dụ:

```ts
import type { FloorPlan } from "../types/map.types";
```

Nếu quên rule này, build sẽ fail.

## 12. Khi nào được dùng mock data

Cho phép dùng mock data khi:

- chưa có backend thật
- cần dựng UI trước
- cần fallback tạm thời cho trải nghiệm frontend

Nhưng phải tuân thủ:

- đặt mock trong chính feature đó
- không trộn mock với shared global state nếu không cần
- khi backend đã có thật thì ưu tiên chuyển sang API thật

## 13. Checklist khi thêm code mới

Trước khi merge một feature/page mới, cần tự kiểm:

1. Code thuộc đúng `feature`, `page`, hay `core` chưa.
2. Route đang đặt ở đúng level app hay feature chưa.
3. API call đã đi qua `core/api/api.ts` chưa.
4. Ảnh relative path đã đi qua `getImageUrl` chưa.
5. Feature lớn đã lazy-load ở `AppRoutes` chưa.
6. Không tạo thêm pattern thư mục mới trái với các feature hiện có.
7. `npm run build` phải pass.

## 14. Quyết định kiến trúc cho giai đoạn hiện tại

Đây là rule chốt cho repo này:

- `App.tsx` giữ app shell chung
- `AppRoutes.tsx` giữ route cấp app và lazy-load
- feature lớn có `routes` riêng
- trang đơn có thể đi qua `src/pages` wrapper mỏng
- API dùng client chung ở `core/api`
- image URL relative phải đi qua helper chung
- mở rộng pattern hiện có, không tái thiết kế lại frontend theo một style mới khác

Nếu code mới mâu thuẫn với tài liệu này, ưu tiên chỉnh code theo tài liệu này, trừ khi team đã thống nhất đổi pattern toàn dự án.
