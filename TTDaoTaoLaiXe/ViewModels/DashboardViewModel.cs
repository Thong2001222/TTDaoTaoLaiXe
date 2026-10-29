
namespace TTDaoTaoLaiXe.ViewModels
{
    public class DashboardViewModel
    {
        public int TongHocVien { get; set; }
        public int HocVienDangHoatDong { get; set; }
        public int TongKhoaHoc { get; set; }
        public int KhoaHocDangHoatDong { get; set; }
        public int TongLopHoc { get; set; }
        public int LopHocDangHoatDong { get; set; }
        public int TongGiaoVien { get; set; }
        public int GiaoVienDangHoatDong { get; set; }
        public int LichHocSapToi { get; set; }

        public decimal TongHocPhi { get; set; }
        public decimal TongDaThu { get; set; }
        public decimal TongConNo { get; set; }

        public List<string> NhanThang { get; set; } = new();
        public List<int> SoHocVienDangKy { get; set; } = new();
        public List<decimal> DoanhThuTheoThang { get; set; } = new();

        public List<string> NhanTrangThaiHocVien { get; set; } = new();
        public List<int> SoLuongTrangThaiHocVien { get; set; } = new();

        public List<string> NhanTrangThaiLopHoc { get; set; } = new();
        public List<int> SoLuongTrangThaiLopHoc { get; set; } = new();

        public List<string> NhanDiemDanh { get; set; } = new();
        public List<int> SoLuongDiemDanh { get; set; } = new();

        public List<string> NhanTrangThaiHocPhi { get; set; } = new();
        public List<decimal> SoLuongTrangThaiHocPhi { get; set; } = new();
    }
}
