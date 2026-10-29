
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TTDaoTaoLaiXe.ViewModels.Account
{
    public class TaiKhoanIndexViewModel
    {
        public List<TaiKhoanItemViewModel> TaiKhoans { get; set; } = new();

        public string? TuKhoa { get; set; }
        public string? MaVaiTro { get; set; }
        public string? TrangThai { get; set; }

        public List<SelectListItem> DanhSachVaiTro { get; set; } = new();

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages =>
    Math.Max(1, (int)Math.Ceiling(
        TotalItems / (double)PageSize));
    }

    public class TaiKhoanItemViewModel
    {
        public string MaTaiKhoan { get; set; } = null!;
        public string TenDangNhap { get; set; } = null!;
        public string HoTen { get; set; } = null!;
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public string MaVaiTro { get; set; } = null!;
        public string TenVaiTro { get; set; } = null!;
        public bool TrangThai { get; set; }
    }

    public class TaiKhoanCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mã tài khoản.")]
        [StringLength(20)]
        public string MaTaiKhoan { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
        [StringLength(50)]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        [StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [StringLength(100)]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [StringLength(15)]
        public string? SoDienThoai { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vai trò.")]
        public string MaVaiTro { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [StringLength(100, MinimumLength = 8,
            ErrorMessage = "Mật khẩu phải có từ 8 đến 100 ký tự.")]
        public string MatKhau { get; set; } = string.Empty;
    }

    public class TaiKhoanEditViewModel
    {
        [Required]
        public string MaTaiKhoan { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
        [StringLength(50)]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        [StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [StringLength(100)]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [StringLength(15)]
        public string? SoDienThoai { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vai trò.")]
        public string MaVaiTro { get; set; } = string.Empty;
    }

    public class TaiKhoanResetPasswordViewModel
    {
        [Required]
        public string MaTaiKhoan { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
        [StringLength(100, MinimumLength = 8,
            ErrorMessage = "Mật khẩu phải có từ 8 đến 100 ký tự.")]
        public string MatKhauMoi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu.")]
        [Compare(nameof(MatKhauMoi),
            ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string XacNhanMatKhau { get; set; } = string.Empty;
    }
}
