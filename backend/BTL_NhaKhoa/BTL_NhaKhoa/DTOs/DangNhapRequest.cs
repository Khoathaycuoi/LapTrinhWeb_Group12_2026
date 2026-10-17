using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BTL_NhaKhoa.DTOs;

public sealed class DangNhapRequest
{
    [JsonPropertyName("username")]
    [Required]
    public string SoDienThoai { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    [Required]
    public string MatKhau { get; set; } = string.Empty;
}
