namespace BTL_NhaKhoa.DTOs;

public sealed class ThanhToanResponse
{
    public string MaThanhToan { get; init; } = string.Empty;
    public decimal SoTien { get; init; }
    public string PhuongThuc { get; init; } = string.Empty;
    public string TrangThai { get; init; } = string.Empty;
    public DateTime ThoiDiemThanhToan { get; init; }
    public string? NguoiXacNhan { get; init; }
    public string? GhiChu { get; init; }
    public string? LyDoHuy { get; init; }
    public DateTime? HuyLuc { get; init; }
    public string? NguoiHuy { get; init; }
}
