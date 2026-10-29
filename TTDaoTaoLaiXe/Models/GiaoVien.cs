using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TTDaoTaoLaiXe.Models;

[Table("GiaoVien")]
[Index("Cccd", Name = "UQ_GiaoVien_CCCD", IsUnique = true)]
public partial class GiaoVien
{
    [Key]
    [StringLength(20)]
    [Unicode(false)]
    public string MaGiaoVien { get; set; } = null!;

    [StringLength(100)]
    public string HoTen { get; set; } = null!;

    public DateOnly? NgaySinh { get; set; }

    [StringLength(10)]
    public string? GioiTinh { get; set; }

    [Column("CCCD")]
    [StringLength(12)]
    [Unicode(false)]
    public string? Cccd { get; set; }

    [StringLength(15)]
    [Unicode(false)]
    public string? SoDienThoai { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Email { get; set; }

    [StringLength(255)]
    public string? DiaChi { get; set; }

    [StringLength(200)]
    public string? ChuyenMon { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? SoGiayPhepLaiXe { get; set; }

    [StringLength(100)]
    public string? HangDuocPhepDay { get; set; }

    public bool TrangThai { get; set; }

    [StringLength(500)]
    public string? GhiChu { get; set; }

    [InverseProperty("MaGiaoVienNavigation")]
    public virtual ICollection<LichHoc> LichHocs { get; set; } = new List<LichHoc>();

    [InverseProperty("MaGiaoVienNavigation")]
    public virtual ICollection<LopHoc> LopHocs { get; set; } = new List<LopHoc>();
}
