using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TTDaoTaoLaiXe.Models;

[Table("BuoiDAT")]
public partial class BuoiDat
{
    [Key]
    [Column("MaBuoiDAT")]
    [StringLength(20)]
    [Unicode(false)]
    public string MaBuoiDat { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string MaHocVien { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string MaDangKy { get; set; } = null!;

    public DateOnly NgayThucHien { get; set; }

    public TimeOnly GioBatDau { get; set; }

    public TimeOnly GioKetThuc { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal SoKm { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal SoGio { get; set; }

    [Column("LoaiDAT")]
    [StringLength(30)]
    public string LoaiDat { get; set; } = null!;

    [StringLength(30)]
    public string TrangThai { get; set; } = null!;

    [StringLength(500)]
    public string? GhiChu { get; set; }

    [ForeignKey("MaDangKy")]
    [InverseProperty("BuoiDats")]
    public virtual DangKyHoc MaDangKyNavigation { get; set; } = null!;

    [ForeignKey("MaHocVien")]
    [InverseProperty("BuoiDats")]
    public virtual HocVien MaHocVienNavigation { get; set; } = null!;
}
