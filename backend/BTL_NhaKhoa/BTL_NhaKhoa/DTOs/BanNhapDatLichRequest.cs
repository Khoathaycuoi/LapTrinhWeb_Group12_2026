using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BTL_NhaKhoa.DTOs;

public sealed class BanNhapDatLichRequest
{
    [JsonPropertyName("danhSachMaDichVu")]
    [MaxLength(50)]
    public List<string> DanhSachMaDichVu { get; set; } = [];

    [JsonPropertyName("maBacSi")]
    [StringLength(32)]
    public string? MaBacSi { get; set; }

    [JsonPropertyName("ngayHen")]
    public DateOnly? NgayHen { get; set; }

    [JsonPropertyName("gioHen")]
    public TimeOnly? GioHen { get; set; }

    [JsonPropertyName("buoc")]
    [Range(1, 3)]
    public int Buoc { get; set; } = 1;
}
