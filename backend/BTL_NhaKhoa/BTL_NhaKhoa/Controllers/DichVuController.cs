using BTL_NhaKhoa.DTOs;
using BTL_NhaKhoa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTL_NhaKhoa.Controllers;

[ApiController]
[Authorize]
[Route("api/dichvu")]
public sealed class DichVuController(QuanLyDichVuService quanLyDichVuService) : ControllerBase
{
    [HttpPost("add")]
    [Authorize(Roles = "QUAN_LY")]
    public async Task<IActionResult> Them([FromBody] ThemDichVuRequest yeuCau)
    {
        var ketQua = await quanLyDichVuService.ThemAsync(yeuCau);
        if (ketQua.Loi != null)
        {
            return ketQua.TrungDuLieu ? Conflict(ketQua.Loi) : BadRequest(ketQua.Loi);
        }
        return Content("Thêm dịch vụ thành công!", "text/plain");
    }

    [HttpGet("all")]
    public async Task<IActionResult> LayTatCa([FromQuery] PhanTrangRequest? phanTrang = null)
    {
        if (phanTrang?.CoPhanTrang == true)
        {
            if (!phanTrang.HopLe) return BadRequest("Trang hoặc kích thước trang không hợp lệ.");
            return Ok(await quanLyDichVuService.LayTrangTatCaAsync(
                phanTrang.SoTrang, phanTrang.KichThuocTrang));
        }
        var danhSach = await quanLyDichVuService.LayTatCaAsync();
        return danhSach.Count == 0 ? NotFound("Không tìm thấy danh sách dịch vụ") : Ok(danhSach);
    }

    [HttpPut("dv")]
    [Authorize(Roles = "QUAN_LY")]
    public async Task<IActionResult> CapNhat([FromBody] CapNhatDichVuRequest yeuCau)
    {
        var ketQua = await quanLyDichVuService.CapNhatAsync(yeuCau);
        if (ketQua.Loi != null)
        {
            return ketQua.KhongTimThay ? NotFound(ketQua.Loi) : BadRequest(ketQua.Loi);
        }
        return Content("Cập nhật thông tin thành công!", "text/plain");
    }

    [HttpPut("{maDichVu}/xoa")]
    [Authorize(Roles = "QUAN_LY")]
    public async Task<IActionResult> VoHieuHoa(string maDichVu)
    {
        return await DoiTrangThai(maDichVu, true);
    }

    [HttpPut("{maDichVu}/khoi-phuc")]
    [Authorize(Roles = "QUAN_LY")]
    public async Task<IActionResult> KhoiPhuc(string maDichVu)
    {
        return await DoiTrangThai(maDichVu, false);
    }

    [HttpGet("search-all")]
    public async Task<IActionResult> TimKiem(
        [FromQuery] string? ten, [FromQuery] string? tenDichVu,
        [FromQuery] string? status, [FromQuery] string? trangThai,
        [FromQuery] decimal? min, [FromQuery] decimal? max,
        [FromQuery] PhanTrangRequest? phanTrang = null)
    {
        var trangThaiCanTim = !string.IsNullOrWhiteSpace(status) ? status : trangThai;
        if (!QuanLyDichVuService.TrangThaiHopLe(trangThaiCanTim))
        {
            return BadRequest("Trạng thái dịch vụ không hợp lệ.");
        }
        if (min < 0 || max < 0 || (min.HasValue && max.HasValue && min > max))
        {
            return BadRequest("Khoảng đơn giá không hợp lệ.");
        }
        if (phanTrang?.CoPhanTrang == true)
        {
            if (!phanTrang.HopLe) return BadRequest("Trang hoặc kích thước trang không hợp lệ.");
            return Ok(await quanLyDichVuService.TimKiemTrangAsync(
                !string.IsNullOrWhiteSpace(ten) ? ten : tenDichVu, trangThaiCanTim,
                min, max, phanTrang.SoTrang, phanTrang.KichThuocTrang));
        }
        return Ok(await quanLyDichVuService.TimKiemAsync(
            !string.IsNullOrWhiteSpace(ten) ? ten : tenDichVu, trangThaiCanTim, min, max));
    }

    private async Task<IActionResult> DoiTrangThai(string maDichVu, bool voHieuHoa)
    {
        var ketQua = await quanLyDichVuService.DoiTrangThaiAsync(maDichVu, voHieuHoa);
        if (ketQua.Loi != null)
        {
            return ketQua.KhongTimThay ? NotFound(ketQua.Loi) : BadRequest(ketQua.Loi);
        }
        return Content(voHieuHoa
            ? "Đã vô hiệu hóa dịch vụ thành công!"
            : "Đã khôi phục dịch vụ thành công!", "text/plain");
    }
}
