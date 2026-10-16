using System.ComponentModel.DataAnnotations;

namespace BTL_NhaKhoa.DTOs;

public sealed class GhiNhanThanhToanRequest
{
    [Range(0.01, 9999999999999.99)]
    public decimal SoTien { get; set; }

    [Required]
    public string PhuongThuc { get; set; } = string.Empty;

    [StringLength(255)]
    public string? GhiChu { get; set; }
}
