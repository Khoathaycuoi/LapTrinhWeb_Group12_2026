using BTL_NhaKhoa.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BTL_NhaKhoa.Data.CauHinh;

public sealed class ChiTietHoaDonCauHinh : IEntityTypeConfiguration<ChiTietHoaDon>
{
    public void Configure(EntityTypeBuilder<ChiTietHoaDon> entity)
    {
        entity.ToTable("chi_tiet_hoa_don", table =>
        {
            table.HasCheckConstraint("check_chi_tiet_hoa_don_gia", "don_gia >= 0");
            table.HasCheckConstraint("check_chi_tiet_hoa_don_so_luong", "so_luong > 0");
        });
        entity.HasKey(x => x.MaChiTietHoaDon);
        entity.Property(x => x.MaChiTietHoaDon).HasColumnType("bigint unsigned").ValueGeneratedOnAdd();
        entity.Property(x => x.MaHoaDon).HasColumnType("bigint unsigned");
        entity.Property(x => x.MaDichVuThucHien).HasColumnType("bigint unsigned");
        entity.Property(x => x.TenDichVu).HasMaxLength(150).IsRequired();
        entity.Property(x => x.DonGia).HasPrecision(15, 2);
        entity.Property(x => x.SoLuong).HasColumnType("smallint unsigned").HasDefaultValue((ushort)1);
        entity.Property(x => x.ThanhTien).HasPrecision(17, 2).HasComputedColumnSql("don_gia * so_luong", stored: false);
        entity.HasIndex(x => x.MaDichVuThucHien).IsUnique();
        entity.HasIndex(x => x.MaHoaDon);
        entity.HasOne(x => x.HoaDon).WithMany(x => x.ChiTietHoaDons).HasForeignKey(x => x.MaHoaDon).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.DichVuThucHien).WithOne(x => x.ChiTietHoaDon).HasForeignKey<ChiTietHoaDon>(x => x.MaDichVuThucHien).OnDelete(DeleteBehavior.Restrict);
    }
}
