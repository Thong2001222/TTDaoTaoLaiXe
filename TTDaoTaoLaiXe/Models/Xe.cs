using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TTDaoTaoLaiXe.Models;

[Table("Xe")]
[Index("BienSo", Name = "UQ_Xe_BienSo", IsUnique = true)]
public partial class Xe
{
    [Key]
    [StringLength(20)]
    [Unicode(false)]
    public string MaXe { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string BienSo { get; set; } = null!;

    [StringLength(100)]
    public string TenXe { get; set; } = null!;

    [StringLength(10)]
    [Unicode(false)]
    public string? MaHang { get; set; }

    [StringLength(50)]
    public string LoaiXe { get; set; } = null!;

    public int? SoCho { get; set; }

    public int? NamSanXuat { get; set; }

    [StringLength(30)]
    public string TrangThai { get; set; } = null!;

    [StringLength(500)]
    public string? GhiChu { get; set; }

    [InverseProperty("MaXeNavigation")]
    public virtual ICollection<LichHoc> LichHocs { get; set; } = new List<LichHoc>();

    [ForeignKey("MaHang")]
    [InverseProperty("Xes")]
    public virtual HangGplx? MaHangNavigation { get; set; }
}
