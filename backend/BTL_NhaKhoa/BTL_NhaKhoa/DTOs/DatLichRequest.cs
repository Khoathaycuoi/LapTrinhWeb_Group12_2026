using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BTL_NhaKhoa.DTOs;

public sealed class DatLichRequest
{
    [JsonPropertyName("maKhachHang")]
    public string? MaKhachHang { get; set; }

    [JsonPropertyName("maBacSi")]
    public string? MaBacSi { get; set; }

    [JsonPropertyName("ngayHen")]
    [Required(ErrorMessage = "Ngày hẹn không được để trống.")]
    public DateOnly? NgayHen { get; set; }

    [JsonPropertyName("gioHen")]
    [Required(ErrorMessage = "Giờ hẹn không được để trống.")]
    public TimeOnly? GioHen { get; set; }

    [JsonPropertyName("danhSachMaDichVu")]
    [Required(ErrorMessage = "Phải chọn ít nhất một dịch vụ.")]
    [MinLength(1, ErrorMessage = "Phải chọn ít nhất một dịch vụ.")]
    public List<string> DanhSachMaDichVu { get; set; } = [];
}
