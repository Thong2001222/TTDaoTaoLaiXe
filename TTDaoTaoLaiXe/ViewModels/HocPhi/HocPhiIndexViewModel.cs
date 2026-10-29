
using System;
using System.Collections.Generic;

namespace TTDaoTaoLaiXe.ViewModels.HocPhi
{
    public class HocPhiIndexViewModel
    {
        public string MaHocPhi { get; set; } = string.Empty;
        public string MaDangKy { get; set; } = string.Empty;
        public string MaHocVien { get; set; } = string.Empty;
        public string HoTenHocVien { get; set; } = string.Empty;
        public string TenKhoaHoc { get; set; } = string.Empty;

        public decimal TongHocPhi { get; set; }
        public decimal DaThu { get; set; }

        public decimal ConNo => Math.Max(0, TongHocPhi - DaThu);

        public DateOnly? HanThanhToan { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public string? GhiChu { get; set; }

        public int SoLanThanhToan { get; set; }
    }

    public class HocPhiIndexPageViewModel
    {
        public List<HocPhiIndexViewModel> DanhSach { get; set; } = new();

        public string TuKhoa { get; set; } = string.Empty;
        public string TrangThaiLoc { get; set; } = string.Empty;
        public DateOnly? TuNgay { get; set; }
        public DateOnly? DenNgay { get; set; }

        public decimal TongPhaiThu { get; set; }
        public decimal TongDaThu { get; set; }
        public decimal TongConNo { get; set; }
        public decimal TongQuaHan { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
    }
}
