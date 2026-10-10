namespace BTL_NhaKhoa.Entities;

public sealed class ThietBi
{
    public ulong MaThietBi { get; set; }
    public ulong? MaPhong { get; set; }
    public string TenThietBi { get; set; } = null!;
    public string? LoaiThietBi { get; set; }
    public DateOnly? NgayMua { get; set; }
    public string TinhTrang { get; set; } = "HOAT_DONG";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Phong? Phong { get; set; }
    public ICollection<ThucHienBaoTri> ThucHienBaoTris { get; set; } = new List<ThucHienBaoTri>();
}
