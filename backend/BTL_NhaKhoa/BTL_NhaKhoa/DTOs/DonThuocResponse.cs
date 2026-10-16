namespace BTL_NhaKhoa.DTOs;

public sealed class DonThuocResponse
{
    public string MaDonThuoc { get; init; } = string.Empty;
    public DateTime NgayKe { get; init; }
    public string? GhiChu { get; init; }
    public List<ChiTietDonThuocResponse> ChiTietDonThuocs { get; init; } = [];
}
