
namespace TTDaoTaoLaiXe.ViewModels
{
    public class TienDoDATViewModel
    {
        public string MaHocVien { get; set; } = "";
        public string HoTen { get; set; } = "";
        public string MaDangKy { get; set; } = "";
        public string MaKhoaHoc { get; set; } = "";

        public decimal TongKmDaChay { get; set; }
        public decimal SoGioTuDong { get; set; }
        public decimal SoGioBanDem { get; set; }

        public decimal ChiTieuKm { get; set; } = 810;
        public decimal ChiTieuGioTuDong { get; set; } = 4;
        public decimal ChiTieuGioBanDem { get; set; } = 4;

        public decimal KmConThieu =>
            Math.Max(0, ChiTieuKm - TongKmDaChay);

        public decimal GioTuDongConThieu =>
            Math.Max(0, ChiTieuGioTuDong - SoGioTuDong);

        public decimal GioBanDemConThieu =>
            Math.Max(0, ChiTieuGioBanDem - SoGioBanDem);

        public bool DatYeuCau =>
            TongKmDaChay >= ChiTieuKm &&
            SoGioTuDong >= ChiTieuGioTuDong &&
            SoGioBanDem >= ChiTieuGioBanDem;
    }
}
