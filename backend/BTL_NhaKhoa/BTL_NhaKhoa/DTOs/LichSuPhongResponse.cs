namespace BTL_NhaKhoa.DTOs;

public sealed class LichSuPhongResponse
{
    public string MaLichHen { get; init; } = string.Empty;
    public string MaPhong { get; init; } = string.Empty;
    public DateTime ThoiGianBatDau { get; init; }
    public DateTime ThoiGianKetThucDuKien { get; init; }
    public string TrangThai { get; init; } = string.Empty;
    public string MaKhachHang { get; init; } = string.Empty;
    public string TenKhachHang { get; init; } = string.Empty;
    public string? MaBacSi { get; init; }
    public string? TenBacSi { get; init; }
    public DateTime? XacNhanLuc { get; init; }
    public DateTime? HuyLuc { get; init; }
    public string? LyDoHuy { get; init; }
    public string? NguoiHuy { get; init; }
    public DateTime? KhongDenLuc { get; init; }
    public string? GhiChuKhongDen { get; init; }
}
