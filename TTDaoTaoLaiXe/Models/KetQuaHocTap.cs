using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TTDaoTaoLaiXe.Models;

[Table("KetQuaHocTap")]
public partial class KetQuaHocTap
{
    [Key]
    [StringLength(20)]
    [Unicode(false)]
    public string MaKetQua { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string MaDangKy { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string MaMonHoc { get; set; } = null!;

    public int LanThi { get; set; }

    public DateOnly? NgayThi { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? Diem { get; set; }

    [StringLength(30)]
    public string KetQua { get; set; } = null!;

    [StringLength(500)]
    public string? GhiChu { get; set; }

    [ForeignKey("MaDangKy")]
    [InverseProperty("KetQuaHocTaps")]
    public virtual DangKyHoc MaDangKyNavigation { get; set; } = null!;

    [ForeignKey("MaMonHoc")]
    [InverseProperty("KetQuaHocTaps")]
    public virtual MonHoc MaMonHocNavigation { get; set; } = null!;
}
