namespace BTL_NhaKhoa.Entities;

public sealed class ConNguoi
{
    public ulong MaConNguoi { get; set; }
    public string? SoDinhDanh { get; set; }
    public string HoTen { get; set; } = null!;
    public string SoDienThoai { get; set; } = null!;
    public string? GioiTinh { get; set; }
    public DateOnly? NgaySinh { get; set; }
    public string? DiaChi { get; set; }
    public string? TenAnhDaiDien { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public TaiKhoan? TaiKhoan { get; set; }
    public KhachHang? KhachHang { get; set; }
    public NhanVien? NhanVien { get; set; }
}
