
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TTDaoTaoLaiXe.Models;

namespace TTDaoTaoLaiXe.Controllers
{
    public class LichHocController : Controller
    {
        private readonly TTDaoTaoLaiXeContext _context;

        private const int PageSize = 10;

        private const string TrangThaiDaLenLich = "Đã lên lịch";

        public LichHocController(TTDaoTaoLaiXeContext context)
        {
            _context = context;
        }

        // =========================
        // DANH SÁCH + TÌM KIẾM + PHÂN TRANG
        // =========================
        public async Task<IActionResult> Index(
            string? tuKhoa,
            string? trangThai,
            DateOnly? ngayHoc,
            int page = 1)
        {
            page = Math.Max(page, 1);

            var query = _context.LichHocs
                .AsNoTracking()
                .Include(x => x.MaLopHocNavigation)
                .Include(x => x.MaGiaoVienNavigation)
                .Include(x => x.MaPhongNavigation)
                .Include(x => x.MaXeNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();

                query = query.Where(x =>
                    x.MaLichHoc.Contains(tuKhoa) ||
                    x.MaLopHoc.Contains(tuKhoa) ||
                    x.MaLopHocNavigation.TenLopHoc.Contains(tuKhoa) ||
                    x.MaGiaoVienNavigation.HoTen.Contains(tuKhoa) ||
                    (x.NoiDung != null &&
                     x.NoiDung.Contains(tuKhoa)));
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(x => x.TrangThai == trangThai);
            }

            if (ngayHoc.HasValue)
            {
                query = query.Where(x => x.NgayHoc == ngayHoc.Value);
            }

            int tongSoBanGhi = await query.CountAsync();

            int tongSoTrang = (int)Math.Ceiling(
                tongSoBanGhi / (double)PageSize);

            if (tongSoTrang > 0 && page > tongSoTrang)
            {
                page = tongSoTrang;
            }

            var danhSach = await query
                .OrderByDescending(x => x.NgayHoc)
                .ThenBy(x => x.GioBatDau)
                .ThenBy(x => x.MaLichHoc)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.TrangThaiFilter = trangThai;
            ViewBag.NgayHocFilter = ngayHoc?.ToString("yyyy-MM-dd");

            ViewBag.DanhSachTrangThai = await _context.LichHocs
                .AsNoTracking()
                .Select(x => x.TrangThai)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.PageSize = PageSize;
            ViewBag.TongSoBanGhi = tongSoBanGhi;
            ViewBag.TongSoTrang = tongSoTrang;

            return View(danhSach);
        }

        // =========================
        // MỞ MODAL THÊM
        // =========================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await NapDuLieuModalAsync();

            var model = new LichHoc
            {
                NgayHoc = DateOnly.FromDateTime(DateTime.Today),
                GioBatDau = new TimeOnly(7, 0),
                GioKetThuc = new TimeOnly(9, 0),
                LoaiBuoiHoc = "Thực hành",
                TrangThai = TrangThaiDaLenLich
            };

            return PartialView("_CreateModal", model);
        }

