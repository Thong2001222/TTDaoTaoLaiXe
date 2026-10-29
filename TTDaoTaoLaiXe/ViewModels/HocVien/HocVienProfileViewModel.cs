
using TTDaoTaoLaiXe.Models;

namespace TTDaoTaoLaiXe.ViewModels.HocVien
{
    public class HocVienProfileViewModel
    {
        // ==========================================
        // THÔNG TIN HỌC VIÊN
        // ==========================================

        public string MaHocVien { get; set; } = null!;
        public List<string> NoiDungChuaHoanThanh { get; set; } = new();

        public string HoTen { get; set; } = null!;

        public DateOnly NgaySinh { get; set; }

        public string GioiTinh { get; set; } = null!;

        public string Cccd { get; set; } = null!;

        public DateOnly? NgayCapCccd { get; set; }

        public string? NoiCapCccd { get; set; }

        public string SoDienThoai { get; set; } = null!;

        public string? Email { get; set; }

        public string? DiaChi { get; set; }

        public string? AnhHocVien { get; set; }

        public bool TrangThai { get; set; }

        public string? GhiChu { get; set; }


        // ==========================================
        // ĐĂNG KÝ HỌC
        // ==========================================

        public List<DangKyHoc> DangKyHocs { get; set; } = new();
    }
}
