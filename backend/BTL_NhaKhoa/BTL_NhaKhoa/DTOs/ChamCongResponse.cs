namespace BTL_NhaKhoa.DTOs;

public sealed class ChamCongResponse
{
    public string MaChamCong { get; set; } = string.Empty;
    public string MaNhanVien { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public DateOnly NgayChamCong { get; set; }
    public DateTime? GioVaoSang { get; set; }
    public DateTime? GioRaSang { get; set; }
    public DateTime? GioVaoChieu { get; set; }
    public DateTime? GioRaChieu { get; set; }
    public string TinhTrangSang { get; set; } = string.Empty;
    public string TinhTrangChieu { get; set; } = string.Empty;
    public string? LyDoSang { get; set; }
    public string? LyDoChieu { get; set; }
    public int SoPhutTreSang { get; set; }
    public int SoPhutTreChieu { get; set; }
    public int SoPhutVeSomSang { get; set; }
    public int SoPhutVeSomChieu { get; set; }
    public int TongPhutLamViec { get; set; }
    public string TrangThaiDuyet { get; set; } = string.Empty;
    public string? NguoiDuyet { get; set; }
    public DateTime? DuyetLuc { get; set; }
    public string? NguoiTuChoi { get; set; }
    public DateTime? TuChoiLuc { get; set; }
    public string? LyDoTuChoi { get; set; }
    public List<LichSuChinhSuaChamCongResponse> LichSuChinhSua { get; set; } = [];
}
