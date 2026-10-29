using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TTDaoTaoLaiXe.Helpers;
using TTDaoTaoLaiXe.Models;
using TTDaoTaoLaiXe.ViewModels.Account;
using System.Security.Claims;

namespace TTDaoTaoLaiXe.Controllers
{
    [Authorize(Roles = "VT01")]
    public class TaiKhoanController : Controller
    {
        private readonly TTDaoTaoLaiXeContext _context;
        private const int PageSize = 10;

        public TaiKhoanController(TTDaoTaoLaiXeContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? tuKhoa,
            string? maVaiTro,
            string? trangThai,
            int page = 1)
        {
            page = Math.Max(1, page);

            var query = _context.TaiKhoans
                .AsNoTracking()
                .Include(x => x.MaVaiTroNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();

                query = query.Where(x =>
                    x.MaTaiKhoan.Contains(tuKhoa) ||
                    x.TenDangNhap.Contains(tuKhoa) ||
                    x.HoTen.Contains(tuKhoa) ||
                    (x.Email != null && x.Email.Contains(tuKhoa)));
            }

            if (!string.IsNullOrWhiteSpace(maVaiTro))
            {
                query = query.Where(x => x.MaVaiTro == maVaiTro);
            }

            if (trangThai == "1")
            {
                query = query.Where(x => x.TrangThai);
            }
            else if (trangThai == "0")
            {
                query = query.Where(x => !x.TrangThai);
            }

            var totalItems = await query.CountAsync();
            var totalPages = Math.Max(
                1,
                (int)Math.Ceiling(totalItems / (double)PageSize));

            page = Math.Min(page, totalPages);

            var taiKhoans = await query
                .OrderBy(x => x.MaTaiKhoan)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(x => new TaiKhoanItemViewModel
                {
                    MaTaiKhoan = x.MaTaiKhoan,
                    TenDangNhap = x.TenDangNhap,
                    HoTen = x.HoTen,
                    Email = x.Email,
                    SoDienThoai = x.SoDienThoai,
                    MaVaiTro = x.MaVaiTro,
                    TenVaiTro = x.MaVaiTroNavigation.TenVaiTro,
                    TrangThai = x.TrangThai
                })
                .ToListAsync();


            var model = new TaiKhoanIndexViewModel
            {
                TaiKhoans = taiKhoans,
                TuKhoa = tuKhoa,
                MaVaiTro = maVaiTro,
                TrangThai = trangThai,
                Page = page,
                PageSize = PageSize,
                TotalItems = totalItems,
                DanhSachVaiTro = await _context.VaiTros
                    .AsNoTracking()
                    .OrderBy(x => x.MaVaiTro)
                    .Select(x => new SelectListItem
                    {
                        Value = x.MaVaiTro,
                        Text = x.TenVaiTro
                    })
                    .ToListAsync()
            };


            return View(model);
        }

        private async Task NapDanhSachVaiTro(string? selected = null)
        {
            var danhSach = await _context.VaiTros
                .AsNoTracking()
                .OrderBy(x => x.MaVaiTro)
                .ToListAsync();

            ViewBag.DanhSachVaiTro = new SelectList(
                danhSach,
                "MaVaiTro",
                "TenVaiTro",
                selected);
        }

        private async Task<bool> VaiTroTonTai(string? maVaiTro)
        {
            if (string.IsNullOrWhiteSpace(maVaiTro))
                return false;

            return await _context.VaiTros
                .AnyAsync(x => x.MaVaiTro == maVaiTro);
        }

        private static string ChuanHoa(string? value)
        {
            return value?.Trim() ?? string.Empty;
        }

