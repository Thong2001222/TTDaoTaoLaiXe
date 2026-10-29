using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TTDaoTaoLaiXe.Models;

[Table("PhongHoc")]
[Index("TenPhong", Name = "UQ_PhongHoc_TenPhong", IsUnique = true)]
public partial class PhongHoc
{
    [Key]
    [StringLength(20)]
    [Unicode(false)]
    public string MaPhong { get; set; } = null!;

    [StringLength(100)]
    public string TenPhong { get; set; } = null!;

    [StringLength(50)]
    public string LoaiPhong { get; set; } = null!;

    public int SucChua { get; set; }

    [StringLength(255)]
    public string? ViTri { get; set; }

    public bool TrangThai { get; set; }

    [StringLength(500)]
    public string? GhiChu { get; set; }

    [InverseProperty("MaPhongNavigation")]
    public virtual ICollection<LichHoc> LichHocs { get; set; } = new List<LichHoc>();
}
