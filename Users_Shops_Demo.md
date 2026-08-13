# Users + Shops Demo Flow

## 1. Mục tiêu của demo

Tài liệu này tóm tắt flow demo trên UI cho phần `Users` và các màn hình shop/rental đang đi kèm trong code hiện tại của worktree `CODE/.worktrees/users-phase1`.

Phạm vi bám theo đúng code đang có:
- đăng nhập, OTP login khi bị lock theo số lần nhập sai
- quên mật khẩu bằng OTP
- flow mới: đổi mật khẩu lần đầu qua link `change-initial-password`
- dashboard theo role
- profile manager/admin
- admin management
- create manager
- user management
- rental areas
- revenue statistics
- manager shop management
- shop rental information

Lưu ý quan trọng:
- Flow `Change Initial Password` đã có trên UI và backend, nhưng chỉ xuất hiện khi tài khoản đăng nhập trả về `requiresPasswordChange = true`.
- `Register` hiện tại vẫn là form tạo manager theo kiểu cũ, tức là admin nhập trực tiếp `password`, chưa phải form Phase 2 nhiều trường.
- `User Management` hiện tại vẫn là update/delete manager account, chưa phải bản inactive/activate/profile approval tab.
- `Profile` hiện tại đang lưu trực tiếp và ghi history; chưa phải bản approval workflow.

---

## 2. Route demo chính

### Authentication
- `/login`
- `/forgot-password`
- `/change-initial-password`
- `/dashboard`

### Admin
- `/admin-management`
- `/register`
- `/admin-management/users`
- `/admin-management/revenue`
- `/rental-areas`

### Manager
- `/profile`
- `/shop-info`
- `/manager-shops`

---

## 3. Flow demo đề xuất theo thứ tự

## Flow A. Demo đăng nhập cơ bản

### A1. Mở trang Login
- Vào `/login`.
- UI hiển thị form:
  - Email
  - Password
  - OTP chỉ hiện khi cần
  - link `Forgot password?`

### A2. Đăng nhập thành công
- Người dùng nhập email + password.
- Nếu backend trả login bình thường:
  - token được lưu vào localStorage
  - profile được gọi lại bằng `/Auth/getprofile`
  - role được lưu
  - hệ thống điều hướng sang `/dashboard`

### A3. Login OTP sau nhiều lần nhập sai
- Nếu backend trả `requiresOtp = true`, UI sẽ:
  - hiển thị message yêu cầu OTP
  - bật ô nhập OTP 6 chữ số
- Người dùng nhập OTP rồi nhấn `Sign In` lần nữa để hoàn tất đăng nhập.

### A4. Flow mới: Change Initial Password
- Đây là phần mới vừa cập nhật.
- Nếu backend trả:
  - `requiresPasswordChange = true`
  - `passwordSetupToken = ...`
- UI sẽ không vào dashboard ngay mà redirect sang:
  - `/change-initial-password?token=...`

---

## Flow B. Demo đổi mật khẩu lần đầu

### B1. Mở trang Change Initial Password
- Route: `/change-initial-password?token=...`
- UI hiển thị:
  - New Password
  - Confirm Password
  - nút `Change Password`
  - link `Back to Login`

### B2. Validation trên UI
- Token rỗng: báo lỗi `Invalid or missing password setup token.`
- Password ngắn hơn 6 ký tự: báo lỗi.
- Confirm password không khớp: báo lỗi.

### B3. Submit thành công
- UI gọi `POST /Auth/initial-password/change`.
- Nếu thành công:
  - hiển thị `Password changed successfully. Please sign in again.`
  - xóa `token`, `refreshToken`, `role`, `profile` khỏi localStorage
  - quay về `/login`

### Cách demo hợp lý
- Dùng tài khoản đã được backend gắn `MustChangePassword`.
- Đăng nhập ở `/login`.
- Cho người xem thấy hệ thống tự redirect sang `/change-initial-password`.
- Đổi password xong, quay về login và đăng nhập lại bình thường.

---

