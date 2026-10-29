using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TTDaoTaoLaiXe.Models;

namespace TTDaoTaoLaiXe.Controllers
{
    [Authorize]
    public class QuyDinhDatController : Controller
    {
        private readonly TTDaoTaoLaiXeContext _context;
        private const int PageSize = 10;

        public QuyDinhDatController(TTDaoTaoLaiXeContext context)
        {
            _context = context;
        }

        // GET: QuyDinhDat
        public async Task<IActionResult> Index(
            string? tuKhoa,
            int page = 1)
        {
            page = Math.Max(1, page);
            tuKhoa = tuKhoa?.Trim();

            var query = _context.QuyDinhDats
                .AsNoTracking()
                .Include(x => x.MaHangNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                query = query.Where(x =>
                    x.MaQuyDinhDat.Contains(tuKhoa) ||
                    x.MaHang.Contains(tuKhoa) ||
                    x.MaHangNavigation.TenHang.Contains(tuKhoa));
            }

            query = query
                .OrderBy(x => x.MaHang)
                .ThenByDescending(x => x.TrangThai)
                .ThenBy(x => x.MaQuyDinhDat);

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
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = PageSize;

            ViewBag.HangGplxes = await _context.HangGplxes
                .AsNoTracking()
                .Where(x => x.TrangThai)
                .OrderBy(x => x.TenHang)
                .ToListAsync();

            return View(data);
        }

