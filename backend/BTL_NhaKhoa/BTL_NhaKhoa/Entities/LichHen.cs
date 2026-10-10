namespace BTL_NhaKhoa.Entities;

public sealed class LichHen
{
    public ulong MaLichHen { get; set; }
    public ulong MaKhachHang { get; set; }
    public ulong? MaBacSi { get; set; }
    public ulong? MaPhong { get; set; }
    public ulong NguoiTaoTaiKhoanId { get; set; }
    public ulong? NguoiXacNhanTaiKhoanId { get; set; }
    public DateTime ThoiGianBatDau { get; set; }
    public ushort ThoiLuongDuKienPhut { get; set; }
    public string TrangThai { get; set; } = "CHO_XAC_NHAN";
    public string? GhiChu { get; set; }
    public string? LyDoHuy { get; set; }
    public DateTime? XacNhanLuc { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public KhachHang KhachHang { get; set; } = null!;
    public BacSi? BacSi { get; set; }
    public Phong? Phong { get; set; }
    public TaiKhoan NguoiTaoTaiKhoan { get; set; } = null!;
    public TaiKhoan? NguoiXacNhanTaiKhoan { get; set; }
    public ICollection<ChiTietLichHen> ChiTietLichHens { get; set; } = new List<ChiTietLichHen>();
    public HoSoKham? HoSoKham { get; set; }
}
