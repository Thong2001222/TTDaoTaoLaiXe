using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TTDaoTaoLaiXe.Models;

[Table("KhoaHoc")]
[Index("TenKhoaHoc", Name = "UQ_KhoaHoc_Ten", IsUnique = true)]
public partial class KhoaHoc
{
    [Key]
    [StringLength(20)]
    [Unicode(false)]
    public string MaKhoaHoc { get; set; } = null!;

    [StringLength(200)]
    public string TenKhoaHoc { get; set; } = null!;

    [Column("HangGPLX")]
    [StringLength(10)]
    [Unicode(false)]
    public string HangGplx { get; set; } = null!;

    public int ThoiGianDaoTao { get; set; }

    public int SoGioLyThuyet { get; set; }

    public int SoGioThucHanh { get; set; }

    [Column("SoGioDAT")]
    public int SoGioDat { get; set; }

    public DateOnly? NgayBatDau { get; set; }

    public DateOnly? NgayKetThuc { get; set; }

    public bool TrangThai { get; set; }

    [StringLength(500)]
    public string? MoTa { get; set; }

    [InverseProperty("MaKhoaHocNavigation")]
    public virtual ICollection<DangKyHoc> DangKyHocs { get; set; } = new List<DangKyHoc>();

    [ForeignKey("HangGplx")]
    [InverseProperty("KhoaHocs")]
    public virtual HangGplx? HangGplxNavigation { get; set; } = null!;

    [InverseProperty("MaKhoaHocNavigation")]
    public virtual ICollection<KhoaHocMonHoc> KhoaHocMonHocs { get; set; } = new List<KhoaHocMonHoc>();

    [InverseProperty("MaKhoaHocNavigation")]
    public virtual ICollection<LopHoc> LopHocs { get; set; } = new List<LopHoc>();
}
