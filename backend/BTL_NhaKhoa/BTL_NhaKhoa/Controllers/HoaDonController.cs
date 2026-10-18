using BTL_NhaKhoa.DTOs;
using BTL_NhaKhoa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BTL_NhaKhoa.Controllers;

[ApiController]
[Authorize]
[Route("api/hoa-don")]
public sealed class HoaDonController(QuanLyHoaDonService quanLyHoaDonService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "LE_TAN,QUAN_LY")]
    public async Task<IActionResult> Lap([FromBody] LapHoaDonRequest yeuCau)
    {
        if (!ThuDocTaiKhoan(out var maTaiKhoan)) return Unauthorized();
        var ketQua = await quanLyHoaDonService.LapAsync(yeuCau, maTaiKhoan);
        return ketQua.Loi != null ? Loi(ketQua) : Ok(new { ketQua.MaHoaDon });
    }

    [HttpGet]
    [Authorize(Roles = "KHACH_HANG,LE_TAN,QUAN_LY")]
    public async Task<IActionResult> LayDanhSach([FromQuery] string? maKhachHang,
        [FromQuery] bool conNo = false, [FromQuery] DateOnly? tuNgay = null,
        [FromQuery] DateOnly? denNgay = null, [FromQuery] string? trangThai = null,
        [FromQuery] PhanTrangRequest? phanTrang = null)
    {
        if (!ThuDocTaiKhoan(out var maTaiKhoan)) return Unauthorized();
        if (tuNgay.HasValue && denNgay.HasValue && tuNgay.Value > denNgay.Value)
            return BadRequest("Từ ngày không được sau đến ngày.");
        if (trangThai != null && trangThai is not ("CHUA_THANH_TOAN" or
                "THANH_TOAN_MOT_PHAN" or "DA_THANH_TOAN" or "DA_HUY"))
            return BadRequest("Trạng thái hóa đơn không hợp lệ.");
        if (conNo && trangThai != null)
            return BadRequest("Không thể kết hợp conNo và trangThai.");
        if (phanTrang?.CoPhanTrang == true)
        {
            if (!phanTrang.HopLe) return BadRequest("Trang hoặc kích thước trang không hợp lệ.");
            return Ok(await quanLyHoaDonService.LayTrangAsync(maTaiKhoan, maKhachHang,
                conNo, tuNgay, denNgay, trangThai, phanTrang.SoTrang, phanTrang.KichThuocTrang));
        }
        return Ok(await quanLyHoaDonService.LayDanhSachAsync(maTaiKhoan, maKhachHang,
            conNo, tuNgay, denNgay, trangThai));
    }

    [HttpGet("cho-lap")]
    [Authorize(Roles = "LE_TAN,QUAN_LY")]
    public async Task<IActionResult> LayHoSoChoLap([FromQuery] PhanTrangRequest? phanTrang = null)
    {
        var soTrang = phanTrang?.SoTrang ?? 1;
        var kichThuocTrang = phanTrang?.KichThuocTrang ?? 20;
        if (phanTrang?.CoPhanTrang == true && !phanTrang.HopLe)
            return BadRequest("Trang hoặc kích thước trang không hợp lệ.");
        return Ok(await quanLyHoaDonService.LayHoSoChoLapAsync(soTrang, kichThuocTrang));
    }

    [HttpGet("{maHoaDon}")]
    [Authorize(Roles = "KHACH_HANG,LE_TAN,QUAN_LY")]
    public async Task<IActionResult> LayChiTiet(string maHoaDon)
    {
        if (!ThuDocTaiKhoan(out var maTaiKhoan)) return Unauthorized();
        var (ketQua, duLieu) = await quanLyHoaDonService.LayChiTietAsync(maHoaDon, maTaiKhoan);
        return ketQua.Loi != null ? Loi(ketQua) : Ok(duLieu);
    }

    [HttpPost("{maHoaDon}/thanh-toan")]
    [Authorize(Roles = "LE_TAN,QUAN_LY")]
    public async Task<IActionResult> GhiNhanThanhToan(
        string maHoaDon, [FromBody] GhiNhanThanhToanRequest yeuCau)
    {
        if (!ThuDocTaiKhoan(out var maTaiKhoan)) return Unauthorized();
        var ketQua = await quanLyHoaDonService.GhiNhanThanhToanAsync(maHoaDon, yeuCau, maTaiKhoan);
        return ketQua.Loi != null ? Loi(ketQua) : Ok(new { ketQua.MaThanhToan });
    }

    [HttpPut("{maHoaDon}/thanh-toan/{maThanhToan}/huy")]
    [Authorize(Roles = "QUAN_LY")]
    public async Task<IActionResult> HuyThanhToan(
        string maHoaDon, string maThanhToan, [FromBody] HuyThanhToanRequest yeuCau)
    {
        if (!ThuDocTaiKhoan(out var maTaiKhoan)) return Unauthorized();
        var ketQua = await quanLyHoaDonService.HuyThanhToanAsync(
            maHoaDon, maThanhToan, yeuCau, maTaiKhoan);
        return ketQua.Loi != null ? Loi(ketQua) : NoContent();
    }

    private bool ThuDocTaiKhoan(out ulong maTaiKhoan) =>
        ulong.TryParse(User.FindFirstValue("ma_tai_khoan"), out maTaiKhoan);

    private IActionResult Loi(KetQuaHoaDon ketQua) => ketQua.KhongTimThay
        ? NotFound(ketQua.Loi)
        : ketQua.KhongCoQuyen ? StatusCode(403, ketQua.Loi) : BadRequest(ketQua.Loi);
}
