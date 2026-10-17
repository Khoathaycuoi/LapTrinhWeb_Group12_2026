namespace BTL_NhaKhoa.Entities;

public sealed class LichSuChinhSuaChamCong
{
    public ulong MaLichSu { get; set; }
    public ulong MaChamCong { get; set; }
    public ulong NguoiSuaTaiKhoanId { get; set; }
    public string Buoi { get; set; } = "CA_NGAY";
    public string? TinhTrangCu { get; set; }
    public string? TinhTrangMoi { get; set; }
    public DateTime? GioVaoCu { get; set; }
    public DateTime? GioRaCu { get; set; }
    public DateTime? GioVaoMoi { get; set; }
    public DateTime? GioRaMoi { get; set; }
    public string LyDo { get; set; } = null!;
    public DateTime ThoiDiemSua { get; set; }
    public ChamCong ChamCong { get; set; } = null!;
    public TaiKhoan NguoiSuaTaiKhoan { get; set; } = null!;
}
