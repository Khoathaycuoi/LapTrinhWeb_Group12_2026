using System.ComponentModel.DataAnnotations;

namespace BTL_NhaKhoa.DTOs;

public sealed class HuyThanhToanRequest
{
    [Required]
    public string LyDo { get; set; } = string.Empty;
}
