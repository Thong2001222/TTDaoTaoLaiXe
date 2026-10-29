using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TTDaoTaoLaiXe.Models;

[Table("ThongBao")]
public partial class ThongBao
{
    [Key]
    [StringLength(20)]
    [Unicode(false)]
    public string MaThongBao { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string? MaTaiKhoan { get; set; }

    [StringLength(200)]
    public string TieuDe { get; set; } = null!;

    [StringLength(1000)]
    public string NoiDung { get; set; } = null!;

    [StringLength(50)]
    public string? LoaiThongBao { get; set; }

    public bool DaDoc { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ThoiGianTao { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? MaDoiTuong { get; set; }

    public bool TrangThai { get; set; }

    [ForeignKey("MaTaiKhoan")]
    [InverseProperty("ThongBaos")]
    public virtual TaiKhoan? MaTaiKhoanNavigation { get; set; }
}
