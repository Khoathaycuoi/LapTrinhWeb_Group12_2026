namespace BTL_NhaKhoa.DTOs;

public sealed class ChiTietLichHenResponse
{
    public string MaDichVu { get; init; } = string.Empty;
    public string TenDichVu { get; init; } = string.Empty;
    public decimal DonGiaTaiThoiDiemDat { get; init; }
    public ushort ThoiLuongTaiThoiDiemDatPhut { get; init; }
    public bool DaThucHien { get; init; }
}
