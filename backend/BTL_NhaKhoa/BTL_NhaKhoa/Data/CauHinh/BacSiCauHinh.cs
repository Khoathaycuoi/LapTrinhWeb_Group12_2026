using BTL_NhaKhoa.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BTL_NhaKhoa.Data.CauHinh;

public sealed class BacSiCauHinh : IEntityTypeConfiguration<BacSi>
{
    public void Configure(EntityTypeBuilder<BacSi> entity)
    {
        entity.ToTable("bac_si");
        entity.HasKey(x => x.MaBacSi);
        entity.Property(x => x.MaBacSi).HasColumnType("bigint unsigned").ValueGeneratedOnAdd();
        entity.Property(x => x.MaNhanVien).HasColumnType("bigint unsigned");
        entity.Property(x => x.ChungChi).HasMaxLength(255);
        entity.Property(x => x.BangCap).HasMaxLength(255);
        entity.Property(x => x.SoNamKinhNghiem).HasColumnType("smallint unsigned");
        CauHinhChung.CauHinhNgayTaoCapNhat(entity);
        entity.HasIndex(x => x.MaNhanVien).IsUnique();
        entity.HasOne(x => x.NhanVien).WithOne(x => x.BacSi).HasForeignKey<BacSi>(x => x.MaNhanVien).OnDelete(DeleteBehavior.Restrict);
    }
}
