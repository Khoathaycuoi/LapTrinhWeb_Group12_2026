namespace BTL_NhaKhoa.Entities;

public sealed class TaiKhoan
{
    public ulong MaTaiKhoan { get; set; }
    public ulong MaConNguoi { get; set; }
    public string MaVaiTro { get; set; } = null!;
    public string MatKhauHash { get; set; } = null!;
    public string TrangThai { get; set; } = "HOAT_DONG";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ConNguoi ConNguoi { get; set; } = null!;
    public VaiTro VaiTro { get; set; } = null!;
    public ICollection<LichHen> LichHenDaTao { get; set; } = new List<LichHen>();
    public ICollection<LichHen> LichHenDaXacNhan { get; set; } = new List<LichHen>();
    public ICollection<LichHen> LichHenDaHuy { get; set; } = new List<LichHen>();
    public ICollection<LichHen> LichHenDaTiepNhan { get; set; } = new List<LichHen>();
    public ICollection<LichHen> LichHenDaDanhDauKhongDen { get; set; } = new List<LichHen>();
    public ICollection<HoaDon> HoaDonDaLap { get; set; } = new List<HoaDon>();
    public ICollection<ThanhToan> ThanhToanDaXacNhan { get; set; } = new List<ThanhToan>();
    public ICollection<ThanhToan> ThanhToanDaHuy { get; set; } = new List<ThanhToan>();
    public ICollection<ChamCong> ChamCongDaDuyet { get; set; } = new List<ChamCong>();
    public ICollection<ChamCong> ChamCongDaTuChoi { get; set; } = new List<ChamCong>();
    public ICollection<LichSuChinhSuaChamCong> LichSuDaSua { get; set; } = new List<LichSuChinhSuaChamCong>();
}
