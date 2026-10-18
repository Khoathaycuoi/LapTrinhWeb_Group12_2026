namespace BTL_NhaKhoa.DTOs;

public sealed class TomTatHoSoKhamResponse
{
    public string MaHoSoKham { get; init; } = string.Empty;
    public DateTime NgayKham { get; init; }
    public string MaBacSi { get; init; } = string.Empty;
    public string TenBacSi { get; init; } = string.Empty;
    public string TomTat { get; init; } = string.Empty;
    public DateTime? ChotLuc { get; init; }
}
