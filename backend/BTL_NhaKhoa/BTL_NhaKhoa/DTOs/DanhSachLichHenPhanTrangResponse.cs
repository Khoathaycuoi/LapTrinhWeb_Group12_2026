using System.Text.Json.Serialization;

namespace BTL_NhaKhoa.DTOs;

public sealed record DanhSachLichHenPhanTrangResponse(
    [property: JsonPropertyName("items")] IReadOnlyList<LichHenResponse> Items,
    [property: JsonPropertyName("totalItems")] int TotalItems,
    [property: JsonPropertyName("page")] int Page,
    [property: JsonPropertyName("pageSize")] int PageSize);
