using System.Text.Json.Serialization;

namespace BTL_NhaKhoa.DTOs;

public sealed record DanhSachPhanTrangResponse<T>(
    [property: JsonPropertyName("items")] IReadOnlyList<T> Items,
    [property: JsonPropertyName("totalItems")] int TotalItems,
    [property: JsonPropertyName("page")] int Page,
    [property: JsonPropertyName("pageSize")] int PageSize);
