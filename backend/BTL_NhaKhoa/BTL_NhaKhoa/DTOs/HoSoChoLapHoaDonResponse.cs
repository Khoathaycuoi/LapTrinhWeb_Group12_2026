namespace BTL_NhaKhoa.DTOs;

public sealed class HoSoChoLapHoaDonResponse
{
    public string MaHoSoKham { get; init; } = string.Empty;
    public string MaKhachHang { get; init; } = string.Empty;
    public string TenKhachHang { get; init; } = string.Empty;
    public DateTime NgayKham { get; init; }
    public decimal TongTienDuKien { get; init; }
    public List<ChiTietHoaDonResponse> DichVusThucHien { get; init; } = [];
}
