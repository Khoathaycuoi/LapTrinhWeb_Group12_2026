using System.ComponentModel.DataAnnotations;

namespace BTL_NhaKhoa.DTOs;

public sealed class TuChoiChamCongRequest
{
    [Required]
    [MaxLength(500)]
    public string LyDo { get; set; } = string.Empty;
}
