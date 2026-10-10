namespace BTL_NhaKhoa.Entities;

public sealed class KhachHang
{
    public ulong MaKhachHang { get; set; }
    public ulong MaConNguoi { get; set; }
    public DateOnly NgayDangKy { get; set; }
    public string? TienSuBenh { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ConNguoi ConNguoi { get; set; } = null!;
    public ICollection<LichHen> LichHens { get; set; } = new List<LichHen>();
    public ICollection<HoSoKham> HoSoKhams { get; set; } = new List<HoSoKham>();
}
