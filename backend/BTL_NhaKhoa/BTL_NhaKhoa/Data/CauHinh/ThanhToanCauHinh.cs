using BTL_NhaKhoa.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BTL_NhaKhoa.Data.CauHinh;

public sealed class ThanhToanCauHinh : IEntityTypeConfiguration<ThanhToan>
{
    public void Configure(EntityTypeBuilder<ThanhToan> entity)
    {
        entity.ToTable("thanh_toan", table =>
        {
            table.HasCheckConstraint("check_thanh_toan_so_tien", "so_tien > 0");
            table.HasCheckConstraint("check_thanh_toan_phuong_thuc", "phuong_thuc IN ('TIEN_MAT', 'CHUYEN_KHOAN', 'KHAC')");
            table.HasCheckConstraint("check_thanh_toan_trang_thai", "trang_thai IN ('DA_XAC_NHAN', 'DA_HUY')");
        });
        entity.HasKey(x => x.MaThanhToan);
        entity.Property(x => x.MaThanhToan).HasColumnType("bigint unsigned").ValueGeneratedOnAdd();
        entity.Property(x => x.MaHoaDon).HasColumnType("bigint unsigned");
        entity.Property(x => x.NguoiXacNhanTaiKhoanId).HasColumnType("bigint unsigned");
        entity.Property(x => x.SoTien).HasPrecision(15, 2);
        entity.Property(x => x.PhuongThuc).HasMaxLength(30).IsRequired();
        entity.Property(x => x.TrangThai).HasMaxLength(20).HasDefaultValue("DA_XAC_NHAN");
        entity.Property(x => x.ThoiDiemThanhToan).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
        entity.Property(x => x.LyDoHuy).HasColumnType("text");
        entity.Property(x => x.GhiChu).HasMaxLength(255);
        entity.HasIndex(x => x.MaHoaDon);
        entity.HasOne(x => x.HoaDon).WithMany(x => x.ThanhToans).HasForeignKey(x => x.MaHoaDon).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.NguoiXacNhanTaiKhoan).WithMany(x => x.ThanhToanDaXacNhan).HasForeignKey(x => x.NguoiXacNhanTaiKhoanId).OnDelete(DeleteBehavior.Restrict);
    }
}
