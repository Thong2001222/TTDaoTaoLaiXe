using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TTDaoTaoLaiXe.Models;
using TTDaoTaoLaiXe.ViewModels.HocVien;

namespace TTDaoTaoLaiXe.Controllers
{
    [Authorize]
    public class HocVienController : Controller
    {
        private readonly TTDaoTaoLaiXeContext _context;

        public HocVienController(TTDaoTaoLaiXeContext context)
        {
            _context = context;
        }

        // GET: /HocVien
        public async Task<IActionResult> Index(string? searchString,bool? trangThai)
        {
            var query = _context.HocViens
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim();

                query = query.Where(x =>
                    x.MaHocVien.Contains(searchString) ||
                    x.HoTen.Contains(searchString) ||
                    x.Cccd.Contains(searchString));
            }

            if (trangThai.HasValue)
            {
                query = query.Where(x =>
                    x.TrangThai == trangThai.Value);
            }

            var hocViens = await query
                .OrderBy(x => x.HoTen)
                .ToListAsync();

            return View(hocViens);
        }

// GET: /HocVien/Details/HV0001
public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            // Lấy học viên
            var hocVien = await _context.HocViens
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaHocVien == id);

            if (hocVien == null)
                return NotFound();

            // Lấy đăng ký học + thông tin khóa học
            var dangKyHocs = await _context.DangKyHocs
                .AsNoTracking()
                .Include(x => x.MaKhoaHocNavigation)
                .Where(x => x.MaHocVien == id)
                .OrderByDescending(x => x.NgayDangKy)
                .ToListAsync();

            // Tạo ViewModel
            var model = new HocVienProfileViewModel
            {
                MaHocVien = hocVien.MaHocVien,
                HoTen = hocVien.HoTen,
                NgaySinh = hocVien.NgaySinh,
                GioiTinh = hocVien.GioiTinh,
                Cccd = hocVien.Cccd,
                NgayCapCccd = hocVien.NgayCapCccd,
                NoiCapCccd = hocVien.NoiCapCccd,
                SoDienThoai = hocVien.SoDienThoai,
                Email = hocVien.Email,
                DiaChi = hocVien.DiaChi,
                AnhHocVien = hocVien.AnhHocVien,
                TrangThai = hocVien.TrangThai,
                GhiChu = hocVien.GhiChu,

                DangKyHocs = dangKyHocs
            };

            return View(model);
        }


        // GET: /HocVien/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /HocVien/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HocVien hocVien)
        {
            if (!ModelState.IsValid)
                return View(hocVien);

            _context.HocViens.Add(hocVien);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /HocVien/EditModal/HV0001
        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> EditModal(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var hocVien = await _context.HocViens
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaHocVien == id);

            if (hocVien == null)
                return NotFound();

            return PartialView("_EditModal", hocVien);
        }


        // POST: /HocVien/EditModal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditModal(HocVien hocVien, IFormFile? AnhFile)
        {
            if (string.IsNullOrWhiteSpace(hocVien.MaHocVien))
                return BadRequest();

            // Lấy bản ghi thật từ database
            var hocVienCu = await _context.HocViens
                .FirstOrDefaultAsync(x => x.MaHocVien == hocVien.MaHocVien);

            if (hocVienCu == null)
                return NotFound();

            // Kiểm tra CCCD trùng
            bool trungCccd = await _context.HocViens
                .AnyAsync(x =>
                    x.Cccd == hocVien.Cccd &&
                    x.MaHocVien != hocVien.MaHocVien);

            if (trungCccd)
            {
                ModelState.AddModelError(
                    nameof(HocVien.Cccd),
                    "Số CCCD này đã được sử dụng cho học viên khác.");
            }

            if (!ModelState.IsValid)
            {
                return PartialView("_EditModal", hocVien);
            }

            // Không cho thay đổi MaHocVien
            hocVienCu.HoTen = hocVien.HoTen;
            hocVienCu.NgaySinh = hocVien.NgaySinh;
            hocVienCu.GioiTinh = hocVien.GioiTinh;
            hocVienCu.Cccd = hocVien.Cccd;
            hocVienCu.NgayCapCccd = hocVien.NgayCapCccd;
            hocVienCu.NoiCapCccd = hocVien.NoiCapCccd;
            hocVienCu.SoDienThoai = hocVien.SoDienThoai;
            hocVienCu.Email = hocVien.Email;
            hocVienCu.DiaChi = hocVien.DiaChi;
            hocVienCu.AnhHocVien = hocVien.AnhHocVien;
            hocVienCu.TrangThai = hocVien.TrangThai;
            hocVienCu.GhiChu = hocVien.GhiChu;

            if (AnhFile != null && AnhFile.Length > 0)
            {
                var allowedExtensions = new[]
                {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

                var extension = Path.GetExtension(AnhFile.FileName)
                    .ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        "AnhFile",
                        "Chỉ được chọn ảnh JPG, JPEG, PNG hoặc WEBP.");

                    return PartialView("_EditModal", hocVien);
                }

                if (AnhFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError(
                        "AnhFile",
                        "Ảnh không được lớn hơn 5MB.");

                    return PartialView("_EditModal", hocVien);
                }

                var folderPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "images",
                    "hocvien"
                );

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var fileName =
                    hocVien.MaHocVien +
                    "_" +
                    Guid.NewGuid().ToString("N") +
                    extension;

                var filePath = Path.Combine(
                    folderPath,
                    fileName
                );

                using (var stream = new FileStream(
                    filePath,
                    FileMode.Create))
                {
                    await AnhFile.CopyToAsync(stream);
                }

                hocVienCu.AnhHocVien =
                    "/images/hocvien/" + fileName;
            }

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Cập nhật thông tin học viên thành công."
            });
        }
        // GET: /HocVien/Delete/HV0001
        [HttpPost]
        [ValidateAntiForgeryToken]


        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Json(new
                {
                    success = false,
                    message = "Mã học viên không hợp lệ."
                });
            }

            var hocVien = await _context.HocViens
                .FirstOrDefaultAsync(x => x.MaHocVien == id);

            if (hocVien == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Không tìm thấy học viên."
                });
            }

            // ==========================================
            // KIỂM TRA DỮ LIỆU LIÊN QUAN
            // ==========================================

            bool coDangKyHoc = await _context.DangKyHocs
                .AnyAsync(x => x.MaHocVien == id);

            bool coLopHoc = await _context.HocVienLopHocs
                .AnyAsync(x => x.MaHocVien == id);

            bool coDiemDanh = await _context.DiemDanhs
                .AnyAsync(x => x.MaHocVien == id);

            bool coTienDoDAT = await _context.TienDoDats
                .AnyAsync(x => x.MaHocVien == id);

            bool coBuoiDAT = await _context.BuoiDats
                .AnyAsync(x => x.MaHocVien == id);

            bool coHoSo = await _context.HoSoHocViens
                .AnyAsync(x => x.MaHocVien == id);

            bool coThe = await _context.TheHocViens
                .AnyAsync(x => x.MaHocVien == id);


            bool coDuLieuLienQuan =
                coDangKyHoc ||
                coLopHoc ||
                coDiemDanh ||
                coTienDoDAT ||
                coBuoiDAT ||
                coHoSo ||
                coThe;


            // ==========================================
            // CÓ DỮ LIỆU LIÊN QUAN
            // => KHÔNG XÓA CỨNG
            // => CHUYỂN NGỪNG HOẠT ĐỘNG
            // ==========================================

            if (coDuLieuLienQuan)
            {
                if (!hocVien.TrangThai)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Học viên này đã ở trạng thái ngừng hoạt động."
                    });
                }

                hocVien.TrangThai = false;

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    softDelete = true,
                    message = "Học viên đã được chuyển sang trạng thái ngừng hoạt động để bảo toàn dữ liệu."
                });
            }


            // ==========================================
            // KHÔNG CÓ DỮ LIỆU LIÊN QUAN
            // => XÓA CỨNG
            // ==========================================

            _context.HocViens.Remove(hocVien);

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                softDelete = false,
                message = "Đã xóa học viên thành công."
            });
        }
    }
}