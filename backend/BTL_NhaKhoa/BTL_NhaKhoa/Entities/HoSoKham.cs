namespace BTL_NhaKhoa.Entities;

public sealed class HoSoKham
{
    public ulong MaHoSoKham { get; set; }
    public ulong MaKhachHang { get; set; }
    public ulong MaBacSi { get; set; }
    public ulong? MaLichHen { get; set; }
    public DateTime NgayKham { get; set; }
    public string? TrieuChung { get; set; }
    public string? ChanDoan { get; set; }
    public string? TomTatChoKhach { get; set; }
    public string? GhiChuNoiBo { get; set; }
    public DateOnly? NgayTaiKham { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public KhachHang KhachHang { get; set; } = null!;
    public BacSi BacSi { get; set; } = null!;
    public LichHen? LichHen { get; set; }
    public ICollection<DichVuThucHien> DichVuThucHiens { get; set; } = new List<DichVuThucHien>();
    public ICollection<DonThuoc> DonThuocs { get; set; } = new List<DonThuoc>();
    public HoaDon? HoaDon { get; set; }
}
