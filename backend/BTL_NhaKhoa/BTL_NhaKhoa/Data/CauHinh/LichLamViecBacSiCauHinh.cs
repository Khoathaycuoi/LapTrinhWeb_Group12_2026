using BTL_NhaKhoa.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BTL_NhaKhoa.Data.CauHinh;

public sealed class LichLamViecBacSiCauHinh : IEntityTypeConfiguration<LichLamViecBacSi>
{
    public void Configure(EntityTypeBuilder<LichLamViecBacSi> entity)
    {
        entity.ToTable("lich_lam_viec_bac_si", table =>
        {
            table.HasCheckConstraint("check_lich_lam_viec_thu", "thu_trong_tuan BETWEEN 1 AND 7");
            table.HasCheckConstraint("check_lich_lam_viec_gio", "gio_bat_dau < gio_ket_thuc");
            table.HasCheckConstraint("check_lich_lam_viec_ngay", "ngay_hieu_luc_den IS NULL OR ngay_hieu_luc_den >= ngay_hieu_luc_tu");
            table.HasCheckConstraint("check_lich_lam_viec_trang_thai", "trang_thai IN ('HOAT_DONG', 'NGUNG_HOAT_DONG')");
        });
        entity.HasKey(x => x.MaLichLamViec);
        entity.Property(x => x.MaLichLamViec).HasColumnType("bigint unsigned").ValueGeneratedOnAdd();
        entity.Property(x => x.MaBacSi).HasColumnType("bigint unsigned");
        entity.Property(x => x.ThuTrongTuan).HasColumnType("tinyint unsigned");
        entity.Property(x => x.TrangThai).HasMaxLength(20).HasDefaultValue("HOAT_DONG");
        entity.HasIndex(x => new { x.MaBacSi, x.ThuTrongTuan, x.NgayHieuLucTu, x.NgayHieuLucDen });
        entity.HasOne(x => x.BacSi).WithMany(x => x.LichLamViecs).HasForeignKey(x => x.MaBacSi).OnDelete(DeleteBehavior.Restrict);
    }
}
