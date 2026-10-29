
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TTDaoTaoLaiXe.Models;

namespace TTDaoTaoLaiXe.Controllers
{
    public class DangKyHocController : Controller
    {
        private readonly TTDaoTaoLaiXeContext _context;
        private const int PageSize = 10;

        public DangKyHocController(TTDaoTaoLaiXeContext context)
        {
            _context = context;
        }

        // =====================================================
        // GET: DangKyHoc
        // Tìm kiếm, lọc trạng thái và phân trang
        // =====================================================
        public async Task<IActionResult> Index(
            string? tuKhoa,
            string? trangThaiLoc,
            int page = 1)
        {
            page = Math.Max(1, page);

            var query = _context.DangKyHocs
                .AsNoTracking()
                .Include(d => d.MaHocVienNavigation)
                .Include(d => d.MaKhoaHocNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();

                query = query.Where(d =>
                    d.MaDangKy.Contains(tuKhoa) ||
                    d.MaHocVien.Contains(tuKhoa) ||
                    d.MaKhoaHoc.Contains(tuKhoa) ||
                    (d.MaHocVienNavigation != null &&
                     d.MaHocVienNavigation.HoTen.Contains(tuKhoa)) ||
                    (d.MaKhoaHocNavigation != null &&
                     d.MaKhoaHocNavigation.TenKhoaHoc.Contains(tuKhoa)));
            }

            if (!string.IsNullOrWhiteSpace(trangThaiLoc))
            {
                query = query.Where(d => d.TrangThai == trangThaiLoc);
            }

            int totalItems = await query.CountAsync();

            int totalPages = Math.Max(
                1,
                (int)Math.Ceiling(totalItems / (double)PageSize));

            if (page > totalPages)
            {
                page = totalPages;
            }

            var danhSach = await query
                .OrderByDescending(d => d.NgayDangKy)
                .ThenBy(d => d.MaDangKy)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            await LoadEditData();

            ViewBag.TuKhoa = tuKhoa ?? string.Empty;
            ViewBag.TrangThaiLoc = trangThaiLoc ?? string.Empty;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = PageSize;

            return View(danhSach);
        }

        // =====================================================
        // Nạp dữ liệu cho modal chỉnh sửa
        // Bao gồm cả học viên/khóa học ngừng hoạt động
        // để hiển thị được dữ liệu đăng ký cũ.
        // =====================================================
        private async Task LoadEditData()
        {
            ViewBag.EditHocViens = await _context.HocViens
                .AsNoTracking()
                .OrderBy(x => x.HoTen)
                .ToListAsync();

            ViewBag.EditKhoaHocs = await _context.KhoaHocs
                .AsNoTracking()
                .OrderBy(x => x.TenKhoaHoc)
                .ToListAsync();
        }

        // =====================================================
        // GET: DangKyHoc/Details/DK0001
        // =====================================================
        public async Task<IActionResult> Details(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var dangKyHoc = await _context.DangKyHocs
                .AsNoTracking()
                .Include(x => x.MaHocVienNavigation)
                .Include(x => x.MaKhoaHocNavigation)
                .FirstOrDefaultAsync(x => x.MaDangKy == id);

            if (dangKyHoc == null)
            {
                return NotFound();
            }

            await LoadEditData();

            return View(dangKyHoc);
        }

        // =====================================================
        // GET: DangKyHoc/Create
        // =====================================================
        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();

            return View();
        }

