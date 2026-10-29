
using TTDaoTaoLaiXe.Helpers;
using TTDaoTaoLaiXe.Models;
using TTDaoTaoLaiXe.Services.Interfaces;
using TTDaoTaoLaiXe.ViewModels.Account;

namespace TTDaoTaoLaiXe.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly TTDaoTaoLaiXeContext _context;

        public AuthService(TTDaoTaoLaiXeContext context)
        {
            _context = context;
        }

        public TaiKhoan? DangNhap(LoginViewModel model)
        {
            var taiKhoan = _context.TaiKhoans
                .FirstOrDefault(x =>
                    x.TenDangNhap == model.TenDangNhap &&
                    x.TrangThai == true);

            if (taiKhoan == null)
                return null;

            bool matKhauDung = PasswordHelper.Verify(
                model.MatKhau,
                taiKhoan.MatKhau);

            if (!matKhauDung)
                return null;

            return taiKhoan;
        }
    }
}