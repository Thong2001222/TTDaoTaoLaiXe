
using System;
using System.ComponentModel.DataAnnotations;

namespace TTDaoTaoLaiXe.ViewModels.HocPhi
{
    public class ThanhToanHocPhiCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn khoản học phí.")]
        [StringLength(20)]
        public string MaHocPhi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số tiền thanh toán.")]
        [Range(typeof(decimal), "1", "999999999999",
            ErrorMessage = "Số tiền thanh toán phải lớn hơn 0.")]
        public decimal SoTien { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán.")]
        public string PhuongThuc { get; set; } = "Tiền mặt";

        [StringLength(100, ErrorMessage = "Mã giao dịch không được vượt quá 100 ký tự.")]
        public string? MaGiaoDich { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
        public string? GhiChu { get; set; }
    }
}
