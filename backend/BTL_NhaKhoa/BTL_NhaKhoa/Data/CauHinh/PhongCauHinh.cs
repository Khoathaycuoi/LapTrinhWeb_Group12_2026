using BTL_NhaKhoa.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BTL_NhaKhoa.Data.CauHinh;

public sealed class PhongCauHinh : IEntityTypeConfiguration<Phong>
{
    public void Configure(EntityTypeBuilder<Phong> entity)
    {
        entity.ToTable("phong", table =>
        {
            table.HasCheckConstraint("check_phong_trang_thai", "trang_thai IN ('HOAT_DONG', 'TAM_NGUNG', 'BAO_TRI')");
        });
        entity.HasKey(x => x.MaPhong);
        entity.Property(x => x.MaPhong).HasColumnType("bigint unsigned").ValueGeneratedOnAdd();
        entity.Property(x => x.TenPhong).HasMaxLength(100).IsRequired();
        entity.Property(x => x.TrangThai).HasMaxLength(20).HasDefaultValue("HOAT_DONG");
        CauHinhChung.CauHinhNgayTaoCapNhat(entity);
        entity.HasIndex(x => x.TenPhong).IsUnique();
    }
}
