namespace BTL_NhaKhoa.Entities;

public sealed class ChiTietLichHen
{
    public ulong MaChiTietLichHen { get; set; }
    public ulong MaLichHen { get; set; }
    public ulong MaDichVu { get; set; }
    public decimal DonGiaTaiThoiDiemDat { get; set; }
    public ushort ThoiLuongTaiThoiDiemDatPhut { get; set; }
    public LichHen LichHen { get; set; } = null!;
    public DichVu DichVu { get; set; } = null!;
    public DichVuThucHien? DichVuThucHien { get; set; }
}