        // POST: QuyDinhDat/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind(
                "MaQuyDinhDat,MaHang,TongKm," +
                "SoGioTuDong,SoGioBanDem,MoTa,TrangThai")]
            QuyDinhDat model)
        {
            // MaHangNavigation là navigation property,
            // không phải trường nhập liệu từ form.
            ModelState.Remove(nameof(model.MaHangNavigation));

            model.MaQuyDinhDat =
                model.MaQuyDinhDat?.Trim() ?? "";

            model.MaHang = model.MaHang?.Trim() ?? "";

            ValidateQuyDinh(model);

            bool hangTonTai = await _context.HangGplxes
                .AnyAsync(x =>
                    x.MaHang == model.MaHang &&
                    x.TrangThai);

            if (!hangTonTai)
            {
                ModelState.AddModelError(
                    nameof(model.MaHang),
                    "Hạng GPLX không tồn tại hoặc đã ngừng hoạt động.");
            }

            bool trungMa = await _context.QuyDinhDats
                .AnyAsync(x =>
                    x.MaQuyDinhDat == model.MaQuyDinhDat);

            if (trungMa)
            {
                ModelState.AddModelError(
                    nameof(model.MaQuyDinhDat),
                    "Mã quy định DAT đã tồn tại.");
            }

            if (model.TrangThai)
            {
                bool daCoQuyDinhHoatDong =
                    await _context.QuyDinhDats.AnyAsync(x =>
                        x.MaHang == model.MaHang &&
                        x.TrangThai);

                if (daCoQuyDinhHoatDong)
                {
                    ModelState.AddModelError(
                        nameof(model.MaHang),
                        "Hạng GPLX này đã có quy định DAT đang hoạt động.");
                }
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = GetModelErrors()
                });
            }

            model.MoTa = string.IsNullOrWhiteSpace(model.MoTa)
                ? null
                : model.MoTa.Trim();

            try
            {
                _context.QuyDinhDats.Add(model);

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Thêm quy định DAT thành công."
                });
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message =
                        "Không thể thêm quy định DAT. " +
                        "Vui lòng kiểm tra dữ liệu và khóa ngoại."
                });
            }
        }

        // POST: QuyDinhDat/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            [Bind(
                "MaQuyDinhDat,MaHang,TongKm," +
                "SoGioTuDong,SoGioBanDem,MoTa,TrangThai")]
            QuyDinhDat model)
        {
            // Loại bỏ lỗi validation navigation property.
            ModelState.Remove(nameof(model.MaHangNavigation));

            model.MaQuyDinhDat =
                model.MaQuyDinhDat?.Trim() ?? "";

            model.MaHang = model.MaHang?.Trim() ?? "";

            var entity = await _context.QuyDinhDats
                .FirstOrDefaultAsync(x =>
                    x.MaQuyDinhDat == model.MaQuyDinhDat);

            if (entity == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy quy định DAT."
                });
            }

            ValidateQuyDinh(model);

            bool hangTonTai = await _context.HangGplxes
                .AnyAsync(x =>
                    x.MaHang == model.MaHang &&
                    x.TrangThai);

            if (!hangTonTai)
            {
                ModelState.AddModelError(
                    nameof(model.MaHang),
                    "Hạng GPLX không tồn tại hoặc đã ngừng hoạt động.");
            }

            // Mỗi hạng chỉ có một quy định đang hoạt động.
            if (model.TrangThai)
            {
                bool trungQuyDinh = await _context.QuyDinhDats
                    .AnyAsync(x =>
                        x.MaHang == model.MaHang &&
                        x.MaQuyDinhDat != model.MaQuyDinhDat &&
                        x.TrangThai);

                if (trungQuyDinh)
                {
                    ModelState.AddModelError(
                        nameof(model.MaHang),
                        "Hạng GPLX này đã có quy định DAT đang hoạt động.");
                }
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = GetModelErrors()
                });
            }

            // Chỉ cập nhật các trường được phép chỉnh sửa.
            // Không gán entity.MaHangNavigation từ dữ liệu form.
            entity.MaHang = model.MaHang;
            entity.TongKm = model.TongKm;
            entity.SoGioTuDong = model.SoGioTuDong;
            entity.SoGioBanDem = model.SoGioBanDem;
            entity.MoTa = string.IsNullOrWhiteSpace(model.MoTa)
                ? null
                : model.MoTa.Trim();
            entity.TrangThai = model.TrangThai;

            try
            {
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Cập nhật quy định DAT thành công."
                });
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message =
                        "Không thể lưu thay đổi quy định DAT. " +
                        "Vui lòng kiểm tra dữ liệu trong cơ sở dữ liệu."
                });
            }
        }

        // POST: QuyDinhDat/ToggleTrangThai
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleTrangThai(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Mã quy định DAT không hợp lệ."
                });
            }

            id = id.Trim();

            var entity = await _context.QuyDinhDats
                .FirstOrDefaultAsync(x =>
                    x.MaQuyDinhDat == id);

            if (entity == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy quy định DAT."
                });
            }

            // Chỉ kiểm tra các điều kiện này khi kích hoạt lại.
            if (!entity.TrangThai)
            {
                bool hangKhongHoatDong =
                    !await _context.HangGplxes.AnyAsync(x =>
                        x.MaHang == entity.MaHang &&
                        x.TrangThai);

                if (hangKhongHoatDong)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =
                            "Hạng GPLX đã ngừng hoạt động, " +
                            "không thể kích hoạt quy định."
                    });
                }

                bool daCoQuyDinhKhac =
                    await _context.QuyDinhDats.AnyAsync(x =>
                        x.MaHang == entity.MaHang &&
                        x.MaQuyDinhDat != entity.MaQuyDinhDat &&
                        x.TrangThai);

                if (daCoQuyDinhKhac)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =
                            "Hạng GPLX đã có quy định DAT khác đang hoạt động."
                    });
                }
            }

            entity.TrangThai = !entity.TrangThai;

            try
            {
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    trangThai = entity.TrangThai,
                    message = entity.TrangThai
                        ? "Đã kích hoạt quy định DAT."
                        : "Đã ngừng kích hoạt quy định DAT."
                });
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Không thể cập nhật trạng thái quy định DAT."
                });
            }
        }

        // Kiểm tra dữ liệu dùng chung cho Create và Edit.
        private void ValidateQuyDinh(QuyDinhDat model)
        {
            if (string.IsNullOrWhiteSpace(model.MaQuyDinhDat))
            {
                ModelState.AddModelError(
                    nameof(model.MaQuyDinhDat),
                    "Vui lòng nhập mã quy định DAT.");
            }

            if (string.IsNullOrWhiteSpace(model.MaHang))
            {
                ModelState.AddModelError(
                    nameof(model.MaHang),
                    "Vui lòng chọn hạng GPLX.");
            }

            if (model.TongKm <= 0)
            {
                ModelState.AddModelError(
                    nameof(model.TongKm),
                    "Tổng số km phải lớn hơn 0.");
            }

            if (model.SoGioTuDong < 0 ||
                model.SoGioTuDong > 8760)
            {
                ModelState.AddModelError(
                    nameof(model.SoGioTuDong),
                    "Số giờ tự động phải từ 0 đến 8760.");
            }

            if (model.SoGioBanDem < 0 ||
                model.SoGioBanDem > 8760)
            {
                ModelState.AddModelError(
                    nameof(model.SoGioBanDem),
                    "Số giờ ban đêm phải từ 0 đến 8760.");
            }

            if (model.MaQuyDinhDat.Length > 20)
            {
                ModelState.AddModelError(
                    nameof(model.MaQuyDinhDat),
                    "Mã quy định DAT không được vượt quá 20 ký tự.");
            }

            if (model.MaHang.Length > 10)
            {
                ModelState.AddModelError(
                    nameof(model.MaHang),
                    "Mã hạng GPLX không được vượt quá 10 ký tự.");
            }

            if (model.MoTa?.Length > 500)
            {
                ModelState.AddModelError(
                    nameof(model.MoTa),
                    "Mô tả không được vượt quá 500 ký tự.");
            }
        }

        // Gom lỗi validation thành thông báo JSON cho SweetAlert2.
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
