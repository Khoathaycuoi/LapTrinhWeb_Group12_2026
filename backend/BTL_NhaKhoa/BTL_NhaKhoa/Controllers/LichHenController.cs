using BTL_NhaKhoa.DTOs;
using BTL_NhaKhoa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BTL_NhaKhoa.Controllers;

[ApiController]
[Authorize]
[Route("api/lich-hen")]
public sealed class LichHenController(HuyLichHenService huyLichHenService,
    QuanLyLichHenService quanLyLichHenService,
    QuanLyKhamBenhService quanLyKhamBenhService) : ControllerBase
{
    [HttpPost("dat-lich")]
    [Authorize(Roles = "KHACH_HANG,LE_TAN,QUAN_LY")]
    public async Task<IActionResult> DatLich([FromBody] DatLichRequest yeuCau)
    {
        if (!ThuDocTaiKhoan(out var maTaiKhoan)) return Unauthorized();
        var ketQua = await quanLyLichHenService.DatAsync(yeuCau, maTaiKhoan);
        if (ketQua.Loi != null) return Loi(ketQua);
        return Content($"Đặt lịch hẹn thành công! Mã lịch hẹn: {ketQua.MaLichHen}", "text/plain");
    }

    [HttpGet("all")]
    [Authorize(Roles = "KHACH_HANG,LE_TAN,QUAN_LY,BAC_SI")]
    public async Task<IActionResult> LayTatCa()
    {
        if (!ThuDocTaiKhoan(out var maTaiKhoan)) return Unauthorized();
        return Ok(await quanLyLichHenService.LayDanhSachAsync(maTaiKhoan));
    }

    [HttpGet("tim-kiem")]
    [Authorize(Roles = "KHACH_HANG,LE_TAN,QUAN_LY,BAC_SI")]
    public async Task<IActionResult> TimKiem([FromQuery] string? trangThai, [FromQuery] DateOnly? ngayHen,
        [FromQuery] string? maKH, [FromQuery] string? tenKH, [FromQuery] string? sdtKH,
        [FromQuery] string? maBacSi, [FromQuery] string? tenBacSi, [FromQuery] string? sdtBacSi)
    {
        if (!ThuDocTaiKhoan(out var maTaiKhoan)) return Unauthorized();
        if (!QuanLyLichHenService.TrangThaiHopLe(trangThai)) return BadRequest("Trạng thái lịch hẹn không hợp lệ.");
        return Ok(await quanLyLichHenService.LayDanhSachAsync(maTaiKhoan, trangThai, ngayHen,
            maKH, tenKH, sdtKH, maBacSi, tenBacSi, sdtBacSi));
    }

    [HttpGet("ca-trong")]
    [Authorize(Roles = "KHACH_HANG,LE_TAN,QUAN_LY,BAC_SI")]
    public async Task<IActionResult> LayCaTrong([FromQuery] string maBacSi, [FromQuery] DateOnly? ngay,
        [FromQuery] int thoiLuongPhut = 30)
    {
        if (!QuanLyLichHenService.ThuDocMa(maBacSi, "BS_", out var idBacSi) || !ngay.HasValue)
            return BadRequest("Mã bác sĩ và ngày phải hợp lệ.");
        if (thoiLuongPhut <= 0 || thoiLuongPhut > ushort.MaxValue)
            return BadRequest("Thời lượng khám không hợp lệ.");
        if (!await quanLyLichHenService.BacSiTonTaiAsync(idBacSi)) return NotFound("Không tìm thấy bác sĩ.");
        return Ok(await quanLyLichHenService.LayCaTrongAsync(idBacSi, ngay.Value, thoiLuongPhut));
    }

    [HttpPut("xac-nhan/{maLichHen}")]
    [Authorize(Roles = "LE_TAN,QUAN_LY,BAC_SI")]
    public async Task<IActionResult> XacNhan(string maLichHen, [FromQuery] string? maPhong)
    {
        if (!ThuDocTaiKhoan(out var maTaiKhoan)) return Unauthorized();
        var ketQua = await quanLyLichHenService.XacNhanAsync(maLichHen, maTaiKhoan, maPhong);
        return ketQua.Loi != null ? Loi(ketQua) : Content("Xác nhận lịch hẹn thành công!", "text/plain");
    }

    [HttpGet("phong-trong/{maLichHen}")]
    [Authorize(Roles = "LE_TAN,QUAN_LY,BAC_SI")]
    public async Task<IActionResult> LayPhongTrong(string maLichHen)
    {
        if (!ThuDocTaiKhoan(out var maTaiKhoan)) return Unauthorized();
        var (ketQua, phongs) = await quanLyLichHenService.LayPhongTrongAsync(maLichHen, maTaiKhoan);
        return ketQua.Loi != null ? Loi(ketQua) : Ok(phongs);
    }

    [HttpPut("gan-bac-si/{maLichHen}")]
    [Authorize(Roles = "LE_TAN,QUAN_LY")]
    public async Task<IActionResult> GanBacSi(string maLichHen, [FromBody] GanBacSiLichHenRequest yeuCau)
    {
        var ketQua = await quanLyLichHenService.GanBacSiAsync(maLichHen, yeuCau.MaBacSi);
        return ketQua.Loi != null ? Loi(ketQua) : Content("Đã gán bác sĩ cho lịch hẹn.", "text/plain");
    }

    [HttpPut("hoan-thanh/{maLichHen}")]
    [Authorize(Roles = "LE_TAN,QUAN_LY,BAC_SI")]
    public async Task<IActionResult> HoanThanh(string maLichHen)
    {
        if (!ThuDocTaiKhoan(out var maTaiKhoan)) return Unauthorized();
        var ketQua = await quanLyLichHenService.HoanThanhAsync(maLichHen, maTaiKhoan);
        return ketQua.Loi != null ? Loi(ketQua) : Content("Đã đánh dấu hoàn thành lịch hẹn!", "text/plain");
    }

    [HttpPut("tiep-nhan/{maLichHen}")]
    [Authorize(Roles = "LE_TAN,QUAN_LY")]
    public async Task<IActionResult> TiepNhan(string maLichHen)
    {
        if (!ThuDocTaiKhoan(out var maTaiKhoan)) return Unauthorized();
        var ketQua = await quanLyLichHenService.TiepNhanAsync(maLichHen, maTaiKhoan);
        return ketQua.Loi != null ? Loi(ketQua) : NoContent();
    }

    [HttpPut("khong-den/{maLichHen}")]
    [Authorize(Roles = "LE_TAN,QUAN_LY")]
    public async Task<IActionResult> DanhDauKhongDen(string maLichHen,
        [FromBody] DanhDauKhongDenRequest yeuCau)
    {
        if (!ThuDocTaiKhoan(out var maTaiKhoan)) return Unauthorized();
        var ketQua = await quanLyLichHenService.DanhDauKhongDenAsync(
            maLichHen, yeuCau.GhiChu, maTaiKhoan);
        return ketQua.Loi != null ? Loi(ketQua) : NoContent();
    }

    [HttpPut("bat-dau-kham/{maLichHen}")]
    [Authorize(Roles = "BAC_SI")]
    public async Task<IActionResult> BatDauKham(string maLichHen)
    {
        if (!ThuDocTaiKhoan(out var maTaiKhoan)) return Unauthorized();
        var ketQua = await quanLyKhamBenhService.BatDauAsync(maLichHen, maTaiKhoan);
        if (ketQua.Loi != null)
            return ketQua.KhongTimThay ? NotFound(ketQua.Loi)
                : ketQua.KhongCoQuyen ? StatusCode(403, ketQua.Loi) : BadRequest(ketQua.Loi);
        return Ok(new { ketQua.MaHoSoKham });
    }

    [HttpPut("huy-lich")]
    [Authorize(Roles = "KHACH_HANG,LE_TAN,QUAN_LY,BAC_SI")]
    public async Task<IActionResult> HuyLich([FromBody] HuyLichRequest yeuCau)
    {
        if (!ulong.TryParse(User.FindFirstValue("ma_tai_khoan"), out var maTaiKhoan))
        {
            return Unauthorized();
        }

        var ketQua = await huyLichHenService.HuyAsync(yeuCau, maTaiKhoan);
        if (ketQua.Loi != null)
        {
            if (ketQua.KhongTimThay) return NotFound(ketQua.Loi);
            if (ketQua.KhongCoQuyen) return StatusCode(403, ketQua.Loi);
            return BadRequest(ketQua.Loi);
        }
        return Content("Đã hủy lịch hẹn thành công!", "text/plain");
    }

    private bool ThuDocTaiKhoan(out ulong maTaiKhoan) =>
        ulong.TryParse(User.FindFirstValue("ma_tai_khoan"), out maTaiKhoan);

    private IActionResult Loi(KetQuaLichHen ketQua) => ketQua.KhongTimThay
        ? NotFound(ketQua.Loi)
        : ketQua.KhongCoQuyen ? StatusCode(403, ketQua.Loi) : BadRequest(ketQua.Loi);
}
