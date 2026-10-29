using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TTDaoTaoLaiXe.Models;

namespace TTDaoTaoLaiXe.Controllers
{
    public class KhoaHocController : Controller
    {
        private readonly TTDaoTaoLaiXeContext _context;

        public KhoaHocController(TTDaoTaoLaiXeContext context)
        {
            _context = context;
        }

        // =========================
        // INDEX
        // =========================
        public async Task<IActionResult> Index(
            string? search,
            string? hangGplx,
            string? trangThai,
            int page = 1)
        {
            const int pageSize = 10;

            if (page < 1)
                page = 1;

            var query = _context.KhoaHocs
                .Include(k => k.HangGplxNavigation)
                .AsNoTracking()
                .AsQueryable();

            // =========================
            // TÌM KIẾM
            // =========================
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(k =>
                    k.MaKhoaHoc.Contains(search) ||
                    k.TenKhoaHoc.Contains(search));
            }

            // =========================
            // LỌC HẠNG GPLX
            // =========================
            if (!string.IsNullOrWhiteSpace(hangGplx))
            {
                query = query.Where(k =>
                    k.HangGplx == hangGplx);
            }

            // =========================
            // LỌC TRẠNG THÁI
            // =========================
            if (trangThai == "1")
            {
                query = query.Where(k => k.TrangThai);
            }
            else if (trangThai == "0")
            {
                query = query.Where(k => !k.TrangThai);
            }

            // =========================
            // TỔNG SỐ BẢN GHI
            // =========================
            var totalItems = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(
                totalItems / (double)pageSize);

            if (totalPages > 0 && page > totalPages)
                page = totalPages;

            // =========================
            // PHÂN TRANG DATABASE
            // =========================
            var khoaHocs = await query
                .OrderByDescending(k => k.TrangThai)
                .ThenBy(k => k.TenKhoaHoc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Danh sách hạng GPLX cho bộ lọc
            var hangGplxes = await _context.HangGplxes
                .AsNoTracking()
                .OrderBy(h => h.MaHang)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.HangGplx = hangGplx;
            ViewBag.TrangThai = trangThai;

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = totalPages;

            ViewBag.HangGplxes = hangGplxes;

            return View(khoaHocs);
        }

        // =========================
        // CREATE - HIỂN THỊ FORM
        // =========================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadHangGplx();

            return View(new KhoaHoc
            {
                TrangThai = true
            });
        }


