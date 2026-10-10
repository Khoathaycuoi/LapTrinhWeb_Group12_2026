using BTL_NhaKhoa.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BTL_NhaKhoa.Data.CauHinh;

public sealed class ThucHienBaoTriCauHinh : IEntityTypeConfiguration<ThucHienBaoTri>
{
    public void Configure(EntityTypeBuilder<ThucHienBaoTri> entity)
    {
        entity.ToTable("thuc_hien_bao_tri");
        entity.HasKey(x => new { x.MaBaoTri, x.MaThietBi });
        entity.Property(x => x.MaBaoTri).HasColumnType("bigint unsigned");
        entity.Property(x => x.MaThietBi).HasColumnType("bigint unsigned");
        entity.Property(x => x.GhiChu).HasColumnType("text");
        entity.HasOne(x => x.BaoTri).WithMany(x => x.ThucHienBaoTris).HasForeignKey(x => x.MaBaoTri).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.ThietBi).WithMany(x => x.ThucHienBaoTris).HasForeignKey(x => x.MaThietBi).OnDelete(DeleteBehavior.Restrict);
    }
}
