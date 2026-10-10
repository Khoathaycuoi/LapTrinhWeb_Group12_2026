namespace BTL_NhaKhoa.Entities;

public sealed class DichVu
{
    public ulong MaDichVu { get; set; }
    public string TenDichVu { get; set; } = null!;
    public string? MoTa { get; set; }
    public decimal DonGia { get; set; }
    public ushort ThoiLuongDuKienPhut { get; set; }
    public string TrangThai { get; set; } = "DANG_HOAT_DONG";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<ChiTietLichHen> ChiTietLichHens { get; set; } = new List<ChiTietLichHen>();
    public ICollection<DichVuThucHien> DichVuThucHiens { get; set; } = new List<DichVuThucHien>();
}
