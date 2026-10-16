using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BTL_NhaKhoa.DTOs;

public sealed class CapNhatDichVuRequest
{
    [JsonPropertyName("tenDichVu")]
    [Required(ErrorMessage = "Vui lòng nhập tên dịch vụ.")]
    [StringLength(150)]
    public string TenDichVu { get; set; } = string.Empty;

    [JsonPropertyName("moTa")]
    public string? MoTa { get; set; }

    [JsonPropertyName("donGia")]
    public decimal? DonGia { get; set; }

    [JsonPropertyName("thoiLuongDuKienPhut")]
    public ushort? ThoiLuongDuKienPhut { get; set; }
}
