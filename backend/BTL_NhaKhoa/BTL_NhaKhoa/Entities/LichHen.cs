namespace BTL_NhaKhoa.Entities;

public sealed class LichHen
{
    public ulong MaLichHen { get; set; }
    public ulong MaKhachHang { get; set; }
    public ulong? MaBacSi { get; set; }
    public ulong? MaPhong { get; set; }
    public ulong NguoiTaoTaiKhoanId { get; set; }
    public ulong? NguoiXacNhanTaiKhoanId { get; set; }
    public ulong? NguoiHuyTaiKhoanId { get; set; }
    public ulong? NguoiTiepNhanTaiKhoanId { get; set; }
    public ulong? NguoiDanhDauKhongDenTaiKhoanId { get; set; }
    public DateTime ThoiGianBatDau { get; set; }
    public ushort ThoiLuongDuKienPhut { get; set; }
    public string TrangThai { get; set; } = "CHO_XAC_NHAN";
    public string? GhiChu { get; set; }
    public string? LyDoHuy { get; set; }
    public string? LyDoTuChoi { get; set; }
    public DateTime? XacNhanLuc { get; set; }
    public DateTime? DenLuc { get; set; }
    public DateTime? BatDauKhamLuc { get; set; }
    public DateTime? HoanThanhLuc { get; set; }
    public DateTime? KhongDenLuc { get; set; }
    public string? GhiChuKhongDen { get; set; }
    public DateTime? HuyLuc { get; set; }
    public DateTime? TuChoiLuc { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public KhachHang KhachHang { get; set; } = null!;
    public BacSi? BacSi { get; set; }
    public Phong? Phong { get; set; }
    public TaiKhoan NguoiTaoTaiKhoan { get; set; } = null!;
    public TaiKhoan? NguoiXacNhanTaiKhoan { get; set; }
    public TaiKhoan? NguoiHuyTaiKhoan { get; set; }
    public TaiKhoan? NguoiTiepNhanTaiKhoan { get; set; }
    public TaiKhoan? NguoiDanhDauKhongDenTaiKhoan { get; set; }
    public ICollection<ChiTietLichHen> ChiTietLichHens { get; set; } = new List<ChiTietLichHen>();
    public HoSoKham? HoSoKham { get; set; }
}
