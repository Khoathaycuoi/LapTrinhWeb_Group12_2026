using BTL_NhaKhoa.DTOs;
using BTL_NhaKhoa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTL_NhaKhoa.Controllers;

[ApiController]
[Authorize(Roles = "QUAN_LY,LE_TAN,BAC_SI")]
[Route("api/phong")]
public sealed class PhongController(QuanLyPhongService quanLyPhongService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> LayDanhSach() => Ok(await quanLyPhongService.LayDanhSachAsync());

    [HttpGet("{maPhong}/lich-su")]
    [Authorize(Roles = "QUAN_LY,LE_TAN")]
    public async Task<IActionResult> LayLichSu(string maPhong, [FromQuery] DateOnly? tuNgay,
        [FromQuery] DateOnly? denNgay)
    {
        var (loi, khongTimThay, duLieu) = await quanLyPhongService.LayLichSuAsync(
            maPhong, tuNgay, denNgay);
        if (loi != null) return khongTimThay ? NotFound(loi) : BadRequest(loi);
        return Ok(duLieu);
    }

    [HttpPost]
    [Authorize(Roles = "QUAN_LY")]
    public async Task<IActionResult> Them([FromBody] ThemPhongRequest yeuCau)
    {
        var ketQua = await quanLyPhongService.ThemAsync(yeuCau);
        return ketQua.Loi != null ? BadRequest(ketQua.Loi) : Ok(new { ketQua.MaPhong });
    }

    [HttpPut("{maPhong}/trang-thai")]
    [Authorize(Roles = "QUAN_LY")]
    public async Task<IActionResult> DoiTrangThai(string maPhong,
        [FromBody] CapNhatTrangThaiPhongRequest yeuCau)
    {
        var loi = await quanLyPhongService.DoiTrangThaiAsync(maPhong, yeuCau.TrangThai);
        return loi == null ? NoContent() : BadRequest(loi);
    }
}
