using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TTDaoTaoLaiXe.Models;

[Table("TaiKhoan")]
[Index("TenDangNhap", Name = "UQ_TaiKhoan_TenDangNhap", IsUnique = true)]
public partial class TaiKhoan
{
    [Key]
    [StringLength(20)]
    [Unicode(false)]
    public string MaTaiKhoan { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string TenDangNhap { get; set; } = null!;

    [StringLength(255)]
    [Unicode(false)]
    public string MatKhau { get; set; } = null!;

    [StringLength(100)]
    public string HoTen { get; set; } = null!;

    [StringLength(100)]
    [Unicode(false)]
    public string? Email { get; set; }

    [StringLength(15)]
    [Unicode(false)]
    public string? SoDienThoai { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string MaVaiTro { get; set; } = null!;

    public bool TrangThai { get; set; }

    [InverseProperty("NguoiTiepNhanNavigation")]
    public virtual ICollection<HoSoHocVien> HoSoHocViens { get; set; } = new List<HoSoHocVien>();

    [InverseProperty("MaTaiKhoanNavigation")]
    public virtual ICollection<LichSuHoatDong> LichSuHoatDongs { get; set; } = new List<LichSuHoatDong>();

    [ForeignKey("MaVaiTro")]
    [InverseProperty("TaiKhoans")]
    public virtual VaiTro MaVaiTroNavigation { get; set; } = null!;

    [InverseProperty("NguoiThuNavigation")]
    public virtual ICollection<ThanhToanHocPhi> ThanhToanHocPhis { get; set; } = new List<ThanhToanHocPhi>();

    [InverseProperty("NguoiCapNavigation")]
    public virtual ICollection<TheHocVien> TheHocViens { get; set; } = new List<TheHocVien>();

    [InverseProperty("MaTaiKhoanNavigation")]
    public virtual ICollection<ThongBao> ThongBaos { get; set; } = new List<ThongBao>();
}