        // =========================
        // LƯU THÊM LỊCH HỌC
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind(
                "MaLopHoc,MaGiaoVien,MaPhong,MaXe," +
                "NgayHoc,GioBatDau,GioKetThuc," +
                "LoaiBuoiHoc,NoiDung,SoGio,TrangThai,GhiChu")]
            LichHoc model)
        {
            model.MaLichHoc = string.Empty;

            // Tự sinh mã lịch ở phía server.
            ModelState.Remove(nameof(model.MaLichHoc));

            ModelState.Remove(nameof(LichHoc.MaLopHocNavigation));
            ModelState.Remove(nameof(LichHoc.MaGiaoVienNavigation));
            ModelState.Remove(nameof(LichHoc.MaPhongNavigation));
            ModelState.Remove(nameof(LichHoc.MaXeNavigation));

            // Kiểm tra dữ liệu theo các quy tắc khai báo trong Model.
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Dữ liệu nhập chưa hợp lệ.",
                    errors = LayLoiModelState()
                });
            }

            var loiNghiepVu = await KiemTraLichHocAsync(model);

            if (loiNghiepVu != null)
            {
                return Json(new
                {
                    success = false,
                    message = loiNghiepVu
                });
            }

            model.MaLichHoc = await TaoMaLichHocAsync();

            // Trạng thái ban đầu do hệ thống thiết lập.
            model.TrangThai = TrangThaiDaLenLich;

            // Tự tính số giờ nếu người dùng không nhập.
            model.SoGio = TinhSoGio(
                model.GioBatDau,
                model.GioKetThuc);

            _context.LichHocs.Add(model);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Thêm lịch học thành công.",
                id = model.MaLichHoc
            });
        }

        // =========================
        // MỞ MODAL SỬA
        // =========================
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var model = await _context.LichHocs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaLichHoc == id);

            if (model == null)
            {
                return NotFound();
            }

            if (model.TrangThai != TrangThaiDaLenLich)
            {
                return BadRequest(
                    "Chỉ được chỉnh sửa lịch đang ở trạng thái Đã lên lịch.");
            }

            await NapDuLieuModalAsync(model.MaLopHoc);

            return PartialView("_EditModal", model);
        }

        // =========================
        // LƯU SỬA LỊCH HỌC
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            string id,
            [Bind(
                "MaLichHoc,MaLopHoc,MaGiaoVien,MaPhong,MaXe," +
                "NgayHoc,GioBatDau,GioKetThuc," +
                "LoaiBuoiHoc,NoiDung,SoGio,TrangThai,GhiChu")]
            LichHoc model)
        {
            ModelState.Remove(nameof(LichHoc.MaLopHocNavigation));
            ModelState.Remove(nameof(LichHoc.MaGiaoVienNavigation));
            ModelState.Remove(nameof(LichHoc.MaPhongNavigation));
            ModelState.Remove(nameof(LichHoc.MaXeNavigation));

            var lichHoc = await _context.LichHocs
                .FirstOrDefaultAsync(x => x.MaLichHoc == id);

            if (lichHoc == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Lịch học không tồn tại hoặc đã bị xóa."
                });
            }

            if (lichHoc.TrangThai != TrangThaiDaLenLich)
            {
                return Json(new
                {
                    success = false,
                    message = "Chỉ được sửa lịch đang ở trạng thái Đã lên lịch."
                });
            }

            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Dữ liệu nhập chưa hợp lệ.",
                    errors = LayLoiModelState()
                });
            }

            var loiNghiepVu = await KiemTraLichHocAsync(model, id);

            if (loiNghiepVu != null)
            {
                return Json(new
                {
                    success = false,
                    message = loiNghiepVu
                });
            }

            // Chỉ cập nhật các trường được phép chỉnh sửa.
            lichHoc.MaLopHoc = model.MaLopHoc;
            lichHoc.MaGiaoVien = model.MaGiaoVien;
            lichHoc.MaPhong = model.MaPhong;
            lichHoc.MaXe = model.MaXe;
            lichHoc.NgayHoc = model.NgayHoc;
            lichHoc.GioBatDau = model.GioBatDau;
            lichHoc.GioKetThuc = model.GioKetThuc;
            lichHoc.LoaiBuoiHoc = model.LoaiBuoiHoc;
            lichHoc.NoiDung = model.NoiDung;
            lichHoc.GhiChu = model.GhiChu;
            lichHoc.SoGio = TinhSoGio(
                model.GioBatDau,
                model.GioKetThuc);

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Cập nhật lịch học thành công.",
                id = lichHoc.MaLichHoc
            });
        }

        // =========================
        // KIỂM TRA NGHIỆP VỤ
        // =========================
        private async Task<string?> KiemTraLichHocAsync(
            LichHoc model,
            string? boQuaMaLich = null)
        {
            if (string.IsNullOrWhiteSpace(model.MaLopHoc) ||
                string.IsNullOrWhiteSpace(model.MaGiaoVien))
            {
                return "Vui lòng chọn lớp học và giáo viên.";
            }

            if (model.GioKetThuc <= model.GioBatDau)
            {
                return "Giờ kết thúc phải sau giờ bắt đầu.";
            }

            if (string.IsNullOrWhiteSpace(model.LoaiBuoiHoc) ||
                model.LoaiBuoiHoc.Trim().Length > 30)
            {
                return "Loại buổi học không được để trống và tối đa 30 ký tự.";
            }

            if (model.NoiDung?.Length > 500 ||
                model.GhiChu?.Length > 500)
            {
                return "Nội dung và ghi chú không được vượt quá 500 ký tự.";
            }

            // Chỉ cho xếp lịch vào lớp còn hoạt động học.
            var lopHoc = await _context.LopHocs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaLopHoc == model.MaLopHoc);

            if (lopHoc == null)
            {
                return "Lớp học được chọn không tồn tại.";
            }

            if (lopHoc.TrangThai == "Đã kết thúc" ||
                lopHoc.TrangThai == "Tạm ngưng")
            {
                return "Không thể xếp lịch cho lớp đã kết thúc hoặc đang tạm ngưng.";
            }

            if (model.NgayHoc < lopHoc.NgayBatDau)
            {
                return "Ngày học không được trước ngày bắt đầu lớp.";
            }

            if (lopHoc.NgayKetThuc.HasValue &&
                model.NgayHoc > lopHoc.NgayKetThuc.Value)
            {
                return "Ngày học không được sau ngày kết thúc lớp.";
            }


            // Giáo viên phải tồn tại và đang hoạt động.
            var giaoVien = await _context.GiaoViens
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.MaGiaoVien == model.MaGiaoVien);

            if (giaoVien == null)
            {
                return "Giáo viên được chọn không tồn tại.";
            }

            if (!giaoVien.TrangThai)
            {
                return "Giáo viên đã ngừng hoạt động, không thể xếp lịch.";
            }

            // Lấy hạng bằng của khóa học thuộc lớp.
            var khoaHoc = await _context.KhoaHocs
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.MaKhoaHoc == lopHoc.MaKhoaHoc);

            if (khoaHoc == null)
            {
                return "Không tìm thấy khóa học của lớp được chọn.";
            }

            string hangKhoaHoc = khoaHoc.HangGplx.Trim();

            // HangDuocPhepDay lưu dạng "B, C1".
            var cacHangDuocPhep = (giaoVien.HangDuocPhepDay ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (!cacHangDuocPhep.Contains(hangKhoaHoc))
            {
                return $"Giáo viên {giaoVien.HoTen} không được phép dạy hạng {hangKhoaHoc}.";
            }


            // Phòng học là tùy chọn, nhưng nếu chọn phải hoạt động.
            if (!string.IsNullOrWhiteSpace(model.MaPhong))
            {
                bool phongHopLe = await _context.PhongHocs
                    .AnyAsync(x =>
                        x.MaPhong == model.MaPhong &&
                        x.TrangThai);

                if (!phongHopLe)
                {
                    return "Phòng học không tồn tại hoặc đã ngừng hoạt động.";
                }
            }

            // Xe là tùy chọn. Kiểm tra tồn tại; chưa áp đặt
            // giá trị trạng thái xe vì cần xác nhận dữ liệu thực tế.
            if (!string.IsNullOrWhiteSpace(model.MaXe))
            {
                var xe = await _context.Xes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.MaXe == model.MaXe);

                if (xe == null)
                {
                    return "Xe được chọn không tồn tại.";
                }

                if (xe.TrangThai != "Đang sử dụng")
                {
                    return "Xe không ở trạng thái Đang sử dụng nên không thể phân công lịch học.";
                }
            }

            // Kiểm tra trùng giờ với lịch khác trong cùng ngày.

            var lichCungNgay = _context.LichHocs
                .AsNoTracking()
                .Where(x =>
                    x.NgayHoc == model.NgayHoc &&
                    x.MaLichHoc != (boQuaMaLich ?? "") &&
                    x.TrangThai != "Đã hủy" &&
                    x.GioBatDau < model.GioKetThuc &&
                    x.GioKetThuc > model.GioBatDau);


            // Giáo viên không thể dạy hai nơi cùng thời điểm.
            bool trungGiaoVien = await lichCungNgay.AnyAsync(x =>
                x.MaGiaoVien == model.MaGiaoVien);

            if (trungGiaoVien)
            {
                return "Giáo viên đã có lịch dạy trùng khung giờ này.";
            }

            // Phòng không thể được sử dụng cho hai buổi cùng lúc.
            if (!string.IsNullOrWhiteSpace(model.MaPhong))
            {
                bool trungPhong = await lichCungNgay.AnyAsync(x =>
                    x.MaPhong == model.MaPhong);

                if (trungPhong)
                {
                    return "Phòng học đã được sử dụng trong khung giờ này.";
                }
            }

            // Xe không thể phục vụ hai buổi cùng lúc.
            if (!string.IsNullOrWhiteSpace(model.MaXe))
            {
                bool trungXe = await lichCungNgay.AnyAsync(x =>
                    x.MaXe == model.MaXe);

                if (trungXe)
                {
                    return "Xe đã được xếp cho lịch học khác trong khung giờ này.";
                }
            }

            return null;
        }

        // =========================
        // NẠP DỮ LIỆU CHO DROPDOWN MODAL
        // =========================
        private async Task NapDuLieuModalAsync(
            string? maLopHienTai = null)
        {
            var lopHocs = await _context.LopHocs
                .AsNoTracking()
                .Where(x =>
                    x.TrangThai != "Đã kết thúc" &&
                    x.TrangThai != "Tạm ngưng")
                .OrderBy(x => x.TenLopHoc)
                .Select(x => new
                {
                    x.MaLopHoc,
                    x.TenLopHoc
                })
                .ToListAsync();

            // Khi sửa, vẫn cho hiển thị lớp hiện tại nếu lớp
            // đã bị tạm ngưng sau khi lịch được tạo.
            if (!string.IsNullOrWhiteSpace(maLopHienTai) &&
                !lopHocs.Any(x => x.MaLopHoc == maLopHienTai))
            {
                var lopHienTai = await _context.LopHocs
                    .AsNoTracking()
                    .Where(x => x.MaLopHoc == maLopHienTai)
                    .Select(x => new
                    {
                        x.MaLopHoc,
                        x.TenLopHoc
                    })
                    .FirstOrDefaultAsync();

                if (lopHienTai != null)
                {
                    lopHocs.Add(lopHienTai);
                }
            }

            ViewBag.DanhSachLopHoc = new SelectList(
                lopHocs,
                "MaLopHoc",
                "TenLopHoc",
                maLopHienTai);

            var giaoViens = await _context.GiaoViens
                .AsNoTracking()
                .Where(x => x.TrangThai)
                .OrderBy(x => x.HoTen)
                .Select(x => new
                {
                    x.MaGiaoVien,
                    x.HoTen
                })
                .ToListAsync();

            ViewBag.DanhSachGiaoVien = new SelectList(
                giaoViens,
                "MaGiaoVien",
                "HoTen");

            var phongHocs = await _context.PhongHocs
                .AsNoTracking()
                .Where(x => x.TrangThai)
                .OrderBy(x => x.TenPhong)
                .Select(x => new
                {
                    x.MaPhong,
                    x.TenPhong
                })
                .ToListAsync();

            ViewBag.DanhSachPhongHoc = new SelectList(
                phongHocs,
                "MaPhong",
                "TenPhong");

            var xeList = await _context.Xes
    .AsNoTracking()
    .Where(x => x.TrangThai == "Đang sử dụng")
    .OrderBy(x => x.BienSo)
    .Select(x => new
    {
        x.MaXe,
        TenHienThi = x.BienSo + " - " + x.TenXe
    })
    .ToListAsync();

            ViewBag.DanhSachXe = new SelectList(
                xeList,
                "MaXe",
                "TenHienThi");

            ViewBag.DanhSachXe = new SelectList(
                xeList,
                "MaXe",
                "TenHienThi");

            ViewBag.DanhSachLoaiBuoiHoc = new SelectList(
                new[]
                {
                    "Lý thuyết",
                    "Thực hành",
                    "Mô phỏng",
                    "DAT",
                    "Đường trường"
                });
        }

        // =========================
        // TỰ SINH MÃ LỊCH LH0001, LH0002...
        // =========================
        private async Task<string> TaoMaLichHocAsync()
        {
            var danhSachMa = await _context.LichHocs
                .AsNoTracking()
                .Select(x => x.MaLichHoc)
                .ToListAsync();

            int soLonNhat = danhSachMa
                .Where(x =>
                    x.StartsWith("LH", StringComparison.OrdinalIgnoreCase))
                .Select(x =>
                {
                    string hauTo = x.Substring(2);

                    return int.TryParse(hauTo, out int so)
                        ? so
                        : 0;
                })
                .DefaultIfEmpty(0)
                .Max();

            return $"LH{soLonNhat + 1:D4}";
        }

        // =========================
        // TÍNH SỐ GIỜ
        // =========================
        private static decimal TinhSoGio(
            TimeOnly gioBatDau,
            TimeOnly gioKetThuc)
        {
            return Math.Round(
                (decimal)(gioKetThuc - gioBatDau).TotalMinutes / 60m,
                2);
        }

        // =========================
        // TRẢ LỖI VALIDATION CHO AJAX
        // =========================
        private Dictionary<string, string[]> LayLoiModelState()
        {
            return ModelState
                .Where(x => x.Value != null &&
                            x.Value.Errors.Count > 0)
                .ToDictionary(
                    x => x.Key,
                    x => x.Value!.Errors
                        .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)
                            ? "Giá trị không hợp lệ."
                            : e.ErrorMessage)
                        .ToArray());
        }
    }
}
