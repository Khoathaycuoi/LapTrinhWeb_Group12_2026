namespace BTL_NhaKhoa.Entities;

public sealed class ThanhToan
{
    public ulong MaThanhToan { get; set; }
    public ulong MaHoaDon { get; set; }
    public ulong NguoiXacNhanTaiKhoanId { get; set; }
    public decimal SoTien { get; set; }
    public string PhuongThuc { get; set; } = null!;
    public string TrangThai { get; set; } = "DA_XAC_NHAN";
    public DateTime ThoiDiemThanhToan { get; set; }
    public string? LyDoHuy { get; set; }
    public string? GhiChu { get; set; }
    public HoaDon HoaDon { get; set; } = null!;
    public TaiKhoan NguoiXacNhanTaiKhoan { get; set; } = null!;
}
