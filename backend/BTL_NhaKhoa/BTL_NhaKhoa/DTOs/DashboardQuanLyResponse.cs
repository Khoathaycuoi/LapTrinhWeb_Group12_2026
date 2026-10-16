namespace BTL_NhaKhoa.DTOs;

public sealed class DashboardQuanLyResponse
{
    public DateOnly Ngay { get; init; }
    public int SoLichHomNay { get; init; }
    public int SoLichChoXacNhan { get; init; }
    public decimal DoanhThuThang { get; init; }
    public decimal TongCongNo { get; init; }
    public int SoPhongHoatDong { get; init; }
    public int SoPhongTamNgung { get; init; }
    public int SoPhongBaoTri { get; init; }
    public List<DoanhThuTheoTuanResponse> DoanhThuBonTuan { get; init; } = [];
    public List<LichHenTheoTrangThaiResponse> LichHenTheoTrangThaiBayNgay { get; init; } = [];
}
