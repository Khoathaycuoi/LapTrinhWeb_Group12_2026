namespace BTL_NhaKhoa.Entities;

public sealed class ChiTietHoaDon
{
    public ulong MaChiTietHoaDon { get; set; }
    public ulong MaHoaDon { get; set; }
    public ulong MaDichVuThucHien { get; set; }
    public string TenDichVu { get; set; } = null!;
    public decimal DonGia { get; set; }
    public ushort SoLuong { get; set; } = 1;
    public decimal ThanhTien { get; private set; }
    public HoaDon HoaDon { get; set; } = null!;
    public DichVuThucHien DichVuThucHien { get; set; } = null!;
}
