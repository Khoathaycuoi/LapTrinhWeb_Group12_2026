namespace BTL_NhaKhoa.Entities;

public sealed class Phong
{
    public ulong MaPhong { get; set; }
    public string TenPhong { get; set; } = null!;
    public string TrangThai { get; set; } = "HOAT_DONG";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<LichHen> LichHens { get; set; } = new List<LichHen>();
}
