
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
using TTDaoTaoLaiXe.Models;

namespace TTDaoTaoLaiXe.Controllers
{
    [Authorize]
    public class KetQuaHocTapController : Controller
    {
        private readonly TTDaoTaoLaiXeContext _context;
        private const int PageSize = 10;

        private static readonly string[] DanhSachKetQua =
        {
            "Đạt",
            "Không đạt",
            "Chưa thi"
        };

        public KetQuaHocTapController(TTDaoTaoLaiXeContext context)
        {
            _context = context;
        }

        // =========================================================
        // INDEX - TÌM KIẾM, LỌC, PHÂN TRANG
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Index(
            string? tuKhoa,
            string? maMonHoc,
            string? ketQuaLoc,
            int page = 1)
        {
            page = Math.Max(1, page);

            var query = TruyVanKetQua(
                tuKhoa,
                maMonHoc,
                ketQuaLoc);

            int totalItems = await query.CountAsync();

            int totalPages = Math.Max(
                1,
                (int)Math.Ceiling(totalItems / (double)PageSize));

            if (page > totalPages)
                page = totalPages;

            var danhSach = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            await NapDuLieuDropdownAsync();

            ViewBag.TuKhoa = tuKhoa ?? "";
            ViewBag.MaMonHoc = maMonHoc ?? "";
            ViewBag.KetQuaLoc = ketQuaLoc ?? "";
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(danhSach);
        }

        // =========================================================
        // TRUY VẤN CHUNG
        // =========================================================
        private IQueryable<KetQuaHocTap> TruyVanKetQua(
            string? tuKhoa,
            string? maMonHoc,
            string? ketQuaLoc)
        {
            var query = _context.KetQuaHocTaps
                .AsNoTracking()
                .Include(x => x.MaDangKyNavigation)
                    .ThenInclude(x => x.MaHocVienNavigation)
                .Include(x => x.MaMonHocNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                string keyword = tuKhoa.Trim();

                query = query.Where(x =>
                    x.MaKetQua.Contains(keyword) ||
                    x.MaDangKy.Contains(keyword) ||
                    x.MaMonHoc.Contains(keyword) ||
                    x.MaDangKyNavigation.MaHocVienNavigation.HoTen
                        .Contains(keyword) ||
                    x.MaMonHocNavigation.TenMonHoc.Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(maMonHoc))
            {
                query = query.Where(x => x.MaMonHoc == maMonHoc.Trim());
            }

            if (!string.IsNullOrWhiteSpace(ketQuaLoc))
            {
                query = query.Where(x => x.KetQua == ketQuaLoc.Trim());
            }

            return query
                .OrderBy(x => x.MaDangKy)
                .ThenBy(x => x.MaMonHoc)
                .ThenBy(x => x.LanThi);
        }

        // =========================================================
        // DỮ LIỆU DROPDOWN
        // =========================================================
        private async Task NapDuLieuDropdownAsync()
        {
            ViewBag.MonHocList = await _context.MonHocs
                .AsNoTracking()
                .OrderBy(x => x.TenMonHoc)
                .Select(x => new
                {
                    x.MaMonHoc,
                    x.TenMonHoc
                })
                .ToListAsync();

            ViewBag.DangKyList = await _context.DangKyHocs
                .AsNoTracking()
                .Include(x => x.MaHocVienNavigation)
                .OrderBy(x => x.MaDangKy)
                .Select(x => new
                {
                    x.MaDangKy,
                    HoTen = x.MaHocVienNavigation.HoTen
                })
                .ToListAsync();

            ViewBag.KetQuaList = DanhSachKetQua;
        }

        // =========================================================
        // KIỂM TRA DỮ LIỆU
        // =========================================================
        private static string? KiemTraDuLieu(KetQuaHocTap model)
        {
            if (string.IsNullOrWhiteSpace(model.MaKetQua))
                return "Vui lòng nhập mã kết quả.";

            if (model.MaKetQua.Trim().Length > 20)
                return "Mã kết quả không được vượt quá 20 ký tự.";

            if (string.IsNullOrWhiteSpace(model.MaDangKy))
                return "Vui lòng chọn đăng ký học.";

            if (model.MaDangKy.Trim().Length > 20)
                return "Mã đăng ký không hợp lệ.";

            if (string.IsNullOrWhiteSpace(model.MaMonHoc))
                return "Vui lòng chọn môn học.";

            if (model.MaMonHoc.Trim().Length > 20)
                return "Mã môn học không hợp lệ.";

            if (model.LanThi < 1 || model.LanThi > 999)
                return "Lần thi phải nằm trong khoảng từ 1 đến 999.";

            if (string.IsNullOrWhiteSpace(model.KetQua) ||
                !DanhSachKetQua.Contains(model.KetQua.Trim()))
            {
                return "Kết quả thi không hợp lệ.";
            }

            if (model.GhiChu != null && model.GhiChu.Length > 500)
                return "Ghi chú không được vượt quá 500 ký tự.";

            if (model.Diem.HasValue)
            {
                if (model.Diem.Value < 0 || model.Diem.Value > 10)
                    return "Điểm phải nằm trong khoảng từ 0 đến 10.";

                if (decimal.Round(model.Diem.Value, 2) != model.Diem.Value)
                    return "Điểm chỉ được có tối đa 2 chữ số thập phân.";
            }

            if (model.KetQua == "Chưa thi")
            {
                if (model.Diem.HasValue || model.NgayThi.HasValue)
                {
                    return "Kết quả Chưa thi không được có điểm hoặc ngày thi.";
                }
            }
            else
            {
                if (!model.Diem.HasValue)
                    return "Vui lòng nhập điểm cho kết quả thi.";

                if (!model.NgayThi.HasValue)
                    return "Vui lòng nhập ngày thi.";
            }

            return null;
        }

        private async Task<string?> KiemTraKhoaNgoaiAsync(
            string maDangKy,
            string maMonHoc)
        {
            bool dangKyTonTai = await _context.DangKyHocs
                .AnyAsync(x => x.MaDangKy == maDangKy);

            if (!dangKyTonTai)
                return $"Mã đăng ký '{maDangKy}' không tồn tại.";

            bool monHocTonTai = await _context.MonHocs
                .AnyAsync(x => x.MaMonHoc == maMonHoc);

            if (!monHocTonTai)
                return $"Mã môn học '{maMonHoc}' không tồn tại.";

            return null;
        }

        private async Task<bool> BiTrungLanThiAsync(
            string maDangKy,
            string maMonHoc,
            int lanThi,
            string? maKetQuaHienTai = null)
        {
            return await _context.KetQuaHocTaps.AnyAsync(x =>
                x.MaDangKy == maDangKy &&
                x.MaMonHoc == maMonHoc &&
                x.LanThi == lanThi &&
                x.MaKetQua != maKetQuaHienTai);
        }

        // =========================================================
        // CREATE - AJAX
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KetQuaHocTap model)
        {
            ChuanHoaDuLieu(model);

            string? loi = KiemTraDuLieu(model);

            if (loi != null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = loi
                });
            }