## Flow C. Demo quên mật khẩu bằng OTP

### C1. Mở trang Forgot Password
- Từ `/login`, bấm `Forgot password?`
- Vào `/forgot-password`

### C2. Bước gửi OTP
- Người dùng nhập:
  - email
  - new password
  - confirm password
- Nhấn `Send OTP`
- UI gọi `POST /Auth/forgotpassword/request-otp`

### C3. Bước xác nhận OTP
- Sau khi gửi OTP thành công:
  - UI chuyển sang bước nhập OTP
  - người dùng nhập mã 6 số
- Nhấn `Confirm OTP`
- UI gọi `POST /Auth/forgotpassword/confirm-otp`

### C4. Kết quả
- Thành công:
  - hiện thông báo reset password thành công
  - tự redirect về `/login`
- Sai OTP:
  - hiện lỗi `Invalid OTP.`

---

## Flow D. Dashboard theo role

### D1. Dashboard sau login
- Route: `/dashboard`
- UI đọc `role` từ localStorage để hiển thị quyền hiện tại.

### D2. Với Manager
- Có các hướng chính:
  - `Open Profile`
  - `Shop Info`
  - `Manage My Shop`

### D3. Với Admin
- Có hướng:
  - `Open Profile`
  - `Admin Management`

### Điểm demo
- Đăng nhập bằng admin và manager để cho thấy dashboard khác nhau theo role.

---

## Flow E. Demo Profile

### E1. Mở Profile
- Route: `/profile`
- UI hiển thị:
  - avatar
  - full name
  - email
  - role
  - address
  - CCCD
  - form chỉnh sửa

### E2. Cập nhật profile
- Người dùng có thể sửa:
  - full name
  - address
  - avatar
  - CCCD
- Nhấn `Save Profile`
- UI gửi `FormData` lên `PUT /Auth/updateprofile`

### E3. Profile update history
- UI gọi `/Auth/profile-update-history`
- Bên dưới có khối `History`
- Hiển thị các thay đổi trước và sau cho:
  - Full Name
  - Address
  - Avatar/Image
  - CCCD

### E4. Đổi mật khẩu khi đã đăng nhập
- Trong trang Profile có khu vực `Change Password`
- Flow:
  - nhập password mới
  - bấm `Send OTP`
  - backend gửi OTP qua email
  - nhập OTP
  - bấm `Confirm OTP`

### Lưu ý trạng thái hiện tại
- Ở code hiện tại, profile update đang lưu trực tiếp và ghi lịch sử.
- Chưa có khu vực pending approval/review request trên UI ở branch này.

---

## Flow F. Demo Admin Management

### F1. Mở Admin Management
- Route: `/admin-management`
- Chỉ admin mới vào được.
- UI đóng vai trò như trang hub điều hướng cho admin.

### F2. Các khối chức năng hiện có
- `Rental Areas`
- `User Management`
- `Revenue Statistics`
- `Create Manager`

### Cách demo
- Sau khi đăng nhập admin:
  1. vào `Admin Management`
  2. giải thích đây là trang gom toàn bộ thao tác vận hành của admin
  3. đi lần lượt sang từng khối

---

## Flow G. Demo Create Manager

### G1. Mở trang Register
- Route: `/register`
- Có thể đi từ `Admin Management > Create Manager`

### G2. Thông tin hiện tại cần nhập
- Full Name
- Email
- Shop Name
- CCCD
- Password
- Confirm Password

### G3. Submit
- Bấm `Create User`
- UI gọi `POST /Auth/register`

### G4. Kết quả
- Thành công:
  - hiện thông báo tạo user thành công
  - nếu email gửi được thì báo đã gửi notification email
- Thất bại:
  - hiện lỗi trả về từ backend

### Lưu ý trạng thái hiện tại
- Form này vẫn là bản register đơn giản.
- Chưa có:
  - map slot
  - floor
  - lease term
  - electricity/water/service fee
  - upload CCCD front/back
  - upload contract

---

## Flow H. Demo User Management

