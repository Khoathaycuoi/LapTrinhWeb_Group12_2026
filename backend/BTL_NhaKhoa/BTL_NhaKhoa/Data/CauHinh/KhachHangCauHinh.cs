using BTL_NhaKhoa.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BTL_NhaKhoa.Data.CauHinh;

public sealed class KhachHangCauHinh : IEntityTypeConfiguration<KhachHang>
{
    public void Configure(EntityTypeBuilder<KhachHang> entity)
    {
        entity.ToTable("khach_hang");
        entity.HasKey(x => x.MaKhachHang);
        entity.Property(x => x.MaKhachHang).HasColumnType("bigint unsigned").ValueGeneratedOnAdd();
        entity.Property(x => x.MaConNguoi).HasColumnType("bigint unsigned");
        entity.Property(x => x.NgayDangKy).HasDefaultValueSql("(CURRENT_DATE)").ValueGeneratedOnAdd();
        entity.Property(x => x.TienSuBenh).HasColumnType("text");
        CauHinhChung.CauHinhNgayTaoCapNhat(entity);
        entity.HasIndex(x => x.MaConNguoi).IsUnique();
        entity.HasOne(x => x.ConNguoi).WithOne(x => x.KhachHang).HasForeignKey<KhachHang>(x => x.MaConNguoi).OnDelete(DeleteBehavior.Restrict);
    }
}
