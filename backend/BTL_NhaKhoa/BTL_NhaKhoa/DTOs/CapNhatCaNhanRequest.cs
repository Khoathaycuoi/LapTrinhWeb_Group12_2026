using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BTL_NhaKhoa.DTOs;

public sealed class CapNhatCaNhanRequest
{
    [JsonPropertyName("ten")]
    [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
    [StringLength(150)]
    public string Ten { get; set; } = string.Empty;

    [JsonPropertyName("gioiTinh")]
    [StringLength(20)]
    public string? GioiTinh { get; set; }

    [JsonPropertyName("ngaySinh")]
    public DateOnly? NgaySinh { get; set; }

    [JsonPropertyName("diaChi")]
    [StringLength(255)]
    public string? DiaChi { get; set; }

    [JsonPropertyName("bangCap")]
    [StringLength(255)]
    public string? BangCap { get; set; }

    [JsonPropertyName("chungChi")]
    [StringLength(255)]
    public string? ChungChi { get; set; }

    [JsonPropertyName("soNamKinhNghiem")]
    [Range(0, 65535)]
    public ushort? SoNamKinhNghiem { get; set; }

    [JsonPropertyName("tienSuBenh")]
    public string? TienSuBenh { get; set; }
}
