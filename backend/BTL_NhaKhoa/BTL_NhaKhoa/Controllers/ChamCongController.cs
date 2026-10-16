using BTL_NhaKhoa.DTOs;
using BTL_NhaKhoa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BTL_NhaKhoa.Controllers;

[ApiController]
[Authorize]
[Route("api/cham-cong")]
public sealed class ChamCongController(QuanLyChamCongService quanLyChamCongService) : ControllerBase
{
    [HttpPost("check-in/{buoi}")]
    [Authorize(Roles = "LE_TAN,BAC_SI")]
    public async Task<IActionResult> CheckIn(string buoi)
    {
        if (!ThuDocTaiKhoan(out var id)) return Unauthorized();
        return TraKetQua(await quanLyChamCongService.CheckInAsync(id, buoi));
    }

    [HttpPost("check-out/{buoi}")]
    [Authorize(Roles = "LE_TAN,BAC_SI")]
    public async Task<IActionResult> CheckOut(string buoi)
    {
        if (!ThuDocTaiKhoan(out var id)) return Unauthorized();
        return TraKetQua(await quanLyChamCongService.CheckOutAsync(id, buoi));
    }

    [HttpGet("hom-nay")]
    [Authorize(Roles = "LE_TAN,BAC_SI")]
    public async Task<IActionResult> LayHomNay()
    {
        if (!ThuDocTaiKhoan(out var id)) return Unauthorized();
        var duLieu = await quanLyChamCongService.LayHomNayAsync(id);
        return duLieu == null ? NotFound("Không tìm thấy hồ sơ nhân viên.") : Ok(duLieu);
    }

    [HttpGet("me")]
    [Authorize(Roles = "LE_TAN,BAC_SI")]
    public async Task<IActionResult> LayCuaToi([FromQuery] DateOnly? tuNgay, [FromQuery] DateOnly? denNgay)
    {
        if (!ThuDocTaiKhoan(out var id)) return Unauthorized();
        if (tuNgay > denNgay) return BadRequest("Từ ngày không được sau đến ngày.");
        return Ok(await quanLyChamCongService.LayCuaToiAsync(id, tuNgay, denNgay));
    }

    [HttpGet]
    [Authorize(Roles = "QUAN_LY")]
    public async Task<IActionResult> LayDanhSach([FromQuery] string? maNhanVien,
        [FromQuery] DateOnly? tuNgay, [FromQuery] DateOnly? denNgay,
        [FromQuery] string? trangThaiDuyet)
    {
        if (tuNgay > denNgay) return BadRequest("Từ ngày không được sau đến ngày.");
        return Ok(await quanLyChamCongService.LayDanhSachAsync(
            maNhanVien, tuNgay, denNgay, trangThaiDuyet));
    }

    [HttpGet("ngay/{ngay}")]
    [Authorize(Roles = "QUAN_LY")]
    public async Task<IActionResult> LayTongHopNgay(DateOnly ngay)
    {
        return Ok(await quanLyChamCongService.LayTongHopNgayAsync(ngay));
    }

    [HttpPost("thu-cong")]
    [Authorize(Roles = "QUAN_LY")]
    public async Task<IActionResult> ThemThuCong([FromBody] ThemChamCongThuCongRequest yeuCau)
    {
        if (!ThuDocTaiKhoan(out var id)) return Unauthorized();
        return TraKetQua(await quanLyChamCongService.ThemThuCongAsync(yeuCau, id));
    }

    [HttpGet("{maChamCong}")]
    [Authorize(Roles = "QUAN_LY")]
    public async Task<IActionResult> LayChiTiet(string maChamCong)
    {
        if (!ThuDocMa(maChamCong, out var id)) return BadRequest("Mã chấm công không hợp lệ.");
        return TraKetQua(await quanLyChamCongService.LayChiTietAsync(id));
    }

    [HttpPut("{maChamCong}/chinh-sua")]
    [Authorize(Roles = "QUAN_LY")]
    public async Task<IActionResult> ChinhSua(string maChamCong, [FromBody] ChinhSuaChamCongRequest yeuCau)
    {
        if (!ThuDocTaiKhoan(out var maTaiKhoan)) return Unauthorized();
        if (!ThuDocMa(maChamCong, out var id)) return BadRequest("Mã chấm công không hợp lệ.");
        return TraKetQua(await quanLyChamCongService.ChinhSuaAsync(id, yeuCau, maTaiKhoan));
    }

    [HttpPut("{maChamCong}/duyet")]
    [Authorize(Roles = "QUAN_LY")]
    public async Task<IActionResult> Duyet(string maChamCong)
    {
        if (!ThuDocTaiKhoan(out var maTaiKhoan)) return Unauthorized();
        if (!ThuDocMa(maChamCong, out var id)) return BadRequest("Mã chấm công không hợp lệ.");
        return TraKetQua(await quanLyChamCongService.DuyetAsync(id, maTaiKhoan));
    }

    [HttpPut("{maChamCong}/tu-choi")]
    [Authorize(Roles = "QUAN_LY")]
    public async Task<IActionResult> TuChoi(string maChamCong, [FromBody] TuChoiChamCongRequest yeuCau)
    {
        if (!ThuDocTaiKhoan(out var maTaiKhoan)) return Unauthorized();
        if (!ThuDocMa(maChamCong, out var id)) return BadRequest("Mã chấm công không hợp lệ.");
        return TraKetQua(await quanLyChamCongService.TuChoiAsync(id, yeuCau, maTaiKhoan));
    }

    private bool ThuDocTaiKhoan(out ulong id) =>
        ulong.TryParse(User.FindFirstValue("ma_tai_khoan"), out id);

    private static bool ThuDocMa(string ma, out ulong id) =>
        QuanLyLichHenService.ThuDocMa(ma, "CC_", out id);

    private IActionResult TraKetQua(KetQuaChamCong ketQua) => ketQua.Loi == null
        ? Ok(ketQua.DuLieu)
        : ketQua.KhongTimThay ? NotFound(ketQua.Loi)
        : ketQua.XungDot ? Conflict(ketQua.Loi) : BadRequest(ketQua.Loi);
}
