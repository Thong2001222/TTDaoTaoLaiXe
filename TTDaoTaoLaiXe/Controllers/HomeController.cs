
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TTDaoTaoLaiXe.Models;

namespace TTDaoTaoLaiXe.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly TTDaoTaoLaiXeContext _context;

        public HomeController(TTDaoTaoLaiXeContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var homNay = DateTime.Today;
            var dauThangHienTai = new DateTime(
                homNay.Year, homNay.Month, 1);

            var dauThangDauBieuDo = dauThangHienTai.AddMonths(-11);
            var dauThangTiepTheo = dauThangHienTai.AddMonths(1);

            var ngayBatDau = DateOnly.FromDateTime(dauThangDauBieuDo);
            var ngayKetThuc = DateOnly.FromDateTime(dauThangTiepTheo);

            var model = new DashboardViewModel
            {
                TongHocVien = await _context.HocViens.CountAsync(),

                HocVienDangHoatDong = await _context.HocViens
                    .CountAsync(x => x.TrangThai),

                TongKhoaHoc = await _context.KhoaHocs.CountAsync(),

                KhoaHocDangHoatDong = await _context.KhoaHocs
                    .CountAsync(x => x.TrangThai),

                TongLopHoc = await _context.LopHocs.CountAsync(),

                LopHocDangHoatDong = await _context.LopHocs
                    .CountAsync(x =>
                        x.TrangThai == "Đang mở" ||
                        x.TrangThai == "Đang học"),

                TongGiaoVien = await _context.GiaoViens.CountAsync(),

                GiaoVienDangHoatDong = await _context.GiaoViens
                    .CountAsync(x => x.TrangThai)
            };

            var dangKyGanDay = await _context.DangKyHocs
                .AsNoTracking()
                .Where(x =>
                    x.NgayDangKy >= ngayBatDau &&
                    x.NgayDangKy < ngayKetThuc)
                .Select(x => x.NgayDangKy)
                .ToListAsync();

            var giaoDichGanDay = await _context.ThanhToanHocPhis
                .AsNoTracking()
                .Where(x =>
                    x.TrangThaiGiaoDich == "Thành công" &&
                    x.NgayThanhToan >= dauThangDauBieuDo &&
                    x.NgayThanhToan < dauThangTiepTheo)
                .Select(x => new
                {
                    x.NgayThanhToan,
                    x.SoTien
                })
                .ToListAsync();

            for (int i = 0; i < 12; i++)
            {
                var thang = dauThangDauBieuDo.AddMonths(i);
                var thangKeTiep = thang.AddMonths(1);

                var ngayDauThang = DateOnly.FromDateTime(thang);
                var ngayDauThangKeTiep =
                    DateOnly.FromDateTime(thangKeTiep);

                model.NhanThang.Add(thang.ToString("MM/yyyy"));

                model.SoHocVienDangKy.Add(
                    dangKyGanDay.Count(x =>
                        x >= ngayDauThang &&
                        x < ngayDauThangKeTiep));

                model.DoanhThuTheoThang.Add(
                    giaoDichGanDay
                        .Where(x =>
                            x.NgayThanhToan >= thang &&
                            x.NgayThanhToan < thangKeTiep)
                        .Sum(x => x.SoTien));
            }

            var trangThaiHocVien = await _context.HocViens
                .AsNoTracking()
                .GroupBy(x => x.TrangThai)
                .Select(g => new
                {
                    TrangThai = g.Key,
                    SoLuong = g.Count()
                })
                .ToListAsync();

            model.NhanTrangThaiHocVien = new List<string>
            {
                "Đang hoạt động",
                "Ngừng hoạt động"
            };

            model.SoLuongTrangThaiHocVien = new List<int>
            {
                trangThaiHocVien
                    .Where(x => x.TrangThai)
                    .Sum(x => x.SoLuong),

                trangThaiHocVien
                    .Where(x => !x.TrangThai)
                    .Sum(x => x.SoLuong)
            };

            var trangThaiLopHoc = await _context.LopHocs
                .AsNoTracking()
                .GroupBy(x => x.TrangThai)
                .Select(g => new
                {
                    TrangThai = g.Key,
                    SoLuong = g.Count()
                })
                .ToListAsync();

            model.NhanTrangThaiLopHoc = trangThaiLopHoc
                .Select(x => x.TrangThai)
                .ToList();

            model.SoLuongTrangThaiLopHoc = trangThaiLopHoc
                .Select(x => x.SoLuong)
                .ToList();

            var diemDanh = await _context.DiemDanhs
                .AsNoTracking()
                .GroupBy(x => x.TrangThai)
                .Select(g => new
                {
                    TrangThai = g.Key,
                    SoLuong = g.Count()
                })
                .ToListAsync();

            model.NhanDiemDanh = diemDanh
                .Select(x => x.TrangThai)
                .ToList();

            model.SoLuongDiemDanh = diemDanh
                .Select(x => x.SoLuong)
                .ToList();

            var hocPhi = await _context.HocPhis
                .AsNoTracking()
                .GroupBy(x => x.TrangThai)
                .Select(g => new
                {
                    TrangThai = g.Key,
                    SoLuong = g.Count()
                })
                .ToListAsync();

            model.NhanTrangThaiHocPhi = hocPhi
                .Select(x => x.TrangThai)
                .ToList();

            model.SoLuongTrangThaiHocPhi = hocPhi
                .Select(x => (decimal)x.SoLuong)
                .ToList();

            model.TongHocPhi = await _context.HocPhis
                .AsNoTracking()
                .SumAsync(x => (decimal?)x.TongHocPhi) ?? 0m;

            model.TongDaThu = await _context.ThanhToanHocPhis
                .AsNoTracking()
                .Where(x =>
                    x.TrangThaiGiaoDich == "Thành công" &&
                    x.NgayThanhToan <= DateTime.Now)
                .SumAsync(x => (decimal?)x.SoTien) ?? 0m;

            model.TongConNo = Math.Max(
                0m,
                model.TongHocPhi - model.TongDaThu);

            return View(model);
        }
    }

    public class DashboardViewModel
    {
        public int TongHocVien { get; set; }
        public int HocVienDangHoatDong { get; set; }

        public int TongKhoaHoc { get; set; }
        public int KhoaHocDangHoatDong { get; set; }

        public int TongLopHoc { get; set; }
        public int LopHocDangHoatDong { get; set; }

        public int TongGiaoVien { get; set; }
        public int GiaoVienDangHoatDong { get; set; }

        public decimal TongHocPhi { get; set; }
        public decimal TongDaThu { get; set; }
        public decimal TongConNo { get; set; }

        public List<string> NhanThang { get; set; } = new();
        public List<int> SoHocVienDangKy { get; set; } = new();
        public List<decimal> DoanhThuTheoThang { get; set; } = new();

        public List<string> NhanTrangThaiHocVien { get; set; } = new();
        public List<int> SoLuongTrangThaiHocVien { get; set; } = new();

        public List<string> NhanTrangThaiLopHoc { get; set; } = new();
        public List<int> SoLuongTrangThaiLopHoc { get; set; } = new();

        public List<string> NhanDiemDanh { get; set; } = new();
        public List<int> SoLuongDiemDanh { get; set; } = new();

        public List<string> NhanTrangThaiHocPhi { get; set; } = new();
        public List<decimal> SoLuongTrangThaiHocPhi { get; set; } = new();
    }
}
