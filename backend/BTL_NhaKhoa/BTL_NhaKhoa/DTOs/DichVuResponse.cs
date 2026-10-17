using System.Text.Json.Serialization;

namespace BTL_NhaKhoa.DTOs;

public sealed class DichVuResponse
{
    [JsonPropertyName("maDichVu")]
    public string MaDichVu { get; init; } = string.Empty;

    [JsonPropertyName("tenDichVu")]
    public string TenDichVu { get; init; } = string.Empty;

    [JsonPropertyName("moTa")]
    public string? MoTa { get; init; }

    [JsonPropertyName("donGia")]
    public decimal DonGia { get; init; }

    [JsonPropertyName("trangThai")]
    public string TrangThai { get; init; } = string.Empty;

    [JsonPropertyName("thoiLuongDuKienPhut")]
    public ushort ThoiLuongDuKienPhut { get; init; }
}
