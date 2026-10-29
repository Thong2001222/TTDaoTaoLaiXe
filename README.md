# TTDaoTaoLaiXe – Hệ thống quản lý trung tâm đào tạo lái xe ô tô

## Giới thiệu
TTDaoTaoLaiXe là website hỗ trợ quản lý hoạt động đào tạo lái xe ô tô, giúp số hóa nghiệp vụ quản lý học viên, lớp học, giáo viên, lịch học, học phí và theo dõi tiến độ đào tạo.

## Công nghệ sử dụng
- **Backend:** C#, ASP.NET Core MVC (.NET 8)
- **Database:** SQL Server, Entity Framework Core (Database First)
- **Frontend:** HTML5, CSS3, JavaScript, Bootstrap 5
- **Thư viện:** SweetAlert2, BCrypt.Net-Next, ClosedXML, QuestPDF
- **Thanh toán:** VNPay
- **IDE:** Visual Studio 2022

## Chức năng chính
- Quản lý tài khoản và phân quyền người dùng.
- Quản lý học viên, đăng ký khóa học và lớp học.
- Quản lý giáo viên, phòng học và phương tiện.
- Quản lý lịch học và điểm danh.
- Theo dõi tiến độ học tập và đào tạo DAT.
- Quản lý học phí, thanh toán và công nợ.
- Dashboard thống kê và báo cáo doanh thu, tình trạng học tập.
- Xuất dữ liệu Excel, PDF và hỗ trợ thanh toán trực tuyến VNPay.

## Cài đặt
1. Clone repository:
   ```bash
   git clone https://github.com/Thong2001222/TTDaoTaoLaiXe.git
   ```
2. Tạo hoặc khôi phục database `TTDaoTaoLaiXe` trên SQL Server.
3. Cấu hình chuỗi kết nối trong `appsettings.json`.
4. Mở solution bằng Visual Studio 2022 hoặc chạy:
   ```bash
   dotnet restore
   dotnet build
   dotnet run
   ```

## Tác giả
- **Lĩnh vực:** Công nghệ thông tin – Kỹ thuật phần mềm
- **Mục đích:** Đồ án tốt nghiệp
- **GitHub:** [TTDaoTaoLaiXe](https://github.com/Thong2001222/TTDaoTaoLaiXe)
