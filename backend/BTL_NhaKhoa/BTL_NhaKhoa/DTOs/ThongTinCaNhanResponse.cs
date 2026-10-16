using System.Text.Json.Serialization;

namespace BTL_NhaKhoa.DTOs;

public sealed class ThongTinCaNhanResponse
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("ten")]
    public string Ten { get; init; } = string.Empty;

    [JsonPropertyName("sdt")]
    public string SoDienThoai { get; init; } = string.Empty;

    [JsonPropertyName("gioiTinh")]
    public string? GioiTinh { get; init; }

    [JsonPropertyName("role")]
    public string Role { get; init; } = string.Empty;

    [JsonPropertyName("diaChi")]
    public string? DiaChi { get; init; }

    [JsonPropertyName("bangCap")]
    public string? BangCap { get; init; }

    [JsonPropertyName("chungChi")]
    public string? ChungChi { get; init; }

    [JsonPropertyName("ngaySinh")]
    public DateOnly? NgaySinh { get; init; }

    [JsonPropertyName("tienSuBenh")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TienSuBenh { get; init; }

    [JsonPropertyName("soNamKinhNghiem")]
    public ushort? SoNamKinhNghiem { get; init; }

    [JsonPropertyName("trangThai")]
    public string TrangThai { get; init; } = string.Empty;
}
