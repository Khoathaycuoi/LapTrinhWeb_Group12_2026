namespace BTL_NhaKhoa.Entities;

public sealed class NhanVien
{
    public ulong MaNhanVien { get; set; }
    public ulong MaConNguoi { get; set; }
    public DateOnly? NgayVaoLam { get; set; }
    public string TrangThai { get; set; } = "DANG_LAM_VIEC";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ConNguoi ConNguoi { get; set; } = null!;
    public BacSi? BacSi { get; set; }
    public ICollection<ChamCong> ChamCongs { get; set; } = new List<ChamCong>();
}
