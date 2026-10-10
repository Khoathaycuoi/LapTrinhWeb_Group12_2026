using BTL_NhaKhoa.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BTL_NhaKhoa.Data.CauHinh;

public sealed class ChiTietLichHenCauHinh : IEntityTypeConfiguration<ChiTietLichHen>
{
    public void Configure(EntityTypeBuilder<ChiTietLichHen> entity)
    {
        entity.ToTable("chi_tiet_lich_hen", table =>
        {
            table.HasCheckConstraint("check_chi_tiet_lich_hen_gia", "don_gia_tai_thoi_diem_dat >= 0");
            table.HasCheckConstraint("check_chi_tiet_lich_hen_thoi_luong", "thoi_luong_tai_thoi_diem_dat_phut > 0");
        });
        entity.HasKey(x => x.MaChiTietLichHen);
        entity.Property(x => x.MaChiTietLichHen).HasColumnType("bigint unsigned").ValueGeneratedOnAdd();
        entity.Property(x => x.MaLichHen).HasColumnType("bigint unsigned");
        entity.Property(x => x.MaDichVu).HasColumnType("bigint unsigned");
        entity.Property(x => x.DonGiaTaiThoiDiemDat).HasPrecision(15, 2);
        entity.Property(x => x.ThoiLuongTaiThoiDiemDatPhut).HasColumnType("smallint unsigned");
        entity.HasIndex(x => new { x.MaLichHen, x.MaDichVu }).IsUnique();
        entity.HasOne(x => x.LichHen).WithMany(x => x.ChiTietLichHens).HasForeignKey(x => x.MaLichHen).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.DichVu).WithMany(x => x.ChiTietLichHens).HasForeignKey(x => x.MaDichVu).OnDelete(DeleteBehavior.Restrict);
    }
}
