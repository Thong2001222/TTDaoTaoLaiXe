
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TTDaoTaoLaiXe.ViewModels
{
    public class DiemDanhIndexViewModel
    {
        public List<DiemDanhLichHocItemViewModel> LichHocs { get; set; } = new();

        public List<SelectListItem> DanhSachLopHoc { get; set; } = new();

        public string? TuKhoa { get; set; }
        public string? MaLopHoc { get; set; }
        public DateOnly? NgayTu { get; set; }
        public DateOnly? NgayDen { get; set; }

        public int TrangHienTai { get; set; } = 1;
        public int TongTrang { get; set; }
        public int TongSoBanGhi { get; set; }
    }

    public class DiemDanhLichHocItemViewModel
    {
        public string MaLichHoc { get; set; } = string.Empty;
        public string MaLopHoc { get; set; } = string.Empty;
        public string TenLopHoc { get; set; } = string.Empty;
        public string TenGiaoVien { get; set; } = string.Empty;

        public DateOnly NgayHoc { get; set; }
        public TimeOnly GioBatDau { get; set; }
        public TimeOnly GioKetThuc { get; set; }
        public string LoaiBuoiHoc { get; set; } = string.Empty;
        public string TrangThaiLichHoc { get; set; } = string.Empty;

        public int TongHocVien { get; set; }
        public int DaDiemDanh { get; set; }
    }

    public class DiemDanhRosterViewModel
    {
        public string MaLichHoc { get; set; } = string.Empty;
        public string MaLopHoc { get; set; } = string.Empty;
        public string TenLopHoc { get; set; } = string.Empty;

        public DateOnly NgayHoc { get; set; }
        public TimeOnly GioBatDau { get; set; }
        public TimeOnly GioKetThuc { get; set; }
        public decimal SoGioBuoiHoc { get; set; }

        public string LoaiBuoiHoc { get; set; } = string.Empty;
        public string TrangThaiLichHoc { get; set; } = string.Empty;

        public List<DiemDanhHocVienViewModel> HocViens { get; set; } = new();
    }

    public class DiemDanhHocVienViewModel
    {
        public string MaHocVien { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn trạng thái điểm danh.")]
        public string TrangThai { get; set; } = "Có mặt";

        [Range(typeof(decimal), "0", "9999",
            ErrorMessage = "Số giờ tham gia không hợp lệ.")]
        public decimal GioThamGia { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự.")]
        public string? GhiChu { get; set; }

        public bool DaCoDiemDanh { get; set; }
    }
}
