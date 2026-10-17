namespace BTL_NhaKhoa.DTOs;

public sealed class DichVuThucHienResponse
{
    public string MaDichVuThucHien { get; init; } = string.Empty;
    public string MaDichVu { get; init; } = string.Empty;
    public string TenDichVu { get; init; } = string.Empty;
    public decimal DonGia { get; init; }
    public ushort SoLuong { get; init; }
    public DateTime ThoiDiemThucHien { get; init; }
    public string? GhiChu { get; init; }
}
