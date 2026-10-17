namespace BTL_NhaKhoa.DTOs;

public sealed class LichHenDashboardResponse
{
    public string MaLichHen { get; init; } = string.Empty;
    public DateTime ThoiGianBatDau { get; init; }
    public string TrangThai { get; init; } = string.Empty;
    public string MaKhachHang { get; init; } = string.Empty;
    public string TenKhachHang { get; init; } = string.Empty;
    public string? MaBacSi { get; init; }
    public string? TenBacSi { get; init; }
}
