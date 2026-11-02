# Hướng dẫn cài đặt Nha Khoa G12

## 1. Yêu cầu môi trường

- MySQL Server 8.0 và MySQL Workbench.
- .NET SDK 10.0.
- Node.js 20.19 trở lên hoặc 22.12 trở lên, kèm npm.
- Hai cổng `7184` (backend HTTPS) và `5173` (frontend) chưa bị chương trình khác sử dụng.

## 2. Tạo cơ sở dữ liệu

Trong MySQL Workbench, kết nối đến MySQL Server và nhập file `nha_khoa_g12.sql` bằng **Server → Data Import → Import from Self-Contained File → Start Import**. Nhập trên MySQL mới hoặc database chưa có các bảng của dự án.

File SQL tự tạo database `nha_khoa_g12`, 19 bảng nghiệp vụ và bốn tài khoản demo. Không cần chạy thêm `dotnet ef database update` sau khi nhập file này.

## 3. Cấu hình backend

Trong Visual Studio, nhấp phải project backend và chọn **Manage User Secrets**. Điền nội dung sau vào `secrets.json`, thay `TEN_USER_MYSQL` và `MAT_KHAU_MYSQL` bằng thông tin MySQL trên máy cài đặt:

```json
{
  "ConnectionStrings:NhaKhoa": "Server=localhost;Port=3306;Database=nha_khoa_g12;User ID=TEN_USER_MYSQL;Password=MAT_KHAU_MYSQL",
  "Jwt:Key": "OTyBUlvhl97731e+5URaDCDNYWs++XVNVwI0gHKpdIc="
}
```

Tên database phải là `nha_khoa_g12` để khớp db trong file SQL. Khóa JWT trên là khóa dùng cho bản demo; mật khẩu của bốn tài khoản đã được lưu dưới dạng hash trong database.

Sau khi cấu hình, mở terminal tại thư mục gốc của dự án và chạy backend:

```powershell
cd backend/BTL_NhaKhoa/BTL_NhaKhoa
dotnet restore
dotnet dev-certs https --trust
dotnet run --launch-profile https
```

## 4. Cài đặt và chạy frontend

Mở terminal thứ hai tại thư mục gốc của dự án:

```powershell
cd frontend/BTL_NhaKhoa
npm ci
npm run dev
```

Mở http://localhost:5173 và giữ cả hai terminal(frontend và backend) chạy.

## 5. Tài khoản demo

| Vai trò    | Số điện thoại | Mật khẩu      |
| ---------- | ------------- | ------------- |
| Quản lý    | 0900000002    | quanly1234    |
| Lễ tân     | 0900000003    | letan1234     |
| Bác sĩ     | 0900000004    | bacsi1234     |
| Khách hàng | 0900000005    | khachhang1234 |

## 6. Lưu ý khi cài đặt

- Nếu MySQL sử dụng cổng khác `3306`, sửa `Port` trong connection string.
- Nếu thay đổi cổng backend, sửa `VITE_API_PROXY_TARGET` trong cấu hình môi trường của frontend cho khớp.
