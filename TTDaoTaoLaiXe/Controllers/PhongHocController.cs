
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TTDaoTaoLaiXe.Models;

namespace TTDaoTaoLaiXe.Controllers
{
    [Authorize]
    public class PhongHocController : Controller
    {
        private readonly TTDaoTaoLaiXeContext _context;
        private const int PageSize = 10;

        public PhongHocController(TTDaoTaoLaiXeContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            string? tuKhoa,
            string? trangThai,
            int page = 1)
        {
            page = Math.Max(1, page);

            var query = _context.PhongHocs
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();

                query = query.Where(p =>
                    p.MaPhong.Contains(tuKhoa) ||
                    p.TenPhong.Contains(tuKhoa) ||
                    p.LoaiPhong.Contains(tuKhoa) ||
                    (p.ViTri != null && p.ViTri.Contains(tuKhoa)));
            }

            if (trangThai == "1")
            {
                query = query.Where(p => p.TrangThai);
            }
            else if (trangThai == "0")
            {
                query = query.Where(p => !p.TrangThai);
            }

            var totalItems = await query.CountAsync();
            var totalPages = Math.Max(1,
                (int)Math.Ceiling(totalItems / (double)PageSize));

            if (page > totalPages)
                page = totalPages;

            var danhSach = await query
                .OrderBy(p => p.MaPhong)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.TrangThaiLoc = trangThai;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(danhSach);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PhongHoc model)
        {
            if (string.IsNullOrWhiteSpace(model.MaPhong))
                ModelState.AddModelError(nameof(model.MaPhong),
                    "Vui lòng nhập mã phòng.");

            if (string.IsNullOrWhiteSpace(model.TenPhong))
                ModelState.AddModelError(nameof(model.TenPhong),
                    "Vui lòng nhập tên phòng.");

            if (string.IsNullOrWhiteSpace(model.LoaiPhong))
                ModelState.AddModelError(nameof(model.LoaiPhong),
                    "Vui lòng chọn loại phòng.");

            if (model.SucChua <= 0)
                ModelState.AddModelError(nameof(model.SucChua),
                    "Sức chứa phải lớn hơn 0.");

            model.MaPhong = model.MaPhong?.Trim() ?? "";
            model.TenPhong = model.TenPhong?.Trim() ?? "";
            model.LoaiPhong = model.LoaiPhong?.Trim() ?? "";
            model.ViTri = model.ViTri?.Trim();
            model.GhiChu = model.GhiChu?.Trim();

            if (await _context.PhongHocs.AnyAsync(p =>
                p.MaPhong == model.MaPhong))
            {
                ModelState.AddModelError(nameof(model.MaPhong),
                    "Mã phòng đã tồn tại.");
            }

            if (await _context.PhongHocs.AnyAsync(p =>
                p.TenPhong == model.TenPhong))
            {
                ModelState.AddModelError(nameof(model.TenPhong),
                    "Tên phòng đã tồn tại.");
            }

            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Vui lòng kiểm tra lại thông tin.",
                    errors = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            x => x.Key,
                            x => x.Value!.Errors
                                .Select(e => e.ErrorMessage).ToArray())
                });

            model.TrangThai = true;

            try
            {
                _context.PhongHocs.Add(model);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Thêm phòng học thành công."
                });
            }
            catch (DbUpdateException)
            {
                return Json(new
                {
                    success = false,
                    message = "Không thể thêm phòng. Hãy kiểm tra mã phòng, tên phòng và dữ liệu trong database."
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PhongHoc model)
        {
            var phong = await _context.PhongHocs
                .FirstOrDefaultAsync(p => p.MaPhong == model.MaPhong);

            if (phong == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Không tìm thấy phòng học."
                });
            }

            if (string.IsNullOrWhiteSpace(model.TenPhong))
                ModelState.AddModelError(nameof(model.TenPhong),
                    "Vui lòng nhập tên phòng.");

            if (string.IsNullOrWhiteSpace(model.LoaiPhong))
                ModelState.AddModelError(nameof(model.LoaiPhong),
                    "Vui lòng chọn loại phòng.");

            if (model.SucChua <= 0)
                ModelState.AddModelError(nameof(model.SucChua),
                    "Sức chứa phải lớn hơn 0.");

            model.TenPhong = model.TenPhong?.Trim() ?? "";
            model.LoaiPhong = model.LoaiPhong?.Trim() ?? "";
            model.ViTri = model.ViTri?.Trim();
            model.GhiChu = model.GhiChu?.Trim();

            if (await _context.PhongHocs.AnyAsync(p =>
                p.MaPhong != phong.MaPhong &&
                p.TenPhong == model.TenPhong))
            {
                ModelState.AddModelError(nameof(model.TenPhong),
                    "Tên phòng đã được sử dụng.");
            }

            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Vui lòng kiểm tra lại thông tin.",
                    errors = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            x => x.Key,
                            x => x.Value!.Errors
                                .Select(e => e.ErrorMessage).ToArray())
                });
            }

            phong.TenPhong = model.TenPhong;
            phong.LoaiPhong = model.LoaiPhong;
            phong.SucChua = model.SucChua;
            phong.ViTri = model.ViTri;
            phong.GhiChu = model.GhiChu;

            try
            {
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Cập nhật phòng học thành công."
                });
            }
            catch (DbUpdateException)
            {
                return Json(new
                {
                    success = false,
                    message = "Không thể cập nhật phòng học. Vui lòng kiểm tra lại dữ liệu."
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleTrangThai(string maPhong)
        {
            if (string.IsNullOrWhiteSpace(maPhong))
            {
                return Json(new
                {
                    success = false,
                    message = "Mã phòng không hợp lệ."
                });
            }

            var phong = await _context.PhongHocs
                .FirstOrDefaultAsync(p => p.MaPhong == maPhong);

            if (phong == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Không tìm thấy phòng học."
                });
            }

            if (phong.TrangThai)
            {
                var homNay = DateOnly.FromDateTime(DateTime.Today);

                var coLichSapToi = await _context.LichHocs.AnyAsync(l =>
                    l.MaPhong == maPhong &&
                    l.NgayHoc >= homNay &&
                    l.TrangThai != "Đã hủy" &&
                    l.TrangThai != "Đã hoàn thành");

                if (coLichSapToi)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Phòng còn lịch học từ hôm nay trở đi. Hãy xử lý các lịch học liên quan trước khi ngừng hoạt động phòng."
                    });
                }
            }

            phong.TrangThai = !phong.TrangThai;

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = phong.TrangThai
                    ? "Đã kích hoạt phòng học."
                    : "Đã ngừng hoạt động phòng học.",
                trangThai = phong.TrangThai
            });
        }
    }
}
