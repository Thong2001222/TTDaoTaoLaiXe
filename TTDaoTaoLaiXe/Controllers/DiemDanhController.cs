
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TTDaoTaoLaiXe.Models;
using TTDaoTaoLaiXe.ViewModels;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace TTDaoTaoLaiXe.Controllers
{
    public class DiemDanhController : Controller
    {
        private readonly TTDaoTaoLaiXeContext _context;
        private const int PageSize = 10;

        private static readonly string[] TrangThaiHopLe =
        {
            "Có mặt",
            "Đi trễ",
            "Vắng có phép",
            "Vắng không phép"
        };

        public DiemDanhController(TTDaoTaoLaiXeContext context)
        {
            _context = context;
        }

        // GET: DiemDanh
        public async Task<IActionResult> Index(
            string? tuKhoa,
            string? maLopHoc,
            DateOnly? ngayTu,
            DateOnly? ngayDen,
            int trang = 1)
        {
            if (trang < 1)
                trang = 1;

            var query = _context.LichHocs
                .AsNoTracking()
                .Include(x => x.MaLopHocNavigation)
                .Include(x => x.MaGiaoVienNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();

                query = query.Where(x =>
                    x.MaLichHoc.Contains(tuKhoa) ||
                    x.MaLopHocNavigation.TenLopHoc.Contains(tuKhoa));
            }

            if (!string.IsNullOrWhiteSpace(maLopHoc))
                query = query.Where(x => x.MaLopHoc == maLopHoc);

            if (ngayTu.HasValue)
                query = query.Where(x => x.NgayHoc >= ngayTu.Value);

            if (ngayDen.HasValue)
                query = query.Where(x => x.NgayHoc <= ngayDen.Value);

            var tongSoBanGhi = await query.CountAsync();

            var lichHocs = await query
                .OrderByDescending(x => x.NgayHoc)
                .ThenByDescending(x => x.GioBatDau)
                .ThenBy(x => x.MaLichHoc)
                .Skip((trang - 1) * PageSize)
                .Take(PageSize)
                .Select(x => new DiemDanhLichHocItemViewModel
                {
                    MaLichHoc = x.MaLichHoc,
                    MaLopHoc = x.MaLopHoc,
                    TenLopHoc = x.MaLopHocNavigation.TenLopHoc,
                    TenGiaoVien = x.MaGiaoVienNavigation.HoTen,
                    NgayHoc = x.NgayHoc,
                    GioBatDau = x.GioBatDau,
                    GioKetThuc = x.GioKetThuc,
                    LoaiBuoiHoc = x.LoaiBuoiHoc,
                    TrangThaiLichHoc = x.TrangThai,
                    TongHocVien = x.MaLopHocNavigation.HocVienLopHocs
                        .Count(h =>
                            h.TrangThai == "Đang học" &&
                            h.NgayRaLop == null &&
                            h.MaHocVienNavigation.TrangThai),
                    DaDiemDanh = x.DiemDanhs.Count()
                })
                .ToListAsync();

            var model = new DiemDanhIndexViewModel
            {
                LichHocs = lichHocs,
                TuKhoa = tuKhoa,
                MaLopHoc = maLopHoc,
                NgayTu = ngayTu,
                NgayDen = ngayDen,
                TrangHienTai = trang,
                TongSoBanGhi = tongSoBanGhi,
                TongTrang = (int)Math.Ceiling(tongSoBanGhi / (double)PageSize),
                DanhSachLopHoc = await _context.LopHocs
                    .AsNoTracking()
                    .OrderBy(x => x.TenLopHoc)
                    .Select(x => new SelectListItem
                    {
                        Value = x.MaLopHoc,
                        Text = x.TenLopHoc
                    })
                    .ToListAsync()
            };

            return View(model);
        }

        // GET: DiemDanh/DanhSach?maLichHoc=...
        public async Task<IActionResult> DanhSach(string maLichHoc)
        {
            if (string.IsNullOrWhiteSpace(maLichHoc))
                return BadRequest("Mã lịch học không hợp lệ.");

            var lichHoc = await _context.LichHocs
                .AsNoTracking()
                .Include(x => x.MaLopHocNavigation)
                .FirstOrDefaultAsync(x => x.MaLichHoc == maLichHoc);

            if (lichHoc == null)
                return NotFound("Không tìm thấy lịch học.");

            var hocViens = await _context.HocVienLopHocs
                .AsNoTracking()
                .Where(x =>
                    x.MaLopHoc == lichHoc.MaLopHoc &&
                    x.TrangThai == "Đang học" &&
                    x.NgayRaLop == null &&
                    x.MaHocVienNavigation.TrangThai)
                .OrderBy(x => x.MaHocVienNavigation.HoTen)
                .Select(x => new
                {
                    x.MaHocVien,
                    x.MaHocVienNavigation.HoTen
                })
                .ToListAsync();

            var diemDanhs = await _context.DiemDanhs
                .AsNoTracking()
                .Where(x => x.MaLichHoc == maLichHoc)
                .ToDictionaryAsync(x => x.MaHocVien);

            var gioBuoiHoc = lichHoc.SoGio ??
                (decimal)(lichHoc.GioKetThuc.ToTimeSpan() -
                          lichHoc.GioBatDau.ToTimeSpan()).TotalHours;

            var model = new DiemDanhRosterViewModel
            {
                MaLichHoc = lichHoc.MaLichHoc,
                MaLopHoc = lichHoc.MaLopHoc,
                TenLopHoc = lichHoc.MaLopHocNavigation.TenLopHoc,
                NgayHoc = lichHoc.NgayHoc,
                GioBatDau = lichHoc.GioBatDau,
                GioKetThuc = lichHoc.GioKetThuc,
                SoGioBuoiHoc = Math.Max(0, gioBuoiHoc),
                LoaiBuoiHoc = lichHoc.LoaiBuoiHoc,
                TrangThaiLichHoc = lichHoc.TrangThai,
                HocViens = hocViens.Select(hv =>
                {
                    diemDanhs.TryGetValue(hv.MaHocVien, out var dd);

                    return new DiemDanhHocVienViewModel
                    {
                        MaHocVien = hv.MaHocVien,
                        HoTen = hv.HoTen,
                        TrangThai = dd?.TrangThai ?? "Có mặt",
                        GioThamGia = dd?.GioThamGia ?? Math.Max(0, gioBuoiHoc),
                        GhiChu = dd?.GhiChu,
                        DaCoDiemDanh = dd != null
                    };
                }).ToList()
            };

            return View(model);
        }

       

        // POST: DiemDanh/Luu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Luu(DiemDanhRosterViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.MaLichHoc))
            {
                TempData["Error"] = "Mã lịch học không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            var maLichHoc = model.MaLichHoc.Trim();
            if (!ModelState.IsValid)
            {
                TempData["Error"] =
                    "Dữ liệu điểm danh không hợp lệ. Vui lòng kiểm tra trạng thái, số giờ và ghi chú.";
                return RedirectToAction(nameof(DanhSach),
                    new { maLichHoc });
            }
            try
            {
                var lichHoc = await _context.LichHocs
                    .FirstOrDefaultAsync(x => x.MaLichHoc == maLichHoc);

                if (lichHoc == null)
                {
                    TempData["Error"] = "Không tìm thấy lịch học.";
                    return RedirectToAction(nameof(Index));
                }

                if (lichHoc.GioKetThuc <= lichHoc.GioBatDau)
                {
                    TempData["Error"] =
                        "Thời gian kết thúc phải sau thời gian bắt đầu. Vui lòng kiểm tra lại lịch học.";

                    return RedirectToAction(nameof(DanhSach),
                        new { maLichHoc });
                }

                if (lichHoc.TrangThai == "Đã hủy")
                {
                    TempData["Error"] = "Lịch học đã hủy nên không thể điểm danh.";
                    return RedirectToAction(nameof(DanhSach),
                        new { maLichHoc });
                }

                if (model.HocViens == null || model.HocViens.Count == 0)
                {
                    TempData["Error"] = "Không có học viên để lưu điểm danh.";
                    return RedirectToAction(nameof(DanhSach),
                        new { maLichHoc });
                }

                var gioBuoiHoc = lichHoc.SoGio ??
                    (decimal)(lichHoc.GioKetThuc.ToTimeSpan() -
                              lichHoc.GioBatDau.ToTimeSpan()).TotalHours;

                gioBuoiHoc = Math.Max(0, gioBuoiHoc);

                // Chỉ cho phép điểm danh học viên đang học trong lớp này.
                var hocVienHopLe = await _context.HocVienLopHocs
                    .AsNoTracking()
                    .Where(x =>
                        x.MaLopHoc == lichHoc.MaLopHoc &&
                        x.TrangThai == "Đang học" &&
                        x.NgayRaLop == null &&
                        x.MaHocVienNavigation.TrangThai)
                    .Select(x => x.MaHocVien)
                    .Distinct()
                    .ToListAsync();

                var maHocViensGuiLen = model.HocViens
                    .Select(x => x.MaHocVien?.Trim() ?? string.Empty)
                    .ToList();

                if (maHocViensGuiLen.Any(string.IsNullOrWhiteSpace) ||
                    maHocViensGuiLen.Distinct().Count() != maHocViensGuiLen.Count ||
                    maHocViensGuiLen.Any(x => !hocVienHopLe.Contains(x)) ||
                    maHocViensGuiLen.Count != hocVienHopLe.Count)
                {
                    TempData["Error"] =
                        "Danh sách học viên không hợp lệ hoặc đã thay đổi. Vui lòng tải lại trang.";
                    return RedirectToAction(nameof(DanhSach),
                        new { maLichHoc });
                }

                // Kiểm tra toàn bộ dữ liệu trước khi thay đổi entity.
                foreach (var hv in model.HocViens)
                {
                    hv.MaHocVien = hv.MaHocVien.Trim();

                    if (!TrangThaiHopLe.Contains(hv.TrangThai))
                    {
                        TempData["Error"] =
                            $"Trạng thái điểm danh của học viên {hv.MaHocVien} không hợp lệ.";
                        return RedirectToAction(nameof(DanhSach),
                            new { maLichHoc });
                    }

                    if (hv.GhiChu?.Length > 500)
                    {
                        TempData["Error"] =
                            $"Ghi chú của học viên {hv.MaHocVien} vượt quá 500 ký tự.";
                        return RedirectToAction(nameof(DanhSach),
                            new { maLichHoc });
                    }

                    if (hv.GioThamGia < 0 || hv.GioThamGia > gioBuoiHoc)
                    {
                        TempData["Error"] =
                            $"Số giờ tham gia của học viên {hv.MaHocVien} phải từ 0 đến {gioBuoiHoc:0.##} giờ.";
                        return RedirectToAction(nameof(DanhSach),
                            new { maLichHoc });
                    }

                    if (hv.TrangThai == "Vắng có phép" ||
                        hv.TrangThai == "Vắng không phép")
                    {
                        hv.GioThamGia = 0;
                    }
                }

                var maHocVienCanLuu = maHocViensGuiLen.Distinct().ToList();

                // Lấy bản ghi hiện có để cập nhật, tránh thêm trùng khóa chính.
                var diemDanhsHienCo = await _context.DiemDanhs
                    .Where(x =>
                        x.MaLichHoc == maLichHoc &&
                        maHocVienCanLuu.Contains(x.MaHocVien))
                    .ToListAsync();

                var diemDanhTheoHocVien = diemDanhsHienCo
                    .ToDictionary(x => x.MaHocVien);

                var thoiGianDiemDanh = DateTime.Now;

                foreach (var hv in model.HocViens)
                {
                    if (diemDanhTheoHocVien.TryGetValue(hv.MaHocVien, out var dd))
                    {
                        dd.TrangThai = hv.TrangThai;
                        dd.GioThamGia = hv.GioThamGia;
                        dd.GhiChu = hv.GhiChu?.Trim();
                        dd.ThoiGianDiemDanh = thoiGianDiemDanh;
                    }
                    else
                    {
                        _context.DiemDanhs.Add(new DiemDanh
                        {
                            MaLichHoc = maLichHoc,
                            MaHocVien = hv.MaHocVien,
                            TrangThai = hv.TrangThai,
                            GioThamGia = hv.GioThamGia,
                            GhiChu = hv.GhiChu?.Trim(),
                            ThoiGianDiemDanh = thoiGianDiemDanh
                        });
                    }
                }

                var soDongThayDoi = await _context.SaveChangesAsync();

                TempData["Success"] = soDongThayDoi > 0
                    ? $"Đã lưu điểm danh cho {model.HocViens.Count} học viên."
                    : "Không có thay đổi mới cần lưu.";

                return RedirectToAction(nameof(DanhSach),
                    new { maLichHoc });
            }
            catch (DbUpdateException ex)
            {
                // Ghi chi tiết vào Output của ứng dụng để phục vụ chẩn đoán.
                var logger = HttpContext.RequestServices
                    .GetRequiredService<ILogger<DiemDanhController>>();

                logger.LogError(ex,
                    "Lỗi SQL khi lưu điểm danh cho lịch {MaLichHoc}",
                    maLichHoc);

                TempData["Error"] =
                    "Không thể lưu điểm danh vào cơ sở dữ liệu. " +
                    "Vui lòng kiểm tra lỗi SQL Server trong Output.";

                return RedirectToAction(nameof(DanhSach),
                    new { maLichHoc });
            }
            catch (Exception ex)
            {
                var logger = HttpContext.RequestServices
                    .GetRequiredService<ILogger<DiemDanhController>>();

                logger.LogError(ex,
                    "Lỗi khi lưu điểm danh cho lịch {MaLichHoc}",
                    maLichHoc);

                TempData["Error"] =
                    "Đã xảy ra lỗi khi lưu điểm danh. Vui lòng kiểm tra Output của ứng dụng.";

                return RedirectToAction(nameof(DanhSach),
                    new { maLichHoc });
            }
        }

    
    private class DiemDanhExportRow
    {
        public string MaHocVien { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string TrangThai { get; set; } = string.Empty;
        public decimal? GioThamGia { get; set; }
        public string? GhiChu { get; set; }
}

private async Task<(LichHoc? LichHoc, List<DiemDanhExportRow> Rows)>
    LayDuLieuXuatAsync(string maLichHoc)
        {
            var lichHoc = await _context.LichHocs
                .AsNoTracking()
                .Include(x => x.MaLopHocNavigation)
                .FirstOrDefaultAsync(x => x.MaLichHoc == maLichHoc);

            if (lichHoc == null)
                return (null, new List<DiemDanhExportRow>());

            var hocViens = await _context.HocVienLopHocs
                .AsNoTracking()
                .Where(x =>
                    x.MaLopHoc == lichHoc.MaLopHoc &&
                    x.TrangThai == "Đang học" &&
                    x.NgayRaLop == null &&
                    x.MaHocVienNavigation.TrangThai)
                .OrderBy(x => x.MaHocVienNavigation.HoTen)
                .Select(x => new
                {
                    x.MaHocVien,
                    HoTen = x.MaHocVienNavigation.HoTen
                })
                .ToListAsync();

            var diemDanhs = await _context.DiemDanhs
                .AsNoTracking()
                .Where(x => x.MaLichHoc == maLichHoc)
                .ToDictionaryAsync(x => x.MaHocVien);

            var rows = hocViens.Select(hv =>
            {
                diemDanhs.TryGetValue(hv.MaHocVien, out var dd);

                return new DiemDanhExportRow
                {
                    MaHocVien = hv.MaHocVien,
                    HoTen = hv.HoTen,
                    TrangThai = dd?.TrangThai ?? "Chưa điểm danh",
                    GioThamGia = dd?.GioThamGia,
                    GhiChu = dd?.GhiChu
                };
            }).ToList();

            return (lichHoc, rows);
        }
        [HttpGet]
        public async Task<IActionResult> ExportExcel(string maLichHoc)
        {
            if (string.IsNullOrWhiteSpace(maLichHoc))
                return BadRequest("Mã lịch học không hợp lệ.");

            var (lichHoc, rows) = await LayDuLieuXuatAsync(maLichHoc);

            if (lichHoc == null)
                return NotFound("Không tìm thấy lịch học.");

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Danh sách điểm danh");

            ws.Cell(1, 1).Value = "DANH SÁCH ĐIỂM DANH HỌC VIÊN";
            ws.Range(1, 1, 1, 6).Merge();
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 16;
            ws.Cell(1, 1).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            ws.Cell(3, 1).Value = "Lớp học:";
            ws.Cell(3, 2).Value = lichHoc.MaLopHocNavigation.TenLopHoc;

            ws.Cell(4, 1).Value = "Mã lịch học:";
            ws.Cell(4, 2).Value = lichHoc.MaLichHoc;

            ws.Cell(5, 1).Value = "Ngày học:";
            ws.Cell(5, 2).Value = lichHoc.NgayHoc.ToString("dd/MM/yyyy");

            ws.Cell(6, 1).Value = "Thời gian:";
            ws.Cell(6, 2).Value =
                $"{lichHoc.GioBatDau:HH:mm} - {lichHoc.GioKetThuc:HH:mm}";

            string[] headers =
            {
        "STT", "Mã học viên", "Họ và tên",
        "Trạng thái", "Số giờ tham gia", "Ghi chú"
    };

            for (int col = 0; col < headers.Length; col++)
                ws.Cell(8, col + 1).Value = headers[col];

            var header = ws.Range(8, 1, 8, headers.Length);
            header.Style.Font.Bold = true;
            header.Style.Fill.BackgroundColor = XLColor.LightBlue;
            header.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;
            header.Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;
            header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            for (int i = 0; i < rows.Count; i++)
            {
                int r = i + 9;
                var hv = rows[i];

                ws.Cell(r, 1).Value = i + 1;
                ws.Cell(r, 2).Value = hv.MaHocVien;
                ws.Cell(r, 3).Value = hv.HoTen;
                ws.Cell(r, 4).Value = hv.TrangThai;

                if (hv.GioThamGia.HasValue)
                    ws.Cell(r, 5).Value = (double)hv.GioThamGia.Value;
                else
                    ws.Cell(r, 5).Value = "—";

                ws.Cell(r, 6).Value = hv.GhiChu ?? "";

                ws.Range(r, 1, r, 6).Style.Border.OutsideBorder =
                    XLBorderStyleValues.Thin;
                ws.Range(r, 1, r, 6).Style.Border.InsideBorder =
                    XLBorderStyleValues.Thin;
            }

            ws.Columns().AdjustToContents();
            ws.Column(3).Width = Math.Max(ws.Column(3).Width, 22);
            ws.Column(6).Width = Math.Max(ws.Column(6).Width, 25);
            ws.SheetView.FreezeRows(8);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            string fileName =
                $"DiemDanh_{lichHoc.MaLichHoc}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        [HttpGet]
        public async Task<IActionResult> ExportPdf(string maLichHoc)
        {
            if (string.IsNullOrWhiteSpace(maLichHoc))
                return BadRequest("Mã lịch học không hợp lệ.");

            var (lichHoc, rows) = await LayDuLieuXuatAsync(maLichHoc);

            if (lichHoc == null)
                return NotFound("Không tìm thấy lịch học.");

            var pdf = Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(25);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Column(col =>
                    {
                        col.Item().AlignCenter().Text(
                            "DANH SÁCH ĐIỂM DANH HỌC VIÊN")
                            .Bold().FontSize(16);

                        col.Item().PaddingTop(8).Text(
                            $"Lớp: {lichHoc.MaLopHocNavigation.TenLopHoc}");

                        col.Item().Text(
                            $"Mã lịch học: {lichHoc.MaLichHoc}");

                        col.Item().Text(
                            $"Ngày học: {lichHoc.NgayHoc:dd/MM/yyyy} | " +
                            $"Thời gian: {lichHoc.GioBatDau:HH:mm} - " +
                            $"{lichHoc.GioKetThuc:HH:mm}");
                    });

                    page.Content().PaddingTop(15).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30);
                            columns.ConstantColumn(85);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1.3f);
                            columns.ConstantColumn(80);
                            columns.RelativeColumn(1.5f);
                        });

                        table.Header(header =>
                        {
                            string[] headers =
                            {
                        "STT", "Mã học viên", "Họ và tên",
                        "Trạng thái", "Số giờ", "Ghi chú"
                    };

                            foreach (var title in headers)
                            {
                                header.Cell()
                                    .Element(HeaderCell)
                                    .Text(title);
                            }
                        });

                        for (int i = 0; i < rows.Count; i++)
                        {
                            var hv = rows[i];

                            table.Cell().Element(BodyCell).Text((i + 1).ToString());
                            table.Cell().Element(BodyCell).Text(hv.MaHocVien);
                            table.Cell().Element(BodyCell).Text(hv.HoTen);
                            table.Cell().Element(BodyCell).Text(hv.TrangThai);

                            table.Cell().Element(BodyCell).Text(
                                hv.GioThamGia.HasValue
                                    ? hv.GioThamGia.Value.ToString("0.##")
                                    : "—");

                            table.Cell().Element(BodyCell).Text(hv.GhiChu ?? "");
                        }
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Trang ");
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
                });
            });

            byte[] fileBytes = pdf.GeneratePdf();

            string fileName =
                $"DiemDanh_{lichHoc.MaLichHoc}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            return File(fileBytes, "application/pdf", fileName);
        }

        private static IContainer HeaderCell(IContainer container)
        {
            return container
                .Background(Colors.Grey.Lighten2)
                .Border(1)
                .BorderColor(Colors.Grey.Lighten1)
                .Padding(5)
                .DefaultTextStyle(x => x.Bold());
        }

        private static IContainer BodyCell(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(5);
        }
    }

 }
