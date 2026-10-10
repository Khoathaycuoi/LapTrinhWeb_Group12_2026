namespace BTL_NhaKhoa.Entities;

public sealed class LichLamViecBacSi
{
    public ulong MaLichLamViec { get; set; }
    public ulong MaBacSi { get; set; }
    public byte ThuTrongTuan { get; set; }
    public TimeOnly GioBatDau { get; set; }
    public TimeOnly GioKetThuc { get; set; }
    public DateOnly NgayHieuLucTu { get; set; }
    public DateOnly? NgayHieuLucDen { get; set; }
    public string TrangThai { get; set; } = "HOAT_DONG";
    public BacSi BacSi { get; set; } = null!;
}
