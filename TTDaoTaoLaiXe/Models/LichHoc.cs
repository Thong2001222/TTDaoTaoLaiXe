using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TTDaoTaoLaiXe.Models;

[Table("LichHoc")]
public partial class LichHoc
{
    [Key]
    [StringLength(20)]
    [Unicode(false)]
    public string MaLichHoc { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string MaLopHoc { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string MaGiaoVien { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string? MaPhong { get; set; }

    public DateOnly NgayHoc { get; set; }

    public TimeOnly GioBatDau { get; set; }

    public TimeOnly GioKetThuc { get; set; }

    [StringLength(30)]
    public string LoaiBuoiHoc { get; set; } = null!;

    [StringLength(500)]
    public string? NoiDung { get; set; }

    [Column(TypeName = "decimal(4, 2)")]
    public decimal? SoGio { get; set; }

    [StringLength(30)]
    public string TrangThai { get; set; } = null!;

    [StringLength(500)]
    public string? GhiChu { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? MaXe { get; set; }

    [InverseProperty("MaLichHocNavigation")]
    public virtual ICollection<DiemDanh> DiemDanhs { get; set; } = new List<DiemDanh>();

    [ForeignKey("MaGiaoVien")]
    [InverseProperty("LichHocs")]
    public virtual GiaoVien MaGiaoVienNavigation { get; set; } = null!;

    [ForeignKey("MaLopHoc")]
    [InverseProperty("LichHocs")]
    public virtual LopHoc MaLopHocNavigation { get; set; } = null!;

    [ForeignKey("MaPhong")]
    [InverseProperty("LichHocs")]
    public virtual PhongHoc? MaPhongNavigation { get; set; }

    [ForeignKey("MaXe")]
    [InverseProperty("LichHocs")]
    public virtual Xe? MaXeNavigation { get; set; }
}
