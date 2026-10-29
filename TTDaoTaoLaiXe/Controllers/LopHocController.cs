using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TTDaoTaoLaiXe.Models;

namespace TTDaoTaoLaiXe.Controllers
{
    public class LopHocController : Controller
    {
        private readonly TTDaoTaoLaiXeContext _context;

        private const int PageSize = 10;

        public LopHocController(TTDaoTaoLaiXeContext context)
        {
            _context = context;
        }

        // GET: LopHoc
        public async Task<IActionResult> Index(
    string? search,
    string? trangThai,
    int page = 1)
        {
            if (page < 1)
                page = 1;

            // =========================
            // TỰ ĐỘNG CẬP NHẬT TRẠNG THÁI
            // =========================

            var homNay = DateOnly.FromDateTime(DateTime.Today);

            var lopCanChuyenTrangThai = await _context.LopHocs
                .Where(l =>
                    l.TrangThai == "Đang mở" &&
                    l.NgayBatDau <= homNay)
                .ToListAsync();

            foreach (var lop in lopCanChuyenTrangThai)
            {
                lop.TrangThai = "Đang học";
            }

            if (lopCanChuyenTrangThai.Any())
            {
                await _context.SaveChangesAsync();
            }


            // =========================
            // QUERY DANH SÁCH
            // =========================

            var query = _context.LopHocs
                .Include(l => l.MaKhoaHocNavigation)
                .Include(l => l.MaGiaoVienNavigation)
                .Include(l => l.HocVienLopHocs)
                .AsNoTracking()
                .AsQueryable();


            // =========================
            // TÌM KIẾM
            // =========================

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(l =>
                    l.MaLopHoc.Contains(search) ||
                    l.TenLopHoc.Contains(search));
            }


            // =========================
            // LỌC TRẠNG THÁI
            // =========================

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(l =>
                    l.TrangThai == trangThai);
            }


            // =========================
            // TỔNG SỐ BẢN GHI
            // =========================

            int totalItems = await query.CountAsync();

            int totalPages = (int)Math.Ceiling(
                totalItems / (double)PageSize);

            if (totalPages > 0 && page > totalPages)
                page = totalPages;


            // =========================
            // PHÂN TRANG
            // =========================

            var danhSach = await query
                .OrderByDescending(l => l.NgayBatDau)
                .ThenBy(l => l.MaLopHoc)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();


            // =========================
            // VIEW DATA
            // =========================

            ViewBag.Search = search;
            ViewBag.TrangThai = trangThai;

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = PageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = totalPages;


