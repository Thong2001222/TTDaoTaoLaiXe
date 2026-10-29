
using System;

namespace TTDaoTaoLaiXe.ViewModels.HocPhi
{
    public class BienLaiHocPhiViewModel
    {
        public string MaThanhToan { get; set; } = string.Empty;
        public string MaHocPhi { get; set; } = string.Empty;
        public string MaDangKy { get; set; } = string.Empty;

        public DateTime NgayThanhToan { get; set; }

        public string MaHocVien { get; set; } = string.Empty;
        public string HoTenHocVien { get; set; } = string.Empty;
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }

        public string TenKhoaHoc { get; set; } = string.Empty;

        public decimal TongHocPhi { get; set; }
        public decimal DaThuTruocDo { get; set; }
        public decimal SoTienThu { get; set; }
        public decimal DaThuSauGiaoDich { get; set; }
        public decimal ConNo { get; set; }

        public string PhuongThuc { get; set; } = string.Empty;
        public string? MaGiaoDich { get; set; }
        public string? NguoiThu { get; set; }
        public string? GhiChu { get; set; }
    }
}
