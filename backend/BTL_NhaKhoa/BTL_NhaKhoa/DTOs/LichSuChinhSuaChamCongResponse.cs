namespace BTL_NhaKhoa.DTOs;

public sealed class LichSuChinhSuaChamCongResponse
{
    public string Buoi { get; set; } = string.Empty;
    public string? TinhTrangCu { get; set; }
    public string? TinhTrangMoi { get; set; }
    public DateTime? GioVaoCu { get; set; }
    public DateTime? GioRaCu { get; set; }
    public DateTime? GioVaoMoi { get; set; }
    public DateTime? GioRaMoi { get; set; }
    public string LyDo { get; set; } = string.Empty;
    public string NguoiSua { get; set; } = string.Empty;
    public DateTime ThoiDiemSua { get; set; }
}
