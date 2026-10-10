using BTL_NhaKhoa.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BTL_NhaKhoa.Data.CauHinh;

public sealed class DonThuocCauHinh : IEntityTypeConfiguration<DonThuoc>
{
    public void Configure(EntityTypeBuilder<DonThuoc> entity)
    {
        entity.ToTable("don_thuoc");
        entity.HasKey(x => x.MaDonThuoc);
        entity.Property(x => x.MaDonThuoc).HasColumnType("bigint unsigned").ValueGeneratedOnAdd();
        entity.Property(x => x.MaHoSoKham).HasColumnType("bigint unsigned");
        entity.Property(x => x.NgayKe).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
        entity.Property(x => x.GhiChu).HasColumnType("text");
        entity.HasIndex(x => x.MaHoSoKham);
        entity.HasOne(x => x.HoSoKham).WithMany(x => x.DonThuocs).HasForeignKey(x => x.MaHoSoKham).OnDelete(DeleteBehavior.Restrict);
    }
}
