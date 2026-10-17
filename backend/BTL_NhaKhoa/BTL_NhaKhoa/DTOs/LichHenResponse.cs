using System.Text.Json.Serialization;

namespace BTL_NhaKhoa.DTOs;

public sealed class LichHenResponse
{
    [JsonPropertyName("maLichHen")]
    public string MaLichHen { get; init; } = string.Empty;

    [JsonPropertyName("ngayHen")]
    public DateOnly NgayHen { get; init; }

    [JsonPropertyName("gioHen")]
    public TimeOnly GioHen { get; init; }

    [JsonPropertyName("trangThai")]
    public string TrangThai { get; init; } = string.Empty;

    [JsonPropertyName("maKH")]
    public string MaKH { get; init; } = string.Empty;

    [JsonPropertyName("tenKH")]
    public string? TenKH { get; init; }

    [JsonPropertyName("maBacSi")]
    public string? MaBacSi { get; init; }

    [JsonPropertyName("tenBacSi")]
    public string? TenBacSi { get; init; }

    [JsonPropertyName("maPhong")]
    public string? MaPhong { get; init; }

    [JsonPropertyName("ghi_chu")]
    public string? GhiChu { get; init; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; init; }

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; init; }

    [JsonPropertyName("thoiLuongDuKienPhut")]
    public ushort ThoiLuongDuKienPhut { get; init; }

    [JsonPropertyName("lyDoHuy")]
    public string? LyDoHuy { get; init; }

    [JsonPropertyName("huyLuc")]
    public DateTime? HuyLuc { get; init; }

    [JsonPropertyName("lyDoTuChoi")]
    public string? LyDoTuChoi { get; init; }

    [JsonPropertyName("tuChoiLuc")]
    public DateTime? TuChoiLuc { get; init; }

    [JsonPropertyName("nguoiHuy")]
    public string? NguoiHuy { get; init; }

    [JsonPropertyName("denLuc")]
    public DateTime? DenLuc { get; init; }

    [JsonPropertyName("batDauKhamLuc")]
    public DateTime? BatDauKhamLuc { get; init; }

    [JsonPropertyName("hoanThanhLuc")]
    public DateTime? HoanThanhLuc { get; init; }

    [JsonPropertyName("khongDenLuc")]
    public DateTime? KhongDenLuc { get; init; }

    [JsonPropertyName("ghiChuKhongDen")]
    public string? GhiChuKhongDen { get; init; }

    [JsonPropertyName("nguoiDanhDauKhongDen")]
    public string? NguoiDanhDauKhongDen { get; init; }

    [JsonPropertyName("dichVusDuKien")]
    public List<ChiTietLichHenResponse> DichVusDuKien { get; init; } = [];
}
