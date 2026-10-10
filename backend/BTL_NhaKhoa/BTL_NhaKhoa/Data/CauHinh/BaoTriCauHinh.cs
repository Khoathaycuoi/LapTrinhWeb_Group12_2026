using BTL_NhaKhoa.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BTL_NhaKhoa.Data.CauHinh;

public sealed class BaoTriCauHinh : IEntityTypeConfiguration<BaoTri>
{
    public void Configure(EntityTypeBuilder<BaoTri> entity)
    {
        entity.ToTable("bao_tri", table =>
        {
            table.HasCheckConstraint("check_bao_tri_chi_phi", "chi_phi IS NULL OR chi_phi >= 0");
            table.HasCheckConstraint("check_bao_tri_trang_thai", "trang_thai IN ('MOI_TIEP_NHAN', 'DANG_XU_LY', 'DA_HOAN_TAT', 'DA_HUY')");
        });
        entity.HasKey(x => x.MaBaoTri);
        entity.Property(x => x.MaBaoTri).HasColumnType("bigint unsigned").ValueGeneratedOnAdd();
        entity.Property(x => x.NguoiBaoTaiKhoanId).HasColumnType("bigint unsigned");
        entity.Property(x => x.NguoiXuLyTaiKhoanId).HasColumnType("bigint unsigned");
        entity.Property(x => x.NgayBao).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
        entity.Property(x => x.NgayHoanTat).HasColumnType("datetime");
        entity.Property(x => x.NoiDung).HasColumnType("text").IsRequired();
        entity.Property(x => x.ChiPhi).HasPrecision(15, 2);
        entity.Property(x => x.TrangThai).HasMaxLength(20).HasDefaultValue("MOI_TIEP_NHAN");
        entity.HasOne(x => x.NguoiBaoTaiKhoan).WithMany(x => x.BaoTriDaBao).HasForeignKey(x => x.NguoiBaoTaiKhoanId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.NguoiXuLyTaiKhoan).WithMany(x => x.BaoTriDaXuLy).HasForeignKey(x => x.NguoiXuLyTaiKhoanId).OnDelete(DeleteBehavior.Restrict);
    }
}
