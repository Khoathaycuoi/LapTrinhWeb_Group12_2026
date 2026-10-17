namespace BTL_NhaKhoa.DTOs;

public sealed class CapNhatHoSoKhamRequest
{
    public string? TrieuChung { get; set; }
    public string? ChanDoan { get; set; }
    public string? TomTatChoKhach { get; set; }
    public string? GhiChuNoiBo { get; set; }
    public DateOnly? NgayTaiKham { get; set; }
}