        private static string? ChuanHoaKhongBatBuoc(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private async Task<bool> ConQuanTriVienKhacHoatDong(
            string maTaiKhoan)
        {
            return await _context.TaiKhoans.AnyAsync(x =>
                x.MaTaiKhoan != maTaiKhoan &&
                x.MaVaiTro == "VT01" &&
                x.TrangThai);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await NapDanhSachVaiTro();

            return PartialView(
                "_CreateModal",
                new TaiKhoanTaoMoiViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            TaiKhoanTaoMoiViewModel model)
        {
            model.MaTaiKhoan = ChuanHoa(model.MaTaiKhoan);
            model.TenDangNhap = ChuanHoa(model.TenDangNhap);
            model.HoTen = ChuanHoa(model.HoTen);
            model.Email = ChuanHoaKhongBatBuoc(model.Email);
            model.SoDienThoai = ChuanHoaKhongBatBuoc(model.SoDienThoai);

            if (!string.IsNullOrWhiteSpace(model.MaTaiKhoan) &&
                await _context.TaiKhoans.AnyAsync(
                    x => x.MaTaiKhoan == model.MaTaiKhoan))
            {
                ModelState.AddModelError(
                    nameof(model.MaTaiKhoan),
                    "Mã tài khoản đã tồn tại.");
            }

            if (!string.IsNullOrWhiteSpace(model.TenDangNhap) &&
                await _context.TaiKhoans.AnyAsync(
                    x => x.TenDangNhap == model.TenDangNhap))
            {
                ModelState.AddModelError(
                    nameof(model.TenDangNhap),
                    "Tên đăng nhập đã tồn tại.");
            }

            if (!await VaiTroTonTai(model.MaVaiTro))
            {
                ModelState.AddModelError(
                    nameof(model.MaVaiTro),
                    "Vui lòng chọn vai trò hợp lệ.");
            }

            if (!ModelState.IsValid)
            {
                await NapDanhSachVaiTro(model.MaVaiTro);
                Response.StatusCode = 400;
                return PartialView("_CreateModal", model);
            }

            var taiKhoan = new TaiKhoan
            {
                MaTaiKhoan = model.MaTaiKhoan,
                TenDangNhap = model.TenDangNhap,
                HoTen = model.HoTen,
                Email = model.Email,
                SoDienThoai = model.SoDienThoai,
                MaVaiTro = model.MaVaiTro,
                MatKhau = PasswordHelper.Hash(model.MatKhau),
                TrangThai = true
            };

            try
            {
                _context.TaiKhoans.Add(taiKhoan);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Không thể tạo tài khoản. Hãy kiểm tra dữ liệu và thử lại.");

                await NapDanhSachVaiTro(model.MaVaiTro);
                Response.StatusCode = 400;
                return PartialView("_CreateModal", model);
            }

            return Json(new
            {
                success = true,
                message = "Thêm tài khoản thành công."
            });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var taiKhoan = await _context.TaiKhoans
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaTaiKhoan == id);

            if (taiKhoan == null)
                return NotFound();

            var model = new TaiKhoanEditViewModel
            {
                MaTaiKhoan = taiKhoan.MaTaiKhoan,
                TenDangNhap = taiKhoan.TenDangNhap,
                HoTen = taiKhoan.HoTen,
                Email = taiKhoan.Email,
                SoDienThoai = taiKhoan.SoDienThoai,
                MaVaiTro = taiKhoan.MaVaiTro
            };

            await NapDanhSachVaiTro(model.MaVaiTro);

            return PartialView("_EditModal", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            TaiKhoanEditViewModel model)
        {
            model.MaTaiKhoan = ChuanHoa(model.MaTaiKhoan);
            model.TenDangNhap = ChuanHoa(model.TenDangNhap);
            model.HoTen = ChuanHoa(model.HoTen);
            model.Email = ChuanHoaKhongBatBuoc(model.Email);
            model.SoDienThoai = ChuanHoaKhongBatBuoc(model.SoDienThoai);

            var taiKhoan = await _context.TaiKhoans
                .FirstOrDefaultAsync(x =>
                    x.MaTaiKhoan == model.MaTaiKhoan);

            if (taiKhoan == null)
                return NotFound();

            if (!string.IsNullOrWhiteSpace(model.TenDangNhap) &&
                await _context.TaiKhoans.AnyAsync(x =>
                    x.TenDangNhap == model.TenDangNhap &&
                    x.MaTaiKhoan != model.MaTaiKhoan))
            {
                ModelState.AddModelError(
                    nameof(model.TenDangNhap),
                    "Tên đăng nhập đã được tài khoản khác sử dụng.");
            }

            if (!await VaiTroTonTai(model.MaVaiTro))
            {
                ModelState.AddModelError(
                    nameof(model.MaVaiTro),
                    "Vui lòng chọn vai trò hợp lệ.");
            }

            var maTaiKhoanDangDangNhap =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (taiKhoan.MaTaiKhoan == maTaiKhoanDangDangNhap &&
                taiKhoan.TrangThai &&
                taiKhoan.MaVaiTro == "VT01" &&
                model.MaVaiTro != "VT01")
            {
                ModelState.AddModelError(
                    nameof(model.MaVaiTro),
                    "Bạn không thể tự hạ quyền quản trị viên của chính mình.");
            }

            if (taiKhoan.TrangThai &&
                taiKhoan.MaVaiTro == "VT01" &&
                model.MaVaiTro != "VT01" &&
                !await ConQuanTriVienKhacHoatDong(taiKhoan.MaTaiKhoan))
            {
                ModelState.AddModelError(
                    nameof(model.MaVaiTro),
                    "Không thể hạ quyền quản trị viên cuối cùng đang hoạt động.");
            }

            if (!ModelState.IsValid)
            {
                await NapDanhSachVaiTro(model.MaVaiTro);
                Response.StatusCode = 400;
                return PartialView("_EditModal", model);
            }

            taiKhoan.TenDangNhap = model.TenDangNhap;
            taiKhoan.HoTen = model.HoTen;
            taiKhoan.Email = model.Email;
            taiKhoan.SoDienThoai = model.SoDienThoai;
            taiKhoan.MaVaiTro = model.MaVaiTro;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Không thể cập nhật tài khoản. Hãy kiểm tra dữ liệu và thử lại.");

                await NapDanhSachVaiTro(model.MaVaiTro);
                Response.StatusCode = 400;
                return PartialView("_EditModal", model);
            }

            return Json(new
            {
                success = true,
                message = "Cập nhật tài khoản thành công."
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleTrangThai(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var taiKhoan = await _context.TaiKhoans
                .FirstOrDefaultAsync(x => x.MaTaiKhoan == id);

            if (taiKhoan == null)
                return NotFound();

            var maTaiKhoanDangDangNhap =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (taiKhoan.MaTaiKhoan == maTaiKhoanDangDangNhap)
            {
                TempData["Error"] =
                    "Không thể khóa tài khoản đang đăng nhập.";

                return RedirectToAction(nameof(Index));
            }

            if (taiKhoan.TrangThai &&
                taiKhoan.MaVaiTro == "VT01" &&
                !await ConQuanTriVienKhacHoatDong(taiKhoan.MaTaiKhoan))
            {
                TempData["Error"] =
                    "Không thể khóa quản trị viên cuối cùng đang hoạt động.";

                return RedirectToAction(nameof(Index));
            }

            taiKhoan.TrangThai = !taiKhoan.TrangThai;
            await _context.SaveChangesAsync();

            TempData["Success"] = taiKhoan.TrangThai
                ? "Đã mở khóa tài khoản."
                : "Đã khóa tài khoản.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var taiKhoan = await _context.TaiKhoans
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaTaiKhoan == id);

            if (taiKhoan == null)
                return NotFound();

            ViewBag.TenDangNhap = taiKhoan.TenDangNhap;
            ViewBag.HoTen = taiKhoan.HoTen;

            return PartialView(
                "_ResetPasswordModal",
                new TaiKhoanResetPasswordViewModel
                {
                    MaTaiKhoan = taiKhoan.MaTaiKhoan
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(
            TaiKhoanResetPasswordViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.MaTaiKhoan))
                return BadRequest();

            var taiKhoan = await _context.TaiKhoans
                .FirstOrDefaultAsync(x =>
                    x.MaTaiKhoan == model.MaTaiKhoan);

            if (taiKhoan == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.TenDangNhap = taiKhoan.TenDangNhap;
                ViewBag.HoTen = taiKhoan.HoTen;
                Response.StatusCode = 400;
                return PartialView("_ResetPasswordModal", model);
            }

            taiKhoan.MatKhau = PasswordHelper.Hash(model.MatKhauMoi);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Đặt lại mật khẩu thành công."
            });
        }
    }
}
