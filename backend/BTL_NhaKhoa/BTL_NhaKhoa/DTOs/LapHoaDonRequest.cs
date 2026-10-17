using System.ComponentModel.DataAnnotations;

namespace BTL_NhaKhoa.DTOs;

public sealed class LapHoaDonRequest
{
    [Required]
    public string MaHoSoKham { get; set; } = string.Empty;
}
