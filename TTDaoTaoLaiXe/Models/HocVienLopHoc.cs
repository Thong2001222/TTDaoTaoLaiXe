using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TTDaoTaoLaiXe.Models;

[PrimaryKey("MaHocVien", "MaLopHoc")]
[Table("HocVien_LopHoc")]
public partial class HocVienLopHoc
{
    [Key]
    [StringLength(20)]
    [Unicode(false)]
    public string MaHocVien { get; set; } = null!;

    [Key]
    [StringLength(20)]
    [Unicode(false)]
    public string MaLopHoc { get; set; } = null!;

    public DateOnly NgayVaoLop { get; set; }

    public DateOnly? NgayRaLop { get; set; }

    [StringLength(30)]
    public string TrangThai { get; set; } = null!;

    [StringLength(255)]
    public string? LyDoRaLop { get; set; }

    [StringLength(500)]
    public string? GhiChu { get; set; }

    [ForeignKey("MaHocVien")]
    [InverseProperty("HocVienLopHocs")]
    public virtual HocVien MaHocVienNavigation { get; set; } = null!;

    [ForeignKey("MaLopHoc")]
    [InverseProperty("HocVienLopHocs")]
    public virtual LopHoc MaLopHocNavigation { get; set; } = null!;
}
