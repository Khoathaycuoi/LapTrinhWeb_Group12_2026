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
            table.HasCheckConstraint("check_cham_cong_gio_sang", "gio_vao_sang IS NULL OR gio_ra_sang IS NULL OR gio_ra_sang >= gio_vao_sang");
            table.HasCheckConstraint("check_cham_cong_gio_chieu", "gio_vao_chieu IS NULL OR gio_ra_chieu IS NULL OR gio_ra_chieu >= gio_vao_chieu");
            table.HasCheckConstraint("check_cham_cong_tinh_trang_sang", "tinh_trang_sang IN ('CHUA_XAC_DINH', 'LAM_VIEC', 'NGHI_PHEP', 'VANG_KHONG_PHEP')");
            table.HasCheckConstraint("check_cham_cong_tinh_trang_chieu", "tinh_trang_chieu IN ('CHUA_XAC_DINH', 'LAM_VIEC', 'NGHI_PHEP', 'VANG_KHONG_PHEP')");
            table.HasCheckConstraint("check_cham_cong_duyet", "trang_thai_duyet IN ('CHUA_HOAN_TAT', 'CHO_DUYET', 'DA_DUYET', 'TU_CHOI')");
        });
        entity.HasKey(x => x.MaChamCong);
        entity.Property(x => x.MaChamCong).HasColumnType("bigint unsigned").ValueGeneratedOnAdd();
        entity.Property(x => x.MaNhanVien).HasColumnType("bigint unsigned");
        entity.Property(x => x.NgayChamCong).HasColumnType("date");
        entity.Property(x => x.GioVaoSang).HasColumnType("datetime");
        entity.Property(x => x.GioRaSang).HasColumnType("datetime");
        entity.Property(x => x.GioVaoChieu).HasColumnType("datetime");
        entity.Property(x => x.GioRaChieu).HasColumnType("datetime");
        entity.Property(x => x.TinhTrangSang).HasMaxLength(25).HasDefaultValue("CHUA_XAC_DINH");
        entity.Property(x => x.TinhTrangChieu).HasMaxLength(25).HasDefaultValue("CHUA_XAC_DINH");
        entity.Property(x => x.LyDoSang).HasMaxLength(500);
        entity.Property(x => x.LyDoChieu).HasMaxLength(500);
        entity.Property(x => x.TrangThaiDuyet).HasMaxLength(20).HasDefaultValue("CHUA_HOAN_TAT");
        entity.Property(x => x.NguoiDuyetTaiKhoanId).HasColumnType("bigint unsigned");
        entity.Property(x => x.DuyetLuc).HasColumnType("datetime");
        entity.Property(x => x.NguoiTuChoiTaiKhoanId).HasColumnType("bigint unsigned");
        entity.Property(x => x.TuChoiLuc).HasColumnType("datetime");
        entity.Property(x => x.LyDoTuChoi).HasMaxLength(500);
        entity.HasIndex(x => new { x.MaNhanVien, x.NgayChamCong }).IsUnique();
        entity.HasIndex(x => new { x.NgayChamCong, x.TrangThaiDuyet });
        entity.HasOne(x => x.NhanVien).WithMany(x => x.ChamCongs).HasForeignKey(x => x.MaNhanVien).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.NguoiDuyetTaiKhoan).WithMany(x => x.ChamCongDaDuyet).HasForeignKey(x => x.NguoiDuyetTaiKhoanId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.NguoiTuChoiTaiKhoan).WithMany(x => x.ChamCongDaTuChoi).HasForeignKey(x => x.NguoiTuChoiTaiKhoanId).OnDelete(DeleteBehavior.Restrict);
    }
}
