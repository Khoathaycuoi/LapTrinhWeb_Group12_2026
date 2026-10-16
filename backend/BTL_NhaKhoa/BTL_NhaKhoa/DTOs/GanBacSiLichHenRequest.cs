using System.ComponentModel.DataAnnotations;

namespace BTL_NhaKhoa.DTOs;

public sealed class GanBacSiLichHenRequest
{
    [Required]
    public string MaBacSi { get; set; } = string.Empty;
}
