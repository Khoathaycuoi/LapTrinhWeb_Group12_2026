namespace BTL_NhaKhoa.DTOs;

public sealed class ChiTietDonThuocResponse
{
    public string TenThuoc { get; init; } = string.Empty;
    public string? HamLuong { get; init; }
    public string LieuDung { get; init; } = string.Empty;
    public ushort? SoLanMoiNgay { get; init; }
    public ushort? SoNgayDung { get; init; }
    public decimal SoLuong { get; init; }
    public string? HuongDan { get; init; }
}
