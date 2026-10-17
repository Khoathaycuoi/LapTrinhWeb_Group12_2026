namespace BTL_NhaKhoa.DTOs;

public sealed class DashboardBacSiResponse
{
    public DateOnly Ngay { get; init; }
    public int SoLichHomNay { get; init; }
    public int SoLichChoXacNhan { get; init; }
    public int SoKhachChoKham { get; init; }
    public int SoCaDangKham { get; init; }
    public int SoHoSoChuaChot { get; init; }
    public List<LichHenDashboardResponse> LichHomNay { get; init; } = [];
}
