namespace BTL_NhaKhoa.DTOs;

public sealed class TongHopChamCongNgayResponse
{
    public string MaNhanVien { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string VaiTro { get; set; } = string.Empty;
    public DateOnly NgayChamCong { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public ChamCongResponse? ChamCong { get; set; }
}
