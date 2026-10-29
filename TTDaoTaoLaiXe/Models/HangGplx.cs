using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TTDaoTaoLaiXe.Models;

[Table("HangGPLX")]
[Index("TenHang", Name = "UQ_HangGPLX_TenHang", IsUnique = true)]
public partial class HangGplx
{
    [Key]
    [StringLength(10)]
    [Unicode(false)]
    public string MaHang { get; set; } = null!;

    [StringLength(100)]
    public string TenHang { get; set; } = null!;

    [StringLength(50)]
    public string? LoaiHopSo { get; set; }

    [StringLength(500)]
    public string? MoTa { get; set; }

    public bool TrangThai { get; set; }

    [InverseProperty("HangGplxNavigation")]
    public virtual ICollection<KhoaHoc> KhoaHocs { get; set; } = new List<KhoaHoc>();

    [InverseProperty("MaHangNavigation")]
    public virtual ICollection<QuyDinhDat> QuyDinhDats { get; set; } = new List<QuyDinhDat>();

    [InverseProperty("MaHangNavigation")]
    public virtual ICollection<Xe> Xes { get; set; } = new List<Xe>();
}
