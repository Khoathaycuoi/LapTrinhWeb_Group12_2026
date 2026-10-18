using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BTL_NhaKhoa.DTOs;

public sealed class GanBacSiVaXacNhanLichHenRequest
{
    [JsonPropertyName("maBacSi")]
    [Required(ErrorMessage = "Vui lòng chọn bác sĩ.")]
    public string MaBacSi { get; set; } = string.Empty;

    [JsonPropertyName("maPhong")]
    public string? MaPhong { get; set; }
}
