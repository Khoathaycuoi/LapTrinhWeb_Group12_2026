using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BTL_NhaKhoa.DTOs;

public sealed class HuyLichRequest
{
    [JsonPropertyName("maLichHen")]
    [Required(ErrorMessage = "Vui lòng nhập mã lịch hẹn.")]
    public string MaLichHen { get; set; } = string.Empty;

    [JsonPropertyName("lyDo")]
    [Required(ErrorMessage = "Vui lòng nhập lý do hủy.")]
    [StringLength(2000, ErrorMessage = "Lý do hủy không được vượt quá 2000 ký tự.")]
    public string LyDo { get; set; } = string.Empty;
}
