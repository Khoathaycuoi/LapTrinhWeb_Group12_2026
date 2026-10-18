namespace BTL_NhaKhoa.DTOs;

public sealed class HoSoKhamResponse
{
    public string MaHoSoKham { get; init; } = string.Empty;
    public string? MaLichHen { get; init; }
    public string? TrangThaiLichHen { get; init; }
    public DateTime NgayKham { get; init; }
    public string MaKhachHang { get; init; } = string.Empty;
    public string MaBacSi { get; init; } = string.Empty;
    public string? TrieuChung { get; init; }
    public string? ChanDoan { get; init; }
    public string? TomTatChoKhach { get; init; }
    public string? GhiChuNoiBo { get; init; }
    public DateOnly? NgayTaiKham { get; init; }
    public DateTime? ChotLuc { get; init; }
    public List<DichVuThucHienResponse> DichVuThucHiens { get; init; } = [];
    public List<ChiTietLichHenResponse> DichVusDuKien { get; init; } = [];
    public List<DonThuocResponse> DonThuocs { get; init; } = [];
}
