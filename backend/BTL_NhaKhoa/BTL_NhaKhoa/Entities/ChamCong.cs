namespace BTL_NhaKhoa.Entities;

public sealed class ChamCong
{
    public ulong MaChamCong { get; set; }
    public ulong MaNhanVien { get; set; }
    public DateOnly NgayChamCong { get; set; }
    public DateTime? GioVao { get; set; }
    public DateTime? GioRa { get; set; }
    public ushort SoPhutTre { get; set; }
    public string TrangThai { get; set; } = "DI_LAM";
    public string TrangThaiDuyet { get; set; } = "CHO_DUYET";
    public ulong? NguoiDuyetTaiKhoanId { get; set; }
    public DateTime? DuyetLuc { get; set; }
    public string? GhiChu { get; set; }
    public NhanVien NhanVien { get; set; } = null!;
    public TaiKhoan? NguoiDuyetTaiKhoan { get; set; }
    public ICollection<LichSuChinhSuaChamCong> LichSuChinhSuas { get; set; } = new List<LichSuChinhSuaChamCong>();
}
