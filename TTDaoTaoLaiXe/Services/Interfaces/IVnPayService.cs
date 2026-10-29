
namespace TTDaoTaoLaiXe.Services.Interfaces
{
    public interface IVnPayService
    {
        string TaoUrlThanhToan(
            string maThanhToan,
            decimal soTien,
            string noiDung,
            string ipAddress);

        bool XacMinhChuKy(
            IDictionary<string, string> thamSo);
    }
}
