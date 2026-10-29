
using System.ComponentModel.DataAnnotations;

namespace TTDaoTaoLaiXe.ViewModels.Account
{
    public class TaiKhoanTaoMoiViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mã tài khoản.")]
        [StringLength(20, ErrorMessage = "Mã tài khoản không được vượt quá 20 ký tự.")]
        public string MaTaiKhoan { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
        [StringLength(50, ErrorMessage = "Tên đăng nhập không được vượt quá 50 ký tự.")]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự.")]
        public string HoTen { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự.")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
        [StringLength(15, ErrorMessage = "Số điện thoại không được vượt quá 15 ký tự.")]
        public string? SoDienThoai { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vai trò.")]
        public string MaVaiTro { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [StringLength(100, MinimumLength = 8,
            ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự.")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; } = string.Empty;
    }
}
