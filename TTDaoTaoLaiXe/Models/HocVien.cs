using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TTDaoTaoLaiXe.Models;

[Table("HocVien")]
[Index("Cccd", Name = "UQ_HocVien_CCCD", IsUnique = true)]
public partial class HocVien
{
    [Key]
    [StringLength(20)]
    [Unicode(false)]
    public string MaHocVien { get; set; } = null!;

    [StringLength(100)]
    public string HoTen { get; set; } = null!;

    public DateOnly NgaySinh { get; set; }

    [StringLength(10)]
    public string GioiTinh { get; set; } = null!;

    [Column("CCCD")]
    [StringLength(12)]
    [Unicode(false)]
    public string Cccd { get; set; } = null!;

    [Column("NgayCapCCCD")]
    public DateOnly? NgayCapCccd { get; set; }

    [Column("NoiCapCCCD")]
    [StringLength(200)]
    public string? NoiCapCccd { get; set; }

    [StringLength(15)]
    [Unicode(false)]
    public string SoDienThoai { get; set; } = null!;

    [StringLength(100)]
    [Unicode(false)]
    public string? Email { get; set; }

    [StringLength(255)]
    public string? DiaChi { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string? AnhHocVien { get; set; }

    public bool TrangThai { get; set; }

    [StringLength(500)]
    public string? GhiChu { get; set; }

    [InverseProperty("MaHocVienNavigation")]
    public virtual ICollection<BuoiDat> BuoiDats { get; set; } = new List<BuoiDat>();

    [InverseProperty("MaHocVienNavigation")]
    public virtual ICollection<DangKyHoc> DangKyHocs { get; set; } = new List<DangKyHoc>();

    [InverseProperty("MaHocVienNavigation")]
    public virtual ICollection<DiemDanh> DiemDanhs { get; set; } = new List<DiemDanh>();

    [InverseProperty("MaHocVienNavigation")]
    public virtual HoSoHocVien? HoSoHocVien { get; set; }

    [InverseProperty("MaHocVienNavigation")]
    public virtual ICollection<HocVienLopHoc> HocVienLopHocs { get; set; } = new List<HocVienLopHoc>();

    [InverseProperty("MaHocVienNavigation")]
    public virtual TheHocVien? TheHocVien { get; set; }

    [InverseProperty("MaHocVienNavigation")]
    public virtual ICollection<TienDoDat> TienDoDats { get; set; } = new List<TienDoDat>();
}
