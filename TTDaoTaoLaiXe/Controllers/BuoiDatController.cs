
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TTDaoTaoLaiXe.Models;

namespace TTDaoTaoLaiXe.Controllers
{
    [Authorize]
    public class BuoiDatController : Controller
    {
        private readonly TTDaoTaoLaiXeContext _context;
        private const int PageSize = 10;

        private static readonly string[] TrangThaiHopLe =
        {
            "Chưa thực hiện",
            "Đã hoàn thành",
            "Vắng mặt",
            "Đã hủy"
        };

        private static readonly string[] LoaiDatHopLe =
        {
            "DAT",
            "Đường trường",
            "Trong sa hình",
            "Ban đêm",
            "Tự động"
        };

        public BuoiDatController(TTDaoTaoLaiXeContext context)
        {
            _context = context;
        }

        // GET: BuoiDat
        public async Task<IActionResult> Index(
            string? tuKhoa,
            string? trangThaiLoc,
            int page = 1)
        {
            page = Math.Max(1, page);
            tuKhoa = tuKhoa?.Trim();

            var query = _context.BuoiDats
                .AsNoTracking()
                .Include(x => x.MaHocVienNavigation)
                .Include(x => x.MaDangKyNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                query = query.Where(x =>
                    x.MaBuoiDat.Contains(tuKhoa) ||
                    x.MaHocVien.Contains(tuKhoa) ||
                    x.MaDangKy.Contains(tuKhoa) ||
                    x.MaHocVienNavigation.HoTen.Contains(tuKhoa));
            }

            if (!string.IsNullOrWhiteSpace(trangThaiLoc))
            {
                query = query.Where(x => x.TrangThai == trangThaiLoc);
            }

            query = query
                .OrderByDescending(x => x.NgayThucHien)
                .ThenBy(x => x.GioBatDau)
                .ThenBy(x => x.MaBuoiDat);

            int totalItems = await query.CountAsync();

            int totalPages = Math.Max(
                1,
                (int)Math.Ceiling(totalItems / (double)PageSize));

            if (page > totalPages)
            {
                page = totalPages;
            }

            var data = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.TuKhoa = tuKhoa ?? "";
            ViewBag.TrangThaiLoc = trangThaiLoc ?? "";
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = PageSize;
            ViewBag.TrangThaiList = TrangThaiHopLe;

            await LoadDropdowns();

            return View(data);
        }

        // Nạp các đăng ký hợp lệ để tạo buổi DAT.
        private async Task LoadDropdowns(
            string? selectedDangKy = null)
        {
            var dangKys = await _context.DangKyHocs
                .AsNoTracking()
                .Include(x => x.MaHocVienNavigation)
                .Include(x => x.MaKhoaHocNavigation)
                .Where(x => x.TrangThai == "Đang học")
                .OrderBy(x => x.MaDangKy)
                .Select(x => new
                {
                    x.MaDangKy,
                    x.MaHocVien,
                    HoTen = x.MaHocVienNavigation.HoTen,
                    x.MaKhoaHoc,
                    TenKhoaHoc = x.MaKhoaHocNavigation.TenKhoaHoc
                })
                .ToListAsync();

            ViewBag.DangKyList = dangKys;
            ViewBag.SelectedDangKy = selectedDangKy;
            ViewBag.TrangThaiList = TrangThaiHopLe;
            ViewBag.LoaiDatList = LoaiDatHopLe;
        }

