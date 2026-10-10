using BTL_NhaKhoa.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BTL_NhaKhoa.Data.CauHinh;

public sealed class TaiKhoanCauHinh : IEntityTypeConfiguration<TaiKhoan>
{
    public void Configure(EntityTypeBuilder<TaiKhoan> entity)
    {
        entity.ToTable("tai_khoan", table =>
        {
            table.HasCheckConstraint("check_tai_khoan_trang_thai", "trang_thai IN ('HOAT_DONG', 'VO_HIEU_HOA')");
        });
        entity.HasKey(x => x.MaTaiKhoan);
        entity.Property(x => x.MaTaiKhoan).HasColumnType("bigint unsigned").ValueGeneratedOnAdd();
        entity.Property(x => x.MaConNguoi).HasColumnType("bigint unsigned");
        entity.Property(x => x.MaVaiTro).HasMaxLength(30).IsRequired();
        entity.Property(x => x.MatKhauHash).HasMaxLength(255).IsRequired();
        entity.Property(x => x.TrangThai).HasMaxLength(20).HasDefaultValue("HOAT_DONG");
        CauHinhChung.CauHinhNgayTaoCapNhat(entity);
        entity.HasIndex(x => x.MaConNguoi).IsUnique();
        entity.HasIndex(x => x.MaVaiTro);
        entity.HasOne(x => x.ConNguoi).WithOne(x => x.TaiKhoan).HasForeignKey<TaiKhoan>(x => x.MaConNguoi).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.VaiTro).WithMany(x => x.TaiKhoans).HasForeignKey(x => x.MaVaiTro).OnDelete(DeleteBehavior.Restrict);
    }
}
