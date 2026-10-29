using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TTDaoTaoLaiXe.Models;

[Table("TheHocVien")]
[Index("SoThe", Name = "UQ_TheHocVien_SoThe", IsUnique = true)]
public partial class TheHocVien
{
    [Key]
    [StringLength(20)]
    [Unicode(false)]
    public string MaThe { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string MaHocVien { get; set; } = null!;

    [StringLength(30)]
    [Unicode(false)]
    public string SoThe { get; set; } = null!;

    public DateOnly NgayCap { get; set; }

    public DateOnly? NgayHetHan { get; set; }

    [StringLength(30)]
    public string TrangThai { get; set; } = null!;

    public int SoLanCapLai { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? NguoiCap { get; set; }

    [StringLength(500)]
    public string? GhiChu { get; set; }

    [ForeignKey("MaHocVien")]
    [InverseProperty("TheHocVien")]
    public virtual HocVien MaHocVienNavigation { get; set; } = null!;

    [ForeignKey("NguoiCap")]
    [InverseProperty("TheHocViens")]
    public virtual TaiKhoan? NguoiCapNavigation { get; set; }
}
