namespace BTL_NhaKhoa.Entities;

public sealed class ThucHienBaoTri
{
    public ulong MaBaoTri { get; set; }
    public ulong MaThietBi { get; set; }
    public string? GhiChu { get; set; }
    public BaoTri BaoTri { get; set; } = null!;
    public ThietBi ThietBi { get; set; } = null!;
}
