using BTL_NhaKhoa.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BTL_NhaKhoa.Data.CauHinh;

public sealed class NhanVienCauHinh : IEntityTypeConfiguration<NhanVien>
{
    public void Configure(EntityTypeBuilder<NhanVien> entity)
    {
        entity.ToTable("nhan_vien", table =>
        {
            table.HasCheckConstraint("check_nhan_vien_trang_thai", "trang_thai IN ('DANG_LAM_VIEC', 'DA_NGHI')");
        });
        entity.HasKey(x => x.MaNhanVien);
        entity.Property(x => x.MaNhanVien).HasColumnType("bigint unsigned").ValueGeneratedOnAdd();
        entity.Property(x => x.MaConNguoi).HasColumnType("bigint unsigned");
        entity.Property(x => x.TrangThai).HasMaxLength(20).HasDefaultValue("DANG_LAM_VIEC");
        CauHinhChung.CauHinhNgayTaoCapNhat(entity);
        entity.HasIndex(x => x.MaConNguoi).IsUnique();
        entity.HasOne(x => x.ConNguoi).WithOne(x => x.NhanVien).HasForeignKey<NhanVien>(x => x.MaConNguoi).OnDelete(DeleteBehavior.Restrict);
    }
}
