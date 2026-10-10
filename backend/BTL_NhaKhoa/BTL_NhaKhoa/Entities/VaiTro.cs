namespace BTL_NhaKhoa.Entities;

public sealed class VaiTro
{
    public string MaVaiTro { get; set; } = null!;
    public string TenVaiTro { get; set; } = null!;
    public ICollection<TaiKhoan> TaiKhoans { get; set; } = new List<TaiKhoan>();
}
