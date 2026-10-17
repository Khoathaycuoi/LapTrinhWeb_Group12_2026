using System.ComponentModel.DataAnnotations;

namespace BTL_NhaKhoa.DTOs;

public sealed class ThemPhongRequest
{
    [Required]
    [StringLength(100)]
    public string TenPhong { get; set; } = string.Empty;
}
