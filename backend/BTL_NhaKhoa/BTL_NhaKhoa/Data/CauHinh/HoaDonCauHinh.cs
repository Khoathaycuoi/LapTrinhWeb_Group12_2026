using BTL_NhaKhoa.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BTL_NhaKhoa.Data.CauHinh;

public sealed class HoaDonCauHinh : IEntityTypeConfiguration<HoaDon>
{
    public void Configure(EntityTypeBuilder<HoaDon> entity)
    {
        entity.ToTable("hoa_don", table =>
        {
            table.HasCheckConstraint("check_hoa_don_tong_tien", "tong_tien >= 0");
            table.HasCheckConstraint("check_hoa_don_trang_thai", "trang_thai IN ('CHUA_THANH_TOAN', 'THANH_TOAN_MOT_PHAN', 'DA_THANH_TOAN', 'DA_HUY')");
        });
        entity.HasKey(x => x.MaHoaDon);
        entity.Property(x => x.MaHoaDon).HasColumnType("bigint unsigned").ValueGeneratedOnAdd();
        entity.Property(x => x.MaHoSoKham).HasColumnType("bigint unsigned");
        entity.Property(x => x.NgayLap).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
        entity.Property(x => x.TongTien).HasPrecision(15, 2).HasDefaultValue(0m);
        entity.Property(x => x.TrangThai).HasMaxLength(30).HasDefaultValue("CHUA_THANH_TOAN");
        entity.Property(x => x.NguoiLapTaiKhoanId).HasColumnType("bigint unsigned");
        CauHinhChung.CauHinhNgayTaoCapNhat(entity);
        entity.HasIndex(x => x.MaHoSoKham).IsUnique();
        entity.HasOne(x => x.HoSoKham).WithOne(x => x.HoaDon).HasForeignKey<HoaDon>(x => x.MaHoSoKham).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.NguoiLapTaiKhoan).WithMany(x => x.HoaDonDaLap).HasForeignKey(x => x.NguoiLapTaiKhoanId).OnDelete(DeleteBehavior.Restrict);
    }
}
