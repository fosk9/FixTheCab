# PMP Racing - Hệ Thống Quản Lý Tiệm Sửa Xe Chuyên Nghiệp

PMP Racing là ứng dụng quản lý gara/tiệm sửa xe máy được xây dựng trên nền tảng **ASP.NET Core 8 MVC**. Hệ thống hỗ trợ quản lý toàn diện quy trình từ tiếp nhận xe, phân công thợ, thực hiện sửa chữa cho đến thanh toán và báo cáo doanh thu.

---

## 🛠️ Công Nghệ Sử Dụng
- **Backend:** .NET 8 (ASP.NET Core MVC).
- **Database:** SQL Server + Entity Framework Core 8.
- **Frontend:** HTML5, CSS3, JavaScript (Vanilla + jQuery).
- **UI Framework:** Bootstrap 5.
- **Biểu đồ:** Chart.js 4.x.
- **Bảo mật:** Cookie Authentication, BCrypt Password Hashing.

---

## 🚀 Các Tính Năng Chính Theo Vai Trò

### 1. Phân Hệ Quản Lý (Manager)
- **Dashboard Doanh Thu:** Biểu đồ đường (Line Chart) trực quan theo dõi doanh thu trong 30 ngày gần nhất.
- **Quản Lý Kho Phụ Tùng:** Theo dõi số lượng tồn kho, cập nhật mức tồn và cảnh báo khi sắp hết hàng.
- **Lịch Làm Việc & Phân Công:** Quản lý ca làm việc của nhân viên và thực hiện giao xe cho Thợ máy dựa trên năng suất hiện tại.
- **Duyệt Nghỉ Phép:** Xem xét và phê duyệt đơn xin nghỉ từ nhân viên.

### 2. Phân Hệ Thợ Máy (Mechanic)
- **Bảng Điều Khiển Công Việc:** Xem danh sách xe vừa được bàn giao, chuyển trạng thái "Bắt đầu làm".
- **Chi Tiết Sửa Chữa:** Thêm linh kiện từ kho, thêm các hạng mục dịch vụ sửa chữa vào đơn hàng.
- **Hoàn Tất & Chuyển Thu Ngân:** Tính toán tổng tiền và hoàn tất phiếu để Thu ngân có thể xuất hóa đơn.
- **Quản Lý Cá Nhân:** Xem lịch làm việc cá nhân và đăng ký nghỉ phép.

### 3. Phân Hệ Thu Ngân (Cashier)
- **Tiếp Nhận Xe:** Ghi nhận thông tin xe khách hàng vào hệ thống.
- **Xử Lý Thanh Toán:** Hỗ trợ thanh toán tiền mặt và quét mã QR (giả lập PayOS).
- **Quản Lý Hóa Đơn:** Theo dõi trạng thái các hóa đơn từ chờ xử lý đến đã thanh toán.

### 4. Phân Hệ Quản Trị (Admin)
- Quản lý danh sách tài khoản, phân quyền vai trò (Admin, Manager, Cashier, Mechanic).

---

## ⚙️ Hướng Dẫn Cài Đặt & Chạy Dự Án

### Bước 1: Thiết lập Cơ sở dữ liệu
1. Mở SQL Server Management Studio (SSMS).
2. Chạy duy nhất file script nằm trong thư mục gốc của dự án:
   - `PMP_Full_Setup.sql`: Tự động xóa/tạo mới Database và nạp đầy đủ cấu trúc + dữ liệu mẫu (12+ dòng/bảng, 31 ngày doanh thu).

### Bước 2: Cấu hình Connection String
Mở file `appsettings.json` và cập nhật chuỗi kết nối tới Server của bạn:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=PmpRacing;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### Bước 3: Chạy ứng dụng
SỬ dụng terminal tại thư mục dự án và chạy lệnh:
```bash
dotnet build
dotnet run
```
Truy cập địa chỉ mặc định: `http://localhost:5043`

---

## 🧪 Tài Khoản & Luồng Kiểm Thử Nghiệp Vụ

### Tài khoản đăng nhập (Mật khẩu mặc định: `123`)
- **Admin:** `admin@pmpracing.vn`
- **Quản lý:** `manager@pmpracing.vn`
- **Thợ máy:** `phuoc@pmpracing.vn` `<tên-thợ-máy>@pmpracing.vn`
- **Thu ngân:** `cashier1@pmpracing.vn` `cashier2@pmpracing.vn`

### Quy trình kiểm thử đề xuất (End-to-End)
1. **Tiếp nhận:** Dùng role Thu ngân tạo xe mới.
2. **Giao việc:** Dùng role Quản lý giao xe đó cho Thợ máy `Tuấn`.
3. **Thực hiện:** Dùng role Thợ máy nhấn "Bắt đầu làm", thêm phụ tùng và nhấn "Hoàn tất".
4. **Thanh toán:** Dùng role Thu ngân lập hóa đơn và thực hiện thanh toán QR.
5. **Báo cáo:** Dùng role Quản lý kiểm tra doanh thu nhảy số trên biểu đồ Dashboard.

---
*Dự án được phát triển bởi sinh viên Đại Học FPT - Môn học PRN222 - Nhóm 3.*
