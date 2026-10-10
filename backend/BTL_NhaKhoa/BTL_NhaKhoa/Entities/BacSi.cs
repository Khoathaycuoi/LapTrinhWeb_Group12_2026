namespace BTL_NhaKhoa.Entities;

public sealed class BacSi
{
    public ulong MaBacSi { get; set; }
    public ulong MaNhanVien { get; set; }
    public string? ChungChi { get; set; }
    public string? BangCap { get; set; }
    public ushort? SoNamKinhNghiem { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public NhanVien NhanVien { get; set; } = null!;
    public ICollection<LichHen> LichHens { get; set; } = new List<LichHen>();
    public ICollection<LichLamViecBacSi> LichLamViecs { get; set; } = new List<LichLamViecBacSi>();
    public ICollection<HoSoKham> HoSoKhams { get; set; } = new List<HoSoKham>();
}
