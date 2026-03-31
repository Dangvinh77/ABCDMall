# 🏢 ABCD Mall - Integrated Online Application

ABCD Mall là một hệ thống quản lý trung tâm thương mại hiện đại, được thiết kế để tối ưu hóa trải nghiệm khách hàng và hiệu quả quản lý. Dự án áp dụng kiến trúc **Modular Monolith (Modulith)** ở phía Backend và **Feature-based Architecture** ở phía Frontend nhằm đảm bảo tính mở rộng, dễ bảo trì và hiệu suất cao.

## 📖 Tổng quan dự án

Dự án mô phỏng hệ thống của trung tâm thương mại ABCD tại Mumbai, bao gồm các khu vực mua sắm, khu ẩm thực (Food Court), và cụm rạp chiếu phim hiện đại.

- Khách hàng (Guest): Khám phá mall, đặt vé xem phim trực tuyến (xử lý đặt chỗ thời gian thực), xem menu ẩm thực và gửi phản hồi.
- Quản lý cửa hàng (Shop Manager): Cập nhật thông tin cửa hàng, danh mục sản phẩm và quản lý phản hồi của khách hàng.
- Quản trị viên (Admin): Quản lý toàn bộ hệ thống, điều phối suất chiếu phim và nhân sự quản lý shop.

## 🛠️ Công nghệ sử dụng (Tech Stack)

| Thành phần | Công nghệ                                                         |
| ---------- | ----------------------------------------------------------------- |
| Backend    | .NET 8 Web API, MongoDB, JWT Auth, Clean Architecture, Modulith   |
| Frontend   | React 18, Vite, TypeScript, Tailwind CSS, Zustand, React Router 6 |
| Database   | MongoDB Atlas (NoSQL)                                             |
| Tools      | Visual Studio 2022, VS Code, Postman, Git                         |

## 🏗️ Kiến trúc hệ thống (Architecture)

Chúng tôi áp dụng triết lý thiết kế đồng bộ từ Backend đến Frontend:

1. **Backend (Modulith & Clean Architecture)**: Chia hệ thống thành các Module độc lập (Movies, Shops, Users, Feedbacks). Mỗi module sở hữu logic nghiệp vụ và cấu trúc dữ liệu riêng, giao tiếp qua các Interface để giảm thiểu sự phụ thuộc (Loose Coupling).
2. **Frontend (Feature-based Architecture)**: Cấu trúc thư mục phản chiếu trực tiếp các module của Backend. Mỗi tính năng (Feature) tự đóng gói logic API, UI components và hooks riêng biệt.

## 🌟 Tính năng nổi bật

- 🎬 **Đặt vé phim (Real-time Concurrency)**: Hệ thống xử lý đặc biệt cho trường hợp nhiều người cùng đặt một ghế tại một thời điểm (Atomic Updates), đảm bảo không xảy ra tình trạng "Double Booking".
- 🛒 **Quản lý Shop đa cấp**: Phân quyền chặt chẽ giữa Admin hệ thống và Manager từng cửa hàng.
- 💬 **Hệ thống Feedback Guest**: Cho phép khách vãng lai gửi phản hồi nhanh chóng mà không cần quy trình đăng nhập phức tạp.
- 🔐 **Bảo mật JWT**: Phân quyền truy cập dựa trên Role-based Access Control (RBAC).

## 📁 Cấu trúc thư mục tổng quát

```bash
ABCD-Mall-Solution/
├── backend/ # .NET 8 Modulith Source Code
│ ├── src/
│ │ ├── ABCD.WebAPI/ # Host chính
│ │ ├── Modules/ # Movies, Shops, Feedbacks, Users
│ │ └── Shared/ # Common logic & Mongo Context
│ └── ABCD-Mall.sln
│
├── frontend/ # React Vite TypeScript Source Code
│ ├── src/
│ │ ├── core/ # Global API & Auth logic
│ │ ├── features/ # Movies, Shops, Feedbacks (Feature-based)
│ │ └── components/ # Shared UI
│ └── package.json
```

## 🚀 Hướng dẫn cài đặt & Chạy dự án

### 1. Cài đặt Backend

```bash
   cd backend/ABCD.WebAPI
```

Cấu hình MongoDB Connection String trong appsettings.json

```bash
dotnet watchrun
```

### 2. Cài đặt Frontend

```bash
cd frontend
npm install
```

Cấu hình VITE_API_URL trong file .env

```bash
npm run dev
```

## 🔐 Tài khoản thử nghiệm (Demo Accounts)

| Vai trò      | Tài khoản               | Mật khẩu    | Quyền hạn           |
| ------------ | ----------------------- | ----------- | ------------------- |
| Admin        | admin@abcdmall.com      | Admin@123   | Toàn quyền hệ thống |
| Shop Manager | manager.shop01@abcd.com | Manager@123 | Quản lý Shop 01     |
| Guest        | N/A                     | N/A         | Đặt vé & Feedback   |

## 🤝 Thành viên thực hiện

- Group 2 - Project ABCD Mall
- Định hướng: Phát triển hệ thống quản lý trung tâm thương mại bền vững với kiến trúc Clean Code.

⭐ **Lưu ý**: Dự án này được thiết kế để xử lý các bài toán thực tế về tranh chấp dữ liệu (Concurrency) và tổ chức mã nguồn chuyên nghiệp cho các dự án lớn.
