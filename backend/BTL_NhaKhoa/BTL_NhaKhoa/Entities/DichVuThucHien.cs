namespace BTL_NhaKhoa.Entities;

public sealed class DichVuThucHien
{
    public ulong MaDichVuThucHien { get; set; }
    public ulong MaHoSoKham { get; set; }
    public ulong? MaChiTietLichHen { get; set; }
    public ulong MaDichVu { get; set; }
    public string TenDichVuTaiThoiDiemThucHien { get; set; } = null!;
    public decimal DonGiaThucTe { get; set; }
    public ushort SoLuong { get; set; } = 1;
    public DateTime ThoiDiemThucHien { get; set; }
    public string? GhiChu { get; set; }
    public HoSoKham HoSoKham { get; set; } = null!;
    public ChiTietLichHen? ChiTietLichHen { get; set; }
    public DichVu DichVu { get; set; } = null!;
    public ChiTietHoaDon? ChiTietHoaDon { get; set; }
}
