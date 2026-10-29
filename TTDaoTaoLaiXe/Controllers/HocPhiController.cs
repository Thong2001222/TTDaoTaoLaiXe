
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TTDaoTaoLaiXe.Models;
using TTDaoTaoLaiXe.ViewModels.HocPhi;
using TTDaoTaoLaiXe.Services.Interfaces;

namespace TTDaoTaoLaiXe.Controllers
{
    [Authorize]
    public class HocPhiController : Controller
    {
        private readonly TTDaoTaoLaiXeContext _context;
        private readonly IVnPayService _vnPayService;

        private const int PageSize = 10;

        public HocPhiController(
            TTDaoTaoLaiXeContext context,
            IVnPayService vnPayService)
        {
            _context = context;
            _vnPayService = vnPayService;
        }

        public async Task<IActionResult> Index(
            string? tuKhoa,
            string? trangThaiLoc,
            DateOnly? tuNgay,
            DateOnly? denNgay,
            int page = 1)
        {
            if (page < 1)
                page = 1;

            if (tuNgay.HasValue &&
                denNgay.HasValue &&
                tuNgay.Value > denNgay.Value)
            {
                TempData["Error"] =
                    "Ngày bắt đầu không được lớn hơn ngày kết thúc.";

                return RedirectToAction(nameof(Index));
            }

            var hocPhis = await _context.HocPhis
                .AsNoTracking()
                .Include(h => h.MaDangKyNavigation)
                    .ThenInclude(d => d.MaHocVienNavigation)
                .Include(h => h.MaDangKyNavigation)
                    .ThenInclude(d => d.MaKhoaHocNavigation)
                .Include(h => h.ThanhToanHocPhis)
                .OrderByDescending(h => h.MaHocPhi)
                .ToListAsync();

            var today = DateOnly.FromDateTime(DateTime.Today);

            var danhSach = hocPhis.Select(h =>
            {
                var ngayHienTai = DateTime.Now;

                var giaoDichDaDenNgay = h.ThanhToanHocPhis
    .Where(t =>
        t.TrangThaiGiaoDich == "Thành công" &&
        t.NgayThanhToan <= ngayHienTai)
    .ToList();

                var daThu = giaoDichDaDenNgay.Sum(t => t.SoTien);

                var conNo = Math.Max(0m, h.TongHocPhi - daThu);

                string trangThai;

                if (conNo <= 0)
                {
                    trangThai = "Đã thanh toán";
                }
                else if (h.HanThanhToan.HasValue &&
                         h.HanThanhToan.Value < today)
                {
                    trangThai = "Quá hạn";
                }
                else if (daThu > 0)
                {
                    trangThai = "Thanh toán một phần";
                }
                else
                {
                    trangThai = "Chưa thanh toán";
                }

                return new HocPhiIndexViewModel
                {
                    MaHocPhi = h.MaHocPhi,
                    MaDangKy = h.MaDangKy,
                    MaHocVien =
                        h.MaDangKyNavigation.MaHocVienNavigation.MaHocVien,
                    HoTenHocVien =
                        h.MaDangKyNavigation.MaHocVienNavigation.HoTen,
                    TenKhoaHoc =
                        h.MaDangKyNavigation.MaKhoaHocNavigation.TenKhoaHoc,
                    TongHocPhi = h.TongHocPhi,
                    DaThu = daThu,
                    HanThanhToan = h.HanThanhToan,
                    TrangThai = trangThai,
                    GhiChu = h.GhiChu,
                    SoLanThanhToan = giaoDichDaDenNgay.Count
                };
            }).ToList();

            // Tìm kiếm theo mã học phí, mã học viên,
            // tên học viên, mã đăng ký hoặc tên khóa học.
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var keyword = tuKhoa.Trim();

                danhSach = danhSach.Where(x =>
                    x.MaHocPhi.Contains(keyword,
                        StringComparison.OrdinalIgnoreCase) ||
                    x.MaDangKy.Contains(keyword,
                        StringComparison.OrdinalIgnoreCase) ||
                    x.MaHocVien.Contains(keyword,
                        StringComparison.OrdinalIgnoreCase) ||
                    x.HoTenHocVien.Contains(keyword,
                        StringComparison.OrdinalIgnoreCase) ||
                    x.TenKhoaHoc.Contains(keyword,
                        StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Lọc theo hạn thanh toán.
            if (tuNgay.HasValue)
            {
                danhSach = danhSach.Where(x =>
                    x.HanThanhToan.HasValue &&
                    x.HanThanhToan.Value >= tuNgay.Value)
                    .ToList();
            }

            if (denNgay.HasValue)
            {
                danhSach = danhSach.Where(x =>
                    x.HanThanhToan.HasValue &&
                    x.HanThanhToan.Value <= denNgay.Value)
                    .ToList();
            }

            // Lọc theo trạng thái công nợ được tính từ giao dịch.
            if (!string.IsNullOrWhiteSpace(trangThaiLoc))
            {
                danhSach = danhSach
                    .Where(x => x.TrangThai == trangThaiLoc)
                    .ToList();
            }

            var totalRecords = danhSach.Count;
            var totalPages = (int)Math.Ceiling(
                totalRecords / (double)PageSize);

            if (totalPages > 0 && page > totalPages)
                page = totalPages;

            var tongPhaiThu = danhSach.Sum(x => x.TongHocPhi);
            var tongDaThu = danhSach.Sum(x => x.DaThu);
            var tongConNo = danhSach.Sum(x => x.ConNo);

            var tongQuaHan = danhSach
                .Where(x => x.TrangThai == "Quá hạn")
                .Sum(x => x.ConNo);

            var model = new HocPhiIndexPageViewModel
            {
                DanhSach = danhSach
                    .Skip((page - 1) * PageSize)
                    .Take(PageSize)
                    .ToList(),

                TuKhoa = tuKhoa ?? string.Empty,
                TrangThaiLoc = trangThaiLoc ?? string.Empty,
                TuNgay = tuNgay,
                DenNgay = denNgay,

                TongPhaiThu = tongPhaiThu,
                TongDaThu = tongDaThu,
                TongConNo = tongConNo,
                TongQuaHan = tongQuaHan,

                Page = page,
                PageSize = PageSize,
                TotalPages = totalPages,
                TotalRecords = totalRecords
            };

            return View(model);
        }

        public async Task<IActionResult> Details(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var hocPhi = await _context.HocPhis
                .AsNoTracking()
                .Include(h => h.MaDangKyNavigation)
                    .ThenInclude(d => d.MaHocVienNavigation)
                .Include(h => h.MaDangKyNavigation)
                    .ThenInclude(d => d.MaKhoaHocNavigation)
                .Include(h => h.ThanhToanHocPhis)
                    .ThenInclude(t => t.NguoiThuNavigation)
                .FirstOrDefaultAsync(h => h.MaHocPhi == id);

            if (hocPhi == null)
                return NotFound();

            var now = DateTime.Now;
            var today = DateOnly.FromDateTime(now);

            // Tạm bỏ qua giao dịch có ngày trong tương lai.
            // Khi tích hợp VNPay cần xác minh trạng thái giao dịch phía máy chủ.
            var giaoDichDaDenNgay = hocPhi.ThanhToanHocPhis
    .Where(t =>
        t.TrangThaiGiaoDich == "Thành công" &&
        t.NgayThanhToan <= now)
    .OrderByDescending(t => t.NgayThanhToan)
    .ToList();

            var daThu = giaoDichDaDenNgay.Sum(t => t.SoTien);
            var conNo = Math.Max(0m, hocPhi.TongHocPhi - daThu);

            string trangThai;

            if (conNo <= 0)
            {
                trangThai = "Đã thanh toán";
            }
            else if (hocPhi.HanThanhToan.HasValue &&
                     hocPhi.HanThanhToan.Value < today)
            {
                trangThai = "Quá hạn";
            }
            else if (daThu > 0)
            {
                trangThai = "Thanh toán một phần";
            }
            else
            {
                trangThai = "Chưa thanh toán";
            }

            var model = new HocPhiDetailsViewModel
            {
                MaHocPhi = hocPhi.MaHocPhi,
                MaDangKy = hocPhi.MaDangKy,

                MaHocVien = hocPhi.MaDangKyNavigation
                    .MaHocVienNavigation.MaHocVien,

                HoTenHocVien = hocPhi.MaDangKyNavigation
                    .MaHocVienNavigation.HoTen,

                SoDienThoai = hocPhi.MaDangKyNavigation
                    .MaHocVienNavigation.SoDienThoai,

                Email = hocPhi.MaDangKyNavigation
                    .MaHocVienNavigation.Email,

                TenKhoaHoc = hocPhi.MaDangKyNavigation
                    .MaKhoaHocNavigation.TenKhoaHoc,

                TongHocPhi = hocPhi.TongHocPhi,
                DaThu = daThu,
                HanThanhToan = hocPhi.HanThanhToan,
                TrangThai = trangThai,
                GhiChu = hocPhi.GhiChu,

                LichSuThanhToan = giaoDichDaDenNgay
                    .Select(t => new ThanhToanHocPhiItemViewModel
                    {
                        MaThanhToan = t.MaThanhToan,
                        NgayThanhToan = t.NgayThanhToan,
                        SoTien = t.SoTien,
                        PhuongThuc = t.PhuongThuc,
                        MaGiaoDich = t.MaGiaoDich,
                        NguoiThu = t.NguoiThuNavigation != null
                            ? t.NguoiThuNavigation.HoTen
                            : t.NguoiThu,
                        GhiChu = t.GhiChu
                    })
                    .ToList()
            };

            return View(model);
        }

        // GET: HocPhi/Thu/HP0001
        [HttpGet]
        public async Task<IActionResult> Thu(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var hocPhi = await _context.HocPhis
                .AsNoTracking()
                .Include(h => h.MaDangKyNavigation)
                    .ThenInclude(d => d.MaHocVienNavigation)
                .Include(h => h.MaDangKyNavigation)
                    .ThenInclude(d => d.MaKhoaHocNavigation)
                .Include(h => h.ThanhToanHocPhis)
                .FirstOrDefaultAsync(h => h.MaHocPhi == id);

            if (hocPhi == null)
                return NotFound();

            var now = DateTime.Now;

            var daThu = hocPhi.ThanhToanHocPhis
    .Where(t =>
        t.TrangThaiGiaoDich == "Thành công" &&
        t.NgayThanhToan <= now)
    .Sum(t => t.SoTien);

            var conNo = Math.Max(0m, hocPhi.TongHocPhi - daThu);

            if (conNo <= 0)
            {
                TempData["Error"] = "Khoản học phí này đã thanh toán đủ.";
                return RedirectToAction(nameof(Details), new { id });
            }

            ViewBag.MaHocPhi = hocPhi.MaHocPhi;
            ViewBag.MaDangKy = hocPhi.MaDangKy;
            ViewBag.MaHocVien =
                hocPhi.MaDangKyNavigation.MaHocVienNavigation.MaHocVien;
            ViewBag.HoTenHocVien =
                hocPhi.MaDangKyNavigation.MaHocVienNavigation.HoTen;
            ViewBag.TenKhoaHoc =
                hocPhi.MaDangKyNavigation.MaKhoaHocNavigation.TenKhoaHoc;
            ViewBag.TongHocPhi = hocPhi.TongHocPhi;
            ViewBag.DaThu = daThu;
            ViewBag.ConNo = conNo;

            return View(new ThanhToanHocPhiCreateViewModel
            {
                MaHocPhi = hocPhi.MaHocPhi,
                PhuongThuc = "Tiền mặt"
            });
        }

        // POST: HocPhi/Thu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Thu(
            ThanhToanHocPhiCreateViewModel model)
        {
            // Chỉ cho phép ghi nhận khoản thu trực tiếp.
            // Thanh toán trực tuyến sẽ được xử lý riêng qua VNPay.
            var phuongThucHopLe = new[]
            {
                "Tiền mặt",
                "Chuyển khoản"
            };

            if (!phuongThucHopLe.Contains(model.PhuongThuc))
            {
                ModelState.AddModelError(
                    nameof(model.PhuongThuc),
                    "Phương thức thanh toán không hợp lệ.");
            }

            if (!ModelState.IsValid)
            {
                await NapDuLieuThuHocPhi(model.MaHocPhi);
                return View(model);
            }

            var maTaiKhoan = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(maTaiKhoan))
                return Forbid();

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    System.Data.IsolationLevel.Serializable);