        // =========================
        // CREATE - LƯU KHÓA HỌC
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KhoaHoc model)
        {
            // =========================
            // CHUẨN HÓA DỮ LIỆU
            // =========================
            model.MaKhoaHoc = model.MaKhoaHoc?.Trim() ?? "";
            model.TenKhoaHoc = model.TenKhoaHoc?.Trim() ?? "";

            // Khóa học mới luôn hoạt động
            model.TrangThai = true;


            // =========================
            // KIỂM TRA MÃ
            // =========================
            if (string.IsNullOrWhiteSpace(model.MaKhoaHoc))
            {
                ModelState.AddModelError(
                    nameof(model.MaKhoaHoc),
                    "Mã khóa học không được để trống.");
            }
            else
            {
                bool maTonTai = await _context.KhoaHocs
                    .AnyAsync(k => k.MaKhoaHoc == model.MaKhoaHoc);

                if (maTonTai)
                {
                    ModelState.AddModelError(
                        nameof(model.MaKhoaHoc),
                        "Mã khóa học đã tồn tại.");
                }
            }


            // =========================
            // KIỂM TRA TÊN
            // =========================
            if (string.IsNullOrWhiteSpace(model.TenKhoaHoc))
            {
                ModelState.AddModelError(
                    nameof(model.TenKhoaHoc),
                    "Tên khóa học không được để trống.");
            }
            else
            {
                bool tenTonTai = await _context.KhoaHocs
                    .AnyAsync(k => k.TenKhoaHoc == model.TenKhoaHoc);

                if (tenTonTai)
                {
                    ModelState.AddModelError(
                        nameof(model.TenKhoaHoc),
                        "Tên khóa học đã tồn tại.");
                }
            }


            // =========================
            // KIỂM TRA HẠNG GPLX
            // =========================
            if (string.IsNullOrWhiteSpace(model.HangGplx))
            {
                ModelState.AddModelError(
                    nameof(model.HangGplx),
                    "Vui lòng chọn hạng GPLX.");
            }
            else
            {
                bool hangTonTai = await _context.HangGplxes
                    .AnyAsync(h =>
                        h.MaHang == model.HangGplx &&
                        h.TrangThai);

                if (!hangTonTai)
                {
                    ModelState.AddModelError(
                        nameof(model.HangGplx),
                        "Hạng GPLX không tồn tại hoặc đang ngừng hoạt động.");
                }
            }


            // =========================
            // KIỂM TRA THỜI GIAN
            // =========================
            if (model.ThoiGianDaoTao <= 0)
            {
                ModelState.AddModelError(
                    nameof(model.ThoiGianDaoTao),
                    "Thời gian đào tạo phải lớn hơn 0.");
            }


            if (model.SoGioLyThuyet < 0)
            {
                ModelState.AddModelError(
                    nameof(model.SoGioLyThuyet),
                    "Số giờ lý thuyết không được âm.");
            }


            if (model.SoGioThucHanh < 0)
            {
                ModelState.AddModelError(
                    nameof(model.SoGioThucHanh),
                    "Số giờ thực hành không được âm.");
            }


            if (model.SoGioDat < 0)
            {
                ModelState.AddModelError(
                    nameof(model.SoGioDat),
                    "Số giờ DAT không được âm.");
            }


            // =========================
            // KIỂM TRA NGÀY
            // =========================
            if (model.NgayBatDau.HasValue &&
                model.NgayKetThuc.HasValue &&
                model.NgayBatDau.Value > model.NgayKetThuc.Value)
            {
                ModelState.AddModelError(
                    nameof(model.NgayKetThuc),
                    "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");
            }


            // =========================
            // KIỂM TRA TỔNG GIỜ
            // =========================
            int tongGio =
                model.SoGioLyThuyet +
                model.SoGioThucHanh +
                model.SoGioDat;

            if (tongGio > model.ThoiGianDaoTao)
            {
                ModelState.AddModelError(
                    nameof(model.ThoiGianDaoTao),
                    "Tổng số giờ lý thuyết + thực hành + DAT không được lớn hơn tổng thời gian đào tạo.");
            }


            // =========================
            // NẾU CÓ LỖI
            // =========================
            if (!ModelState.IsValid)
            {
                await LoadHangGplx();

                return View(model);
            }


            // =========================
            // LƯU DATABASE
            // =========================
            try
            {
                _context.KhoaHocs.Add(model);

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Thêm khóa học thành công.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                // Hiện lỗi để dễ kiểm tra trong quá trình phát triển
                ModelState.AddModelError(
                    "",
                    "Không thể lưu khóa học vào cơ sở dữ liệu. " +
                    ex.InnerException?.Message);

                await LoadHangGplx();

                return View(model);
            }
        }


        // =========================
        // DANH SÁCH HẠNG GPLX
        // =========================
        private async Task LoadHangGplx()
        {
            ViewBag.HangGplxes = await _context.HangGplxes
                .AsNoTracking()
                .Where(h => h.TrangThai)
                .OrderBy(h => h.MaHang)
                .ToListAsync();
        }

        // =========================
        // DETAILS
        // =========================
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var khoaHoc = await _context.KhoaHocs
                .Include(k => k.HangGplxNavigation)
                .Include(k => k.DangKyHocs)
                .Include(k => k.KhoaHocMonHocs)
                .Include(k => k.LopHocs)
                .AsNoTracking()
                .FirstOrDefaultAsync(k => k.MaKhoaHoc == id);

            if (khoaHoc == null)
                return NotFound();

            return View(khoaHoc);
        }

        // =========================
        // EDIT - MỞ MODAL
        // =========================
        [HttpGet]
        public async Task<IActionResult> GetEdit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var khoaHoc = await _context.KhoaHocs
                .AsNoTracking()
                .FirstOrDefaultAsync(k =>
                    k.MaKhoaHoc == id);

            if (khoaHoc == null)
                return NotFound();

            // Nạp danh sách hạng GPLX đang hoạt động
            await LoadHangGplx();

            return PartialView("_EditModal", khoaHoc);
        }


        // =========================
        // EDIT - LƯU TỪ MODAL
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(KhoaHoc model)
        {
            // Không cho sửa mã khóa học
            ModelState.Remove(nameof(model.MaKhoaHoc));

            if (model.ThoiGianDaoTao < 0)
                ModelState.AddModelError(
                    nameof(model.ThoiGianDaoTao),
                    "Thời gian đào tạo không được âm.");

            if (model.SoGioLyThuyet < 0)
                ModelState.AddModelError(
                    nameof(model.SoGioLyThuyet),
                    "Số giờ lý thuyết không được âm.");

            if (model.SoGioThucHanh < 0)
                ModelState.AddModelError(
                    nameof(model.SoGioThucHanh),
                    "Số giờ thực hành không được âm.");

            if (model.SoGioDat < 0)
                ModelState.AddModelError(
                    nameof(model.SoGioDat),
                    "Số giờ DAT không được âm.");

            if (model.NgayBatDau.HasValue &&
                model.NgayKetThuc.HasValue &&
                model.NgayBatDau > model.NgayKetThuc)
            {
                ModelState.AddModelError(
                    nameof(model.NgayKetThuc),
                    "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");
            }

            // Kiểm tra hạng GPLX tồn tại
            var hangGplxExists = await _context.HangGplxes
                .AnyAsync(h => h.MaHang == model.HangGplx);

            if (!hangGplxExists)
            {
                ModelState.AddModelError(
                    nameof(model.HangGplx),
                    "Hạng GPLX không tồn tại.");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] =
                    "Thông tin khóa học chưa hợp lệ. Vui lòng kiểm tra lại.";

                return RedirectToAction(nameof(Index));
            }

            var khoaHoc = await _context.KhoaHocs
                .FirstOrDefaultAsync(k =>
                    k.MaKhoaHoc == model.MaKhoaHoc);

            if (khoaHoc == null)
                return NotFound();

            // Kiểm tra trùng tên
            var duplicateName = await _context.KhoaHocs
                .AnyAsync(k =>
                    k.MaKhoaHoc != model.MaKhoaHoc &&
                    k.TenKhoaHoc == model.TenKhoaHoc);

            if (duplicateName)
            {
                TempData["Error"] =
                    "Tên khóa học đã tồn tại.";

                return RedirectToAction(nameof(Index));
            }

            // Không sửa trạng thái tại Edit
            khoaHoc.TenKhoaHoc = model.TenKhoaHoc;
            khoaHoc.HangGplx = model.HangGplx;
            khoaHoc.ThoiGianDaoTao = model.ThoiGianDaoTao;
            khoaHoc.SoGioLyThuyet = model.SoGioLyThuyet;
            khoaHoc.SoGioThucHanh = model.SoGioThucHanh;
            khoaHoc.SoGioDat = model.SoGioDat;
            khoaHoc.NgayBatDau = model.NgayBatDau;
            khoaHoc.NgayKetThuc = model.NgayKetThuc;
            khoaHoc.MoTa = model.MoTa;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Cập nhật khóa học thành công.";

            return RedirectToAction(nameof(Index));
        }


        // =========================
        // TOGGLE TRẠNG THÁI
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var khoaHoc = await _context.KhoaHocs
                .FirstOrDefaultAsync(k =>
                    k.MaKhoaHoc == id);

            if (khoaHoc == null)
                return NotFound();

            khoaHoc.TrangThai = !khoaHoc.TrangThai;

            await _context.SaveChangesAsync();

            TempData["Success"] = khoaHoc.TrangThai
                ? "Đã chuyển khóa học sang trạng thái Đang hoạt động."
                : "Đã chuyển khóa học sang trạng thái Ngừng hoạt động.";

            return RedirectToAction(nameof(Index));
        }


        // =========================
        // DELETE KHÓA HỌC
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var khoaHoc = await _context.KhoaHocs
                .Include(k => k.DangKyHocs)
                .Include(k => k.KhoaHocMonHocs)
                .Include(k => k.LopHocs)
                .FirstOrDefaultAsync(k => k.MaKhoaHoc == id);

            if (khoaHoc == null)
                return NotFound();

            // ==========================================
            // ĐÃ PHÁT SINH DỮ LIỆU LIÊN QUAN
            // ==========================================
            if (khoaHoc.DangKyHocs.Any() ||
                khoaHoc.KhoaHocMonHocs.Any() ||
                khoaHoc.LopHocs.Any())
            {
                // Không xóa cứng
                // Chỉ chuyển sang ngừng hoạt động
                khoaHoc.TrangThai = false;

                await _context.SaveChangesAsync();

                TempData["Warning"] =
                    $"Khóa học \"{khoaHoc.TenKhoaHoc}\" đã phát sinh dữ liệu liên quan nên không thể xóa. " +
                    "Hệ thống đã chuyển khóa học sang trạng thái Ngừng hoạt động.";

                return RedirectToAction(nameof(Index));
            }

            // ==========================================
            // CHƯA PHÁT SINH DỮ LIỆU LIÊN QUAN
            // ==========================================
            _context.KhoaHocs.Remove(khoaHoc);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Đã xóa khóa học \"{khoaHoc.TenKhoaHoc}\" thành công.";

            return RedirectToAction(nameof(Index));
        }
    }
}