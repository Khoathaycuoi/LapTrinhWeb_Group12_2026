namespace BTL_NhaKhoa.Entities;

public sealed class ChiTietDonThuoc
{
    public ulong MaChiTietDonThuoc { get; set; }
    public ulong MaDonThuoc { get; set; }
    public string TenThuoc { get; set; } = null!;
    public string? HamLuong { get; set; }
    public string LieuDung { get; set; } = null!;
    public ushort? SoLanMoiNgay { get; set; }
    public ushort? SoNgayDung { get; set; }
    public decimal SoLuong { get; set; }
    public string? HuongDan { get; set; }
    public DonThuoc DonThuoc { get; set; } = null!;
}
