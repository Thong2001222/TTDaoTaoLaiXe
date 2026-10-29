using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TTDaoTaoLaiXe.Models;

[Table("ThanhToanHocPhi")]
public partial class ThanhToanHocPhi
{
    [Key]
    [StringLength(20)]
    [Unicode(false)]
    public string MaThanhToan { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string MaHocPhi { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime NgayThanhToan { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal SoTien { get; set; }

    [StringLength(50)]
    public string PhuongThuc { get; set; } = null!;

    [StringLength(100)]
    [Unicode(false)]
    public string? MaGiaoDich { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? NguoiThu { get; set; }

    [StringLength(500)]
    public string? GhiChu { get; set; }
    [StringLength(30)]
    public string? TrangThaiGiaoDich { get; set; }
    [ForeignKey("MaHocPhi")]
    [InverseProperty("ThanhToanHocPhis")]
    public virtual HocPhi MaHocPhiNavigation { get; set; } = null!;

    [ForeignKey("NguoiThu")]
    [InverseProperty("ThanhToanHocPhis")]
    public virtual TaiKhoan? NguoiThuNavigation { get; set; }
}
