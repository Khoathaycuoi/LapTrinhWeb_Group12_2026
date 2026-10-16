using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BTL_NhaKhoa.DTOs;

public sealed class DoiSoDienThoaiRequest
{
    [JsonPropertyName("matKhauHienTai")]
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại.")]
    public string MatKhauHienTai { get; set; } = string.Empty;

    [JsonPropertyName("soDienThoaiMoi")]
    [Required(ErrorMessage = "Vui lòng nhập số điện thoại mới.")]
    public string SoDienThoaiMoi { get; set; } = string.Empty;
}
