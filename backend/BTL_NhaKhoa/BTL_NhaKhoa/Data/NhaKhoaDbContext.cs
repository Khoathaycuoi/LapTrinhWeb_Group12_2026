using BTL_NhaKhoa.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BTL_NhaKhoa.Data;

public sealed class NhaKhoaDbContext(DbContextOptions<NhaKhoaDbContext> options)
    : DbContext(options)
{
    public DbSet<VaiTro> VaiTros => Set<VaiTro>();
    public DbSet<ConNguoi> ConNguois => Set<ConNguoi>();
    public DbSet<TaiKhoan> TaiKhoans => Set<TaiKhoan>();
    public DbSet<KhachHang> KhachHangs => Set<KhachHang>();
    public DbSet<NhanVien> NhanViens => Set<NhanVien>();
    public DbSet<BacSi> BacSis => Set<BacSi>();
    public DbSet<DichVu> DichVus => Set<DichVu>();
    public DbSet<Phong> Phongs => Set<Phong>();
    public DbSet<ThietBi> ThietBis => Set<ThietBi>();
    public DbSet<LichLamViecBacSi> LichLamViecBacSis => Set<LichLamViecBacSi>();
    public DbSet<LichHen> LichHens => Set<LichHen>();
    public DbSet<ChiTietLichHen> ChiTietLichHens => Set<ChiTietLichHen>();
    public DbSet<HoSoKham> HoSoKhams => Set<HoSoKham>();
    public DbSet<DichVuThucHien> DichVuThucHiens => Set<DichVuThucHien>();
    public DbSet<DonThuoc> DonThuocs => Set<DonThuoc>();
    public DbSet<ChiTietDonThuoc> ChiTietDonThuocs => Set<ChiTietDonThuoc>();
    public DbSet<HoaDon> HoaDons => Set<HoaDon>();
    public DbSet<ChiTietHoaDon> ChiTietHoaDons => Set<ChiTietHoaDon>();
    public DbSet<ThanhToan> ThanhToans => Set<ThanhToan>();
    public DbSet<BaoTri> BaoTris => Set<BaoTri>();
    public DbSet<ThucHienBaoTri> ThucHienBaoTris => Set<ThucHienBaoTri>();
    public DbSet<ChamCong> ChamCongs => Set<ChamCong>();
    public DbSet<LichSuChinhSuaChamCong> LichSuChinhSuaChamCongs => Set<LichSuChinhSuaChamCong>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NhaKhoaDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.Name));
            }
        }
    }

    private static string ToSnakeCase(string value)
    {
        var result = new System.Text.StringBuilder(value.Length + 8);
        for (var i = 0; i < value.Length; i++)
        {
            var current = value[i];
            if (char.IsUpper(current) && i > 0)
            {
                result.Append('_');
            }
            result.Append(char.ToLowerInvariant(current));
        }
        return result.ToString();
    }
}
