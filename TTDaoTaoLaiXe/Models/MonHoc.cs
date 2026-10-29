using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TTDaoTaoLaiXe.Models;

[Table("MonHoc")]
[Index("TenMonHoc", Name = "UQ_MonHoc_Ten", IsUnique = true)]
public partial class MonHoc
{
    [Key]
    [StringLength(20)]
    [Unicode(false)]
    public string MaMonHoc { get; set; } = null!;

    [StringLength(200)]
    public string TenMonHoc { get; set; } = null!;

    [StringLength(50)]
    public string LoaiMonHoc { get; set; } = null!;

    [Column(TypeName = "decimal(5, 2)")]
    public decimal SoGioQuyDinh { get; set; }

    [StringLength(500)]
    public string? MoTa { get; set; }

    public bool TrangThai { get; set; }

    [InverseProperty("MaMonHocNavigation")]
    public virtual ICollection<KetQuaHocTap> KetQuaHocTaps { get; set; } = new List<KetQuaHocTap>();

    [InverseProperty("MaMonHocNavigation")]
    public virtual ICollection<KhoaHocMonHoc> KhoaHocMonHocs { get; set; } = new List<KhoaHocMonHoc>();
}
