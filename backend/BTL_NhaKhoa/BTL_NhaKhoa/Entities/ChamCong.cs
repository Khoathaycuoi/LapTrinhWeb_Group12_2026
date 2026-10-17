namespace BTL_NhaKhoa.Entities;

public sealed class ChamCong
{
    public ulong MaChamCong { get; set; }
    public ulong MaNhanVien { get; set; }
    public DateOnly NgayChamCong { get; set; }
    public DateTime? GioVaoSang { get; set; }
    public DateTime? GioRaSang { get; set; }
    public DateTime? GioVaoChieu { get; set; }
    public DateTime? GioRaChieu { get; set; }
    public string TinhTrangSang { get; set; } = "CHUA_XAC_DINH";
    public string TinhTrangChieu { get; set; } = "CHUA_XAC_DINH";
    public string? LyDoSang { get; set; }
    public string? LyDoChieu { get; set; }
    public string TrangThaiDuyet { get; set; } = "CHUA_HOAN_TAT";
    public ulong? NguoiDuyetTaiKhoanId { get; set; }
    public DateTime? DuyetLuc { get; set; }
    public ulong? NguoiTuChoiTaiKhoanId { get; set; }
    public DateTime? TuChoiLuc { get; set; }
    public string? LyDoTuChoi { get; set; }
    public NhanVien NhanVien { get; set; } = null!;
    public TaiKhoan? NguoiDuyetTaiKhoan { get; set; }
    public TaiKhoan? NguoiTuChoiTaiKhoan { get; set; }
    public ICollection<LichSuChinhSuaChamCong> LichSuChinhSuas { get; set; } = new List<LichSuChinhSuaChamCong>();
}
