using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TTDaoTaoLaiXe.Models;

[Table("LichSuHoatDong")]
public partial class LichSuHoatDong
{
    [Key]
    public long MaLichSu { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? MaTaiKhoan { get; set; }

    [StringLength(50)]
    public string HanhDong { get; set; } = null!;

    [StringLength(100)]
    public string BangDuLieu { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string? MaDoiTuong { get; set; }

    [StringLength(1000)]
    public string? NoiDung { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ThoiGian { get; set; }

    [Column("DiaChiIP")]
    [StringLength(45)]
    [Unicode(false)]
    public string? DiaChiIp { get; set; }

    [ForeignKey("MaTaiKhoan")]
    [InverseProperty("LichSuHoatDongs")]
    public virtual TaiKhoan? MaTaiKhoanNavigation { get; set; }
}
