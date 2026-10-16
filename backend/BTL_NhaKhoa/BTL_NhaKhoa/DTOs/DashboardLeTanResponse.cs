namespace BTL_NhaKhoa.DTOs;

public sealed class DashboardLeTanResponse
{
    public DateOnly Ngay { get; init; }
    public int SoLichChoXacNhan { get; init; }
    public int SoLichHomNay { get; init; }
    public int SoKhachChoKham { get; init; }
    public int SoHoaDonConNo { get; init; }
    public List<LichHenDashboardResponse> LichChoXacNhanGanNhat { get; init; } = [];
    public List<LichHenDashboardResponse> LichSapDienRa { get; init; } = [];
}
