
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TTDaoTaoLaiXe.Models;
using TTDaoTaoLaiXe.ViewModels;

namespace TTDaoTaoLaiXe.Controllers
{
    [Authorize]
    public class TienDoDATController : Controller
    {
        private readonly TTDaoTaoLaiXeContext _context;
        private const int PageSize = 10;

        public TienDoDATController(TTDaoTaoLaiXeContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            string? tuKhoa,
            string? trangThaiLoc,
            int page = 1)
        {
            page = Math.Max(1, page);

            // Mỗi dòng tổng hợp theo học viên và lần đăng ký học.
            var dangKys = await _context.DangKyHocs
                .AsNoTracking()
                .Include(d => d.MaHocVienNavigation)
                .Where(d => d.TrangThai == "Đang học" ||
                            d.TrangThai == "Đã hoàn thành")
                .Select(d => new
                {
                    d.MaHocVien,
                    d.MaDangKy,
                    HoTen = d.MaHocVienNavigation.HoTen,
                    d.MaKhoaHoc
                })
                .ToListAsync();

            var buoiDats = await _context.BuoiDats
                .AsNoTracking()
                .Where(b => b.TrangThai == "Hoàn thành" ||
                            b.TrangThai == "Đã hoàn thành")
                .GroupBy(b => new { b.MaHocVien, b.MaDangKy })
                .Select(g => new
                {
                    g.Key.MaHocVien,
                    g.Key.MaDangKy,
                    TongKm = g.Sum(b => b.SoKm),
                    GioTuDong = g.Sum(b =>
                        b.LoaiDat == "Tự động" ? b.SoGio : 0m),
                    GioBanDem = g.Sum(b =>
                        b.LoaiDat == "Ban đêm" ? b.SoGio : 0m)
                })
                .ToListAsync();

            var tienDos = dangKys
                .Select(d =>
                {
                    var buoi = buoiDats.FirstOrDefault(b =>
                        b.MaHocVien == d.MaHocVien &&
                        b.MaDangKy == d.MaDangKy);

                    return new TienDoDATViewModel
                    {
                        MaHocVien = d.MaHocVien,
                        HoTen = d.HoTen,
                        MaDangKy = d.MaDangKy,
                        MaKhoaHoc = d.MaKhoaHoc,
                        TongKmDaChay = buoi?.TongKm ?? 0,
                        SoGioTuDong = buoi?.GioTuDong ?? 0,
                        SoGioBanDem = buoi?.GioBanDem ?? 0
                    };
                })
                .AsEnumerable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();

                tienDos = tienDos.Where(x =>
                    x.MaHocVien.Contains(tuKhoa,
                        StringComparison.OrdinalIgnoreCase) ||
                    x.HoTen.Contains(tuKhoa,
                        StringComparison.OrdinalIgnoreCase) ||
                    x.MaDangKy.Contains(tuKhoa,
                        StringComparison.OrdinalIgnoreCase));
            }

            if (trangThaiLoc == "Đạt")
                tienDos = tienDos.Where(x => x.DatYeuCau);
            else if (trangThaiLoc == "Chưa đạt")
                tienDos = tienDos.Where(x => !x.DatYeuCau);

            var danhSach = tienDos
                .OrderBy(x => x.HoTen)
                .ToList();

            int totalItems = danhSach.Count;
            int totalPages = Math.Max(
                1, (int)Math.Ceiling(totalItems / (double)PageSize));

            if (page > totalPages)
                page = totalPages;

            var ketQua = danhSach
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            ViewBag.TuKhoa = tuKhoa ?? "";
            ViewBag.TrangThaiLoc = trangThaiLoc ?? "";
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(ketQua);
        }
    }
}
