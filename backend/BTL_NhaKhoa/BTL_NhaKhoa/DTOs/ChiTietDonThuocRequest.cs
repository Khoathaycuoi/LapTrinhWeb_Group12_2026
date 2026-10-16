using System.ComponentModel.DataAnnotations;

namespace BTL_NhaKhoa.DTOs;

public sealed class ChiTietDonThuocRequest
{
    [Required]
    public string TenThuoc { get; set; } = string.Empty;
    public string? HamLuong { get; set; }
    [Required]
    public string LieuDung { get; set; } = string.Empty;
    public ushort? SoLanMoiNgay { get; set; }
    public ushort? SoNgayDung { get; set; }
    [Range(0.01, double.MaxValue)]
    public decimal SoLuong { get; set; }
    public string? HuongDan { get; set; }
}
