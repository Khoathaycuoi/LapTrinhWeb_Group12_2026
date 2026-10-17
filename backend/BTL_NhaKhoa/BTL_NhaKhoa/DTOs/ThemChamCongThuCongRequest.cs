using System.ComponentModel.DataAnnotations;

namespace BTL_NhaKhoa.DTOs;

public sealed class ThemChamCongThuCongRequest
{
    [Required]
    public string MaNhanVien { get; set; } = string.Empty;
    public DateOnly NgayChamCong { get; set; }
    [Required]
    public string Buoi { get; set; } = string.Empty;
    public DateTime? GioVao { get; set; }
    public DateTime? GioRa { get; set; }
    public string? TinhTrang { get; set; }
    [Required]
    [MaxLength(500)]
    public string LyDo { get; set; } = string.Empty;
}
