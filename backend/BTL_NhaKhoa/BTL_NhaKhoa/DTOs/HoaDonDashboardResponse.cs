namespace BTL_NhaKhoa.DTOs;

public sealed class HoaDonDashboardResponse
{
    public string MaHoaDon { get; init; } = string.Empty;
    public DateTime NgayLap { get; init; }
    public decimal TongTien { get; init; }
    public decimal DaThanhToan { get; init; }
    public decimal ConNo { get; init; }
}
