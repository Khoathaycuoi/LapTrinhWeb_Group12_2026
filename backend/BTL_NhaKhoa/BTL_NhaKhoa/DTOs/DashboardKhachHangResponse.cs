namespace BTL_NhaKhoa.DTOs;

public sealed class DashboardKhachHangResponse
{
    public DateOnly Ngay { get; init; }
    public int SoLichSapToi { get; init; }
    public LichHenDashboardResponse? LichSapToiGanNhat { get; init; }
    public int SoHoaDonConNo { get; init; }
    public decimal TongCongNo { get; init; }
    public List<HoaDonDashboardResponse> HoaDonConNoGanNhat { get; init; } = [];
    public TomTatHoSoKhamResponse? LanKhamGanNhat { get; init; }
}
