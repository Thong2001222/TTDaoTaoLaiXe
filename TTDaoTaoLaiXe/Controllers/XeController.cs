
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TTDaoTaoLaiXe.Models;

namespace TTDaoTaoLaiXe.Controllers
{
    [Authorize]
    public class XeController : Controller
    {
        private readonly TTDaoTaoLaiXeContext _context;
        private const int PageSize = 10;

        private static readonly string[] TrangThaiHopLe =
        {
            "Đang sử dụng",
            "Bảo dưỡng",
            "Ngừng sử dụng"
        };

        public XeController(TTDaoTaoLaiXeContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            string? tuKhoa,
            string? trangThai,
            int page = 1)
        {
            page = Math.Max(1, page);

            var query = _context.Xes
                .AsNoTracking()
                .Include(x => x.MaHangNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();

                query = query.Where(x =>
                    x.MaXe.Contains(tuKhoa) ||
                    x.BienSo.Contains(tuKhoa) ||
                    x.TenXe.Contains(tuKhoa) ||
                    x.LoaiXe.Contains(tuKhoa) ||
                    (x.MaHang != null && x.MaHang.Contains(tuKhoa)));
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(x => x.TrangThai == trangThai);
            }

            var totalItems = await query.CountAsync();

            var totalPages = Math.Max(
                1,
                (int)Math.Ceiling(totalItems / (double)PageSize));

            if (page > totalPages)
                page = totalPages;

            var danhSach = await query
                .OrderBy(x => x.MaXe)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.TrangThaiLoc = trangThai;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.TrangThaiOptions = TrangThaiHopLe;

            ViewBag.HangGplx = await _context.HangGplxes
                .AsNoTracking()
                .OrderBy(h => h.MaHang)
                .ToListAsync();

            return View(danhSach);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Xe model)
        {
            // Trạng thái do hệ thống tự thiết lập, không yêu cầu người dùng nhập
            ModelState.Remove(nameof(Xe.TrangThai));
            model.TrangThai = "Đang sử dụng";

            ChuanHoa(model);

            if (string.IsNullOrWhiteSpace(model.MaXe))
                ModelState.AddModelError(nameof(model.MaXe), "Vui lòng nhập mã xe.");

            if (string.IsNullOrWhiteSpace(model.BienSo))
                ModelState.AddModelError(nameof(model.BienSo), "Vui lòng nhập biển số xe.");

            if (string.IsNullOrWhiteSpace(model.TenXe))
                ModelState.AddModelError(nameof(model.TenXe), "Vui lòng nhập tên xe.");

            if (string.IsNullOrWhiteSpace(model.LoaiXe))
                ModelState.AddModelError(nameof(model.LoaiXe), "Vui lòng nhập loại xe.");

            if (model.SoCho.HasValue && model.SoCho.Value <= 0)
                ModelState.AddModelError(nameof(model.SoCho), "Số chỗ phải lớn hơn 0.");

            if (model.NamSanXuat.HasValue &&
                (model.NamSanXuat.Value < 1900 ||
                 model.NamSanXuat.Value > DateTime.Today.Year + 1))
            {
                ModelState.AddModelError(
                    nameof(model.NamSanXuat),
                    "Năm sản xuất không hợp lệ.");
            }

            if (await _context.Xes.AnyAsync(x => x.MaXe == model.MaXe))
                ModelState.AddModelError(nameof(model.MaXe), "Mã xe đã tồn tại.");

            if (await _context.Xes.AnyAsync(x => x.BienSo == model.BienSo))
                ModelState.AddModelError(nameof(model.BienSo), "Biển số xe đã tồn tại.");

            if (!string.IsNullOrWhiteSpace(model.MaHang) &&
                !await _context.HangGplxes.AnyAsync(h => h.MaHang == model.MaHang))
            {
                ModelState.AddModelError(nameof(model.MaHang), "Hạng GPLX không tồn tại.");
            }

            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Vui lòng kiểm tra lại thông tin.",
                    errors = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            x => x.Key,
                            x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray())
                });
            }

            try
            {
                _context.Xes.Add(model);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Thêm xe tập lái thành công."
                });
            }
            catch (DbUpdateException)
            {
                return Json(new
                {
                    success = false,
                    message = "Không thể thêm xe. Vui lòng kiểm tra mã xe, biển số và dữ liệu liên quan."
                });
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Xe model)
        {
            var maXe = model.MaXe?.Trim() ?? "";

            var xe = await _context.Xes
                .FirstOrDefaultAsync(x => x.MaXe == maXe);

            if (xe == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Không tìm thấy xe tập lái."
                });
            }

            // Trạng thái không được chỉnh sửa trong form sửa xe
            ModelState.Remove(nameof(Xe.TrangThai));
            ModelState.Remove(nameof(Xe.MaHangNavigation));
            ModelState.Remove(nameof(Xe.LichHocs));

            ChuanHoa(model);

            if (string.IsNullOrWhiteSpace(model.BienSo))
                ModelState.AddModelError(nameof(model.BienSo), "Vui lòng nhập biển số xe.");

            if (string.IsNullOrWhiteSpace(model.TenXe))
                ModelState.AddModelError(nameof(model.TenXe), "Vui lòng nhập tên xe.");

            if (string.IsNullOrWhiteSpace(model.LoaiXe))
                ModelState.AddModelError(nameof(model.LoaiXe), "Vui lòng nhập loại xe.");

            if (model.SoCho.HasValue && model.SoCho.Value <= 0)
                ModelState.AddModelError(nameof(model.SoCho), "Số chỗ phải lớn hơn 0.");

            if (model.NamSanXuat.HasValue &&
                (model.NamSanXuat.Value < 1900 ||
                 model.NamSanXuat.Value > DateTime.Today.Year + 1))
            {
                ModelState.AddModelError(
                    nameof(model.NamSanXuat),
                    "Năm sản xuất không hợp lệ.");
            }

            if (await _context.Xes.AnyAsync(x =>
                x.MaXe != maXe && x.BienSo == model.BienSo))
            {
                ModelState.AddModelError(
                    nameof(model.BienSo),
                    "Biển số xe đã tồn tại.");
            }

            if (!string.IsNullOrWhiteSpace(model.MaHang) &&
                !await _context.HangGplxes.AnyAsync(h => h.MaHang == model.MaHang))
            {
                ModelState.AddModelError(
                    nameof(model.MaHang),
                    "Hạng GPLX không tồn tại.");
            }

            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Vui lòng kiểm tra lại thông tin.",
                    errors = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            x => x.Key,
                            x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray())
                });
            }

            // Chỉ cập nhật thông tin xe, không thay đổi trạng thái
            xe.BienSo = model.BienSo;
            xe.TenXe = model.TenXe;
            xe.MaHang = model.MaHang;
            xe.LoaiXe = model.LoaiXe;
            xe.SoCho = model.SoCho;
            xe.NamSanXuat = model.NamSanXuat;
            xe.GhiChu = model.GhiChu;

            try
            {
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Cập nhật xe tập lái thành công."
                });
            }
            catch (DbUpdateException)
            {
                return Json(new
                {
                    success = false,
                    message = "Không thể cập nhật xe. Vui lòng kiểm tra lại dữ liệu."
                });
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleTrangThai(
            string? maXe,
            string? trangThaiMoi)
        {
            maXe = maXe?.Trim();
            trangThaiMoi = trangThaiMoi?.Trim();

            if (string.IsNullOrWhiteSpace(maXe))
            {
                return Json(new
                {
                    success = false,
                    message = "Mã xe không hợp lệ."
                });
            }

            trangThaiMoi = trangThaiMoi?.Trim();

            var trangThaiHopLe = TrangThaiHopLe.FirstOrDefault(
                x => string.Equals(
                    x,
                    trangThaiMoi,
                    StringComparison.OrdinalIgnoreCase));

            if (trangThaiHopLe == null)
            {
                return Json(new
                {
                    success = false,
                    message = $"Trạng thái nhận được không hợp lệ: [{trangThaiMoi ?? "(null)"}]."
                });
            }

            trangThaiMoi = trangThaiHopLe;

            var xe = await _context.Xes
                .FirstOrDefaultAsync(x => x.MaXe == maXe);

            if (xe == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Không tìm thấy xe tập lái."
                });
            }

            if (xe.TrangThai == trangThaiMoi)
            {
                return Json(new
                {
                    success = false,
                    message = "Xe đang ở trạng thái này."
                });
            }

            // Không cho chuyển sang bảo dưỡng hoặc ngừng sử dụng
            // nếu xe vẫn còn lịch học chưa xử lý
            if (trangThaiMoi != "Đang sử dụng")
            {
                var homNay = DateOnly.FromDateTime(DateTime.Today);

                var coLichSapToi = await _context.LichHocs.AnyAsync(l =>
                    l.MaXe == maXe &&
                    l.NgayHoc >= homNay &&
                    l.TrangThai != "Đã hủy" &&
                    l.TrangThai != "Đã hoàn thành");

                if (coLichSapToi)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Xe còn lịch học từ hôm nay trở đi chưa hoàn thành hoặc chưa hủy. Hãy xử lý các lịch học liên quan trước khi đổi trạng thái."
                    });
                }
            }

            try
            {
                xe.TrangThai = trangThaiMoi;
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Cập nhật trạng thái xe thành công.",
                    trangThai = xe.TrangThai
                });
            }
            catch (DbUpdateException)
            {
                return Json(new
                {
                    success = false,
                    message = "Không thể cập nhật trạng thái xe. Vui lòng thử lại."
                });
            }
        }


        private static void ChuanHoa(Xe model)
        {
            model.MaXe = model.MaXe?.Trim() ?? "";
            model.BienSo = model.BienSo?.Trim().ToUpperInvariant() ?? "";
            model.TenXe = model.TenXe?.Trim() ?? "";
            model.LoaiXe = model.LoaiXe?.Trim() ?? "";
            model.MaHang = string.IsNullOrWhiteSpace(model.MaHang)
                ? null
                : model.MaHang.Trim();
            model.GhiChu = model.GhiChu?.Trim();

            if (string.IsNullOrWhiteSpace(model.TrangThai))
                model.TrangThai = "Đang sử dụng";
        }
    }
}
