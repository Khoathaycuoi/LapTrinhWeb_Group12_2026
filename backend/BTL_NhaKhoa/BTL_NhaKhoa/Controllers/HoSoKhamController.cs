using BTL_NhaKhoa.DTOs;
using BTL_NhaKhoa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BTL_NhaKhoa.Controllers;

[ApiController]
[Authorize]
[Route("api/ho-so-kham")]
public sealed class HoSoKhamController(QuanLyKhamBenhService quanLyKhamBenhService) : ControllerBase
{
    [HttpGet("cua-toi")]
    [Authorize(Roles = "BAC_SI,KHACH_HANG")]
    public async Task<IActionResult> LayCuaToi([FromQuery] PhanTrangRequest? phanTrang = null)
    {
        if (!ThuDocTaiKhoan(out var maTaiKhoan)) return Unauthorized();
        if (phanTrang?.CoPhanTrang == true)
        {
            if (!phanTrang.HopLe) return BadRequest("Trang hoặc kích thước trang không hợp lệ.");
            return Ok(await quanLyKhamBenhService.LayTrangCuaToiAsync(maTaiKhoan,
                phanTrang.SoTrang, phanTrang.KichThuocTrang));
        }
        return Ok(await quanLyKhamBenhService.LayCuaToiAsync(maTaiKhoan));
    }

    [HttpGet("{maHoSoKham}")]
    [Authorize(Roles = "BAC_SI,KHACH_HANG")]
    public async Task<IActionResult> LayChiTiet(string maHoSoKham)
    {
        if (!ThuDocTaiKhoan(out var maTaiKhoan)) return Unauthorized();
        var (ketQua, hoSo) = await quanLyKhamBenhService.LayChiTietAsync(maHoSoKham, maTaiKhoan);
        return ketQua.Loi != null ? Loi(ketQua) : Ok(hoSo);
    }

    [HttpPut("{maHoSoKham}")]
    [Authorize(Roles = "BAC_SI")]
    public async Task<IActionResult> CapNhat(string maHoSoKham, [FromBody] CapNhatHoSoKhamRequest yeuCau)
    {
        if (!ThuDocTaiKhoan(out var maTaiKhoan)) return Unauthorized();
        var ketQua = await quanLyKhamBenhService.CapNhatAsync(maHoSoKham, yeuCau, maTaiKhoan);
        return ketQua.Loi != null ? Loi(ketQua) : NoContent();
    }

    [HttpPost("{maHoSoKham}/dich-vu-thuc-hien")]
    [Authorize(Roles = "BAC_SI")]
    public async Task<IActionResult> GhiNhanDichVu(string maHoSoKham,
        [FromBody] GhiNhanDichVuThucHienRequest yeuCau)
    {
        if (!ThuDocTaiKhoan(out var maTaiKhoan)) return Unauthorized();
        var ketQua = await quanLyKhamBenhService.GhiNhanDichVuAsync(maHoSoKham, yeuCau, maTaiKhoan);
        return ketQua.Loi != null ? Loi(ketQua) : NoContent();
    }

    [HttpPost("{maHoSoKham}/don-thuoc")]
    [Authorize(Roles = "BAC_SI")]
    public async Task<IActionResult> KeDon(string maHoSoKham, [FromBody] KeDonThuocRequest yeuCau)
    {
        if (!ThuDocTaiKhoan(out var maTaiKhoan)) return Unauthorized();
        var ketQua = await quanLyKhamBenhService.KeDonAsync(maHoSoKham, yeuCau, maTaiKhoan);
        return ketQua.Loi != null ? Loi(ketQua) : Ok(new { ketQua.MaDonThuoc });
    }

    [HttpDelete("{maHoSoKham}/dich-vu-thuc-hien/{maDichVu}")]
    [Authorize(Roles = "BAC_SI")]
    public async Task<IActionResult> BoDichVuThucHien(string maHoSoKham, string maDichVu)
    {
        if (!ThuDocTaiKhoan(out var maTaiKhoan)) return Unauthorized();
        var ketQua = await quanLyKhamBenhService.BoDichVuThucHienAsync(
            maHoSoKham, maDichVu, maTaiKhoan);
        return ketQua.Loi != null ? Loi(ketQua) : NoContent();
    }

    [HttpDelete("{maHoSoKham}/don-thuoc/{maDonThuoc}")]
    [Authorize(Roles = "BAC_SI")]
    public async Task<IActionResult> BoDonThuoc(string maHoSoKham, string maDonThuoc)
    {
        if (!ThuDocTaiKhoan(out var maTaiKhoan)) return Unauthorized();
        var ketQua = await quanLyKhamBenhService.BoDonThuocAsync(
            maHoSoKham, maDonThuoc, maTaiKhoan);
        return ketQua.Loi != null ? Loi(ketQua) : NoContent();
    }

    [HttpPut("{maHoSoKham}/chot")]
    [Authorize(Roles = "BAC_SI")]
    public async Task<IActionResult> Chot(string maHoSoKham)
    {
        if (!ThuDocTaiKhoan(out var maTaiKhoan)) return Unauthorized();
        var ketQua = await quanLyKhamBenhService.ChotAsync(maHoSoKham, maTaiKhoan);
        return ketQua.Loi != null ? Loi(ketQua) : NoContent();
    }

    private bool ThuDocTaiKhoan(out ulong maTaiKhoan) =>
        ulong.TryParse(User.FindFirstValue("ma_tai_khoan"), out maTaiKhoan);

    private IActionResult Loi(KetQuaKhamBenh ketQua) => ketQua.KhongTimThay
        ? NotFound(ketQua.Loi)
        : ketQua.KhongCoQuyen ? StatusCode(403, ketQua.Loi) : BadRequest(ketQua.Loi);
}
