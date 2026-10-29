using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TTDaoTaoLaiXe.Models;

[Table("TienDoDAT")]
public partial class TienDoDat
{
    [Key]
    [Column("MaTienDoDAT")]
    [StringLength(20)]
    [Unicode(false)]
    public string MaTienDoDat { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string MaHocVien { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string MaDangKy { get; set; } = null!;

    [Column(TypeName = "decimal(10, 2)")]
    public decimal TongKmDaChay { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal SoGioTuDong { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal SoGioBanDem { get; set; }

    public DateOnly NgayCapNhat { get; set; }

    [StringLength(30)]
    public string TrangThai { get; set; } = null!;

    [StringLength(500)]
    public string? GhiChu { get; set; }

    [ForeignKey("MaDangKy")]
    [InverseProperty("TienDoDats")]
    public virtual DangKyHoc MaDangKyNavigation { get; set; } = null!;

    [ForeignKey("MaHocVien")]
    [InverseProperty("TienDoDats")]
    public virtual HocVien MaHocVienNavigation { get; set; } = null!;
}