### H1. Mở trang User Management
- Route: `/admin-management/users`
- Chỉ admin mới vào được.

### H2. Nội dung chính của trang
- Thống kê:
  - tổng users
  - số admin
  - số manager
- ô search theo:
  - email
  - name
  - shop
  - role
  - CCCD

### H3. Bảng user
- Hiển thị:
  - Email
  - Role
  - Name
  - Shop
  - CCCD
  - Address
  - Failed login attempts
  - Created date

### H4. Update manager account
- Với manager, có nút `Update`
- Mở modal chỉnh:
  - email
  - full name
  - shop name
  - CCCD
  - address
- Bấm save để gọi `PUT /Auth/users/{id}`

### H5. Delete manager account
- Với manager, có nút `Delete`
- Mở modal confirm delete
- Bấm confirm để gọi `DELETE /Auth/users/{id}`

### H6. Giới hạn
- Admin account chỉ xem, không hiện thao tác delete như manager.

### Lưu ý trạng thái hiện tại
- Đây vẫn là flow xóa manager account theo bản cũ.
- Chưa có các tab:
  - Active accounts
  - Inactive accounts
  - Profile approval

---

## Flow I. Demo Rental Areas

### I1. Mở trang Rental Areas
- Route: `/rental-areas`
- Chỉ admin mới vào được.

### I2. Thao tác 1: Add Rental Area
- Admin nhập:
  - area code
  - floor
  - area name
  - size
  - monthly rent
- Bấm `Add Area`

### I3. Thao tác 2: Register tenant cho area
- Trong bảng area, với area `Available`, bấm `Register Tenant`
- Mở modal `Register Rental`
- Flow trong modal:
  - nhập CCCD
  - bấm check manager
  - hệ thống tự load `Manager Name` và `Shop Name`
  - nhập:
    - location
    - start date
    - electricity fee
    - water fee
    - service fee
    - rental duration
    - contract image
  - bấm `Submit Rental`

### I4. Thao tác 3: Update monthly usage
- Với area đã thuê, bấm `Update Monthly Usage`
- Mở modal
- Admin nhập:
  - usage month
  - electricity usage
  - water usage
- Hệ thống reuse fee từ thông tin rental đã đăng ký.

### I5. Thao tác 4: Cancel tenant
- Với area đã thuê, admin bấm `Cancel Tenant`
- Hệ thống hủy rental hiện tại.

### Cách demo hợp lý
- Tạo 1 area mới
- Gắn 1 manager vào area đó
- Cập nhật monthly usage
- Sau đó sang `Revenue Statistics` để xem dữ liệu vừa tạo

---

## Flow J. Demo Revenue Statistics

### J1. Mở trang Revenue Statistics
- Route: `/admin-management/revenue`
- Chỉ admin mới vào được.

### J2. Dữ liệu và filter
- Trang lấy dữ liệu rental details
- Có bộ lọc:
  - Year
  - Month
  - Location

### J3. Khối tổng quan
- Hiển thị tổng số record đang lọc
- hiển thị tổng các khoản:
  - electricity
  - water
  - fee
  - total revenue

### J4. Bảng chi tiết
- Hiển thị theo từng rental bill:
  - Location
  - Billing Month
  - Usage Month
  - các khoản phí
  - Total

### Cách demo
- Sau khi admin cập nhật monthly usage ở `Rental Areas`, chuyển sang đây để cho thấy báo cáo thay đổi theo filter.

### Lưu ý trạng thái hiện tại
- Bản hiện tại là revenue từ rental data nói chung.
- Chưa phải bản mới có tách `Paid` và `Unpaid`.

---

## Flow K. Demo Shop Info cho Manager

### K1. Mở Shop Info
- Route: `/shop-info`
- Chỉ manager mới vào được.

### K2. Nội dung hiện tại
- Trang hiển thị bảng rental information của shop manager
- Thông tin chính:
  - Shop Name
  - Location
  - Billing Month
  - Usage Month
  - Start Date
  - Electricity Usage
  - Electricity Fee
  - Water Usage
  - Water Fee
  - Fee
  - Lease Term
  - Total Due
  - Contract

