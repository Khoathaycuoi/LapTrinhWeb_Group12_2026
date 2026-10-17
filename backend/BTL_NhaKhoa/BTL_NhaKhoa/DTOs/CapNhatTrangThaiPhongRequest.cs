using System.ComponentModel.DataAnnotations;

namespace BTL_NhaKhoa.DTOs;

public sealed class CapNhatTrangThaiPhongRequest
{
    [Required]
    public string TrangThai { get; set; } = string.Empty;
}
