namespace BTL_NhaKhoa.DTOs;

public sealed class ChiTietHoaDonResponse
{
    public string MaDichVuThucHien { get; init; } = string.Empty;
    public string TenDichVu { get; init; } = string.Empty;
    public decimal DonGia { get; init; }
    public ushort SoLuong { get; init; }
    public decimal ThanhTien { get; init; }
}
