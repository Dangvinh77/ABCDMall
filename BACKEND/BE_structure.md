# 🏬 ABCD Mall Solution

A **Modulith Architecture** based backend system for managing a shopping mall, including movie booking, shop management, and user authentication.

---

## 🚀 Tech Stack

- ⚙️ ASP.NET Core Web API
- 🛢️ SQL Server
- 🔐 JWT Authentication
- 🧩 Modulith Architecture (Modular Monolith)

---

## 📁 BE Project Structure

```
ABCD-Mall-Modulith/
├── src/
│   ├── ABCD.WebAPI/                # Dự án Host chính (ASP.NET Core)
│   │
│   ├── Modules/                    # CÁC MODULE TÁCH BIỆT (Mỗi folder là logic riêng)
│   │   ├── ABCD.Modules.Movies/    # MODULE ĐẶT VÉ
│   │   │   ├── Domain/             # Movie, Showtime, Seat
│   │   │   ├── Application/        # Logic đặt vé Guest & Xử lý trùng ghế
│   │   │   └── Infrastructure/     # Database,Interface, Repository của Movies
│   │   │
│   │   ├── ABCD.Modules.Shops/     # MODULE CỬA HÀNG (Dành cho Shop Manager)
│   │   │   ├── Domain/             # Shop, Product, FoodCourt
│   │   │   ├── Application/        # Logic quản lý thông tin & sản phẩm
│   │   │   └── Infrastructure/     # Database,Interface, Repository của Shops
│   │   │
│   │   ├── ABCD.Modules.Feedbacks/ # MODULE FEEDBACK (Dùng chung cho cả Mall)
│   │   │   ├── Domain/             # Feedback Entity
│   │   │   ├── Application/        # Logic nhận Feedback từ Guest
│   │   │   └── Infrastructure/     # Database,Interface, Repository của Feedbacks
│   │   │
│   │   └── ABCD.Modules.Users/     # MODULE AUTH (Admin & Shop Manager)
│   │
│   └── Shared/                     # DÙNG CHUNG TOÀN HỆ THỐNG
│       └── ABCD.Shared/            # AppDbContext, BaseEntity, JWT Helper, DTOs chung
```

---

## 🧠 Architecture

This project follows a **Modular Monolith (Modulith)** architecture:

- Each module is **independent**
- Each module has 3 layers:
  - **Domain** → Business entities & interfaces
  - **Application** → Business logic
  - **Infrastructure** → Database & external services
- Modules communicate through **interfaces (loose coupling)**

---

## 🎯 Features

### 🎬 Movie Booking

- Manage movies & showtimes
- Seat booking system
- Handle **concurrent booking (avoid double booking)**

### 🛒 Shop Management

- Manage shops in mall
- Manage products per shop

### 👤 User & Auth

- Register / Login
- JWT Authentication
- Role-based access (Admin/User)

---

## ⚙️ Setup & Run

### 1️⃣ Clone project

```bash
git clone https://github.com/Dangvinh77/ABCDMall.git
cd ABCD-Mall-Solution
```

2️⃣ Configure MongoDB

Update connection string in: **appsettings.json**

Example:

```bash
"ConnectionStrings": {
  "ConnectDB": "Server=<server_name>;Database=ABCDMall;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
},
```

3️⃣ Run project

```bash
cd src/ABCD.WebAPI
dotnet watchrun
```

🔐 API Authentication

Use JWT token in header:

```bash
Authorization: Bearer <your_token>
```

📌 Future Improvements

- 🧾 Order & Payment module
- 📊 Analytics dashboard
- 🔔 Notification system (email / push)
- 🧵 Event-driven (RabbitMQ / Kafka)

👨‍💻 Author: Group 2

⭐ Notes

This project is designed for:

Learning Modulith Architecture
Handling concurrency problems
Practicing clean architecture in .NET
