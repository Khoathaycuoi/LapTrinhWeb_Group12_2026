using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BTL_NhaKhoa.DTOs;

public sealed class DoiMatKhauRequest
{
    [JsonPropertyName("oldPassword")]
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại.")]
    public string MatKhauCu { get; set; } = string.Empty;

    [JsonPropertyName("newPassword")]
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Mật khẩu mới phải từ 8 ký tự, có ít nhất một chữ cái và một chữ số.")]
    public string MatKhauMoi { get; set; } = string.Empty;
}