### K3. Xem contract
- Bấm `View` trong cột `Contract`
- Mở modal xem ảnh contract

### Lưu ý trạng thái hiện tại
- Trang này hiện là `Rental Payment Overview` theo nghĩa xem dữ liệu rental.
- Chưa có nút thanh toán online trong branch hiện tại.

---

## Flow L. Demo Manager Shop Management

### L1. Mở Manager Shops
- Route: `/manager-shops`
- Chỉ manager mới dùng thực tế.

### L2. Mục tiêu màn hình
- Manager tự quản lý public shop page của mình.
- Có thể:
  - tạo shop page
  - sửa shop page
  - xóa shop page

### L3. Khối create/update form
- Các trường chính:
  - shop name
  - slug
  - category
  - floor
  - location slot
  - summary
  - description
  - open hours
  - badge
  - offer
  - tags

### L4. Upload ảnh
- Logo image
- Cover image

### L5. Featured products
- Manager có thể thêm nhiều product:
  - product name
  - price
  - old price
  - discount percent
  - product image
  - featured / discounted flags

### L6. Save shop
- Nếu đang tạo mới: `Create Shop`
- Nếu đang sửa: `Update Shop`

### L7. Danh sách shop đã sở hữu
- Bên dưới có khu `Owned shop pages`
- Mỗi item có:
  - preview ảnh cover
  - tên shop
  - slug
  - floor, location
  - summary
  - link public `/shops/{slug}`
  - nút edit
  - nút delete

### L8. Quota shop creation
- UI có hiển thị quota:
  - số shop pages đã tạo
  - số rented areas
- Nếu quota đã đầy, form tạo shop sẽ bị khóa.

---

## 4. Kịch bản demo ngắn gọn đề xuất

## Option 1. Demo từ góc nhìn Admin
1. Đăng nhập admin ở `/login`
2. Vào `/dashboard`
3. Vào `/admin-management`
4. Mở `/register` để tạo manager
5. Mở `/admin-management/users` để xem manager vừa tạo
6. Mở `/rental-areas` để tạo area và gắn manager vào area
7. Cập nhật monthly usage
8. Mở `/admin-management/revenue` để xem dữ liệu doanh thu/rental

## Option 2. Demo từ góc nhìn Manager
1. Đăng nhập manager
2. Nếu account có cờ bắt buộc đổi mật khẩu, cho chạy flow `/change-initial-password`
3. Vào `/dashboard`
4. Vào `/profile` để sửa profile và xem history
5. Vào `/shop-info` để xem rental bill và contract
6. Vào `/manager-shops` để tạo hoặc sửa public shop page
7. Mở public page `/shops/{slug}` để kết thúc demo

## Option 3. Demo đầy đủ end-to-end
1. Admin tạo manager
2. Admin gắn manager vào rental area
3. Manager login
4. Manager cập nhật profile
5. Manager tạo public shop page
6. Admin xem revenue

---

## 5. Điểm nhấn nên nói khi thuyết trình

- Hệ thống có phân quyền rõ giữa `Admin` và `Manager`.
- Users module không chỉ là login/register mà còn kéo theo:
  - profile
  - password management
  - tenant/rental registration
  - revenue reporting
  - public shop management
- Flow mới vừa cập nhật là `Change Initial Password`, giúp hỗ trợ onboarding tài khoản được provision sẵn.
- Các màn hình admin và manager đã được tách khá rõ theo vai trò và đường đi dashboard.

---

## 6. Ghi chú trạng thái code hiện tại

Để tránh demo sai so với code:
- Có:
  - login OTP
  - forgot password OTP
  - change initial password
  - direct profile update + history
  - create/update/delete manager account
  - rental area registration
  - rental revenue statistics
  - manager public shop page management

- Chưa ở branch này:
  - profile approval workflow trên UI
  - inactive/activate tabs trong user management
  - register manager bằng one-time password flow
  - payment online cho rental bill
  - paid/unpaid revenue split

