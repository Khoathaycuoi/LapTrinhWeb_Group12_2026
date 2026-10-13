using System.Text.Json.Serialization;

namespace BTL_NhaKhoa.DTOs;

public sealed class DangKyKhachHangRequest : DangKyRequest
{
    [JsonPropertyName("tienSuBenh")]
    public string? TienSuBenh { get; set; }
}
