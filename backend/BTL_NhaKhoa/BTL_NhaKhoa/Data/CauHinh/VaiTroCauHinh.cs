using BTL_NhaKhoa.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BTL_NhaKhoa.Data.CauHinh;

public sealed class VaiTroCauHinh : IEntityTypeConfiguration<VaiTro>
{
    public void Configure(EntityTypeBuilder<VaiTro> entity)
    {
        entity.ToTable("vai_tro");
        entity.HasKey(x => x.MaVaiTro);
        entity.Property(x => x.MaVaiTro).HasMaxLength(30);
        entity.Property(x => x.TenVaiTro).HasMaxLength(100).IsRequired();
        entity.HasIndex(x => x.TenVaiTro).IsUnique();
        entity.HasData(
            new VaiTro { MaVaiTro = "QUAN_LY", TenVaiTro = "Quản lý" },
            new VaiTro { MaVaiTro = "LE_TAN", TenVaiTro = "Lễ tân" },
            new VaiTro { MaVaiTro = "BAC_SI", TenVaiTro = "Bác sĩ" },
            new VaiTro { MaVaiTro = "KHACH_HANG", TenVaiTro = "Khách hàng" });
    }
}
