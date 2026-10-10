using BTL_NhaKhoa.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BTL_NhaKhoa.Data.CauHinh;

public sealed class DichVuThucHienCauHinh : IEntityTypeConfiguration<DichVuThucHien>
{
    public void Configure(EntityTypeBuilder<DichVuThucHien> entity)
    {
        entity.ToTable("dich_vu_thuc_hien", table =>
        {
            table.HasCheckConstraint("check_dich_vu_thuc_hien_gia", "don_gia_thuc_te >= 0");
            table.HasCheckConstraint("check_dich_vu_thuc_hien_so_luong", "so_luong > 0");
        });
        entity.HasKey(x => x.MaDichVuThucHien);
        entity.Property(x => x.MaDichVuThucHien).HasColumnType("bigint unsigned").ValueGeneratedOnAdd();
        entity.Property(x => x.MaHoSoKham).HasColumnType("bigint unsigned");
        entity.Property(x => x.MaChiTietLichHen).HasColumnType("bigint unsigned");
        entity.Property(x => x.MaDichVu).HasColumnType("bigint unsigned");
        entity.Property(x => x.TenDichVuTaiThoiDiemThucHien).HasMaxLength(150).IsRequired();
        entity.Property(x => x.DonGiaThucTe).HasPrecision(15, 2);
        entity.Property(x => x.SoLuong).HasColumnType("smallint unsigned").HasDefaultValue((ushort)1);
        entity.Property(x => x.ThoiDiemThucHien).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
        entity.Property(x => x.GhiChu).HasColumnType("text");
        entity.HasIndex(x => x.MaChiTietLichHen).IsUnique();
        entity.HasIndex(x => x.MaHoSoKham);
        entity.HasOne(x => x.HoSoKham).WithMany(x => x.DichVuThucHiens).HasForeignKey(x => x.MaHoSoKham).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.ChiTietLichHen).WithOne(x => x.DichVuThucHien).HasForeignKey<DichVuThucHien>(x => x.MaChiTietLichHen).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.DichVu).WithMany(x => x.DichVuThucHiens).HasForeignKey(x => x.MaDichVu).OnDelete(DeleteBehavior.Restrict);
    }
}
