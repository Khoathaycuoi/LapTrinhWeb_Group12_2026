namespace BTL_NhaKhoa.DTOs;

public sealed class BaoCaoDoanhThuNamResponse
{
    public int Nam { get; init; }
    public decimal TongDoanhThu { get; init; }
    public int TongKhoanThu { get; init; }
    public List<DoanhThuTheoThangResponse> CacThang { get; init; } = [];
}
