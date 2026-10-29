using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TTDaoTaoLaiXe.Models;

[PrimaryKey("MaKhoaHoc", "MaMonHoc")]
[Table("KhoaHoc_MonHoc")]
public partial class KhoaHocMonHoc
{
    [Key]
    [StringLength(20)]
    [Unicode(false)]
    public string MaKhoaHoc { get; set; } = null!;

    [Key]
    [StringLength(20)]
    [Unicode(false)]
    public string MaMonHoc { get; set; } = null!;

    [Column(TypeName = "decimal(5, 2)")]
    public decimal SoGio { get; set; }

    [StringLength(500)]
    public string? GhiChu { get; set; }

    [ForeignKey("MaKhoaHoc")]
    [InverseProperty("KhoaHocMonHocs")]
    public virtual KhoaHoc MaKhoaHocNavigation { get; set; } = null!;

    [ForeignKey("MaMonHoc")]
    [InverseProperty("KhoaHocMonHocs")]
    public virtual MonHoc MaMonHocNavigation { get; set; } = null!;
}
