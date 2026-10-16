using System.ComponentModel.DataAnnotations;

namespace BTL_NhaKhoa.DTOs;

public sealed class KeDonThuocRequest
{
    public string? GhiChu { get; set; }
    [Required]
    [MinLength(1)]
    public List<ChiTietDonThuocRequest> DanhSachThuoc { get; set; } = [];
}