            try
            {
                var hocPhi = await _context.HocPhis
                    .Include(h => h.ThanhToanHocPhis)
                    .FirstOrDefaultAsync(
                        h => h.MaHocPhi == model.MaHocPhi);

                if (hocPhi == null)
                {
                    await transaction.RollbackAsync();
                    return NotFound();
                }

                var now = DateTime.Now;

                var daThu = hocPhi.ThanhToanHocPhis
    .Where(t =>
        t.TrangThaiGiaoDich == "Thành công" &&
        t.NgayThanhToan <= now)
    .Sum(t => t.SoTien);

                var conNo = Math.Max(
                    0m,
                    hocPhi.TongHocPhi - daThu);

                if (model.SoTien <= 0 || model.SoTien > conNo)
                {
                    await transaction.RollbackAsync();

                    ModelState.AddModelError(
                        nameof(model.SoTien),
                        $"Số tiền phải lớn hơn 0 và không vượt quá công nợ {conNo:N0} đồng.");

                    await NapDuLieuThuHocPhi(model.MaHocPhi);
                    return View(model);
                }

                // Sinh mã giao dịch dạng TT001, TT002...
                var danhSachMa = await _context.ThanhToanHocPhis
                    .Select(t => t.MaThanhToan)
                    .ToListAsync();

                var soThuTuLonNhat = danhSachMa
                    .Where(ma => ma.StartsWith(
                        "TT", StringComparison.OrdinalIgnoreCase))
                    .Select(ma =>
                    {
                        var phanSo = ma.Substring(2);
                        return int.TryParse(phanSo, out var so)
                            ? so
                            : 0;
                    })
                    .DefaultIfEmpty(0)
                    .Max();

                var maThanhToan =
                    $"TT{soThuTuLonNhat + 1:D3}";

                var thanhToan = new ThanhToanHocPhi
                {
                    MaThanhToan = maThanhToan,
                    MaHocPhi = hocPhi.MaHocPhi,
                    NgayThanhToan = now,
                    SoTien = model.SoTien,
                    PhuongThuc = model.PhuongThuc,
                    MaGiaoDich = string.IsNullOrWhiteSpace(
                        model.MaGiaoDich)
                        ? null
                        : model.MaGiaoDich.Trim(),
                    TrangThaiGiaoDich = "Thành công",
                    NguoiThu = maTaiKhoan,
                    GhiChu = string.IsNullOrWhiteSpace(model.GhiChu)
                        ? null
                        : model.GhiChu.Trim()
                };

                _context.ThanhToanHocPhis.Add(thanhToan);

                // Cập nhật trạng thái khoản học phí.
                var tongDaThuSauGiaoDich = daThu + model.SoTien;
                var conNoSauGiaoDich = Math.Max(
                    0m,
                    hocPhi.TongHocPhi - tongDaThuSauGiaoDich);

                var today = DateOnly.FromDateTime(now);

                if (conNoSauGiaoDich <= 0)
                {
                    hocPhi.TrangThai = "Đã thanh toán";
                }
                else if (hocPhi.HanThanhToan.HasValue &&
                         hocPhi.HanThanhToan.Value < today)
                {
                    hocPhi.TrangThai = "Quá hạn";
                }
                else if (tongDaThuSauGiaoDich > 0)
                {
                    hocPhi.TrangThai = "Thanh toán một phần";
                }
                else
                {
                    hocPhi.TrangThai = "Chưa thanh toán";
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] =
                    $"Ghi nhận thanh toán {model.SoTien:N0} đồng thành công. Mã giao dịch thu: {maThanhToan}.";


                return RedirectToAction(
                    nameof(BienLai),
                    new { id = maThanhToan });

            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    string.Empty,
                    "Không thể lưu khoản thu. Có thể mã thanh toán đã bị trùng. Vui lòng thử lại.");

