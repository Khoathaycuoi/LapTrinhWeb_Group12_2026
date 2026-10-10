using BTL_NhaKhoa.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BTL_NhaKhoa.Data.CauHinh;

public sealed class ChiTietDonThuocCauHinh : IEntityTypeConfiguration<ChiTietDonThuoc>
{
    public void Configure(EntityTypeBuilder<ChiTietDonThuoc> entity)
    {
        entity.ToTable("chi_tiet_don_thuoc", table =>
        {
            table.HasCheckConstraint("check_chi_tiet_don_thuoc_so_luong", "so_luong > 0");
        });
        entity.HasKey(x => x.MaChiTietDonThuoc);
        entity.Property(x => x.MaChiTietDonThuoc).HasColumnType("bigint unsigned").ValueGeneratedOnAdd();
        entity.Property(x => x.MaDonThuoc).HasColumnType("bigint unsigned");
        entity.Property(x => x.TenThuoc).HasMaxLength(255).IsRequired();
        entity.Property(x => x.HamLuong).HasMaxLength(100);
        entity.Property(x => x.LieuDung).HasMaxLength(255).IsRequired();
        entity.Property(x => x.SoLanMoiNgay).HasColumnType("smallint unsigned");
        entity.Property(x => x.SoNgayDung).HasColumnType("smallint unsigned");
        entity.Property(x => x.SoLuong).HasPrecision(10, 2);
        entity.Property(x => x.HuongDan).HasColumnType("text");
        entity.HasIndex(x => x.MaDonThuoc);
        entity.HasOne(x => x.DonThuoc).WithMany(x => x.ChiTietDonThuocs).HasForeignKey(x => x.MaDonThuoc).OnDelete(DeleteBehavior.Restrict);
    }
}
