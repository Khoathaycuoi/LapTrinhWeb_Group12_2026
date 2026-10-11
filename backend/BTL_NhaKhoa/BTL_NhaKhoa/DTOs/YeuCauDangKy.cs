using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BTL_NhaKhoa.DTOs;

public class YeuCauDangKy
{
    [JsonPropertyName("maDinhDanh")]
    [RegularExpression(@"^\d{12}$", ErrorMessage = "Mã định danh phải có đúng 12 chữ số.")]
    public string? SoDinhDanh { get; set; }

    [JsonPropertyName("ten")]
    [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
    [StringLength(150)]
    public string HoTen { get; set; } = string.Empty;

    [JsonPropertyName("sdt")]
    [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
    public string SoDienThoai { get; set; } = string.Empty;

    [JsonPropertyName("gioiTinh")]
    [StringLength(20)]
    public string? GioiTinh { get; set; }

    [JsonPropertyName("ngaySinh")]
    public DateOnly? NgaySinh { get; set; }

    [JsonPropertyName("diaChi")]
    [StringLength(255)]
    public string? DiaChi { get; set; }

    [JsonPropertyName("matKhau")]
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Mật khẩu phải từ 8 ký tự, có ít nhất một chữ cái và một chữ số.")]
    public string MatKhau { get; set; } = string.Empty;
}

public sealed class YeuCauDangKyKhachHang : YeuCauDangKy
{
    [JsonPropertyName("tienSuBenh")]
    public string? TienSuBenh { get; set; }
}

public sealed class YeuCauDangKyBacSi : YeuCauDangKy
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
