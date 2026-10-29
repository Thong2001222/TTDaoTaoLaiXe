using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TTDaoTaoLaiXe.Models;

public partial class TTDaoTaoLaiXeContext : DbContext
{
    public TTDaoTaoLaiXeContext()
    {
    }

    public TTDaoTaoLaiXeContext(DbContextOptions<TTDaoTaoLaiXeContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BuoiDat> BuoiDats { get; set; }

    public virtual DbSet<DangKyHoc> DangKyHocs { get; set; }

    public virtual DbSet<DiemDanh> DiemDanhs { get; set; }

    public virtual DbSet<GiaoVien> GiaoViens { get; set; }

    public virtual DbSet<HangGplx> HangGplxes { get; set; }

    public virtual DbSet<HoSoHocVien> HoSoHocViens { get; set; }

    public virtual DbSet<HocPhi> HocPhis { get; set; }

    public virtual DbSet<HocVien> HocViens { get; set; }

    public virtual DbSet<HocVienLopHoc> HocVienLopHocs { get; set; }

    public virtual DbSet<KetQuaHocTap> KetQuaHocTaps { get; set; }

    public virtual DbSet<KhoaHoc> KhoaHocs { get; set; }

    public virtual DbSet<KhoaHocMonHoc> KhoaHocMonHocs { get; set; }

    public virtual DbSet<LichHoc> LichHocs { get; set; }

    public virtual DbSet<LichSuHoatDong> LichSuHoatDongs { get; set; }

    public virtual DbSet<LopHoc> LopHocs { get; set; }

    public virtual DbSet<MonHoc> MonHocs { get; set; }

    public virtual DbSet<PhongHoc> PhongHocs { get; set; }

    public virtual DbSet<QuyDinhDat> QuyDinhDats { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    public virtual DbSet<ThanhToanHocPhi> ThanhToanHocPhis { get; set; }

    public virtual DbSet<TheHocVien> TheHocViens { get; set; }

    public virtual DbSet<ThongBao> ThongBaos { get; set; }

    public virtual DbSet<TienDoDat> TienDoDats { get; set; }

    public virtual DbSet<VaiTro> VaiTros { get; set; }

    public virtual DbSet<Xe> Xes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=TTDaoTaoLaiXe;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BuoiDat>(entity =>
        {
            entity.Property(e => e.TrangThai).HasDefaultValue("Đã hoàn thành");

            entity.HasOne(d => d.MaDangKyNavigation).WithMany(p => p.BuoiDats)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BuoiDAT_DangKy");

            entity.HasOne(d => d.MaHocVienNavigation).WithMany(p => p.BuoiDats)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BuoiDAT_HocVien");
        });

        modelBuilder.Entity<DangKyHoc>(entity =>
        {
            entity.Property(e => e.NgayDangKy).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TrangThai).HasDefaultValue("Đang học");

            entity.HasOne(d => d.MaHocVienNavigation).WithMany(p => p.DangKyHocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DangKyHoc_HocVien");

            entity.HasOne(d => d.MaKhoaHocNavigation).WithMany(p => p.DangKyHocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DangKyHoc_KhoaHoc");
        });

        modelBuilder.Entity<DiemDanh>(entity =>
        {
            entity.Property(e => e.TrangThai).HasDefaultValue("Có mặt");

            entity.HasOne(d => d.MaHocVienNavigation).WithMany(p => p.DiemDanhs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DiemDanh_HocVien");

            entity.HasOne(d => d.MaLichHocNavigation).WithMany(p => p.DiemDanhs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DiemDanh_LichHoc");
        });

        modelBuilder.Entity<GiaoVien>(entity =>
        {
            entity.Property(e => e.TrangThai).HasDefaultValue(true);
        });

        modelBuilder.Entity<HangGplx>(entity =>
        {
            entity.Property(e => e.TrangThai).HasDefaultValue(true);
        });

        modelBuilder.Entity<HoSoHocVien>(entity =>
        {
            entity.Property(e => e.NgayTiepNhan).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TrangThaiHoSo).HasDefaultValue("Chưa đầy đủ");

            entity.HasOne(d => d.MaHocVienNavigation).WithOne(p => p.HoSoHocVien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HoSoHocVien_HocVien");

            entity.HasOne(d => d.NguoiTiepNhanNavigation).WithMany(p => p.HoSoHocViens).HasConstraintName("FK_HoSoHocVien_TaiKhoan");
        });

        modelBuilder.Entity<HocPhi>(entity =>
        {
            entity.Property(e => e.TrangThai).HasDefaultValue("Chưa thanh toán");

            entity.HasOne(d => d.MaDangKyNavigation).WithOne(p => p.HocPhiNavigation)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HocPhi_DangKy");
        });

        modelBuilder.Entity<HocVien>(entity =>
        {
            entity.Property(e => e.TrangThai).HasDefaultValue(true);
        });

        modelBuilder.Entity<HocVienLopHoc>(entity =>
        {
            entity.Property(e => e.NgayVaoLop).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TrangThai).HasDefaultValue("Đang học");

            entity.HasOne(d => d.MaHocVienNavigation).WithMany(p => p.HocVienLopHocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HocVien_LopHoc_HocVien");

            entity.HasOne(d => d.MaLopHocNavigation).WithMany(p => p.HocVienLopHocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HocVien_LopHoc_LopHoc");
        });

        modelBuilder.Entity<KetQuaHocTap>(entity =>
        {
            entity.Property(e => e.KetQua).HasDefaultValue("Chưa có kết quả");
            entity.Property(e => e.LanThi).HasDefaultValue(1);

            entity.HasOne(d => d.MaDangKyNavigation).WithMany(p => p.KetQuaHocTaps)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KetQuaHocTap_DangKy");

            entity.HasOne(d => d.MaMonHocNavigation).WithMany(p => p.KetQuaHocTaps)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KetQuaHocTap_MonHoc");
        });

        modelBuilder.Entity<KhoaHoc>(entity =>
        {
            entity.Property(e => e.TrangThai).HasDefaultValue(true);

            entity.HasOne(d => d.HangGplxNavigation).WithMany(p => p.KhoaHocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KhoaHoc_HangGPLX");
        });

        modelBuilder.Entity<KhoaHocMonHoc>(entity =>
        {
            entity.HasOne(d => d.MaKhoaHocNavigation).WithMany(p => p.KhoaHocMonHocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KhoaHoc_MonHoc_KhoaHoc");

            entity.HasOne(d => d.MaMonHocNavigation).WithMany(p => p.KhoaHocMonHocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KhoaHoc_MonHoc_MonHoc");
        });

        modelBuilder.Entity<LichHoc>(entity =>
        {
            entity.Property(e => e.TrangThai).HasDefaultValue("Đã lên lịch");

            entity.HasOne(d => d.MaGiaoVienNavigation).WithMany(p => p.LichHocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LichHoc_GiaoVien");

            entity.HasOne(d => d.MaLopHocNavigation).WithMany(p => p.LichHocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LichHoc_LopHoc");

            entity.HasOne(d => d.MaPhongNavigation).WithMany(p => p.LichHocs).HasConstraintName("FK_LichHoc_PhongHoc");

            entity.HasOne(d => d.MaXeNavigation).WithMany(p => p.LichHocs).HasConstraintName("FK_LichHoc_Xe");
        });

        modelBuilder.Entity<LichSuHoatDong>(entity =>
        {
            entity.Property(e => e.ThoiGian).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.MaTaiKhoanNavigation).WithMany(p => p.LichSuHoatDongs).HasConstraintName("FK_LichSuHoatDong_TaiKhoan");
        });

        modelBuilder.Entity<LopHoc>(entity =>
        {
            entity.Property(e => e.SiSoToiDa).HasDefaultValue(30);
            entity.Property(e => e.TrangThai).HasDefaultValue("Đang mở");

            entity.HasOne(d => d.MaGiaoVienNavigation).WithMany(p => p.LopHocs).HasConstraintName("FK_LopHoc_GiaoVien");

            entity.HasOne(d => d.MaKhoaHocNavigation).WithMany(p => p.LopHocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LopHoc_KhoaHoc");
        });

        modelBuilder.Entity<MonHoc>(entity =>
        {
            entity.Property(e => e.TrangThai).HasDefaultValue(true);
        });

        modelBuilder.Entity<PhongHoc>(entity =>
        {
            entity.Property(e => e.SucChua).HasDefaultValue(30);
            entity.Property(e => e.TrangThai).HasDefaultValue(true);
        });

        modelBuilder.Entity<QuyDinhDat>(entity =>
        {
            entity.Property(e => e.TrangThai).HasDefaultValue(true);

            entity.HasOne(d => d.MaHangNavigation).WithMany(p => p.QuyDinhDats)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_QuyDinhDAT_HangGPLX");
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.Property(e => e.TrangThai).HasDefaultValue(true);

            entity.HasOne(d => d.MaVaiTroNavigation).WithMany(p => p.TaiKhoans)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TaiKhoan_VaiTro");
        });

        modelBuilder.Entity<ThanhToanHocPhi>(entity =>
        {
            entity.Property(e => e.NgayThanhToan)
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.TrangThaiGiaoDich)
                .HasMaxLength(30);

            entity.HasOne(d => d.MaHocPhiNavigation)
                .WithMany(p => p.ThanhToanHocPhis)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ThanhToanHocPhi_HocPhi");

            entity.HasOne(d => d.NguoiThuNavigation)
                .WithMany(p => p.ThanhToanHocPhis)
                .HasConstraintName("FK_ThanhToanHocPhi_TaiKhoan");
        });

        modelBuilder.Entity<TheHocVien>(entity =>
        {
            entity.HasIndex(e => e.MaHocVien, "UX_TheHocVien_HocVien_DangSuDung")
                .IsUnique()
                .HasFilter("([TrangThai]=N'Đang sử dụng')");

            entity.Property(e => e.NgayCap).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TrangThai).HasDefaultValue("Đang sử dụng");

            entity.HasOne(d => d.MaHocVienNavigation).WithOne(p => p.TheHocVien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TheHocVien_HocVien");

            entity.HasOne(d => d.NguoiCapNavigation).WithMany(p => p.TheHocViens).HasConstraintName("FK_TheHocVien_TaiKhoan");
        });

        modelBuilder.Entity<ThongBao>(entity =>
        {
            entity.Property(e => e.ThoiGianTao).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TrangThai).HasDefaultValue(true);

            entity.HasOne(d => d.MaTaiKhoanNavigation).WithMany(p => p.ThongBaos).HasConstraintName("FK_ThongBao_TaiKhoan");
        });

        modelBuilder.Entity<TienDoDat>(entity =>
        {
            entity.Property(e => e.NgayCapNhat).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TrangThai).HasDefaultValue("Chưa đủ");

            entity.HasOne(d => d.MaDangKyNavigation).WithMany(p => p.TienDoDats)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TienDoDAT_DangKy");

            entity.HasOne(d => d.MaHocVienNavigation).WithMany(p => p.TienDoDats)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TienDoDAT_HocVien");
        });

        modelBuilder.Entity<Xe>(entity =>
        {
            entity.Property(e => e.TrangThai).HasDefaultValue("Đang sử dụng");

            entity.HasOne(d => d.MaHangNavigation).WithMany(p => p.Xes).HasConstraintName("FK_Xe_HangGPLX");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