        // =====================================================
        // POST: DangKyHoc/Create
        // Tạo đăng ký và khoản phải thu trong cùng transaction
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DangKyHoc dangKyHoc)
        {
            ModelState.Remove(nameof(DangKyHoc.TrangThai));
            ModelState.Remove(nameof(DangKyHoc.MaHocVienNavigation));
            ModelState.Remove(nameof(DangKyHoc.MaKhoaHocNavigation));

            // 1. Kiểm tra mã đăng ký
            if (string.IsNullOrWhiteSpace(dangKyHoc.MaDangKy))
            {
                ModelState.AddModelError(
                    nameof(dangKyHoc.MaDangKy),
                    "Vui lòng nhập mã đăng ký.");
            }
            else
            {
                dangKyHoc.MaDangKy = dangKyHoc.MaDangKy.Trim();

                if (await _context.DangKyHocs.AnyAsync(
                    x => x.MaDangKy == dangKyHoc.MaDangKy))
                {
                    ModelState.AddModelError(
                        nameof(dangKyHoc.MaDangKy),
                        "Mã đăng ký đã tồn tại. Vui lòng nhập mã khác.");
                }
            }

            // 2. Kiểm tra học viên
            if (string.IsNullOrWhiteSpace(dangKyHoc.MaHocVien))
            {
                ModelState.AddModelError(
                    nameof(dangKyHoc.MaHocVien),
                    "Vui lòng chọn học viên.");
            }
            else
            {
                var hocVien = await _context.HocViens
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.MaHocVien == dangKyHoc.MaHocVien);

                if (hocVien == null)
                {
                    ModelState.AddModelError(
                        nameof(dangKyHoc.MaHocVien),
                        "Học viên không tồn tại.");
                }
                else if (!hocVien.TrangThai)
                {
                    ModelState.AddModelError(
                        nameof(dangKyHoc.MaHocVien),
                        "Học viên đã ngừng hoạt động, không thể đăng ký.");
                }
            }

            // 3. Kiểm tra khóa học
            if (string.IsNullOrWhiteSpace(dangKyHoc.MaKhoaHoc))
            {
                ModelState.AddModelError(
                    nameof(dangKyHoc.MaKhoaHoc),
                    "Vui lòng chọn khóa học.");
            }
            else
            {
                var khoaHoc = await _context.KhoaHocs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.MaKhoaHoc == dangKyHoc.MaKhoaHoc);

                if (khoaHoc == null)
                {
                    ModelState.AddModelError(
                        nameof(dangKyHoc.MaKhoaHoc),
                        "Khóa học không tồn tại.");
                }
                else if (!khoaHoc.TrangThai)
                {
                    ModelState.AddModelError(
                        nameof(dangKyHoc.MaKhoaHoc),
                        "Khóa học đã ngừng hoạt động, không thể đăng ký.");
                }
            }

            // 4. Kiểm tra đăng ký trùng
            if (!string.IsNullOrWhiteSpace(dangKyHoc.MaHocVien) &&
                !string.IsNullOrWhiteSpace(dangKyHoc.MaKhoaHoc))
            {
                bool daDangKy = await _context.DangKyHocs.AnyAsync(x =>
                    x.MaHocVien == dangKyHoc.MaHocVien &&
                    x.MaKhoaHoc == dangKyHoc.MaKhoaHoc &&
                    x.TrangThai != "Hủy");

                if (daDangKy)
                {
                    ModelState.AddModelError(
                        nameof(dangKyHoc.MaHocVien),
                        "Học viên này đã đăng ký khóa học này.");
                }
            }

            // 5. Kiểm tra học phí
            if (dangKyHoc.HocPhi < 0)
            {
                ModelState.AddModelError(
                    nameof(dangKyHoc.HocPhi),
                    "Học phí không được nhỏ hơn 0.");
            }

            // 6. Kiểm tra ngày đăng ký
            if (dangKyHoc.NgayDangKy == default)
            {
                ModelState.AddModelError(
                    nameof(dangKyHoc.NgayDangKy),
                    "Vui lòng chọn ngày đăng ký.");
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdowns(
                    dangKyHoc.MaHocVien,
                    dangKyHoc.MaKhoaHoc);

                return View(dangKyHoc);
            }

            dangKyHoc.TrangThai = "Đang học";

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                _context.DangKyHocs.Add(dangKyHoc);

                await _context.SaveChangesAsync();

                // Tạo khoản phải thu tương ứng với đăng ký.
                bool daCoHocPhi = await _context.HocPhis
                    .AnyAsync(x => x.MaDangKy == dangKyHoc.MaDangKy);

                if (!daCoHocPhi)
                {
                    var hocPhi = new HocPhi
                    {
                        MaHocPhi = await GenerateMaHocPhiAsync(),
                        MaDangKy = dangKyHoc.MaDangKy,
                        TongHocPhi = dangKyHoc.HocPhi,
                        HanThanhToan = null,
                        TrangThai = "Chưa thanh toán",
                        GhiChu = "Tạo tự động từ đăng ký học"
                    };

                    _context.HocPhis.Add(hocPhi);

                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                TempData["Success"] =
                    "Thêm đăng ký học và tạo khoản phải thu thành công.";
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                _context.ChangeTracker.Clear();

                TempData["Error"] =
                    "Không thể lưu đăng ký học và khoản phải thu. " +
                    "Vui lòng kiểm tra dữ liệu và thử lại.";
            }

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // POST: DangKyHoc/Edit
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DangKyHoc dangKyHoc)
        {
            ModelState.Remove(nameof(DangKyHoc.MaHocVienNavigation));
            ModelState.Remove(nameof(DangKyHoc.MaKhoaHocNavigation));

            // 1. Lấy đăng ký hiện tại
            if (string.IsNullOrWhiteSpace(dangKyHoc.MaDangKy))
            {
                TempData["Warning"] = "Mã đăng ký không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            var dangKyCu = await _context.DangKyHocs
                .FirstOrDefaultAsync(
                    x => x.MaDangKy == dangKyHoc.MaDangKy);

            if (dangKyCu == null)
            {
                TempData["Warning"] =
                    "Không tìm thấy đăng ký cần chỉnh sửa.";

                return RedirectToAction(nameof(Index));
            }

            // 2. Kiểm tra học viên
            var hocVien = await _context.HocViens
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.MaHocVien == dangKyHoc.MaHocVien);

            if (hocVien == null)
            {
                ModelState.AddModelError(
                    nameof(dangKyHoc.MaHocVien),
                    "Học viên không tồn tại.");
            }
            else if (!hocVien.TrangThai &&
                     dangKyCu.MaHocVien != dangKyHoc.MaHocVien)
            {
                ModelState.AddModelError(
                    nameof(dangKyHoc.MaHocVien),
                    "Không thể chuyển đăng ký sang học viên đã ngừng hoạt động.");
            }

            // 3. Kiểm tra khóa học
            var khoaHoc = await _context.KhoaHocs
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.MaKhoaHoc == dangKyHoc.MaKhoaHoc);

            if (khoaHoc == null)
            {
                ModelState.AddModelError(
                    nameof(dangKyHoc.MaKhoaHoc),
                    "Khóa học không tồn tại.");
            }
            else if (!khoaHoc.TrangThai &&
                     dangKyCu.MaKhoaHoc != dangKyHoc.MaKhoaHoc)
            {
                ModelState.AddModelError(
                    nameof(dangKyHoc.MaKhoaHoc),
                    "Không thể chuyển đăng ký sang khóa học đã ngừng hoạt động.");
            }

            // 4. Kiểm tra học phí
            if (dangKyHoc.HocPhi < 0)
            {
                ModelState.AddModelError(
                    nameof(dangKyHoc.HocPhi),
                    "Học phí không được nhỏ hơn 0.");
            }

            // 5. Kiểm tra ngày đăng ký
            if (dangKyHoc.NgayDangKy == default)
            {
                ModelState.AddModelError(
                    nameof(dangKyHoc.NgayDangKy),
                    "Vui lòng chọn ngày đăng ký.");
            }

            // 6. Kiểm tra ngày hoàn thành
            if (dangKyHoc.NgayHoanThanh.HasValue &&
                dangKyHoc.NgayDangKy != default &&
                dangKyHoc.NgayHoanThanh.Value < dangKyHoc.NgayDangKy)
            {
                ModelState.AddModelError(
                    nameof(dangKyHoc.NgayHoanThanh),
                    "Ngày hoàn thành không được trước ngày đăng ký.");
            }

            // 7. Kiểm tra trạng thái
            var trangThaiHopLe = new[]
            {
                "Đang học",
                "Đã hoàn thành",
                "Bảo lưu",
                "Đã nghỉ",
                "Hủy"
            };

            if (string.IsNullOrWhiteSpace(dangKyHoc.TrangThai) ||
                !trangThaiHopLe.Contains(dangKyHoc.TrangThai))
            {
                ModelState.AddModelError(
                    nameof(dangKyHoc.TrangThai),
                    "Trạng thái đăng ký không hợp lệ.");
            }

            // 8. Kiểm tra đăng ký trùng
            if (!string.IsNullOrWhiteSpace(dangKyHoc.MaHocVien) &&
                !string.IsNullOrWhiteSpace(dangKyHoc.MaKhoaHoc))
            {
                bool daTonTai = await _context.DangKyHocs.AnyAsync(x =>
                    x.MaDangKy != dangKyHoc.MaDangKy &&
                    x.MaHocVien == dangKyHoc.MaHocVien &&
                    x.MaKhoaHoc == dangKyHoc.MaKhoaHoc &&
                    x.TrangThai != "Hủy");

                if (daTonTai)
                {
                    ModelState.AddModelError(
                        nameof(dangKyHoc.MaHocVien),
                        "Học viên này đã có đăng ký với khóa học này.");
                }
            }

            if (!ModelState.IsValid)
            {
                TempData["Warning"] =
                    "Thông tin đăng ký chưa hợp lệ. Vui lòng kiểm tra lại.";

                return RedirectToAction(nameof(Index));
            }

            // 9. Kiểm tra dữ liệu đào tạo liên quan
            bool coDuLieuDaoTao =
                await _context.BuoiDats.AnyAsync(
                    x => x.MaDangKy == dangKyCu.MaDangKy)
                ||
                await _context.TienDoDats.AnyAsync(
                    x => x.MaDangKy == dangKyCu.MaDangKy)
                ||
                await _context.KetQuaHocTaps.AnyAsync(
                    x => x.MaDangKy == dangKyCu.MaDangKy);

            if (coDuLieuDaoTao &&
                (dangKyCu.MaHocVien != dangKyHoc.MaHocVien ||
                 dangKyCu.MaKhoaHoc != dangKyHoc.MaKhoaHoc))
            {
                TempData["Warning"] =
                    "Đăng ký đã phát sinh dữ liệu đào tạo nên không thể đổi học viên hoặc khóa học.";

                return RedirectToAction(nameof(Index));
            }

            // 10. Kiểm tra khoản phải thu và giao dịch thành công
            var hocPhi = await _context.HocPhis
                .FirstOrDefaultAsync(
                    x => x.MaDangKy == dangKyCu.MaDangKy);

            bool daThuTienThanhCong = false;

            if (hocPhi != null)
            {
                daThuTienThanhCong =
                    await _context.ThanhToanHocPhis.AnyAsync(x =>
                        x.MaHocPhi == hocPhi.MaHocPhi &&
                        x.TrangThaiGiaoDich == "Thành công");
            }

            bool thayDoiHocPhi = dangKyCu.HocPhi != dangKyHoc.HocPhi;

            if (thayDoiHocPhi && daThuTienThanhCong)
            {
                TempData["Warning"] =
                    "Đăng ký đã có giao dịch thanh toán thành công. " +
                    "Không thể sửa học phí tại đây để tránh sai lệch lịch sử thu tiền.";

                return RedirectToAction(nameof(Index));
            }

            // 11. Cập nhật đăng ký và đồng bộ khoản phải thu
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                dangKyCu.MaHocVien = dangKyHoc.MaHocVien;
                dangKyCu.MaKhoaHoc = dangKyHoc.MaKhoaHoc;
                dangKyCu.NgayDangKy = dangKyHoc.NgayDangKy;
                dangKyCu.HocPhi = dangKyHoc.HocPhi;
                dangKyCu.TrangThai = dangKyHoc.TrangThai;
                dangKyCu.NgayHoanThanh = dangKyHoc.NgayHoanThanh;
                dangKyCu.GhiChu = dangKyHoc.GhiChu;

                if (hocPhi != null && thayDoiHocPhi)
                {
                    hocPhi.TongHocPhi = dangKyHoc.HocPhi;

                    // Chỉ đặt lại trạng thái khi chưa có giao dịch thành công.
                    hocPhi.TrangThai = "Chưa thanh toán";
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] =
                    "Cập nhật đăng ký học thành công.";
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();

                TempData["Warning"] =
                    "Không thể lưu thay đổi. Vui lòng kiểm tra dữ liệu liên quan.";
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();

                TempData["Error"] =
                    "Đã xảy ra lỗi khi cập nhật đăng ký học.";
            }

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // GET: DangKyHoc/Delete/DK0001
        // Hiển thị trang xác nhận xóa/hủy
        // =====================================================
        public async Task<IActionResult> Delete(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var dangKyHoc = await _context.DangKyHocs
                .AsNoTracking()
                .Include(d => d.MaHocVienNavigation)
                .Include(d => d.MaKhoaHocNavigation)
                .Include(d => d.HocPhiNavigation)
                .Include(d => d.BuoiDats)
                .Include(d => d.TienDoDats)
                .Include(d => d.KetQuaHocTaps)
                .FirstOrDefaultAsync(d => d.MaDangKy == id);

            if (dangKyHoc == null)
            {
                return NotFound();
            }

            return View(dangKyHoc);
        }

        // =====================================================
        // POST: DangKyHoc/Delete
        // Có dữ liệu liên quan thì chuyển trạng thái Hủy
        // =====================================================
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var dangKyHoc = await _context.DangKyHocs
                .Include(d => d.BuoiDats)
                .Include(d => d.TienDoDats)
                .Include(d => d.KetQuaHocTaps)
                .Include(d => d.HocPhiNavigation)
                .FirstOrDefaultAsync(d => d.MaDangKy == id);

            if (dangKyHoc == null)
            {
                return NotFound();
            }

            bool coDuLieuLienQuan =
                dangKyHoc.BuoiDats.Any() ||
                dangKyHoc.TienDoDats.Any() ||
                dangKyHoc.KetQuaHocTaps.Any() ||
                dangKyHoc.HocPhiNavigation != null;

            if (coDuLieuLienQuan)
            {
                dangKyHoc.TrangThai = "Hủy";

                try
                {
                    await _context.SaveChangesAsync();

                    TempData["Warning"] =
                        "Đăng ký đã phát sinh dữ liệu liên quan nên không thể xóa cứng. " +
                        "Hệ thống đã chuyển trạng thái thành 'Hủy'.";
                }
                catch (DbUpdateException)
                {
                    TempData["Error"] =
                        "Không thể hủy đăng ký do dữ liệu đang được sử dụng.";
                }

                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.DangKyHocs.Remove(dangKyHoc);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Đã xóa đăng ký học.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "Không thể xóa đăng ký. Dữ liệu có thể đã phát sinh liên kết.";
            }

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // Dropdown cho trang tạo đăng ký
        // Chỉ cho chọn học viên và khóa học đang hoạt động
        // =====================================================
        private async Task LoadDropdowns(
            string? selectedHocVien = null,
            string? selectedKhoaHoc = null)
        {
            var hocViens = await _context.HocViens
                .AsNoTracking()
                .Where(h => h.TrangThai)
                .OrderBy(h => h.HoTen)
                .ToListAsync();

            var khoaHocs = await _context.KhoaHocs
                .AsNoTracking()
                .Where(k => k.TrangThai)
                .OrderBy(k => k.TenKhoaHoc)
                .ToListAsync();

            ViewData["MaHocVien"] = new SelectList(
                hocViens,
                "MaHocVien",
                "HoTen",
                selectedHocVien);

            ViewData["MaKhoaHoc"] = new SelectList(
                khoaHocs,
                "MaKhoaHoc",
                "TenKhoaHoc",
                selectedKhoaHoc);
        }

        // =====================================================
        // Sinh mã học phí tiếp theo: HP0001, HP0002...
        // =====================================================
        private async Task<string> GenerateMaHocPhiAsync()
        {
            var danhSachMa = await _context.HocPhis
                .AsNoTracking()
                .Select(x => x.MaHocPhi)
                .ToListAsync();

            int maxNumber = 0;

            foreach (var ma in danhSachMa)
            {
                if (string.IsNullOrWhiteSpace(ma) ||
                    !ma.StartsWith(
                        "HP",
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (int.TryParse(ma.Substring(2), out int number) &&
                    number > maxNumber)
                {
                    maxNumber = number;
                }
            }

            return $"HP{maxNumber + 1:D4}";
        }
        // =====================================================
        // GET: DangKyHoc/DaHoanThanh
        // Danh sách học viên đã hoàn thành khóa học
        // =====================================================
        public async Task<IActionResult> DaHoanThanh(
            string? tuKhoa,
            string? maKhoaHoc,
            int page = 1)
        {
            page = Math.Max(1, page);

            var query = _context.DangKyHocs
                .AsNoTracking()
                .Include(d => d.MaHocVienNavigation)
                .Include(d => d.MaKhoaHocNavigation)
                .Where(d => d.TrangThai == "Đã hoàn thành")
                .AsQueryable();

            // Tìm theo mã đăng ký, mã học viên, họ tên
            // hoặc tên khóa học
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();

                query = query.Where(d =>
                    d.MaDangKy.Contains(tuKhoa) ||
                    d.MaHocVien.Contains(tuKhoa) ||
                    (d.MaHocVienNavigation != null &&
                     d.MaHocVienNavigation.HoTen.Contains(tuKhoa)) ||
                    (d.MaKhoaHocNavigation != null &&
                     d.MaKhoaHocNavigation.TenKhoaHoc.Contains(tuKhoa)));
            }

            // Lọc theo khóa học
            if (!string.IsNullOrWhiteSpace(maKhoaHoc))
            {
                query = query.Where(d => d.MaKhoaHoc == maKhoaHoc);
            }

            int totalItems = await query.CountAsync();

            int totalPages = Math.Max(
                1,
                (int)Math.Ceiling(totalItems / (double)PageSize));

            if (page > totalPages)
            {
                page = totalPages;
            }

            var danhSach = await query
                .OrderByDescending(d => d.NgayHoanThanh)
                .ThenBy(d => d.MaDangKy)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.DanhSachKhoaHoc = await _context.KhoaHocs
                .AsNoTracking()
                .OrderBy(k => k.TenKhoaHoc)
                .ToListAsync();

            ViewBag.TuKhoa = tuKhoa ?? string.Empty;
            ViewBag.MaKhoaHoc = maKhoaHoc ?? string.Empty;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = PageSize;

            return View(danhSach);
        }
    }
}
