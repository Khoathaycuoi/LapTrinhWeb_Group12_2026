namespace BTL_NhaKhoa.Entities;

public sealed class DonThuoc
{
    public ulong MaDonThuoc { get; set; }
    public ulong MaHoSoKham { get; set; }
    public DateTime NgayKe { get; set; }
    public string? GhiChu { get; set; }
    public HoSoKham HoSoKham { get; set; } = null!;
    public ICollection<ChiTietDonThuoc> ChiTietDonThuocs { get; set; } = new List<ChiTietDonThuoc>();
}
