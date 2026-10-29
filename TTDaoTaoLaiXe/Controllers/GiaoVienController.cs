using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TTDaoTaoLaiXe.Models;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace TTDaoTaoLaiXe.Controllers
{
    [Authorize]
    public class GiaoVienController : Controller
    {
        private readonly TTDaoTaoLaiXeContext _context;

        private const int PageSize = 10;

        public GiaoVienController(TTDaoTaoLaiXeContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateModal(GiaoVien giaoVien)
        {
            giaoVien.MaGiaoVien = giaoVien.MaGiaoVien?.Trim() ?? "";
            giaoVien.HoTen = giaoVien.HoTen?.Trim() ?? "";
            giaoVien.Cccd = string.IsNullOrWhiteSpace(giaoVien.Cccd)
                ? null : giaoVien.Cccd.Trim();
            giaoVien.SoDienThoai = string.IsNullOrWhiteSpace(giaoVien.SoDienThoai)
                ? null : giaoVien.SoDienThoai.Trim();
            giaoVien.Email = string.IsNullOrWhiteSpace(giaoVien.Email)
                ? null : giaoVien.Email.Trim();
            giaoVien.ChuyenMon = string.IsNullOrWhiteSpace(giaoVien.ChuyenMon)
                ? null : giaoVien.ChuyenMon.Trim();
            giaoVien.HangDuocPhepDay =
                string.IsNullOrWhiteSpace(giaoVien.HangDuocPhepDay)
                ? null : giaoVien.HangDuocPhepDay.Trim();

            if (string.IsNullOrWhiteSpace(giaoVien.MaGiaoVien))
                ModelState.AddModelError(nameof(giaoVien.MaGiaoVien),
                    "Vui lòng nhập mã giáo viên.");

            if (string.IsNullOrWhiteSpace(giaoVien.HoTen))
                ModelState.AddModelError(nameof(giaoVien.HoTen),
                    "Vui lòng nhập họ tên.");

            if (await _context.GiaoViens.AnyAsync(
                g => g.MaGiaoVien == giaoVien.MaGiaoVien))
            {
                ModelState.AddModelError(nameof(giaoVien.MaGiaoVien),
                    "Mã giáo viên đã tồn tại.");
            }

            if (giaoVien.Cccd != null &&
                await _context.GiaoViens.AnyAsync(
                    g => g.Cccd == giaoVien.Cccd))
            {
                ModelState.AddModelError(nameof(giaoVien.Cccd),
                    "CCCD đã được sử dụng.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = string.Join(" ",
                        ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage))
                });
            }

            giaoVien.TrangThai = true;

            try
            {
                _context.GiaoViens.Add(giaoVien);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Thêm giáo viên thành công."
                });
            }
            catch (DbUpdateException)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Không thể lưu giáo viên. Hãy kiểm tra mã, CCCD và dữ liệu nhập."
                });
            }
        }


        // GET: /GiaoVien

        public async Task<IActionResult> Index(
            string? searchString,
            bool? trangThai,
            string? hang,
            int page = 1)
        {
            const int pageSize = 10;
            page = Math.Max(page, 1);

            var query = TaoQueryGiaoVien(
                searchString, trangThai, hang);

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(
                totalItems / (double)pageSize);

            page = totalPages > 0
                ? Math.Min(page, totalPages)
                : 1;

            var giaoViens = await query
                .OrderBy(g => g.HoTen)
                .ThenBy(g => g.MaGiaoVien)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.SearchString = searchString;
            ViewBag.TrangThai = trangThai;
            ViewBag.Hang = hang;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;
            ViewBag.DanhSachHangGplx = await _context.HangGplxes
    .OrderBy(h => h.MaHang)
    .ToListAsync();
            ViewBag.DanhSachHang = await _context.HangGplxes
                .AsNoTracking()
                .OrderBy(x => x.TenHang)
                .Select(x => new SelectListItem
                {
                    Value = x.MaHang,
                    Text = x.TenHang
                })
                .ToListAsync();

            return View(giaoViens);
        }


        // GET: /GiaoVien/Details/GV0001
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var giaoVien = await _context.GiaoViens
                .AsNoTracking()
                .Include(g => g.LopHocs)
                .Include(g => g.LichHocs)
                    .ThenInclude(l => l.MaLopHocNavigation)
                .FirstOrDefaultAsync(g => g.MaGiaoVien == id);

            if (giaoVien == null)
                return NotFound();

            return View(giaoVien);
        }

        // GET: /GiaoVien/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new GiaoVien
            {
                TrangThai = true
            });
        }

        // POST: /GiaoVien/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GiaoVien giaoVien)
        {
            // Chuẩn hóa dữ liệu đầu vào
            giaoVien.MaGiaoVien = giaoVien.MaGiaoVien?.Trim() ?? "";
            giaoVien.HoTen = giaoVien.HoTen?.Trim() ?? "";
            giaoVien.Cccd = string.IsNullOrWhiteSpace(giaoVien.Cccd)
                ? null : giaoVien.Cccd.Trim();
            giaoVien.SoDienThoai =
                string.IsNullOrWhiteSpace(giaoVien.SoDienThoai)
                    ? null : giaoVien.SoDienThoai.Trim();
            giaoVien.Email = string.IsNullOrWhiteSpace(giaoVien.Email)
                ? null : giaoVien.Email.Trim();
            giaoVien.ChuyenMon =
                string.IsNullOrWhiteSpace(giaoVien.ChuyenMon)
                    ? null : giaoVien.ChuyenMon.Trim();
            giaoVien.HangDuocPhepDay =
                string.IsNullOrWhiteSpace(giaoVien.HangDuocPhepDay)
                    ? null : giaoVien.HangDuocPhepDay.Trim();

            bool trungMa = await _context.GiaoViens
                .AnyAsync(g => g.MaGiaoVien == giaoVien.MaGiaoVien);

            if (trungMa)
            {
                ModelState.AddModelError(
                    nameof(GiaoVien.MaGiaoVien),
                    "Mã giáo viên đã tồn tại.");
            }

            if (!string.IsNullOrWhiteSpace(giaoVien.Cccd))
            {
                bool trungCccd = await _context.GiaoViens
                    .AnyAsync(g => g.Cccd == giaoVien.Cccd);

                if (trungCccd)
                {
                    ModelState.AddModelError(
                        nameof(GiaoVien.Cccd),
                        "CCCD đã được sử dụng cho giáo viên khác.");
                }
            }

            if (!ModelState.IsValid)
                return View(giaoVien);

            _context.GiaoViens.Add(giaoVien);

            try
            {
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    "Thêm giáo viên thành công.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Không thể lưu giáo viên. Vui lòng kiểm tra mã giáo viên, CCCD và dữ liệu liên quan.");

                return View(giaoVien);
            }
        }

        // POST: /GiaoVien/ToggleTrangThai
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleTrangThai(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Json(new
                {
                    success = false,
                    message = "Mã giáo viên không hợp lệ."
                });
            }

            var giaoVien = await _context.GiaoViens
                .FirstOrDefaultAsync(g => g.MaGiaoVien == id);

            if (giaoVien == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Không tìm thấy giáo viên."
                });
            }

            if (giaoVien.TrangThai)
            {
                // Không ngừng hoạt động nếu còn lịch tương lai
                // chưa kết thúc hoặc chưa bị hủy.
                var now = DateTime.Now;

                bool coLichTuongLai = await _context.LichHocs
                    .AnyAsync(l =>
                        l.MaGiaoVien == id &&
                        l.NgayHoc >= DateOnly.FromDateTime(now) &&
                        l.TrangThai != "Đã hủy" &&
                        (l.NgayHoc > DateOnly.FromDateTime(now) ||
                         l.GioKetThuc > TimeOnly.FromDateTime(now)));

                if (coLichTuongLai)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Giáo viên còn lịch dạy chưa kết thúc. Vui lòng xử lý các lịch dạy liên quan trước khi ngừng hoạt động."
                    });
                }
            }

            giaoVien.TrangThai = !giaoVien.TrangThai;

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                trangThai = giaoVien.TrangThai,
                message = giaoVien.TrangThai
                    ? "Đã kích hoạt lại giáo viên."
                    : "Đã ngừng hoạt động giáo viên."
            });
        }

        [HttpGet]
        public async Task<IActionResult> ExportExcel(
            string? searchString,
            bool? trangThai,
            string? hang)
        {
            var danhSach = await _context.GiaoViens
    .OrderBy(gv => gv.MaGiaoVien)
    .ToListAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("DanhSachGiaoVien");

            ws.Cell(1, 1).Value = "DANH SÁCH GIÁO VIÊN";
            ws.Range(1, 1, 1, 10).Merge();
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 16;
            ws.Cell(1, 1).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            string[] headers =
            {
        "STT", "Mã giáo viên", "Họ tên", "Ngày sinh",
        "Giới tính", "CCCD", "Điện thoại", "Email",
        "Chuyên môn", "Hạng được phép dạy"
    };

            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(3, i + 1).Value = headers[i];
            }

            var headerRange = ws.Range(3, 1, 3, headers.Length);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor =
                XLColor.LightBlue;
            headerRange.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;
            headerRange.Style.Border.OutsideBorder =
                XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder =
                XLBorderStyleValues.Thin;

            int row = 4;

            foreach (var gv in danhSach)
            {
                ws.Cell(row, 1).Value = row - 3;
                ws.Cell(row, 2).Value = gv.MaGiaoVien;
                ws.Cell(row, 3).Value = gv.HoTen;
                ws.Cell(row, 4).Value =
                    gv.NgaySinh?.ToString("dd/MM/yyyy") ?? "";
                ws.Cell(row, 5).Value = gv.GioiTinh ?? "";
                ws.Cell(row, 6).Value = gv.Cccd ?? "";
                ws.Cell(row, 7).Value = gv.SoDienThoai ?? "";
                ws.Cell(row, 8).Value = gv.Email ?? "";
                ws.Cell(row, 9).Value = gv.ChuyenMon ?? "";
                ws.Cell(row, 10).Value =
                    gv.HangDuocPhepDay ?? "";

                row++;
            }

            if (row > 4)
            {
                var dataRange = ws.Range(4, 1, row - 1, 10);
                dataRange.Style.Border.OutsideBorder =
                    XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder =
                    XLBorderStyleValues.Thin;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"DanhSachGiaoVien_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }


        // GET: /GiaoVien/EditModal/GV0001
        [HttpGet]
        public async Task<IActionResult> EditModal(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Mã giáo viên không hợp lệ."
                });
            }

            var giaoVien = await _context.GiaoViens
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.MaGiaoVien == id);

            if (giaoVien == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy giáo viên."
                });
            }

            var danhSachHang = string.IsNullOrWhiteSpace(
                giaoVien.HangDuocPhepDay)
                ? new List<string>()
                : giaoVien.HangDuocPhepDay
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(h => h.Trim())
                    .Where(h => h.Length > 0)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

            return Json(new
            {
                success = true,
                data = new
                {
                    giaoVien.MaGiaoVien,
                    giaoVien.HoTen,
                    NgaySinh = giaoVien.NgaySinh?.ToString("yyyy-MM-dd"),
                    giaoVien.GioiTinh,
                    giaoVien.Cccd,
                    giaoVien.SoDienThoai,
                    giaoVien.Email,
                    giaoVien.SoGiayPhepLaiXe,
                    giaoVien.DiaChi,
                    giaoVien.ChuyenMon,
                    giaoVien.GhiChu,
                    DanhSachHangDuocPhepDay = danhSachHang
                }
            });
        }

        // POST: /GiaoVien/EditModal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditModal(
            GiaoVien giaoVien,
            string[]? DanhSachHangDuocPhepDay)
        {
            string maGiaoVien = giaoVien.MaGiaoVien?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(maGiaoVien))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Mã giáo viên không hợp lệ."
                });
            }

            var giaoVienCu = await _context.GiaoViens
                .FirstOrDefaultAsync(g => g.MaGiaoVien == maGiaoVien);

            if (giaoVienCu == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Giáo viên không tồn tại hoặc đã bị xóa."
                });
            }

            string hoTen = giaoVien.HoTen?.Trim() ?? "";
            string? cccd = string.IsNullOrWhiteSpace(giaoVien.Cccd)
                ? null : giaoVien.Cccd.Trim();

            string? dienThoai = string.IsNullOrWhiteSpace(giaoVien.SoDienThoai)
                ? null : giaoVien.SoDienThoai.Trim();

            string? email = string.IsNullOrWhiteSpace(giaoVien.Email)
                ? null : giaoVien.Email.Trim();

            string? soGplx = string.IsNullOrWhiteSpace(giaoVien.SoGiayPhepLaiXe)
                ? null : giaoVien.SoGiayPhepLaiXe.Trim();

            string? diaChi = string.IsNullOrWhiteSpace(giaoVien.DiaChi)
                ? null : giaoVien.DiaChi.Trim();

            string? chuyenMon = string.IsNullOrWhiteSpace(giaoVien.ChuyenMon)
                ? null : giaoVien.ChuyenMon.Trim();

            string? ghiChu = string.IsNullOrWhiteSpace(giaoVien.GhiChu)
                ? null : giaoVien.GhiChu.Trim();

            string? gioiTinh = string.IsNullOrWhiteSpace(giaoVien.GioiTinh)
                ? null : giaoVien.GioiTinh.Trim();

            if (string.IsNullOrWhiteSpace(hoTen))
            {
                ModelState.AddModelError(
                    nameof(GiaoVien.HoTen),
                    "Vui lòng nhập họ tên.");
            }

            if (hoTen.Length > 100)
            {
                ModelState.AddModelError(
                    nameof(GiaoVien.HoTen),
                    "Họ tên không được vượt quá 100 ký tự.");
            }

            if (cccd != null &&
                (cccd.Length != 12 || !cccd.All(char.IsDigit)))
            {
                ModelState.AddModelError(
                    nameof(GiaoVien.Cccd),
                    "CCCD phải gồm đúng 12 chữ số.");
            }

            if (cccd != null &&
                await _context.GiaoViens.AnyAsync(
                    g => g.MaGiaoVien != maGiaoVien && g.Cccd == cccd))
            {
                ModelState.AddModelError(
                    nameof(GiaoVien.Cccd),
                    "CCCD đã được sử dụng cho giáo viên khác.");
            }

            var cacMaHangChon = (DanhSachHangDuocPhepDay
                    ?? Array.Empty<string>())
                .Select(h => h.Trim())
                .Where(h => h.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var cacMaHangHopLe = (await _context.HangGplxes
                    .Select(h => h.MaHang)
                    .ToListAsync())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (cacMaHangChon.Any(h => !cacMaHangHopLe.Contains(h)))
            {
                ModelState.AddModelError(
                    nameof(DanhSachHangDuocPhepDay),
                    "Có hạng giấy phép không tồn tại trong danh mục.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = string.Join(" ",
                        ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .Where(m => !string.IsNullOrWhiteSpace(m))
                            .Distinct())
                });
            }

            // Chỉ cập nhật các trường thuộc hồ sơ.
            // Giữ nguyên MaGiaoVien, TrangThai và các quan hệ lịch dạy.
            giaoVienCu.HoTen = hoTen;
            giaoVienCu.NgaySinh = giaoVien.NgaySinh;
            giaoVienCu.GioiTinh = gioiTinh;
            giaoVienCu.Cccd = cccd;
            giaoVienCu.SoDienThoai = dienThoai;
            giaoVienCu.Email = email;
            giaoVienCu.SoGiayPhepLaiXe = soGplx;
            giaoVienCu.DiaChi = diaChi;
            giaoVienCu.ChuyenMon = chuyenMon;
            giaoVienCu.HangDuocPhepDay = string.Join(", ",
                cacMaHangChon.OrderBy(h => h, StringComparer.OrdinalIgnoreCase));
            giaoVienCu.GhiChu = ghiChu;

            try
            {
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Cập nhật thông tin giáo viên thành công."
                });
            }
            catch (DbUpdateException)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Không thể cập nhật. Vui lòng kiểm tra dữ liệu trùng hoặc ràng buộc cơ sở dữ liệu."
                });
            }
        }


        // POST: /GiaoVien/Xoa
        // Xóa mềm: chỉ chuyển giáo viên sang trạng thái ngừng hoạt động.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Mã giáo viên không hợp lệ."
                });
            }

            var giaoVien = await _context.GiaoViens
                .FirstOrDefaultAsync(g => g.MaGiaoVien == id);

            if (giaoVien == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy giáo viên."
                });
            }

            if (!giaoVien.TrangThai)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Giáo viên này đã ngừng hoạt động."
                });
            }

            // Không ngừng hoạt động nếu giáo viên còn lịch dạy
            // trong tương lai chưa kết thúc hoặc chưa bị hủy.
            var now = DateTime.Now;
            var ngayHienTai = DateOnly.FromDateTime(now);
            var gioHienTai = TimeOnly.FromDateTime(now);

            bool coLichTuongLai = await _context.LichHocs
                .AnyAsync(l =>
                    l.MaGiaoVien == id &&
                    l.NgayHoc >= ngayHienTai &&
                    l.TrangThai != "Đã hủy" &&
                    (
                        l.NgayHoc > ngayHienTai ||
                        l.GioKetThuc > gioHienTai
                    ));

            if (coLichTuongLai)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Không thể xóa giáo viên vì vẫn còn lịch dạy chưa kết thúc. Vui lòng xử lý các lịch dạy liên quan trước."
                });
            }

            try
            {
                // XÓA MỀM: tuyệt đối không gọi Remove().
                giaoVien.TrangThai = false;

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Đã ngừng hoạt động giáo viên thành công. Hồ sơ và lịch sử giảng dạy vẫn được bảo toàn.",
                    id = giaoVien.MaGiaoVien
                });
            }
            catch (DbUpdateException)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Không thể cập nhật trạng thái giáo viên. Vui lòng thử lại."
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportExcel(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new
                {
                    success = false,
                    message = "Vui lòng chọn file Excel."
                });

            if (!Path.GetExtension(file.FileName)
                .Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Chỉ chấp nhận file Excel .xlsx."
                });
            }

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new
                {
                    success = false,
                    message = "File không được vượt quá 5 MB."
                });

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);
            var ws = workbook.Worksheet(1);
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

            if (lastRow < 2)
                return BadRequest(new
                {
                    success = false,
                    message = "File Excel chưa có dữ liệu."
                });

            string[] headers =
            {
        "MaGiaoVien", "HoTen", "NgaySinh", "GioiTinh",
        "CCCD", "SoDienThoai", "Email", "DiaChi",
        "ChuyenMon", "SoGiayPhepLaiXe",
        "HangDuocPhepDay", "GhiChu"
    };

            for (int i = 0; i < headers.Length; i++)
            {
                if (!string.Equals(
                    ws.Cell(1, i + 1).GetString().Trim(),
                    headers[i],
                    StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"Tiêu đề cột {i + 1} phải là {headers[i]}."
                    });
                }
            }

            var maDaCo = (await _context.GiaoViens
                .Select(g => g.MaGiaoVien)
                .ToListAsync())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var cccdDaCo = (await _context.GiaoViens
                .Where(g => g.Cccd != null)
                .Select(g => g.Cccd!)
                .ToListAsync())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var cacHangHopLe = (await _context.HangGplxes
                .Select(h => h.MaHang)
                .ToListAsync())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var danhSachMoi = new List<GiaoVien>();
            var maTrongFile = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);
            var cccdTrongFile = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);
            var loi = new List<string>();

            for (int r = 2; r <= lastRow; r++)
            {
                var ma = ws.Cell(r, 1).GetString().Trim();
                var ten = ws.Cell(r, 2).GetString().Trim();

                if (string.IsNullOrWhiteSpace(ma) &&
                    string.IsNullOrWhiteSpace(ten))
                    continue;

                if (string.IsNullOrWhiteSpace(ma) ||
                    string.IsNullOrWhiteSpace(ten))
                {
                    loi.Add($"Dòng {r}: thiếu mã giáo viên hoặc họ tên.");
                    continue;
                }

                if (ma.Length > 20 || ten.Length > 100)
                {
                    loi.Add($"Dòng {r}: mã hoặc họ tên vượt quá độ dài cho phép.");
                    continue;
                }

                if (maDaCo.Contains(ma) || !maTrongFile.Add(ma))
                {
                    loi.Add($"Dòng {r}: mã giáo viên {ma} bị trùng.");
                    continue;
                }

                string cccd = ws.Cell(r, 5).GetString().Trim();

                if (!string.IsNullOrWhiteSpace(cccd) &&
                    (cccd.Length != 12 ||
                     !cccd.All(char.IsDigit) ||
                     cccdDaCo.Contains(cccd) ||
                     !cccdTrongFile.Add(cccd)))
                {
                    loi.Add($"Dòng {r}: CCCD không hợp lệ hoặc bị trùng.");
                    continue;
                }

                string hangText = ws.Cell(r, 11).GetString().Trim();
                var hangList = hangText
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(h => h.Trim())
                    .Where(h => h.Length > 0)
                    .ToList();

                if (hangList.Any(h => !cacHangHopLe.Contains(h)))
                {
                    loi.Add($"Dòng {r}: hạng GPLX không tồn tại trong danh mục.");
                    continue;
                }


                DateOnly? ngaySinh = null;
                var cellNgaySinh = ws.Cell(r, 3);

                if (!cellNgaySinh.IsEmpty())
                {
                    if (cellNgaySinh.TryGetValue<DateTime>(out var ngay))
                    {
                        ngaySinh = DateOnly.FromDateTime(ngay);
                    }
                    else
                    {
                        string ngayText = cellNgaySinh.GetString().Trim();

                        if (DateOnly.TryParseExact(
                            ngayText,
                            new[] { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd" },
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None,
                            out var ngayParsed))
                        {
                            ngaySinh = ngayParsed;
                        }
                        else
                        {
                            loi.Add($"Dòng {r}: ngày sinh không hợp lệ. Vui lòng nhập theo định dạng dd/MM/yyyy.");
                            continue;
                        }
                    }
                }


                danhSachMoi.Add(new GiaoVien
                {
                    MaGiaoVien = ma,
                    HoTen = ten,
                    NgaySinh = ngaySinh,
                    GioiTinh = ws.Cell(r, 4).GetString().Trim(),
                    Cccd = string.IsNullOrWhiteSpace(cccd) ? null : cccd,
                    SoDienThoai = ws.Cell(r, 6).GetString().Trim(),
                    Email = ws.Cell(r, 7).GetString().Trim(),
                    DiaChi = ws.Cell(r, 8).GetString().Trim(),
                    ChuyenMon = ws.Cell(r, 9).GetString().Trim(),
                    SoGiayPhepLaiXe = ws.Cell(r, 10).GetString().Trim(),
                    HangDuocPhepDay = string.Join(", ", hangList),
                    GhiChu = ws.Cell(r, 12).GetString().Trim(),
                    TrangThai = true
                });
            }

            // Không nhập bất kỳ dòng nào nếu có lỗi,
            // tránh nhập dữ liệu dở dang.
            if (loi.Count > 0)
                return BadRequest(new
                {
                    success = false,
                    message = string.Join("\n", loi.Take(15)),
                    totalErrors = loi.Count
                });

            if (danhSachMoi.Count == 0)
                return BadRequest(new
                {
                    success = false,
                    message = "Không tìm thấy dòng giáo viên hợp lệ để nhập."
                });

            try
            {
                await using var transaction =
                    await _context.Database.BeginTransactionAsync();

                _context.GiaoViens.AddRange(danhSachMoi);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new
                {
                    success = true,
                    message = $"Đã nhập thành công {danhSachMoi.Count} giáo viên."
                });
            }
            catch (DbUpdateException)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Nhập Excel thất bại do dữ liệu không phù hợp với cơ sở dữ liệu."
                });
            }
        }



        [HttpGet]
        public async Task<IActionResult> ExportPdf(
            string? searchString,
            bool? trangThai,
            string? hang)
        {
            var danhSach = await TaoQueryGiaoVien(
                    searchString, trangThai, hang)
                .OrderBy(g => g.HoTen)
                .ThenBy(g => g.MaGiaoVien)
                .ToListAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(25);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header()
                        .PaddingBottom(12)
                        .Column(column =>
                        {
                            column.Item()
                                .AlignCenter()
                                .Text("DANH SÁCH GIÁO VIÊN")
                                .Bold()
                                .FontSize(16);

                            column.Item()
                                .AlignCenter()
                                .Text($"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}");
                        });

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(28);
                            columns.RelativeColumn(0.9f);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1.0f);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1.3f);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1.1f);
                        });

                        table.Header(header =>
                        {
                            string[] titles =
                            {
                        "STT", "Mã GV", "Họ tên", "Ngày sinh",
                        "CCCD", "Điện thoại", "Chuyên môn",
                        "Hạng được phép dạy"
                    };

                            foreach (string title in titles)
                            {
                                header.Cell()
                                    .Element(CellStyle)
                                    .Background(Colors.Grey.Lighten3)
                                    .Text(title)
                                    .Bold();
                            }
                        });

                        for (int i = 0; i < danhSach.Count; i++)
                        {
                            var gv = danhSach[i];

                            table.Cell().Element(CellStyle)
                                .Text((i + 1).ToString());

                            table.Cell().Element(CellStyle)
                                .Text(gv.MaGiaoVien);

                            table.Cell().Element(CellStyle)
                                .Text(gv.HoTen);

                            table.Cell().Element(CellStyle)
                                .Text(gv.NgaySinh?.ToString("dd/MM/yyyy") ?? "");

                            table.Cell().Element(CellStyle)
                                .Text(gv.Cccd ?? "");

                            table.Cell().Element(CellStyle)
                                .Text(gv.SoDienThoai ?? "");

                            table.Cell().Element(CellStyle)
                                .Text(gv.ChuyenMon ?? "");

                            table.Cell().Element(CellStyle)
                                .Text(gv.HangDuocPhepDay ?? "");
                        }
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Trang ");
                            text.CurrentPageNumber();
                            text.Span(" / ");
                            text.TotalPages();
                        });
                });
            });

            byte[] pdf = document.GeneratePdf();

            return File(
                pdf,
                "application/pdf",
                $"DanhSachGiaoVien_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }

        private static IContainer CellStyle(IContainer container)
        {
            return container
                .Border(0.5f)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(4);
        }

        private IQueryable<GiaoVien> TaoQueryGiaoVien(
    string? searchString,
    bool? trangThai,
    string? hang)
        {
            var query = _context.GiaoViens
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                string tuKhoa = searchString.Trim();

                query = query.Where(g =>
                    g.MaGiaoVien.Contains(tuKhoa) ||
                    g.HoTen.Contains(tuKhoa) ||
                    (g.Cccd != null && g.Cccd.Contains(tuKhoa)) ||
                    (g.SoDienThoai != null &&
                     g.SoDienThoai.Contains(tuKhoa)) ||
                    (g.Email != null && g.Email.Contains(tuKhoa)) ||
                    (g.ChuyenMon != null &&
                     g.ChuyenMon.Contains(tuKhoa)));
            }

            if (trangThai.HasValue)
            {
                query = query.Where(g =>
                    g.TrangThai == trangThai.Value);
            }

            if (!string.IsNullOrWhiteSpace(hang))
            {
                string maHang = hang.Trim();

                // So khớp chính xác từng mã hạng trong chuỗi
                // ví dụ "B, C1", tránh nhầm B với B11.
                query = query.Where(g =>
                    g.HangDuocPhepDay != null &&
                    ("," + g.HangDuocPhepDay.Replace(" ", "") + ",")
                        .Contains("," + maHang + ","));
            }

            return query;
        }
    }
}
