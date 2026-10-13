using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BTL_NhaKhoa.DTOs;

public sealed class DangKyBacSiRequest : DangKyRequest
{
    [JsonPropertyName("chungChi")]
    [StringLength(255)]
    public string? ChungChi { get; set; }

    [JsonPropertyName("bangCap")]
    [StringLength(255)]
    public string? BangCap { get; set; }

    [JsonPropertyName("namKinhNghiem")]
    [Range(0, 65535)]
    public ushort? SoNamKinhNghiem { get; set; }
}
