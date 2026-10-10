using BTL_NhaKhoa.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BTL_NhaKhoa.Data.CauHinh;

public sealed class DichVuCauHinh : IEntityTypeConfiguration<DichVu>
{
    public void Configure(EntityTypeBuilder<DichVu> entity)
    {
        entity.ToTable("dich_vu", table =>
        {
            table.HasCheckConstraint("check_dich_vu_don_gia", "don_gia >= 0");
            table.HasCheckConstraint("check_dich_vu_thoi_luong", "thoi_luong_du_kien_phut > 0");
            table.HasCheckConstraint("check_dich_vu_trang_thai", "trang_thai IN ('DANG_HOAT_DONG', 'NGUNG_HOAT_DONG')");
        });
        entity.HasKey(x => x.MaDichVu);
        entity.Property(x => x.MaDichVu).HasColumnType("bigint unsigned").ValueGeneratedOnAdd();
        entity.Property(x => x.TenDichVu).HasMaxLength(150).IsRequired();
        entity.Property(x => x.MoTa).HasColumnType("text");
        entity.Property(x => x.DonGia).HasPrecision(15, 2);
        entity.Property(x => x.ThoiLuongDuKienPhut).HasColumnType("smallint unsigned");
        entity.Property(x => x.TrangThai).HasMaxLength(20).HasDefaultValue("DANG_HOAT_DONG");
        CauHinhChung.CauHinhNgayTaoCapNhat(entity);
        entity.HasIndex(x => x.TenDichVu).IsUnique();
    }
}