        // POST: BuoiDat/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind(
                "MaBuoiDat,MaHocVien,MaDangKy,NgayThucHien," +
                "GioBatDau,GioKetThuc,SoKm,SoGio,LoaiDat,TrangThai,GhiChu")]
            BuoiDat model)
        {
            ModelState.Remove(nameof(model.MaHocVienNavigation));
            ModelState.Remove(nameof(model.MaDangKyNavigation));

            model.MaBuoiDat = model.MaBuoiDat?.Trim() ?? "";
            model.MaHocVien = model.MaHocVien?.Trim() ?? "";
            model.MaDangKy = model.MaDangKy?.Trim() ?? "";
            model.LoaiDat = model.LoaiDat?.Trim() ?? "";
            model.TrangThai = model.TrangThai?.Trim() ?? "";

            await ValidateBuoiDat(model);

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = GetModelErrors()
                });
            }

            try
            {
                _context.BuoiDats.Add(model);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Thêm buổi DAT thành công."
                });
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message =
                        "Không thể thêm buổi DAT. " +
                        "Hãy kiểm tra mã buổi và dữ liệu liên quan."
                });
            }
        }

        // POST: BuoiDat/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            [Bind(
                "MaBuoiDat,MaHocVien,MaDangKy,NgayThucHien," +
                "GioBatDau,GioKetThuc,SoKm,SoGio,LoaiDat,TrangThai,GhiChu")]
            BuoiDat model)
        {
            ModelState.Remove(nameof(model.MaHocVienNavigation));
            ModelState.Remove(nameof(model.MaDangKyNavigation));

            model.MaBuoiDat = model.MaBuoiDat?.Trim() ?? "";
            model.MaHocVien = model.MaHocVien?.Trim() ?? "";
            model.MaDangKy = model.MaDangKy?.Trim() ?? "";
            model.LoaiDat = model.LoaiDat?.Trim() ?? "";
            model.TrangThai = model.TrangThai?.Trim() ?? "";

            var entity = await _context.BuoiDats
                .FirstOrDefaultAsync(x =>
                    x.MaBuoiDat == model.MaBuoiDat);

            if (entity == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy buổi DAT."
                });
            }

            await ValidateBuoiDat(model, model.MaBuoiDat);

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = GetModelErrors()
                });
            }

            entity.MaHocVien = model.MaHocVien;
            entity.MaDangKy = model.MaDangKy;
            entity.NgayThucHien = model.NgayThucHien;
            entity.GioBatDau = model.GioBatDau;
            entity.GioKetThuc = model.GioKetThuc;
            entity.SoKm = model.SoKm;
            entity.SoGio = model.SoGio;
            entity.LoaiDat = model.LoaiDat;
            entity.TrangThai = model.TrangThai;
            entity.GhiChu = string.IsNullOrWhiteSpace(model.GhiChu)
                ? null
                : model.GhiChu.Trim();

            try
            {
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Cập nhật buổi DAT thành công."
                });
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Không thể cập nhật buổi DAT."
                });
            }
        }

        // POST: BuoiDat/ToggleTrangThai
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleTrangThai(
            string id,
            string trangThai)
        {
            if (string.IsNullOrWhiteSpace(id) ||
                string.IsNullOrWhiteSpace(trangThai))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Thông tin buổi DAT không hợp lệ."
                });
            }

            if (!TrangThaiHopLe.Contains(trangThai))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Trạng thái được chọn không hợp lệ."
                });
            }

            var entity = await _context.BuoiDats
                .FirstOrDefaultAsync(x => x.MaBuoiDat == id);

            if (entity == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy buổi DAT."
                });
            }

            entity.TrangThai = trangThai;

            try
            {
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    trangThai = entity.TrangThai,
                    message = "Cập nhật trạng thái buổi DAT thành công."
                });
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Không thể cập nhật trạng thái buổi DAT."
                });
            }
        }

        // Kiểm tra dữ liệu buổi DAT.
        private async Task ValidateBuoiDat(
            BuoiDat model,
            string? maBuoiHienTai = null)
        {
            if (string.IsNullOrWhiteSpace(model.MaBuoiDat))
            {
                ModelState.AddModelError(
                    nameof(model.MaBuoiDat),
                    "Vui lòng nhập mã buổi DAT.");
            }
            else if (model.MaBuoiDat.Length > 20)
            {
                ModelState.AddModelError(
                    nameof(model.MaBuoiDat),
                    "Mã buổi DAT không được vượt quá 20 ký tự.");
            }

            bool trungMa = await _context.BuoiDats.AnyAsync(x =>
                x.MaBuoiDat == model.MaBuoiDat &&
                x.MaBuoiDat != maBuoiHienTai);

            if (trungMa)
            {
                ModelState.AddModelError(
                    nameof(model.MaBuoiDat),
                    "Mã buổi DAT đã tồn tại.");
            }

            if (model.NgayThucHien == default)
            {
                ModelState.AddModelError(
                    nameof(model.NgayThucHien),
                    "Vui lòng chọn ngày thực hiện.");
            }

            if (model.GioKetThuc <= model.GioBatDau)
            {
                ModelState.AddModelError(
                    nameof(model.GioKetThuc),
                    "Giờ kết thúc phải sau giờ bắt đầu.");
            }

            if (model.SoKm <= 0)
            {
                ModelState.AddModelError(
                    nameof(model.SoKm),
                    "Số km phải lớn hơn 0.");
            }

            if (model.SoGio <= 0)
            {
                ModelState.AddModelError(
                    nameof(model.SoGio),
                    "Số giờ phải lớn hơn 0.");
            }

            if (!LoaiDatHopLe.Contains(model.LoaiDat))
            {
                ModelState.AddModelError(
                    nameof(model.LoaiDat),
                    "Loại DAT không hợp lệ.");
            }

            if (!TrangThaiHopLe.Contains(model.TrangThai))
            {
                ModelState.AddModelError(
                    nameof(model.TrangThai),
                    "Trạng thái buổi DAT không hợp lệ.");
            }

            if (model.GhiChu?.Length > 500)
            {
                ModelState.AddModelError(
                    nameof(model.GhiChu),
                    "Ghi chú không được vượt quá 500 ký tự.");
            }

            var dangKy = await _context.DangKyHocs
                .AsNoTracking()
                .Include(x => x.MaKhoaHocNavigation)
                .FirstOrDefaultAsync(x =>
                    x.MaDangKy == model.MaDangKy);

            if (dangKy == null)
            {
                ModelState.AddModelError(
                    nameof(model.MaDangKy),
                    "Đăng ký học không tồn tại.");
                return;
            }

            if (dangKy.TrangThai != "Đang học")
            {
                ModelState.AddModelError(
                    nameof(model.MaDangKy),
                    "Chỉ được ghi nhận buổi DAT cho đăng ký đang học.");
            }

            if (dangKy.MaHocVien != model.MaHocVien)
            {
                ModelState.AddModelError(
                    nameof(model.MaHocVien),
                    "Học viên không khớp với đăng ký học đã chọn.");
            }

            if (string.IsNullOrWhiteSpace(model.MaHocVien) ||
                !await _context.HocViens.AnyAsync(
                    x => x.MaHocVien == model.MaHocVien))
            {
                ModelState.AddModelError(
                    nameof(model.MaHocVien),
                    "Học viên không tồn tại.");
            }
        }

        private string GetModelErrors()
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e =>
                    string.IsNullOrWhiteSpace(e.ErrorMessage)
                        ? "Dữ liệu nhập không hợp lệ."
                        : e.ErrorMessage))
                .Distinct()
                .ToList();

            return errors.Count > 0
                ? string.Join("\n", errors)
                : "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
        }
    }
}
