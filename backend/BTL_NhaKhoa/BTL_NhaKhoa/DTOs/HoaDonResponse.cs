namespace BTL_NhaKhoa.DTOs;

public sealed class HoaDonResponse
{
    public string MaHoaDon { get; init; } = string.Empty;
    public string MaHoSoKham { get; init; } = string.Empty;
    public string MaKhachHang { get; init; } = string.Empty;
    public string TenKhachHang { get; init; } = string.Empty;
    public DateTime NgayLap { get; init; }
    public decimal TongTien { get; init; }
    public decimal DaThanhToan { get; init; }
    public decimal ConNo { get; init; }
    public string TrangThai { get; init; } = string.Empty;
    public List<ChiTietHoaDonResponse> ChiTietHoaDons { get; init; } = [];
    public List<ThanhToanResponse> ThanhToans { get; init; } = [];
}
