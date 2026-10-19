using System.Text.Json.Serialization;

namespace BTL_NhaKhoa.DTOs;

public sealed class BanNhapDatLichResponse
{
    [JsonPropertyName("danhSachMaDichVu")]
    public List<string> DanhSachMaDichVu { get; init; } = [];

    [JsonPropertyName("maBacSi")]
    public string? MaBacSi { get; init; }

    [JsonPropertyName("ngayHen")]
    public DateOnly? NgayHen { get; init; }

    [JsonPropertyName("gioHen")]
    public TimeOnly? GioHen { get; init; }

    [JsonPropertyName("buoc")]
    public int Buoc { get; init; } = 1;
}
