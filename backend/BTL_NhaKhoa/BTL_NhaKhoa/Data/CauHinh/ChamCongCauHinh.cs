using BTL_NhaKhoa.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BTL_NhaKhoa.Data.CauHinh;

public sealed class ChamCongCauHinh : IEntityTypeConfiguration<ChamCong>
{
    public void Configure(EntityTypeBuilder<ChamCong> entity)
    {
        entity.ToTable("cham_cong", table =>
        {
            table.HasCheckConstraint("check_cham_cong_gio", "gio_vao IS NULL OR gio_ra IS NULL OR gio_ra >= gio_vao");
            table.HasCheckConstraint("check_cham_cong_trang_thai", "trang_thai IN ('DI_LAM', 'NGHI_PHEP', 'NGHI_KHONG_PHEP')");
            table.HasCheckConstraint("check_cham_cong_duyet", "trang_thai_duyet IN ('CHO_DUYET', 'DA_DUYET', 'TU_CHOI')");
        });
        entity.HasKey(x => x.MaChamCong);
        entity.Property(x => x.MaChamCong).HasColumnType("bigint unsigned").ValueGeneratedOnAdd();
        entity.Property(x => x.MaNhanVien).HasColumnType("bigint unsigned");
        entity.Property(x => x.NgayChamCong).HasColumnType("date");
        entity.Property(x => x.GioVao).HasColumnType("datetime");
        entity.Property(x => x.GioRa).HasColumnType("datetime");
        entity.Property(x => x.SoPhutTre).HasColumnType("smallint unsigned").HasDefaultValue((ushort)0);
        entity.Property(x => x.TrangThai).HasMaxLength(30).HasDefaultValue("DI_LAM");
        entity.Property(x => x.TrangThaiDuyet).HasMaxLength(20).HasDefaultValue("CHO_DUYET");
        entity.Property(x => x.NguoiDuyetTaiKhoanId).HasColumnType("bigint unsigned");
        entity.Property(x => x.DuyetLuc).HasColumnType("datetime");
        entity.Property(x => x.GhiChu).HasColumnType("text");
        entity.HasIndex(x => new { x.MaNhanVien, x.NgayChamCong }).IsUnique();
        entity.HasIndex(x => new { x.NgayChamCong, x.TrangThaiDuyet });
        entity.HasOne(x => x.NhanVien).WithMany(x => x.ChamCongs).HasForeignKey(x => x.MaNhanVien).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.NguoiDuyetTaiKhoan).WithMany(x => x.ChamCongDaDuyet).HasForeignKey(x => x.NguoiDuyetTaiKhoanId).OnDelete(DeleteBehavior.Restrict);
    }
}
