using BTL_NhaKhoa.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BTL_NhaKhoa.Data.CauHinh;

public sealed class ThietBiCauHinh : IEntityTypeConfiguration<ThietBi>
{
    public void Configure(EntityTypeBuilder<ThietBi> entity)
    {
        entity.ToTable("thiet_bi", table =>
        {
            table.HasCheckConstraint("check_thiet_bi_tinh_trang", "tinh_trang IN ('HOAT_DONG', 'HONG', 'DANG_BAO_TRI', 'NGUNG_SU_DUNG')");
        });
        entity.HasKey(x => x.MaThietBi);
        entity.Property(x => x.MaThietBi).HasColumnType("bigint unsigned").ValueGeneratedOnAdd();
        entity.Property(x => x.MaPhong).HasColumnType("bigint unsigned");
        entity.Property(x => x.TenThietBi).HasMaxLength(150).IsRequired();
        entity.Property(x => x.LoaiThietBi).HasMaxLength(100);
        entity.Property(x => x.TinhTrang).HasMaxLength(20).HasDefaultValue("HOAT_DONG");
        CauHinhChung.CauHinhNgayTaoCapNhat(entity);
        entity.HasIndex(x => x.MaPhong);
        entity.HasOne(x => x.Phong).WithMany(x => x.ThietBis).HasForeignKey(x => x.MaPhong).OnDelete(DeleteBehavior.Restrict);
    }
}