            return View(danhSach);
        }


        // =========================
        // DETAILS
        // =========================

        // GET: LopHoc/Details/LH001
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var lopHoc = await _context.LopHocs
                .Include(l => l.MaKhoaHocNavigation)
                .Include(l => l.MaGiaoVienNavigation)
                .Include(l => l.HocVienLopHocs)
                    .ThenInclude(h => h.MaHocVienNavigation)
                .Include(l => l.LichHocs)
                .FirstOrDefaultAsync(l =>
                    l.MaLopHoc == id);

            if (lopHoc == null)
                return NotFound();

            return View(lopHoc);
        }


        // GET: LopHoc/Create
        public IActionResult Create()
        {
            var lopHoc = new LopHoc
            {
                NgayBatDau = DateOnly.FromDateTime(DateTime.Today),
                SiSoToiDa = 30,
                TrangThai = "Đang mở"
            };

            LoadDropdowns();

            return View(lopHoc);
        }


        // =========================
        // CREATE - POST
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LopHoc lopHoc)
        {
            // =========================
            // 1. KIỂM TRA MODEL
            // =========================

            if (!ModelState.IsValid)
            {
                LoadDropdowns(
                    lopHoc.MaKhoaHoc,
                    lopHoc.MaGiaoVien);

                return View(lopHoc);
            }


            // =========================
            // 2. KIỂM TRA MÃ LỚP TRÙNG
            // =========================

            bool maLopDaTonTai = await _context.LopHocs
                .AnyAsync(l =>
                    l.MaLopHoc == lopHoc.MaLopHoc);

            if (maLopDaTonTai)
            {
                ModelState.AddModelError(
                    "MaLopHoc",
                    "Mã lớp đã tồn tại. Vui lòng nhập mã khác.");
            }


            // =========================
            // 3. KIỂM TRA KHÓA HỌC
            // =========================

            bool khoaHocTonTai = await _context.KhoaHocs
                .AnyAsync(k =>
                    k.MaKhoaHoc == lopHoc.MaKhoaHoc);

            if (!khoaHocTonTai)
            {
                ModelState.AddModelError(
                    "MaKhoaHoc",
                    "Khóa học không tồn tại.");
            }


            // 4. KIỂM TRA GIÁO VIÊN VÀ ĐIỀU KIỆN GIẢNG DẠY
            var loiGiaoVien = await KiemTraGiaoVienChoLopAsync(
                lopHoc.MaGiaoVien,
                lopHoc.MaKhoaHoc);

            if (loiGiaoVien != null)
            {
                ModelState.AddModelError(
                    "MaGiaoVien",
                    loiGiaoVien);
            }


            // =========================
            // 5. KIỂM TRA NGÀY
            // =========================

            if (lopHoc.NgayKetThuc.HasValue &&
                lopHoc.NgayKetThuc.Value < lopHoc.NgayBatDau)
            {
                ModelState.AddModelError(
                    "NgayKetThuc",
                    "Ngày kết thúc không được trước ngày bắt đầu.");
            }


            // =========================
            // 6. KIỂM TRA SĨ SỐ
            // =========================

            if (lopHoc.SiSoToiDa <= 0)
            {
                ModelState.AddModelError(
                    "SiSoToiDa",
                    "Sĩ số tối đa phải lớn hơn 0.");
            }


            // =========================
            // 7. KIỂM TRA TRẠNG THÁI
            // =========================

            var danhSachTrangThai = new[]
{
    "Đang mở",
    "Đang học",
    "Tạm ngưng",
    "Đã kết thúc"
};

            if (!danhSachTrangThai.Contains(lopHoc.TrangThai))
            {
                ModelState.AddModelError(
                    "TrangThai",
                    "Trạng thái lớp học không hợp lệ.");
            }


            // =========================
            // 8. NẾU CÓ LỖI -> QUAY LẠI FORM
            // =========================

            if (!ModelState.IsValid)
            {
                LoadDropdowns(
                    lopHoc.MaKhoaHoc,
                    lopHoc.MaGiaoVien);

                return View(lopHoc);
            }


            // =========================
            // 9. THÊM DỮ LIỆU
            // =========================

            _context.LopHocs.Add(lopHoc);

            await _context.SaveChangesAsync();


            // =========================
            // 10. THÔNG BÁO
            // =========================

            TempData["Success"] =
                "Thêm lớp học thành công.";


            // =========================
            // 11. QUAY VỀ DANH SÁCH
            // =========================

            return RedirectToAction(nameof(Index));
        }


        

        // =========================
        // EDIT MODAL - GET
        // =========================

        [HttpGet]
        public async Task<IActionResult> EditModal(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var lopHoc = await _context.LopHocs
                .FirstOrDefaultAsync(l => l.MaLopHoc == id);

            if (lopHoc == null)
                return NotFound();

            LoadDropdowns(
                lopHoc.MaKhoaHoc,
                lopHoc.MaGiaoVien);

            return PartialView("_EditModal", lopHoc);
        }

        // =========================
        // EDIT - POST
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            string id,
            LopHoc lopHoc)
        {
            if (id != lopHoc.MaLopHoc)
                return NotFound();

            var lopHocCu = await _context.LopHocs
                .Include(l => l.HocVienLopHocs)
                .Include(l => l.LichHocs)
                .FirstOrDefaultAsync(l =>
                    l.MaLopHoc == id);

            if (lopHocCu == null)
                return NotFound();

            // Kiểm tra khóa học
            bool khoaHocTonTai = await _context.KhoaHocs
                .AnyAsync(k =>
                    k.MaKhoaHoc == lopHoc.MaKhoaHoc);
            // Kiểm tra giáo viên và điều kiện giảng dạy
            var loiGiaoVien = await KiemTraGiaoVienChoLopAsync(
                lopHoc.MaGiaoVien,
                lopHoc.MaKhoaHoc);

            if (loiGiaoVien != null)
            {
                ModelState.AddModelError(
                    "MaGiaoVien",
                    loiGiaoVien);
            }
            if (!khoaHocTonTai)
            {
                ModelState.AddModelError(
                    "MaKhoaHoc",
                    "Khóa học không tồn tại.");
            }

            // Kiểm tra ngày
            if (lopHoc.NgayKetThuc.HasValue &&
                lopHoc.NgayKetThuc.Value < lopHoc.NgayBatDau)
            {
                ModelState.AddModelError(
                    "NgayKetThuc",
                    "Ngày kết thúc không được trước ngày bắt đầu.");
            }

            // Không được giảm sĩ số dưới số học viên hiện tại
            int soHocVien =
                lopHocCu.HocVienLopHocs.Count;

            if (lopHoc.SiSoToiDa < soHocVien)
            {
                ModelState.AddModelError(
                    "SiSoToiDa",
                    $"Sĩ số tối đa không được nhỏ hơn số học viên hiện tại ({soHocVien}).");
            }

            if (!ModelState.IsValid)
            {
                LoadDropdowns(
                    lopHoc.MaKhoaHoc,
                    lopHoc.MaGiaoVien);

                return View(lopHoc);
            }

            lopHocCu.TenLopHoc = lopHoc.TenLopHoc;
            lopHocCu.MaKhoaHoc = lopHoc.MaKhoaHoc;
            lopHocCu.MaGiaoVien = lopHoc.MaGiaoVien;
            lopHocCu.NgayBatDau = lopHoc.NgayBatDau;
            lopHocCu.NgayKetThuc = lopHoc.NgayKetThuc;
            lopHocCu.SiSoToiDa = lopHoc.SiSoToiDa;
            lopHocCu.TrangThai = lopHoc.TrangThai;
            lopHocCu.MoTa = lopHoc.MoTa;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Cập nhật lớp học thành công.";

            return RedirectToAction(nameof(Index));
        }


        // =========================
        // DELETE / NGỪNG HOẠT ĐỘNG
        // =========================

        // GET: LopHoc/Delete/LH001
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var lopHoc = await _context.LopHocs
                .Include(l => l.MaKhoaHocNavigation)
                .Include(l => l.MaGiaoVienNavigation)
                .Include(l => l.HocVienLopHocs)
                .Include(l => l.LichHocs)
                .FirstOrDefaultAsync(l =>
                    l.MaLopHoc == id);

            if (lopHoc == null)
                return NotFound();

            return View(lopHoc);
        }

        // GET: LopHoc/GetHocVienCoTheXep?maLopHoc=LH001
        [HttpGet]
        public async Task<IActionResult> GetHocVienCoTheXep(string maLopHoc)
        {
            if (string.IsNullOrWhiteSpace(maLopHoc))
                return BadRequest("Mã lớp không hợp lệ.");

            var lopHoc = await _context.LopHocs
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.MaLopHoc == maLopHoc);

            if (lopHoc == null)
                return NotFound("Không tìm thấy lớp học.");

            if (lopHoc.TrangThai != "Đang mở" &&
                lopHoc.TrangThai != "Đang học")
            {
                return BadRequest(
                    "Chỉ được xếp học viên vào lớp đang mở hoặc đang học.");
            }

            // Chỉ lấy học viên hiện đang học trong lớp này.
            var daXep = await _context.HocVienLopHocs
                .Where(x =>
                    x.MaLopHoc == maLopHoc &&
                    x.TrangThai == "Đang học" &&
                    x.NgayRaLop == null)
                .Select(x => x.MaHocVien)
                .ToListAsync();

            var hocVienCoTheXep = await _context.HocViens
                .AsNoTracking()
                .Where(hv =>
                    hv.TrangThai &&
                    !daXep.Contains(hv.MaHocVien) &&
                    hv.DangKyHocs.Any(dk =>
                        dk.MaKhoaHoc == lopHoc.MaKhoaHoc &&
                        dk.TrangThai != null &&
                        dk.TrangThai.Trim().ToUpper() == "ĐANG HỌC"))
                .OrderBy(hv => hv.HoTen)
                .Select(hv => new
                {
                    maHocVien = hv.MaHocVien,
                    hoTen = hv.HoTen,
                    cccd = hv.Cccd,
                    soDienThoai = hv.SoDienThoai
                })
                .ToListAsync();

            return Json(hocVienCoTheXep);
        }


        // POST: LopHoc/XepHocVien


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XepHocVien(
            string maLopHoc,
            List<string> danhSachMaHocVien)
        {
            if (string.IsNullOrWhiteSpace(maLopHoc))
                return BadRequest("Mã lớp không hợp lệ.");

            if (danhSachMaHocVien == null || danhSachMaHocVien.Count == 0)
            {
                TempData["Error"] = "Vui lòng chọn ít nhất một học viên.";
                return RedirectToAction(nameof(Details), new { id = maLopHoc });
            }

            danhSachMaHocVien = danhSachMaHocVien
                .Where(ma => !string.IsNullOrWhiteSpace(ma))
                .Select(ma => ma.Trim())
                .Distinct()
                .ToList();

            if (danhSachMaHocVien.Count == 0)
            {
                TempData["Error"] = "Danh sách học viên không hợp lệ.";
                return RedirectToAction(nameof(Details), new { id = maLopHoc });
            }

            var lopHoc = await _context.LopHocs
                .Include(l => l.HocVienLopHocs)
                .FirstOrDefaultAsync(l => l.MaLopHoc == maLopHoc);

            if (lopHoc == null)
                return NotFound("Không tìm thấy lớp học.");

            if (lopHoc.TrangThai != "Đang mở" &&
                lopHoc.TrangThai != "Đang học")
            {
                TempData["Error"] =
                    "Không thể xếp học viên vào lớp đang tạm ngưng hoặc đã kết thúc.";

                return RedirectToAction(nameof(Details), new { id = maLopHoc });
            }

            // Chỉ tính học viên hiện đang học thực tế trong lớp.
            var hocVienDangHoc = lopHoc.HocVienLopHocs
                .Where(x =>
                    x.TrangThai == "Đang học" &&
                    x.NgayRaLop == null)
                .ToList();

            var maHocVienDangHoc = hocVienDangHoc
                .Select(x => x.MaHocVien)
                .ToHashSet();

            // Không cho xếp trùng học viên đang học trong cùng lớp.
            var maBiTrung = danhSachMaHocVien
                .Where(ma => maHocVienDangHoc.Contains(ma))
                .ToList();

            if (maBiTrung.Count > 0)
            {
                TempData["Error"] =
                    "Có học viên đã thuộc lớp này. Vui lòng tải lại danh sách và thử lại.";

                return RedirectToAction(nameof(Details), new { id = maLopHoc });
            }

            // Kiểm tra sĩ số tối đa.
            int soChoConLai = lopHoc.SiSoToiDa - hocVienDangHoc.Count;

            if (danhSachMaHocVien.Count > soChoConLai)
            {
                TempData["Error"] =
                    $"Lớp chỉ còn {Math.Max(0, soChoConLai)} chỗ trống. " +
                    "Vui lòng giảm số học viên được chọn.";

                return RedirectToAction(nameof(Details), new { id = maLopHoc });
            }

            // Lấy học viên và các đăng ký khóa học để kiểm tra nghiệp vụ.
            var hocViens = await _context.HocViens
                .Include(hv => hv.DangKyHocs)
                .Where(hv => danhSachMaHocVien.Contains(hv.MaHocVien))
                .ToListAsync();

            if (hocViens.Count != danhSachMaHocVien.Count)
            {
                TempData["Error"] =
                    "Có học viên không tồn tại. Vui lòng tải lại danh sách.";

                return RedirectToAction(nameof(Details), new { id = maLopHoc });
            }

            // Kiểm tra trạng thái và đăng ký của từng học viên.
            foreach (var hv in hocViens)
            {
                if (!hv.TrangThai)
                {
                    TempData["Error"] =
                        $"Học viên {hv.HoTen} đang ngừng hoạt động.";

                    return RedirectToAction(nameof(Details), new { id = maLopHoc });
                }

                bool dangKyHopLe = hv.DangKyHocs.Any(dk =>
                    dk.MaKhoaHoc == lopHoc.MaKhoaHoc &&
                    !string.IsNullOrWhiteSpace(dk.TrangThai) &&
                    dk.TrangThai.Trim().ToUpper() == "ĐANG HỌC");

                if (!dangKyHopLe)
                {
                    TempData["Error"] =
                        $"Học viên {hv.HoTen} chưa có đăng ký đang học " +
                        "thuộc khóa học của lớp.";

                    return RedirectToAction(nameof(Details), new { id = maLopHoc });
                }
            }

            var ngayHienTai = DateOnly.FromDateTime(DateTime.Today);

            foreach (var hv in hocViens)
            {
                // Do bảng dùng khóa chính ghép MaHocVien + MaLopHoc,
                // cần tìm bản ghi cũ trước khi quyết định thêm mới.
                var banGhiCu = lopHoc.HocVienLopHocs
                    .FirstOrDefault(x => x.MaHocVien == hv.MaHocVien);

                if (banGhiCu != null)
                {
                    // Học viên từng nghỉ khỏi lớp: kích hoạt lại bản ghi cũ.
                    banGhiCu.NgayVaoLop = ngayHienTai;
                    banGhiCu.NgayRaLop = null;
                    banGhiCu.TrangThai = "Đang học";
                    banGhiCu.LyDoRaLop = null;
                }
                else
                {
                    // Học viên chưa từng thuộc lớp: tạo bản ghi mới.
                    _context.HocVienLopHocs.Add(new HocVienLopHoc
                    {
                        MaHocVien = hv.MaHocVien,
                        MaLopHoc = lopHoc.MaLopHoc,
                        NgayVaoLop = ngayHienTai,
                        NgayRaLop = null,
                        TrangThai = "Đang học"
                    });
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Đã xếp thành công {hocViens.Count} học viên vào lớp.";

            return RedirectToAction(nameof(Details), new { id = maLopHoc });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RoiLop(
            string maLopHoc,
            string maHocVien,
            string lyDoRaLop)
        {
            if (string.IsNullOrWhiteSpace(maLopHoc) ||
                string.IsNullOrWhiteSpace(maHocVien))
            {
                TempData["Error"] = "Thông tin lớp học hoặc học viên không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(lyDoRaLop))
            {
                TempData["Error"] = "Vui lòng nhập lý do rời lớp.";
                return RedirectToAction(nameof(Details), new { id = maLopHoc });
            }

            lyDoRaLop = lyDoRaLop.Trim();

            if (lyDoRaLop.Length > 500)
            {
                TempData["Error"] = "Lý do rời lớp không được vượt quá 500 ký tự.";
                return RedirectToAction(nameof(Details), new { id = maLopHoc });
            }

            var lopHoc = await _context.LopHocs
                .FirstOrDefaultAsync(l => l.MaLopHoc == maLopHoc);

            if (lopHoc == null)
                return NotFound("Không tìm thấy lớp học.");

            if (lopHoc.TrangThai == "Đã kết thúc")
            {
                TempData["Error"] =
                    "Không thể cho học viên rời lớp đã kết thúc.";

                return RedirectToAction(nameof(Details), new { id = maLopHoc });
            }

            var hocVienTrongLop = await _context.HocVienLopHocs
                .FirstOrDefaultAsync(x =>
                    x.MaLopHoc == maLopHoc &&
                    x.MaHocVien == maHocVien);

            if (hocVienTrongLop == null)
            {
                TempData["Error"] =
                    "Không tìm thấy học viên trong lớp này.";

                return RedirectToAction(nameof(Details), new { id = maLopHoc });
            }

            if (hocVienTrongLop.TrangThai != "Đang học" ||
                hocVienTrongLop.NgayRaLop.HasValue)
            {
                TempData["Warning"] =
                    "Học viên này đã rời lớp hoặc không còn trong trạng thái đang học.";

                return RedirectToAction(nameof(Details), new { id = maLopHoc });
            }

            hocVienTrongLop.NgayRaLop = DateOnly.FromDateTime(DateTime.Today);
            hocVienTrongLop.TrangThai = "Đã rời lớp";
            hocVienTrongLop.LyDoRaLop = lyDoRaLop;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Đã cập nhật học viên {maHocVien} rời lớp thành công.";

            return RedirectToAction(nameof(Details), new { id = maLopHoc });
        }



        // POST: LopHoc/Delete/LH001
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var lopHoc = await _context.LopHocs
                .Include(l => l.HocVienLopHocs)
                .Include(l => l.LichHocs)
                .FirstOrDefaultAsync(l =>
                    l.MaLopHoc == id);

            if (lopHoc == null)
                return NotFound();

            // ==========================================
            // 1. LỚP ĐÃ PHÁT SINH DỮ LIỆU
            // ==========================================
            if (lopHoc.HocVienLopHocs.Any() ||
                lopHoc.LichHocs.Any())
            {
                // Không xóa cứng
                lopHoc.TrangThai = "Tạm ngưng";

                await _context.SaveChangesAsync();

                TempData["Warning"] =
                    "Lớp đã phát sinh dữ liệu nên không thể xóa. " +
                    "Hệ thống đã chuyển lớp sang trạng thái Tạm ngưng.";

                return RedirectToAction(nameof(Index));
            }

            // ==========================================
            // 2. LỚP CHƯA PHÁT SINH DỮ LIỆU
            // ==========================================
            _context.LopHocs.Remove(lopHoc);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Xóa lớp học thành công.";

            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(
            string id,
            string trangThai)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var lopHoc = await _context.LopHocs
                .FirstOrDefaultAsync(l => l.MaLopHoc == id);

            if (lopHoc == null)
                return NotFound();

            var danhSachTrangThai = new[]
            {
        "Đang mở",
        "Đang học",
        "Tạm ngưng",
        "Đã kết thúc"
    };

            if (string.IsNullOrWhiteSpace(trangThai) ||
                !danhSachTrangThai.Contains(trangThai))
            {
                TempData["Error"] = "Trạng thái lớp học không hợp lệ.";

                return RedirectToAction(nameof(Details), new { id });
            }

            // Không cho chuyển trạng thái nếu không có thay đổi.
            if (lopHoc.TrangThai == trangThai)
            {
                TempData["Warning"] = "Lớp học đang ở trạng thái này rồi.";

                return RedirectToAction(nameof(Details), new { id });
            }

            // Lớp đã kết thúc không được mở lại.
            if (lopHoc.TrangThai == "Đã kết thúc")
            {
                TempData["Error"] =
                    "Lớp học đã kết thúc nên không thể chuyển về trạng thái khác.";

                return RedirectToAction(nameof(Details), new { id });
            }

            // Không cho chuyển trực tiếp từ Đang mở sang Đã kết thúc.
            if (lopHoc.TrangThai == "Đang mở" &&
                trangThai == "Đã kết thúc")
            {
                TempData["Error"] =
                    "Lớp đang mở chưa thể kết thúc trực tiếp. " +
                    "Hãy chuyển sang trạng thái Đang học trước.";

                return RedirectToAction(nameof(Details), new { id });
            }

            // Cập nhật trạng thái.
            lopHoc.TrangThai = trangThai;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Đã chuyển trạng thái lớp sang \"{trangThai}\" thành công.";

            return RedirectToAction(nameof(Details), new { id });
        }

        // =========================
        // DROPDOWN
        // =========================

        private void LoadDropdowns(
            string? maKhoaHoc = null,
            string? maGiaoVien = null)
        {
            ViewBag.MaKhoaHoc = new SelectList(
                _context.KhoaHocs
                    .Where(k => k.TrangThai)
                    .OrderBy(k => k.MaKhoaHoc)
                    .ToList(),
                "MaKhoaHoc",
                "TenKhoaHoc",
                maKhoaHoc);

            ViewBag.MaGiaoVien = new SelectList(
                _context.GiaoViens
                    .Where(g => g.TrangThai)
                    .OrderBy(g => g.HoTen)
                    .ToList(),
                "MaGiaoVien",
                "HoTen",
                maGiaoVien);
        }
        private async Task<string?> KiemTraGiaoVienChoLopAsync(
    string? maGiaoVien,
    string? maKhoaHoc)
        {
            // Giáo viên là thông tin tùy chọn khi tạo lớp.
            if (string.IsNullOrWhiteSpace(maGiaoVien))
                return null;

            var giaoVien = await _context.GiaoViens
                .AsNoTracking()
                .FirstOrDefaultAsync(g =>
                    g.MaGiaoVien == maGiaoVien);

            if (giaoVien == null)
                return "Giáo viên không tồn tại.";

            if (!giaoVien.TrangThai)
                return "Không thể phân công giáo viên đã ngừng hoạt động.";

            var khoaHoc = await _context.KhoaHocs
                .AsNoTracking()
                .FirstOrDefaultAsync(k =>
                    k.MaKhoaHoc == maKhoaHoc);

            if (khoaHoc == null)
                return "Khóa học không tồn tại.";

            if (string.IsNullOrWhiteSpace(khoaHoc.HangGplx))
                return "Khóa học chưa được khai báo hạng GPLX.";

            var hangCanDay = khoaHoc.HangGplx.Trim();

            var danhSachHangDuocPhep = (
                giaoVien.HangDuocPhepDay ?? string.Empty
            )
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(h => h.Trim())
                .Where(h => !string.IsNullOrWhiteSpace(h));

            bool duDieuKien = danhSachHangDuocPhep.Any(h =>
                string.Equals(
                    h,
                    hangCanDay,
                    StringComparison.OrdinalIgnoreCase));

            if (!duDieuKien)
            {
                return $"Giáo viên không đủ điều kiện giảng dạy hạng {hangCanDay}.";
            }

            return null;
        }
    }
}