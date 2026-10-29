using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TTDaoTaoLaiXe.Models;

[Table("QuyDinhDAT")]
public partial class QuyDinhDat
{
    [Key]
    [Column("MaQuyDinhDAT")]
    [StringLength(20)]
    [Unicode(false)]
    public string MaQuyDinhDat { get; set; } = null!;

    [StringLength(10)]
    [Unicode(false)]
    public string MaHang { get; set; } = null!;

    [Column(TypeName = "decimal(10, 2)")]
    public decimal TongKm { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal SoGioTuDong { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal SoGioBanDem { get; set; }

    [StringLength(500)]
    public string? MoTa { get; set; }

    public bool TrangThai { get; set; }

    [ForeignKey("MaHang")]
    [InverseProperty("QuyDinhDats")]
    public virtual HangGplx MaHangNavigation { get; set; } = null!;
}
