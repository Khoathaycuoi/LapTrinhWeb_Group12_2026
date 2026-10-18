namespace BTL_NhaKhoa.DTOs;

public sealed class PhanTrangRequest
{
    public int? Page { get; set; }
    public int? PageSize { get; set; }

    public bool CoPhanTrang => Page.HasValue || PageSize.HasValue;
    public int SoTrang => Page ?? 1;
    public int KichThuocTrang => PageSize ?? 20;
    public bool HopLe => SoTrang >= 1 && KichThuocTrang is >= 1 and <= 50 &&
        (long)(SoTrang - 1) * KichThuocTrang <= int.MaxValue;
}
