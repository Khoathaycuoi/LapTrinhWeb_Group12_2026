using BTL_NhaKhoa.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BTL_NhaKhoa.Data.CauHinh;

public sealed class LichHenCauHinh : IEntityTypeConfiguration<LichHen>
{
    public void Configure(EntityTypeBuilder<LichHen> entity)
    {
        entity.ToTable("lich_hen", table =>
        {
            table.HasCheckConstraint("check_lich_hen_thoi_luong", "thoi_luong_du_kien_phut > 0");
            table.HasCheckConstraint("check_lich_hen_trang_thai", "trang_thai IN ('CHO_XAC_NHAN', 'DA_XAC_NHAN', 'DA_DEN', 'DANG_KHAM', 'DA_HOAN_THANH', 'DA_HUY', 'KHONG_DEN', 'TU_CHOI')");
        });
        entity.HasKey(x => x.MaLichHen);
        entity.Property(x => x.MaLichHen).HasColumnType("bigint unsigned").ValueGeneratedOnAdd();
        entity.Property(x => x.MaKhachHang).HasColumnType("bigint unsigned");
        entity.Property(x => x.MaBacSi).HasColumnType("bigint unsigned");
        entity.Property(x => x.MaPhong).HasColumnType("bigint unsigned");
        entity.Property(x => x.NguoiTaoTaiKhoanId).HasColumnType("bigint unsigned");
        entity.Property(x => x.NguoiXacNhanTaiKhoanId).HasColumnType("bigint unsigned");
        entity.Property(x => x.NguoiHuyTaiKhoanId).HasColumnType("bigint unsigned");
        entity.Property(x => x.NguoiTiepNhanTaiKhoanId).HasColumnType("bigint unsigned");
        entity.Property(x => x.NguoiDanhDauKhongDenTaiKhoanId).HasColumnType("bigint unsigned");
        entity.Property(x => x.ThoiGianBatDau).HasColumnType("datetime");
        entity.Property(x => x.ThoiLuongDuKienPhut).HasColumnType("smallint unsigned");
        entity.Property(x => x.TrangThai).HasMaxLength(30).HasDefaultValue("CHO_XAC_NHAN");
        entity.Property(x => x.GhiChu).HasColumnType("text");
        entity.Property(x => x.LyDoHuy).HasColumnType("text");
        entity.Property(x => x.LyDoTuChoi).HasColumnType("text");
        entity.Property(x => x.XacNhanLuc).HasColumnType("datetime");
        entity.Property(x => x.DenLuc).HasColumnType("datetime");
        entity.Property(x => x.BatDauKhamLuc).HasColumnType("datetime");
        entity.Property(x => x.HoanThanhLuc).HasColumnType("datetime");
        entity.Property(x => x.KhongDenLuc).HasColumnType("datetime");
        entity.Property(x => x.GhiChuKhongDen).HasColumnType("text");
        entity.Property(x => x.HuyLuc).HasColumnType("datetime");
        entity.Property(x => x.TuChoiLuc).HasColumnType("datetime");
        CauHinhChung.CauHinhNgayTaoCapNhat(entity);
        entity.HasIndex(x => new { x.MaKhachHang, x.ThoiGianBatDau });
        entity.HasIndex(x => new { x.MaBacSi, x.ThoiGianBatDau });
        entity.HasIndex(x => new { x.MaPhong, x.ThoiGianBatDau });
        entity.HasIndex(x => x.TrangThai);
        entity.HasOne(x => x.KhachHang).WithMany(x => x.LichHens).HasForeignKey(x => x.MaKhachHang).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.BacSi).WithMany(x => x.LichHens).HasForeignKey(x => x.MaBacSi).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Phong).WithMany(x => x.LichHens).HasForeignKey(x => x.MaPhong).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.NguoiTaoTaiKhoan).WithMany(x => x.LichHenDaTao).HasForeignKey(x => x.NguoiTaoTaiKhoanId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.NguoiXacNhanTaiKhoan).WithMany(x => x.LichHenDaXacNhan).HasForeignKey(x => x.NguoiXacNhanTaiKhoanId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.NguoiHuyTaiKhoan).WithMany(x => x.LichHenDaHuy).HasForeignKey(x => x.NguoiHuyTaiKhoanId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.NguoiTiepNhanTaiKhoan).WithMany(x => x.LichHenDaTiepNhan).HasForeignKey(x => x.NguoiTiepNhanTaiKhoanId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.NguoiDanhDauKhongDenTaiKhoan).WithMany(x => x.LichHenDaDanhDauKhongDen).HasForeignKey(x => x.NguoiDanhDauKhongDenTaiKhoanId).OnDelete(DeleteBehavior.Restrict);
    }
}
