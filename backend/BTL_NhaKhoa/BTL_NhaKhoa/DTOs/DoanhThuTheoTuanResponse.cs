namespace BTL_NhaKhoa.DTOs;

public sealed class DoanhThuTheoTuanResponse
{
    public DateOnly TuanBatDau { get; init; }
    public DateOnly TuanKetThuc { get; init; }
    public decimal SoTien { get; init; }
}
