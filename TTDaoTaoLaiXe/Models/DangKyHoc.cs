using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TTDaoTaoLaiXe.Models;

[Table("DangKyHoc")]
public partial class DangKyHoc
{
    [Key]
    [StringLength(20)]
    [Unicode(false)]
    public string MaDangKy { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string MaHocVien { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string MaKhoaHoc { get; set; } = null!;
    

    public DateOnly NgayDangKy { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal HocPhi { get; set; }

    [StringLength(30)]
    public string TrangThai { get; set; } = null!;

    public DateOnly? NgayHoanThanh { get; set; }

    [StringLength(500)]
    public string? GhiChu { get; set; }

    [InverseProperty("MaDangKyNavigation")]
    public virtual ICollection<BuoiDat> BuoiDats { get; set; } = new List<BuoiDat>();

    [InverseProperty("MaDangKyNavigation")]
    public virtual HocPhi? HocPhiNavigation { get; set; }

    [InverseProperty("MaDangKyNavigation")]
    public virtual ICollection<KetQuaHocTap> KetQuaHocTaps { get; set; } = new List<KetQuaHocTap>();

    [ForeignKey("MaHocVien")]
    [InverseProperty("DangKyHocs")]
    public virtual HocVien MaHocVienNavigation { get; set; } = null!;

    [ForeignKey("MaKhoaHoc")]
    [InverseProperty("DangKyHocs")]
    public virtual KhoaHoc MaKhoaHocNavigation { get; set; } = null!;

    [InverseProperty("MaDangKyNavigation")]
    public virtual ICollection<TienDoDat> TienDoDats { get; set; } = new List<TienDoDat>();
}
