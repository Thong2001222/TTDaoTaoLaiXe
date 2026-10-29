using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TTDaoTaoLaiXe.Models;

[Table("HoSoHocVien")]
[Index("MaHocVien", Name = "UQ_HoSoHocVien_MaHocVien", IsUnique = true)]
public partial class HoSoHocVien
{
    [Key]
    [StringLength(20)]
    [Unicode(false)]
    public string MaHoSo { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string MaHocVien { get; set; } = null!;

    public DateOnly NgayTiepNhan { get; set; }

    [Column("DaCoCCCD")]
    public bool DaCoCccd { get; set; }

    public bool DaCoAnh { get; set; }

    public bool DaCoGiayKhamSucKhoe { get; set; }

    public bool DaCoHoSoDangKy { get; set; }

    [StringLength(30)]
    public string TrangThaiHoSo { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string? NguoiTiepNhan { get; set; }

    [StringLength(500)]
    public string? GhiChu { get; set; }

    [ForeignKey("MaHocVien")]
    [InverseProperty("HoSoHocVien")]
    public virtual HocVien MaHocVienNavigation { get; set; } = null!;

    [ForeignKey("NguoiTiepNhan")]
    [InverseProperty("HoSoHocViens")]
    public virtual TaiKhoan? NguoiTiepNhanNavigation { get; set; }
}
