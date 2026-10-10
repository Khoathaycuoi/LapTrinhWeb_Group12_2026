namespace BTL_NhaKhoa.Entities;

public sealed class BaoTri
{
    public ulong MaBaoTri { get; set; }
    public ulong? NguoiBaoTaiKhoanId { get; set; }
    public ulong? NguoiXuLyTaiKhoanId { get; set; }
    public DateTime NgayBao { get; set; }
    public DateTime? NgayHoanTat { get; set; }
    public string NoiDung { get; set; } = null!;
    public decimal? ChiPhi { get; set; }
    public string TrangThai { get; set; } = "MOI_TIEP_NHAN";
    public TaiKhoan? NguoiBaoTaiKhoan { get; set; }
    public TaiKhoan? NguoiXuLyTaiKhoan { get; set; }
    public ICollection<ThucHienBaoTri> ThucHienBaoTris { get; set; } = new List<ThucHienBaoTri>();
}
