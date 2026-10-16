using System.ComponentModel.DataAnnotations;

namespace BTL_NhaKhoa.DTOs;

public sealed class GhiNhanDichVuThucHienRequest
{
    [Required]
    public string MaDichVu { get; set; } = string.Empty;

    [Range(1, ushort.MaxValue)]
    public ushort SoLuong { get; set; } = 1;

    public string? GhiChu { get; set; }
}
