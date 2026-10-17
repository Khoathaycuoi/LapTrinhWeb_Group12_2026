using BTL_NhaKhoa.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BTL_NhaKhoa.Data.CauHinh;

public sealed class LichSuChinhSuaChamCongCauHinh : IEntityTypeConfiguration<LichSuChinhSuaChamCong>
{
    public void Configure(EntityTypeBuilder<LichSuChinhSuaChamCong> entity)
    {
        entity.ToTable("lich_su_chinh_sua_cham_cong");
        entity.HasKey(x => x.MaLichSu);
        entity.Property(x => x.MaLichSu).HasColumnType("bigint unsigned").ValueGeneratedOnAdd();
        entity.Property(x => x.MaChamCong).HasColumnType("bigint unsigned");
        entity.Property(x => x.NguoiSuaTaiKhoanId).HasColumnType("bigint unsigned");
        entity.Property(x => x.Buoi).HasMaxLength(10).HasDefaultValue("CA_NGAY");
        entity.Property(x => x.TinhTrangCu).HasMaxLength(25);
        entity.Property(x => x.TinhTrangMoi).HasMaxLength(25);
        entity.Property(x => x.GioVaoCu).HasColumnType("datetime");
        entity.Property(x => x.GioRaCu).HasColumnType("datetime");
        entity.Property(x => x.GioVaoMoi).HasColumnType("datetime");
        entity.Property(x => x.GioRaMoi).HasColumnType("datetime");
        entity.Property(x => x.LyDo).HasMaxLength(500).IsRequired();
        entity.Property(x => x.ThoiDiemSua).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
        entity.HasIndex(x => new { x.MaChamCong, x.ThoiDiemSua });
        entity.HasOne(x => x.ChamCong).WithMany(x => x.LichSuChinhSuas).HasForeignKey(x => x.MaChamCong).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.NguoiSuaTaiKhoan).WithMany(x => x.LichSuDaSua).HasForeignKey(x => x.NguoiSuaTaiKhoanId).OnDelete(DeleteBehavior.Restrict);
    }
}