                await NapDuLieuThuHocPhi(model.MaHocPhi);
                return View(model);
            }
        }

        private async Task NapDuLieuThuHocPhi(string? maHocPhi)
        {
            if (string.IsNullOrWhiteSpace(maHocPhi))
                return;

            var hocPhi = await _context.HocPhis
                .AsNoTracking()
                .Include(h => h.MaDangKyNavigation)
                    .ThenInclude(d => d.MaHocVienNavigation)
                .Include(h => h.MaDangKyNavigation)
                    .ThenInclude(d => d.MaKhoaHocNavigation)
                .Include(h => h.ThanhToanHocPhis)
                .FirstOrDefaultAsync(h => h.MaHocPhi == maHocPhi);

            if (hocPhi == null)
                return;

            var now = DateTime.Now;

            var daThu = hocPhi.ThanhToanHocPhis
    .Where(t =>
        t.TrangThaiGiaoDich == "Thành công" &&
        t.NgayThanhToan <= now)
    .Sum(t => t.SoTien);

            ViewBag.MaHocPhi = hocPhi.MaHocPhi;
            ViewBag.MaDangKy = hocPhi.MaDangKy;
            ViewBag.MaHocVien =
                hocPhi.MaDangKyNavigation.MaHocVienNavigation.MaHocVien;
            ViewBag.HoTenHocVien =
                hocPhi.MaDangKyNavigation.MaHocVienNavigation.HoTen;
            ViewBag.TenKhoaHoc =
                hocPhi.MaDangKyNavigation.MaKhoaHocNavigation.TenKhoaHoc;
            ViewBag.TongHocPhi = hocPhi.TongHocPhi;
            ViewBag.DaThu = daThu;
            ViewBag.ConNo = Math.Max(0m, hocPhi.TongHocPhi - daThu);
        }

        // GET: HocPhi/BienLai/TT001
        [HttpGet]
        public async Task<IActionResult> BienLai(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var thanhToan = await _context.ThanhToanHocPhis
                
                .Include(t => t.MaHocPhiNavigation)
                    .ThenInclude(h => h.MaDangKyNavigation)
                        .ThenInclude(d => d.MaHocVienNavigation)
                .Include(t => t.MaHocPhiNavigation)
                    .ThenInclude(h => h.MaDangKyNavigation)
                        .ThenInclude(d => d.MaKhoaHocNavigation)
                .Include(t => t.MaHocPhiNavigation)
                    .ThenInclude(h => h.ThanhToanHocPhis)
                .Include(t => t.NguoiThuNavigation)
                .FirstOrDefaultAsync(t => t.MaThanhToan == id);

            if (thanhToan == null)
                return NotFound();
            if (thanhToan.TrangThaiGiaoDich != "Thành công")
            {
                return BadRequest("Giao dịch chưa thanh toán thành công.");
            }
            var hocPhi = thanhToan.MaHocPhiNavigation;

            var now = DateTime.Now;

            var cacKhoanDaGhiNhan = hocPhi.ThanhToanHocPhis
    .Where(t =>
        t.TrangThaiGiaoDich == "Thành công" &&
        t.NgayThanhToan <= now)
    .ToList();

            var daThuSauGiaoDich = cacKhoanDaGhiNhan
                .Sum(t => t.SoTien);

            var daThuTruocDo = daThuSauGiaoDich - thanhToan.SoTien;

            if (daThuTruocDo < 0)
                daThuTruocDo = 0;

            var model = new BienLaiHocPhiViewModel
            {
                MaThanhToan = thanhToan.MaThanhToan,
                MaHocPhi = hocPhi.MaHocPhi,
                MaDangKy = hocPhi.MaDangKy,

                NgayThanhToan = thanhToan.NgayThanhToan,

                MaHocVien = hocPhi.MaDangKyNavigation
                    .MaHocVienNavigation.MaHocVien,

                HoTenHocVien = hocPhi.MaDangKyNavigation
                    .MaHocVienNavigation.HoTen,

                SoDienThoai = hocPhi.MaDangKyNavigation
                    .MaHocVienNavigation.SoDienThoai,

                Email = hocPhi.MaDangKyNavigation
                    .MaHocVienNavigation.Email,

                TenKhoaHoc = hocPhi.MaDangKyNavigation
                    .MaKhoaHocNavigation.TenKhoaHoc,

                TongHocPhi = hocPhi.TongHocPhi,
                DaThuTruocDo = daThuTruocDo,
                SoTienThu = thanhToan.SoTien,
                DaThuSauGiaoDich = daThuSauGiaoDich,
                ConNo = Math.Max(0m, hocPhi.TongHocPhi - daThuSauGiaoDich),

                PhuongThuc = thanhToan.PhuongThuc,
                MaGiaoDich = thanhToan.MaGiaoDich,

                NguoiThu = thanhToan.NguoiThuNavigation != null
                    ? thanhToan.NguoiThuNavigation.HoTen
                    : thanhToan.NguoiThu,

                GhiChu = thanhToan.GhiChu
            };

            return View(model);
        }
        // POST: HocPhi/TaoThanhToanVnPay
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoThanhToanVnPay(
            string maHocPhi,
            decimal soTien)
        {
            if (string.IsNullOrWhiteSpace(maHocPhi))
                return NotFound();

            var maTaiKhoan = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(maTaiKhoan))
                return Forbid();

            if (soTien <= 0 || soTien != decimal.Truncate(soTien))
            {
                TempData["Error"] =
                    "Số tiền thanh toán phải là số nguyên dương.";

                return RedirectToAction(
                    nameof(Details), new { id = maHocPhi });
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    System.Data.IsolationLevel.Serializable);

            try
            {
                var hocPhi = await _context.HocPhis
                    .Include(h => h.ThanhToanHocPhis)
                    .FirstOrDefaultAsync(h => h.MaHocPhi == maHocPhi);

                if (hocPhi == null)
                    return NotFound();

                var now = DateTime.Now;

                var daThu = hocPhi.ThanhToanHocPhis
                    .Where(t =>
                        t.TrangThaiGiaoDich == "Thành công" &&
                        t.NgayThanhToan <= now)
                    .Sum(t => t.SoTien);

                var conNo = Math.Max(
                    0m, hocPhi.TongHocPhi - daThu);

                if (conNo <= 0 || soTien > conNo)
                {
                    await transaction.RollbackAsync();

                    TempData["Error"] =
                        $"Số tiền thanh toán không hợp lệ. Công nợ hiện tại: {conNo:N0} đồng.";

                    return RedirectToAction(
                        nameof(Details), new { id = maHocPhi });
                }

                // Sinh mã thanh toán TT001, TT002...
                var danhSachMa = await _context.ThanhToanHocPhis
                    .Select(t => t.MaThanhToan)
                    .ToListAsync();

                var soThuTuLonNhat = danhSachMa
                    .Where(ma => ma.StartsWith(
                        "TT", StringComparison.OrdinalIgnoreCase))
                    .Select(ma =>
                    {
                        var phanSo = ma.Substring(2);

                        return int.TryParse(phanSo, out var so)
                            ? so
                            : 0;
                    })
                    .DefaultIfEmpty(0)
                    .Max();

                var maThanhToan = $"TT{soThuTuLonNhat + 1:D3}";

                // Lưu giao dịch trước khi chuyển sang cổng thanh toán.
                var thanhToan = new ThanhToanHocPhi
                {
                    MaThanhToan = maThanhToan,
                    MaHocPhi = hocPhi.MaHocPhi,
                    NgayThanhToan = now,
                    SoTien = soTien,
                    PhuongThuc = "VNPay",
                    MaGiaoDich = null,
                    TrangThaiGiaoDich = "Đang chờ thanh toán",
                    NguoiThu = maTaiKhoan,
                    GhiChu = "Thanh toán học phí trực tuyến qua VNPay"
                };

                _context.ThanhToanHocPhis.Add(thanhToan);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var ipAddress =
                    HttpContext.Connection.RemoteIpAddress?.ToString()
                    ?? "127.0.0.1";

                // Không đưa thông tin bí mật vào URL.
                var urlThanhToan = _vnPayService.TaoUrlThanhToan(
                    maThanhToan,
                    soTien,
                    $"Thanh toan hoc phi {maThanhToan}",
                    ipAddress);

                return Redirect(urlThanhToan);
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();

                TempData["Error"] =
                    "Không thể tạo giao dịch thanh toán. Vui lòng thử lại.";

                return RedirectToAction(
                    nameof(Details), new { id = maHocPhi });
            }
            catch (Exception)
            {
                // Giao dịch đã được lưu ở trạng thái chờ.
                // Không chuyển trạng thái thành công khi xảy ra lỗi.
                TempData["Error"] =
                    "Không thể khởi tạo cổng thanh toán. Giao dịch vẫn chưa được xác nhận thanh toán.";

                return RedirectToAction(
                    nameof(Details), new { id = maHocPhi });
            }
        }
        // GET: HocPhi/VnPayReturn
        // Chỉ hiển thị kết quả, không tự xác nhận đã thu tiền.
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> VnPayReturn()
        {
            var thamSo = Request.Query
                .ToDictionary(
                    x => x.Key,
                    x => x.Value.ToString(),
                    StringComparer.Ordinal);

            if (!_vnPayService.XacMinhChuKy(thamSo))
            {
                TempData["Error"] =
                    "Chữ ký phản hồi VNPay không hợp lệ.";

                return RedirectToAction(nameof(Index));
            }

            var maThanhToan = thamSo.GetValueOrDefault("vnp_TxnRef");

            var thanhToan = await _context.ThanhToanHocPhis
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.MaThanhToan == maThanhToan);

            if (thanhToan == null ||
                !long.TryParse(
                    thamSo.GetValueOrDefault("vnp_Amount"),
                    out var soTienVnPay) ||
                soTienVnPay != thanhToan.SoTien * 100m)
            {
                TempData["Error"] =
                    "Không tìm thấy giao dịch hoặc số tiền không khớp.";

                return RedirectToAction(nameof(Index));
            }

            if (thanhToan.TrangThaiGiaoDich == "Thành công")
            {
                TempData["Success"] =
                    "Giao dịch đã được xác nhận thanh toán thành công.";
            }
            else if (thamSo.GetValueOrDefault("vnp_ResponseCode") == "00" &&
                     thamSo.GetValueOrDefault("vnp_TransactionStatus") == "00")
            {
                TempData["Success"] =
                    "VNPay thông báo giao dịch thành công. Hệ thống đang chờ xác nhận IPN.";
            }
            else
            {
                TempData["Error"] =
                    "Giao dịch chưa được xác nhận thành công. Trạng thái sẽ được cập nhật theo phản hồi máy chủ VNPay.";
            }

            return RedirectToAction(
                nameof(Details),
                new { id = thanhToan.MaHocPhi });
        }


        // GET: HocPhi/VnPayIpn
        // VNPay gọi endpoint này để thông báo kết quả giao dịch.
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> VnPayIpn()
        {
            var thamSo = Request.Query
                .ToDictionary(
                    x => x.Key,
                    x => x.Value.ToString(),
                    StringComparer.Ordinal);

            if (!_vnPayService.XacMinhChuKy(thamSo))
            {
                return Json(new
                {
                    RspCode = "97",
                    Message = "Invalid signature"
                });
            }

            var maThanhToan = thamSo.GetValueOrDefault("vnp_TxnRef");

            if (string.IsNullOrWhiteSpace(maThanhToan))
            {
                return Json(new
                {
                    RspCode = "01",
                    Message = "Order not found"
                });
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    System.Data.IsolationLevel.Serializable);

            try
            {
                var thanhToan = await _context.ThanhToanHocPhis
                    .FirstOrDefaultAsync(
                        t => t.MaThanhToan == maThanhToan);

                if (thanhToan == null)
                {
                    await transaction.RollbackAsync();

                    return Json(new
                    {
                        RspCode = "01",
                        Message = "Order not found"
                    });
                }

                if (!long.TryParse(
                        thamSo.GetValueOrDefault("vnp_Amount"),
                        out var soTienVnPay) ||
                    soTienVnPay != thanhToan.SoTien * 100m)
                {
                    await transaction.RollbackAsync();

                    return Json(new
                    {
                        RspCode = "04",
                        Message = "Invalid amount"
                    });
                }

                // IPN gửi lặp: không ghi nhận giao dịch lần thứ hai.
                if (thanhToan.TrangThaiGiaoDich == "Thành công")
                {
                    await transaction.CommitAsync();

                    return Json(new
                    {
                        RspCode = "00",
                        Message = "Confirm Success"
                    });
                }

                if (thanhToan.TrangThaiGiaoDich != "Đang chờ thanh toán")
                {
                    await transaction.RollbackAsync();

                    return Json(new
                    {
                        RspCode = "02",
                        Message = "Order already processed"
                    });
                }

                var responseCode =
                    thamSo.GetValueOrDefault("vnp_ResponseCode");

                var transactionStatus =
                    thamSo.GetValueOrDefault("vnp_TransactionStatus");

                if (responseCode == "00" && transactionStatus == "00")
                {
                    // Kiểm tra đúng phương thức giao dịch.
                    if (thanhToan.PhuongThuc != "VNPay")
                    {
                        await transaction.RollbackAsync();

                        return Json(new
                        {
                            RspCode = "04",
                            Message = "Invalid payment method"
                        });
                    }

                    thanhToan.TrangThaiGiaoDich = "Thành công";
                    thanhToan.NgayThanhToan = DateTime.Now;

                    var maGiaoDichVnPay =
                        thamSo.GetValueOrDefault("vnp_TransactionNo");

                    if (!string.IsNullOrWhiteSpace(maGiaoDichVnPay))
                    {
                        thanhToan.MaGiaoDich = maGiaoDichVnPay;
                    }

                    // Cập nhật công nợ theo các giao dịch thành công.
                    var hocPhi = await _context.HocPhis
                        .Include(h => h.ThanhToanHocPhis)
                        .FirstOrDefaultAsync(
                            h => h.MaHocPhi == thanhToan.MaHocPhi);

                    if (hocPhi == null)
                    {
                        await transaction.RollbackAsync();

                        return Json(new
                        {
                            RspCode = "01",
                            Message = "Fee not found"
                        });
                    }

                    var tongDaThu = hocPhi.ThanhToanHocPhis
                        .Where(t =>
                            t.TrangThaiGiaoDich == "Thành công" &&
                            t.NgayThanhToan <= DateTime.Now)
                        .Sum(t => t.SoTien);

                    var conNo = Math.Max(
                        0m, hocPhi.TongHocPhi - tongDaThu);

                    if (conNo <= 0)
                    {
                        hocPhi.TrangThai = "Đã thanh toán";
                    }
                    else if (hocPhi.HanThanhToan.HasValue &&
                             hocPhi.HanThanhToan.Value <
                             DateOnly.FromDateTime(DateTime.Today))
                    {
                        hocPhi.TrangThai = "Quá hạn";
                    }
                    else if (tongDaThu > 0)
                    {
                        hocPhi.TrangThai = "Thanh toán một phần";
                    }
                    else
                    {
                        hocPhi.TrangThai = "Chưa thanh toán";
                    }
                }
                else
                {
                    thanhToan.TrangThaiGiaoDich = "Thất bại";
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new
                {
                    RspCode = "00",
                    Message = "Confirm Success"
                });
            }
            catch
            {
                await transaction.RollbackAsync();

                return Json(new
                {
                    RspCode = "99",
                    Message = "Unknown error"
                });
            }
        }   
    }
}
