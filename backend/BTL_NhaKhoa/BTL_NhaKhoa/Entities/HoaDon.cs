namespace BTL_NhaKhoa.Entities;

public sealed class HoaDon
{
    public ulong MaHoaDon { get; set; }
    public ulong MaHoSoKham { get; set; }
    public DateTime NgayLap { get; set; }
    public decimal TongTien { get; set; }
    public string TrangThai { get; set; } = "CHUA_THANH_TOAN";
    public ulong NguoiLapTaiKhoanId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public HoSoKham HoSoKham { get; set; } = null!;
    public TaiKhoan NguoiLapTaiKhoan { get; set; } = null!;
    public ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>();
    public ICollection<ThanhToan> ThanhToans { get; set; } = new List<ThanhToan>();
}
