# Admin Control Center - Frontend

## Overview

Giao diện Admin Control Center cho phép quản lý người dùng (managers) và cấp phát slots (vị trí cửa hàng) trên sàn nhà.

## Features

### 1. User Management (`/admin/users`)
Quản lý tài khoản người dùng, đặc biệt là tài khoản Manager.

**Chức năng:**
- ✅ Tạo tài khoản Manager mới
  - Email, mật khẩu, tên đầy đủ, tên cửa hàng, số CCCD
  - Optional: Chọn slot trên bản đồ ngay khi tạo
- ✅ Xem danh sách tất cả users với vai trò (Admin, Manager, Customer)
- ✅ Xóa tài khoản user
- ⏳ Edit tài khoản (ready to implement)

**API Endpoints:**
```
POST   /api/auth/register          - Tạo tài khoản Manager
GET    /api/auth/users             - Lấy danh sách users
DELETE /api/auth/users/{id}        - Xóa user
PUT    /api/auth/users/{id}        - Update user (sẵn sàng)
```

### 2. Slot Management (`/admin/maps`)
Quản lý các slot (vị trí) cửa hàng trên sàn nhà.

**Chức năng:**
- ✅ Xem floor plan của từng tầng (1-5)
- ✅ Xem trạng thái slots:
  - **Available** (Trắng) - Sẵn sàng để cấp
  - **Reserved** (Vàng) - Đã cấp cho manager
  - **ComingSoon** (Xanh) - Chưa mở bán
  - **Active** (Xanh lá) - Đang hoạt động
- ✅ Gán slot cho Manager (nếu Manager đã tạo)
  - Cần ShopInfo ID của Manager
- ✅ Giải phóng slot (huỷ cấp)

**API Endpoints:**
```
GET    /api/map/floors/admin                    - Lấy tất cả floor plans
GET    /api/map/floors/admin/{floorLevel}      - Lấy floor plan cụ thể
PUT    /api/map/floors/locations/{id}/reserve  - Gán slot
PUT    /api/map/floors/locations/{id}/release  - Giải phóng slot
```

## File Structure

```
src/features/admin/
├── pages/
│   ├── AdminShell.tsx              - Layout chính với sidebar
│   ├── AdminDashboardPage.tsx      - Dashboard overview
│   ├── UserManagementPage.tsx      - Quản lý users
│   └── MapSlotManagementPage.tsx   - Quản lý slots
├── routes/
│   ├── AdminRoutes.tsx             - React Router config
│   └── adminPaths.ts               - Path constants
├── services/
│   └── adminApi.ts                 - API calls & types
└── components/                      - (để mở rộng sau)
```

## Usage Workflow

### Workflow 1: Tạo Manager & Gán Slot (1 bước)
```
1. Vào /admin/users
2. Click "New Manager"
3. Điền thông tin: Email, Password, Full Name, Shop Name, CCCD
4. OPTIONAL: Chọn MapLocationId từ maps page trước
5. Click "Create Manager"
6. Nhận ShopInfo ID từ response
7. DONE ✅ (Nếu chọn slot, nó sẽ tự gán)
```

### Workflow 2: Tạo Manager & Gán Slot Sau (2 bước)
```
1. Vào /admin/users → Tạo Manager (không chọn slot)
2. Nhận ShopInfo ID
3. Vào /admin/maps → Chọn floor
4. Click vào slot muốn gán
5. Click "Assign Slot" → Paste ShopInfo ID → Click "Assign"
6. DONE ✅
```

### Workflow 3: Giải phóng Slot
```
1. Vào /admin/maps → Chọn floor
2. Click vào slot (status = Reserved hoặc Active)
3. Click "Release Slot"
4. Confirm
5. DONE ✅ (Slot quay về Available)
```

## Data Flow

```
┌─────────────────────────────────────────────────────────────┐
│                    ADMIN FRONTEND                           │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  User Management           Map Slot Management               │
│  ┌──────────────┐         ┌──────────────────┐               │
│  │ Create User  │         │ View Floor Plan  │               │
│  │ List Users   │         │ Assign Slot      │               │
│  │ Delete User  │         │ Release Slot     │               │
│  └──────┬───────┘         └────────┬─────────┘               │
│         │                          │                         │
│         ├──────────────────────────┤                         │
│         │ API Calls to Backend     │                         │
│         ▼                          ▼                         │
├─────────────────────────────────────────────────────────────┤
│                    BACKEND API                               │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  Auth Module              UtilityMap Module                  │
│  ┌──────────────┐         ┌──────────────────┐               │
│  │ Register     │         │ Get Floor Plans  │               │
│  │ Get Users    │         │ Reserve Slot     │               │
│  │ Delete User  │         │ Release Slot     │               │
│  └──────────────┘         └──────────────────┘               │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

## Key Types

### User Types
```typescript
interface UserSummary {
  id: string;
  email: string;
  fullName: string;
  role: "Admin" | "Manager" | "Customer";
  createdAt: string;
}
```

### Map Types
```typescript
interface MapLocationAdmin {
  id: number;
  shopName: string;
  locationSlot: string;
  status: "Available" | "Reserved" | "ComingSoon" | "Active";
  shopInfoId?: string;  // ShopInfo.Id của Manager (nếu đã assign)
  x: number;
  y: number;
  storefrontImageUrl: string;
}

interface FloorPlanAdmin {
  id: number;
  floorLevel: string;
  blueprintImageUrl: string;
  locations: MapLocationAdmin[];
}
```

## Integration Points

### 1. Authentication
- Admin routes protected by `Authorize(Roles = "Admin")` in backend
- Frontend uses localStorage token
- Token validation happening in API calls

### 2. Navigation
- Added to main AppRoutes.tsx
- Admin Shell contains sidebar with navigation
- Can access from: `/admin` hoặc `/admin/dashboard`

### 3. Styling
- Using Tailwind CSS (consistent with project)
- Dark theme (gradient backgrounds, similar to movies-admin)
- Responsive design (mobile-friendly)

## Future Enhancements

- [ ] Edit user account details
- [ ] Batch assign slots
- [ ] User activity logs
- [ ] Export user/slot data to CSV
- [ ] Advanced filters & search
- [ ] Audit trail for all admin actions
- [ ] User role management (create custom roles)
- [ ] Floor plan customization

## Error Handling

- All API errors displayed in alert/toast
- Form validation on client-side
- Server validation errors displayed to user
- Network errors handled gracefully

## Security

- ✅ Authorization: `Authorize(Roles = "Admin")` in backend
- ✅ Token in localStorage with JWT
- ✅ CORS enabled (if needed)
- 🔄 TODO: Add role-based access control in frontend (optional UI enhancement)
