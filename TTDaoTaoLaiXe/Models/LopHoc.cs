using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TTDaoTaoLaiXe.Models;

[Table("LopHoc")]
public partial class LopHoc
{
    [Key]
    [StringLength(20)]
    [Unicode(false)]
    public string MaLopHoc { get; set; } = null!;

    [StringLength(150)]
    public string TenLopHoc { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string MaKhoaHoc { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string? MaGiaoVien { get; set; }

    public DateOnly NgayBatDau { get; set; }

    public DateOnly? NgayKetThuc { get; set; }

    public int SiSoToiDa { get; set; }

    [StringLength(30)]
    public string TrangThai { get; set; } = null!;

    [StringLength(500)]
    public string? MoTa { get; set; }

    [InverseProperty("MaLopHocNavigation")]
    public virtual ICollection<HocVienLopHoc> HocVienLopHocs { get; set; } = new List<HocVienLopHoc>();

    [InverseProperty("MaLopHocNavigation")]
    public virtual ICollection<LichHoc> LichHocs { get; set; } = new List<LichHoc>();

    [ForeignKey("MaGiaoVien")]
    [InverseProperty("LopHocs")]
    public virtual GiaoVien? MaGiaoVienNavigation { get; set; }

    [ForeignKey("MaKhoaHoc")]
    [InverseProperty("LopHocs")]
    public virtual KhoaHoc? MaKhoaHocNavigation { get; set; } = null!;
}
