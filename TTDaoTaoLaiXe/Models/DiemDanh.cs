using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TTDaoTaoLaiXe.Models;

[PrimaryKey("MaLichHoc", "MaHocVien")]
[Table("DiemDanh")]
public partial class DiemDanh
{
    [Key]
    [StringLength(20)]
    [Unicode(false)]
    public string MaLichHoc { get; set; } = null!;

    [Key]
    [StringLength(20)]
    [Unicode(false)]
    public string MaHocVien { get; set; } = null!;

    [StringLength(30)]
    public string TrangThai { get; set; } = null!;

    [Column(TypeName = "decimal(4, 2)")]
    public decimal GioThamGia { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ThoiGianDiemDanh { get; set; }

    [StringLength(500)]
    public string? GhiChu { get; set; }

    [ForeignKey("MaHocVien")]
    [InverseProperty("DiemDanhs")]
    public virtual HocVien MaHocVienNavigation { get; set; } = null!;

    [ForeignKey("MaLichHoc")]
    [InverseProperty("DiemDanhs")]
    public virtual LichHoc MaLichHocNavigation { get; set; } = null!;
}
