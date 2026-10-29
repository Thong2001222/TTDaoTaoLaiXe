
using System;
using System.Collections.Generic;

namespace TTDaoTaoLaiXe.ViewModels.HocPhi
{
    public class HocPhiDetailsViewModel
    {
        public string MaHocPhi { get; set; } = string.Empty;
        public string MaDangKy { get; set; } = string.Empty;

        public string MaHocVien { get; set; } = string.Empty;
        public string HoTenHocVien { get; set; } = string.Empty;
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }

        public string TenKhoaHoc { get; set; } = string.Empty;
        public decimal TongHocPhi { get; set; }
        public decimal DaThu { get; set; }

        public decimal ConNo => Math.Max(0m, TongHocPhi - DaThu);

        public DateOnly? HanThanhToan { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public string? GhiChu { get; set; }

        public List<ThanhToanHocPhiItemViewModel> LichSuThanhToan
        {
            get;
            set;
        } = new();
    }

    public class ThanhToanHocPhiItemViewModel
    {
        public string MaThanhToan { get; set; } = string.Empty;
        public DateTime NgayThanhToan { get; set; }
        public decimal SoTien { get; set; }
        public string PhuongThuc { get; set; } = string.Empty;
        public string? MaGiaoDich { get; set; }
        public string? NguoiThu { get; set; }
        public string? GhiChu { get; set; }
    }
}