            if (await _context.KetQuaHocTaps.AnyAsync(
                x => x.MaKetQua == model.MaKetQua))
            {
                return Conflict(new
                {
                    success = false,
                    message = "Mã kết quả đã tồn tại."
                });
            }

            loi = await KiemTraKhoaNgoaiAsync(
                model.MaDangKy,
                model.MaMonHoc);

            if (loi != null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = loi
                });
            }

            if (await BiTrungLanThiAsync(
                model.MaDangKy,
                model.MaMonHoc,
                model.LanThi))
            {
                return Conflict(new
                {
                    success = false,
                    message = "Đăng ký này đã có kết quả cho môn học ở lần thi đó."
                });
            }

            try
            {
                _context.KetQuaHocTaps.Add(model);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Thêm kết quả học tập thành công."
                });
            }
            catch (DbUpdateException)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Không thể lưu dữ liệu. Hãy kiểm tra khóa ngoại và dữ liệu trùng."
                });
            }
        }

        // =========================================================
        // EDIT - AJAX
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(KetQuaHocTap model)
        {
            ChuanHoaDuLieu(model);

            var entity = await _context.KetQuaHocTaps
                .FirstOrDefaultAsync(x => x.MaKetQua == model.MaKetQua);

            if (entity == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy kết quả học tập cần sửa."
                });
            }

            string? loi = KiemTraDuLieu(model);

            if (loi != null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = loi
                });
            }

            loi = await KiemTraKhoaNgoaiAsync(
                model.MaDangKy,
                model.MaMonHoc);

            if (loi != null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = loi
                });
            }

            if (await BiTrungLanThiAsync(
                model.MaDangKy,
                model.MaMonHoc,
                model.LanThi,
                entity.MaKetQua))
            {
                return Conflict(new
                {
                    success = false,
                    message = "Đăng ký này đã có kết quả cho môn học ở lần thi đó."
                });
            }

            entity.MaDangKy = model.MaDangKy;
            entity.MaMonHoc = model.MaMonHoc;
            entity.LanThi = model.LanThi;
            entity.NgayThi = model.NgayThi;
            entity.Diem = model.Diem;
            entity.KetQua = model.KetQua;
            entity.GhiChu = model.GhiChu;

            try
            {
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Cập nhật kết quả học tập thành công."
                });
            }
            catch (DbUpdateException)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Không thể cập nhật dữ liệu. Hãy kiểm tra thông tin liên quan."
                });
            }
        }

        private static void ChuanHoaDuLieu(KetQuaHocTap model)
        {
            model.MaKetQua = model.MaKetQua?.Trim() ?? "";
            model.MaDangKy = model.MaDangKy?.Trim() ?? "";
            model.MaMonHoc = model.MaMonHoc?.Trim() ?? "";
            model.KetQua = model.KetQua?.Trim() ?? "";

            model.GhiChu = string.IsNullOrWhiteSpace(model.GhiChu)
                ? null
                : model.GhiChu.Trim();
        }

        // =========================================================
        // IMPORT EXCEL
        //
        // Thứ tự cột:
        // 1 Mã kết quả
        // 2 Mã đăng ký
        // 3 Họ tên
        // 4 Mã môn học
        // 5 Tên môn học
        // 6 Lần thi
        // 7 Ngày thi
        // 8 Điểm
        // 9 Kết quả
        // 10 Ghi chú
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> ImportExcel(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Vui lòng chọn file Excel."
                });
            }

            if (!Path.GetExtension(file.FileName)
                .Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Chỉ hỗ trợ file Excel định dạng .xlsx."
                });
            }

            int themMoi = 0;
            int boQua = 0;
            int loi = 0;

            var chiTiet = new List<string>();
            var danhSachThem = new List<KetQuaHocTap>();

            var maKetQuaTrongFile = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            try
            {
                using var stream = file.OpenReadStream();
                using var workbook = new XLWorkbook(stream);

                var ws = workbook.Worksheets.FirstOrDefault();

                if (ws == null || ws.LastRowUsed() == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "File Excel không có dữ liệu."
                    });
                }

                int lastRow = ws.LastRowUsed()!.RowNumber();

                if (lastRow < 2)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "File Excel chưa có dòng dữ liệu."
                    });
                }

                string[] headerMongDoi =
                {
                    "Mã kết quả",
                    "Mã đăng ký",
                    "Họ tên",
                    "Mã môn học",
                    "Tên môn học",
                    "Lần thi",
                    "Ngày thi",
                    "Điểm",
                    "Kết quả",
                    "Ghi chú"
                };

                for (int col = 1; col <= headerMongDoi.Length; col++)
                {
                    string header = ws.Cell(1, col).GetString().Trim();

                    if (!string.Equals(
                        header,
                        headerMongDoi[col - 1],
                        StringComparison.OrdinalIgnoreCase))
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = $"Sai mẫu Excel tại cột {col}. " +
                                      $"Cần tiêu đề '{headerMongDoi[col - 1]}'."
                        });
                    }
                }

                // SỬA LỖI CS1061:
                // ToListAsync() truy vấn SQL, ToHashSet() chạy trong bộ nhớ.
                var khoaNgoaiDangKy = (await _context.DangKyHocs
                    .AsNoTracking()
                    .Select(x => x.MaDangKy)
                    .ToListAsync())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var khoaNgoaiMonHoc = (await _context.MonHocs
                    .AsNoTracking()
                    .Select(x => x.MaMonHoc)
                    .ToListAsync())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var maKetQuaDaCo = (await _context.KetQuaHocTaps
                    .AsNoTracking()
                    .Select(x => x.MaKetQua)
                    .ToListAsync())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var lanThiDaCo = await _context.KetQuaHocTaps
                    .AsNoTracking()
                    .Select(x => new
                    {
                        x.MaDangKy,
                        x.MaMonHoc,
                        x.LanThi
                    })
                    .ToListAsync();

                var tapLanThi = lanThiDaCo
                    .Select(x => TaoKhoaLanThi(
                        x.MaDangKy,
                        x.MaMonHoc,
                        x.LanThi))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                for (int row = 2; row <= lastRow; row++)
                {
                    // Bỏ qua dòng trống.
                    bool dongTrong = true;

                    for (int col = 1; col <= 10; col++)
                    {
                        if (!ws.Cell(row, col).IsEmpty())
                        {
                            dongTrong = false;
                            break;
                        }
                    }

                    if (dongTrong)
                        continue;

                    string maKetQua = ws.Cell(row, 1).GetString().Trim();
                    string maDangKy = ws.Cell(row, 2).GetString().Trim();
                    string maMonHoc = ws.Cell(row, 4).GetString().Trim();
                    string lanThiText = ws.Cell(row, 6).GetString().Trim();
                    string ngayThiText = ws.Cell(row, 7).GetString().Trim();
                    string diemText = ws.Cell(row, 8).GetString().Trim();
                    string ketQua = ws.Cell(row, 9).GetString().Trim();
                    string ghiChu = ws.Cell(row, 10).GetString().Trim();

                    if (string.IsNullOrWhiteSpace(maKetQua) ||
                        string.IsNullOrWhiteSpace(maDangKy) ||
                        string.IsNullOrWhiteSpace(maMonHoc))
                    {
                        loi++;
                        chiTiet.Add(
                            $"Dòng {row}: Thiếu mã kết quả, mã đăng ký hoặc mã môn học.");
                        continue;
                    }

                    if (maKetQua.Length > 20 ||
                        maDangKy.Length > 20 ||
                        maMonHoc.Length > 20)
                    {
                        loi++;
                        chiTiet.Add(
                            $"Dòng {row}: Mã kết quả, mã đăng ký hoặc mã môn học vượt quá độ dài cho phép.");
                        continue;
                    }

                    if (maKetQuaDaCo.Contains(maKetQua) ||
                        !maKetQuaTrongFile.Add(maKetQua))
                    {
                        boQua++;
                        chiTiet.Add(
                            $"Dòng {row}: Mã kết quả '{maKetQua}' đã tồn tại hoặc bị trùng trong file.");
                        continue;
                    }

                    if (!khoaNgoaiDangKy.Contains(maDangKy))
                    {
                        loi++;
                        chiTiet.Add(
                            $"Dòng {row}: Mã đăng ký '{maDangKy}' không tồn tại.");
                        continue;
                    }

                    if (!khoaNgoaiMonHoc.Contains(maMonHoc))
                    {
                        loi++;
                        chiTiet.Add(
                            $"Dòng {row}: Mã môn học '{maMonHoc}' không tồn tại.");
                        continue;
                    }

                    if (!int.TryParse(
                        lanThiText,
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out int lanThi) ||
                        lanThi < 1 ||
                        lanThi > 999)
                    {
                        loi++;
                        chiTiet.Add($"Dòng {row}: Lần thi không hợp lệ.");
                        continue;
                    }

                    string khoaLanThi = TaoKhoaLanThi(
                        maDangKy,
                        maMonHoc,
                        lanThi);

                    if (tapLanThi.Contains(khoaLanThi))
                    {
                        boQua++;
                        chiTiet.Add(
                            $"Dòng {row}: Đã tồn tại lần thi {lanThi} " +
                            $"của đăng ký {maDangKy}, môn {maMonHoc}.");
                        continue;
                    }

                    // Đọc ngày thi.
                    DateOnly? ngayThi = null;

                    if (!string.IsNullOrWhiteSpace(ngayThiText))
                    {
                        if (ws.Cell(row, 7).TryGetValue<DateTime>(
                            out var ngayExcel))
                        {
                            ngayThi = DateOnly.FromDateTime(ngayExcel);
                        }
                        else if (DateOnly.TryParseExact(
                            ngayThiText,
                            new[]
                            {
                                "yyyy-MM-dd",
                                "dd/MM/yyyy",
                                "d/M/yyyy"
                            },
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out var ngayParsed))
                        {
                            ngayThi = ngayParsed;
                        }
                        else
                        {
                            loi++;
                            chiTiet.Add(
                                $"Dòng {row}: Ngày thi không đúng định dạng.");
                            continue;
                        }
                    }

                    // Đọc điểm.
                    decimal? diem = null;

                    if (!string.IsNullOrWhiteSpace(diemText))
                    {
                        if (ws.Cell(row, 8).TryGetValue<double>(
                            out var diemExcel))
                        {
                            diem = Convert.ToDecimal(diemExcel);
                        }
                        else if (decimal.TryParse(
                            diemText,
                            NumberStyles.Number,
                            CultureInfo.InvariantCulture,
                            out var diemParsed))
                        {
                            diem = diemParsed;
                        }
                        else if (decimal.TryParse(
                            diemText,
                            NumberStyles.Number,
                            CultureInfo.GetCultureInfo("vi-VN"),
                            out diemParsed))
                        {
                            diem = diemParsed;
                        }
                        else
                        {
                            loi++;
                            chiTiet.Add($"Dòng {row}: Điểm không hợp lệ.");
                            continue;
                        }
                    }

                    var entity = new KetQuaHocTap
                    {
                        MaKetQua = maKetQua,
                        MaDangKy = maDangKy,
                        MaMonHoc = maMonHoc,
                        LanThi = lanThi,
                        NgayThi = ngayThi,
                        Diem = diem,
                        KetQua = ketQua,
                        GhiChu = string.IsNullOrWhiteSpace(ghiChu)
                            ? null
                            : ghiChu
                    };

                    ChuanHoaDuLieu(entity);

                    string? loiDuLieu = KiemTraDuLieu(entity);

                    if (loiDuLieu != null)
                    {
                        loi++;
                        chiTiet.Add($"Dòng {row}: {loiDuLieu}");
                        continue;
                    }

                    danhSachThem.Add(entity);

                    // Đánh dấu ngay để không nhận trùng trong cùng file.
                    maKetQuaDaCo.Add(maKetQua);
                    tapLanThi.Add(khoaLanThi);
                }

                if (danhSachThem.Count > 0)
                {
                    await using var transaction =
                        await _context.Database.BeginTransactionAsync();

                    try
                    {
                        _context.KetQuaHocTaps.AddRange(danhSachThem);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        themMoi = danhSachThem.Count;
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }

                return Json(new
                {
                    success = true,
                    message = "Đã xử lý file Excel.",
                    themMoi,
                    boQua,
                    loi,
                    chiTiet = chiTiet.Take(100).ToList()
                });
            }
            catch (Exception)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Không thể đọc hoặc lưu file Excel. " +
                              "Hãy kiểm tra mẫu file, dữ liệu và kết nối cơ sở dữ liệu."
                });
            }
        }

        private static string TaoKhoaLanThi(
            string maDangKy,
            string maMonHoc,
            int lanThi)
        {
            return $"{maDangKy.Trim()}|{maMonHoc.Trim()}|{lanThi}";
        }

        // =========================================================
        // EXPORT EXCEL - THEO BỘ LỌC, KHÔNG GIỚI HẠN PHÂN TRANG
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> ExportExcel(
            string? tuKhoa,
            string? maMonHoc,
            string? ketQuaLoc)
        {
            var danhSach = await TruyVanKetQua(
                tuKhoa,
                maMonHoc,
                ketQuaLoc)
                .ToListAsync();

            using var workbook = new XLWorkbook();

            var ws = workbook.Worksheets.Add("KetQuaHocTap");

            ws.Cell(1, 1).Value = "BÁO CÁO KẾT QUẢ HỌC TẬP";
            ws.Range(1, 1, 1, 10).Merge();

            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 16;
            ws.Cell(1, 1).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            string[] headers =
            {
                "Mã kết quả",
                "Mã đăng ký",
                "Họ tên",
                "Mã môn học",
                "Tên môn học",
                "Lần thi",
                "Ngày thi",
                "Điểm",
                "Kết quả",
                "Ghi chú"
            };

            for (int col = 1; col <= headers.Length; col++)
            {
                ws.Cell(3, col).Value = headers[col - 1];
            }

            var headerRange = ws.Range(3, 1, 3, headers.Length);

            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
            headerRange.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;
            headerRange.Style.Border.OutsideBorder =
                XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder =
                XLBorderStyleValues.Thin;

            int row = 4;

            foreach (var item in danhSach)
            {
                ws.Cell(row, 1).Value = item.MaKetQua;
                ws.Cell(row, 2).Value = item.MaDangKy;

                ws.Cell(row, 3).Value =
                    item.MaDangKyNavigation?
                        .MaHocVienNavigation?.HoTen ?? "";

                ws.Cell(row, 4).Value = item.MaMonHoc;

                ws.Cell(row, 5).Value =
                    item.MaMonHocNavigation?.TenMonHoc ?? "";

                ws.Cell(row, 6).Value = item.LanThi;

                if (item.NgayThi.HasValue)
                {
                    ws.Cell(row, 7).Value =
                        item.NgayThi.Value.ToDateTime(TimeOnly.MinValue);

                    ws.Cell(row, 7).Style.DateFormat.Format = "dd/MM/yyyy";
                }

                if (item.Diem.HasValue)
                {
                    ws.Cell(row, 8).Value = item.Diem.Value;
                    ws.Cell(row, 8).Style.NumberFormat.Format = "0.##";
                }

                ws.Cell(row, 9).Value = item.KetQua;
                ws.Cell(row, 10).Value = item.GhiChu ?? "";

                row++;
            }

            if (row > 4)
            {
                var dataRange = ws.Range(4, 1, row - 1, 10);

                dataRange.Style.Border.OutsideBorder =
                    XLBorderStyleValues.Thin;

                dataRange.Style.Border.InsideBorder =
                    XLBorderStyleValues.Hair;
            }

            ws.SheetView.FreezeRows(3);
            ws.Columns().AdjustToContents();

            ws.Column(10).Width = 35;
            ws.Column(10).Style.Alignment.WrapText = true;

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);
            stream.Position = 0;

            string fileName =
                $"KetQuaHocTap_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        // =========================================================
        // EXPORT PDF - THEO BỘ LỌC, KHÔNG GIỚI HẠN PHÂN TRANG
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> ExportPdf(
            string? tuKhoa,
            string? maMonHoc,
            string? ketQuaLoc)
        {
            var danhSach = await TruyVanKetQua(
                tuKhoa,
                maMonHoc,
                ketQuaLoc)
                .ToListAsync();

            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(8));

                    page.Header()
                        .Column(column =>
                        {
                            column.Item()
                                .AlignCenter()
                                .Text("BÁO CÁO KẾT QUẢ HỌC TẬP")
                                .FontSize(16)
                                .Bold();

                            column.Item()
                                .AlignCenter()
                                .Text(
                                    $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}")
                                .FontSize(9);

                            column.Item()
                                .PaddingVertical(5)
                                .LineHorizontal(1);
                        });

                    page.Content().Column(column =>
                    {
                        column.Item()
                            .PaddingBottom(8)
                            .Text($"Tổng số kết quả: {danhSach.Count}");

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(55);
                                columns.ConstantColumn(55);
                                columns.RelativeColumn(1.4f);
                                columns.RelativeColumn(1.3f);
                                columns.ConstantColumn(35);
                                columns.ConstantColumn(60);
                                columns.ConstantColumn(40);
                                columns.ConstantColumn(60);
                                columns.RelativeColumn(1.5f);
                            });

                            table.Header(header =>
                            {
                                string[] tieuDe =
                                {
                                    "Mã KQ",
                                    "Mã ĐK",
                                    "Học viên",
                                    "Môn học",
                                    "Lần thi",
                                    "Ngày thi",
                                    "Điểm",
                                    "Kết quả",
                                    "Ghi chú"
                                };

                                foreach (var ten in tieuDe)
                                {
                                    header.Cell()
                                        .Background(Colors.Grey.Lighten2)
                                        .Border(0.5f)
                                        .Padding(4)
                                        .Text(ten)
                                        .Bold();
                                }
                            });

                            foreach (var item in danhSach)
                            {
                                string hoTen =
                                    item.MaDangKyNavigation?
                                        .MaHocVienNavigation?.HoTen ?? "";

                                string tenMon =
                                    item.MaMonHocNavigation?.TenMonHoc ?? "";

                                string ngayThi = item.NgayThi.HasValue
                                    ? item.NgayThi.Value.ToString("dd/MM/yyyy")
                                    : "—";

                                string diem = item.Diem.HasValue
                                    ? item.Diem.Value.ToString("0.##")
                                    : "—";

                                string[] giaTri =
                                {
                                    item.MaKetQua,
                                    item.MaDangKy,
                                    hoTen,
                                    tenMon,
                                    item.LanThi.ToString(),
                                    ngayThi,
                                    diem,
                                    item.KetQua,
                                    item.GhiChu ?? "—"
                                };

                                foreach (var value in giaTri)
                                {
                                    table.Cell()
                                        .Border(0.5f)
                                        .BorderColor(Colors.Grey.Lighten1)
                                        .Padding(4)
                                        .Text(value);
                                }
                            }
                        });
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

            byte[] pdfBytes = document.GeneratePdf();

            return File(
                pdfBytes,
                "application/pdf",
                $"KetQuaHocTap_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }
    }
}
