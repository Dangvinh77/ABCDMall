# 🌐 ABCD Mall Client

A **Feature-based** React frontend for the ABCD Mall management system, designed to work seamlessly with the ABCD Mall Modulith Backend.

---

## 🚀 Tech Stack

- ⚡ Vite - Next Generation Frontend Tooling
- ⚛️ React 18 (TypeScript)
- 🎨 Tailwind CSS - Utility-first CSS framework
- 🛡️ Axios - HTTP Client with Interceptors for JWT
- 🎢 React Router 6 - Client-side routing
- 🐻 Zustand - Lightweight State Management
- 📝 React Hook Form + Zod - Form handling & validation
- 📁 Project FE Structure

---

## 📁 Project FE Structure

```bash
abcd-mall-client/
├── src/
│   ├── core/                       # NỀN TẢNG (Giao tiếp Backend & Auth)
│   │   ├── api/                    # Axios Client & Global Interceptors
│   │   ├── hooks/                  # Global Hooks (useAuth, useTheme)
│   │   └── layouts/                # MainLayout, AdminLayout, ShopLayout
│   │
│   ├── features/                   # CÁC MODULE CHỨC NĂNG (Modulith-like)
│   │   ├── auth/                   # Đăng nhập & Phân quyền Admin/Manager
│   │   ├── movies/                 # Xem phim & Đặt vé (Guest)
│   │   ├── shops/                  # Quản lý Shops & Products
│   │   └── feedbacks/              # Gửi phản hồi (Guest)
│   │
│   ├── components/                 # UI Components dùng chung (Button, Modal...)
│   ├── pages/                      # Điểm cuối của Routing (Page Components)
│   ├── routes/                     # Cấu hình Public/Private Routes
│   └── store/                      # Global State (Zustand)
│
├── .env                            # Chứa VITE_API_URL
└── vite.config.ts                  # Cấu hình Vite & Alias
```

## 📁 Example for each feature: features/movies

```bash
src/features/movies/
├── api/                        # 1. Giao tiếp Backend
│   ├── movieApi.ts             # Gọi API lấy danh sách phim, chi tiết phim
│   └── bookingApi.ts           # Gọi API đặt vé, kiểm tra trạng thái ghế
│
├── components/                 # 2. Các UI Components đặc thù của Movie
│   ├── MovieCard.tsx           # Thẻ hiển thị phim ở trang chủ
│   ├── MovieList.tsx           # Danh sách phim có filter
│   ├── SeatMap/                # Folder riêng cho sơ đồ ghế (vì nó phức tạp)
│   │   ├── Seat.tsx            # Component từng cái ghế (Trống/Đã đặt/Đang chọn)
│   │   └── SeatGrid.tsx        # Lưới hiển thị toàn bộ sơ đồ ghế
│   └── BookingSummary.tsx      # Bảng tóm tắt thông tin vé trước khi thanh toán
│
├── hooks/                      # 3. Logic xử lý (Custom Hooks)
│   ├── useMovies.ts            # Hook quản lý fetch danh sách phim, loading, error
│   └── useBooking.ts           # Hook xử lý chọn ghế, kiểm tra trùng ghế (Concurrency)
│
├── types/                      # 4. Định nghĩa kiểu dữ liệu (TypeScript)
│   └── index.ts                # IMovie, IShowtime, ISeat, IBookingRequest
│
├── utils/                      # 5. Hàm bổ trợ riêng cho Movie
│   └── priceCalculator.ts      # Tính tổng tiền dựa trên loại ghế/ngày chiếu
│
└── index.ts                    # 6. Public API của Module (Cực kỳ quan trọng)
```

## 🧠 Architecture Principles

Frontend áp dụng tư duy Feature-First:

- Encapsulation: Mỗi folder trong features/ tự quản lý api, components, types và hooks riêng của nó.
- Independence: Module movies không phụ thuộc trực tiếp vào module shops.
- Global Core: Các logic về JWT và Axios được tập trung ở core/ để xử lý việc tự động gắn Token vào Header khi Admin/Manager đăng nhập.
- Concurrency UX: Xử lý trạng thái Loading/Optimistic UI để hỗ trợ Backend trong việc đặt vé đồng thời.

## 🎯 Key Features (Frontend Side)

### 🎬 Movie Booking (Guest)

Hiển thị danh sách phim và suất chiếu.
Sơ đồ ghế ngồi Real-time: Chọn ghế trực quan, xử lý lỗi khi ghế đã bị người khác đặt (từ phản hồi Backend).
Thanh toán giả lập với trạng thái xử lý mượt mà.

### 🛒 Mall & Shop Explorer

Xem sơ đồ mặt bằng Mall.
Tìm kiếm Shop, xem menu Food Court (Không cần Login).
Dashboard cho Shop Manager: Quản lý riêng biệt thông tin shop của mình.

### 💬 Feedback System (Guest)

Form gửi phản hồi nhanh cho từng Shop/Phim mà không cần tài khoản.

### 🔐 Auth & Authorization

Phân quyền giao diện dựa trên Role (Admin thấy toàn bộ, Manager thấy 1 shop, Guest thấy trang Client).

## ⚙️ Setup & Run

### 1️⃣ Clone project

```bash
git clone https://github.com/your-repo/abcd-mall-client.git
cd abcd-mall-client
```

### 2️⃣ Install dependencies

```bash
npm install
```

### 3️⃣ Configure Environment

Tạo file .env tại thư mục gốc:

```bash
VITE_API_URL=https://localhost:7001/api/v1
```

### 4️⃣ Run development server

```bash
npm run dev
```

## 🛠️ Integration Notes

- API Mapping: Mọi request trong features/\*/api phải khớp với các Endpoint trong dự án .NET WebAPI.
- Handling Concurrency: Khi Backend trả về lỗi Conflict (409) do trùng ghế, Frontend sẽ hiển thị thông báo yêu cầu người dùng chọn lại ghế khác.
- Token Storage: JWT được lưu an toàn trong LocalStorage hoặc HttpOnly Cookie (tùy cấu hình) và được quản lý bởi useAuth hook.

👨‍💻 Author: Group 2

⭐ Notes: Cấu trúc này giúp chúng ta dễ dàng mở rộng thêm module mới mà không cần cấu trúc lại toàn bộ ứng dụng.
