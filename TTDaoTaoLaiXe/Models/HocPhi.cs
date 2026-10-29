using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TTDaoTaoLaiXe.Models;

[Table("HocPhi")]
[Index("MaDangKy", Name = "UQ_HocPhi_MaDangKy", IsUnique = true)]
public partial class HocPhi
{
    [Key]
    [StringLength(20)]
    [Unicode(false)]
    public string MaHocPhi { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string MaDangKy { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TongHocPhi { get; set; }

    public DateOnly? HanThanhToan { get; set; }

    [StringLength(30)]
    public string TrangThai { get; set; } = null!;

    [StringLength(500)]
    public string? GhiChu { get; set; }

    [ForeignKey("MaDangKy")]
    [InverseProperty("HocPhiNavigation")]
    public virtual DangKyHoc MaDangKyNavigation { get; set; } = null!;

    [InverseProperty("MaHocPhiNavigation")]
    public virtual ICollection<ThanhToanHocPhi> ThanhToanHocPhis { get; set; } = new List<ThanhToanHocPhi>();
}
