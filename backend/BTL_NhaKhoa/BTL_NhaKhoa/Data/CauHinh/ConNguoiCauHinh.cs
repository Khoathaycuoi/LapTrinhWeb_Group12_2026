using BTL_NhaKhoa.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BTL_NhaKhoa.Data.CauHinh;

public sealed class ConNguoiCauHinh : IEntityTypeConfiguration<ConNguoi>
{
    public void Configure(EntityTypeBuilder<ConNguoi> entity)
    {
        entity.ToTable("con_nguoi");
        entity.HasKey(x => x.MaConNguoi);
        entity.Property(x => x.MaConNguoi).HasColumnType("bigint unsigned").ValueGeneratedOnAdd();
        entity.Property(x => x.SoDinhDanh).HasMaxLength(20);
        entity.Property(x => x.HoTen).HasMaxLength(150).IsRequired();
        entity.Property(x => x.SoDienThoai).HasMaxLength(15).IsRequired();
        entity.Property(x => x.GioiTinh).HasMaxLength(20);
        entity.Property(x => x.DiaChi).HasMaxLength(255);
        entity.Property(x => x.TenAnhDaiDien).HasMaxLength(50);
        entity.Property(x => x.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
        entity.Property(x => x.UpdatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP").ValueGeneratedOnAddOrUpdate();
        entity.HasIndex(x => x.SoDinhDanh).IsUnique();
        entity.HasIndex(x => x.SoDienThoai).IsUnique();
    }
}
