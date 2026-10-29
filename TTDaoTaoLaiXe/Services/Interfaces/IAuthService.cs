using TTDaoTaoLaiXe.Models;
using TTDaoTaoLaiXe.ViewModels.Account;

namespace TTDaoTaoLaiXe.Services.Interfaces
{
    public interface IAuthService
    {
        TaiKhoan? DangNhap(LoginViewModel model);
    }
}