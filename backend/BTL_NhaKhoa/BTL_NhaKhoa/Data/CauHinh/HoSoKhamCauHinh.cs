using BTL_NhaKhoa.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BTL_NhaKhoa.Data.CauHinh;

public sealed class HoSoKhamCauHinh : IEntityTypeConfiguration<HoSoKham>
{
    public void Configure(EntityTypeBuilder<HoSoKham> entity)
    {
        entity.ToTable("ho_so_kham");
        entity.HasKey(x => x.MaHoSoKham);
        entity.Property(x => x.MaHoSoKham).HasColumnType("bigint unsigned").ValueGeneratedOnAdd();
        entity.Property(x => x.MaKhachHang).HasColumnType("bigint unsigned");
        entity.Property(x => x.MaBacSi).HasColumnType("bigint unsigned");
        entity.Property(x => x.MaLichHen).HasColumnType("bigint unsigned");
        entity.Property(x => x.NgayKham).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
        entity.Property(x => x.TrieuChung).HasColumnType("text");
        entity.Property(x => x.ChanDoan).HasColumnType("text");
        entity.Property(x => x.TomTatChoKhach).HasColumnType("text");
        entity.Property(x => x.GhiChuNoiBo).HasColumnType("text");
        entity.Property(x => x.ChotLuc).HasColumnType("datetime");
        CauHinhChung.CauHinhNgayTaoCapNhat(entity);
        entity.HasIndex(x => x.MaLichHen).IsUnique();
        entity.HasIndex(x => new { x.MaKhachHang, x.NgayKham });
        entity.HasOne(x => x.KhachHang).WithMany(x => x.HoSoKhams).HasForeignKey(x => x.MaKhachHang).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.BacSi).WithMany(x => x.HoSoKhams).HasForeignKey(x => x.MaBacSi).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.LichHen).WithOne(x => x.HoSoKham).HasForeignKey<HoSoKham>(x => x.MaLichHen).OnDelete(DeleteBehavior.Restrict);
    }
}
