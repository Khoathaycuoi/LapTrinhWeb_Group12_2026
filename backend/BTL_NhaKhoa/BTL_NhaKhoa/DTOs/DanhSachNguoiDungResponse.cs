using System.Text.Json.Serialization;

namespace BTL_NhaKhoa.DTOs;

public sealed class DanhSachNguoiDungResponse
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("ten")]
    public string Ten { get; init; } = string.Empty;

    [JsonPropertyName("sdt")]
    public string SoDienThoai { get; init; } = string.Empty;

    [JsonPropertyName("gioiTinh")]
    public string? GioiTinh { get; init; }

    [JsonPropertyName("vaiTro")]
    public string VaiTro { get; init; } = string.Empty;

    [JsonPropertyName("trangThai")]
    public string TrangThai { get; init; } = string.Empty;
}
