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
    public async Task<IActionResult> LayTatCa([FromQuery] int? page, [FromQuery] int? pageSize,
        [FromQuery] string? nhom)
    {
        if (!ThuDocTaiKhoan(out var maTaiKhoan)) return Unauthorized();
        if (!QuanLyLichHenService.NhomHopLe(nhom)) return BadRequest("Nhóm lịch hẹn không hợp lệ.");
        if (page.HasValue || pageSize.HasValue)
        {
            var soTrang = page ?? 1;
            var kichThuocTrang = pageSize ?? 20;
            if (!PhanTrangHopLe(soTrang, kichThuocTrang)) return BadRequest("Trang hoặc kích thước trang không hợp lệ (page >= 1, pageSize từ 1 đến 50).");
            return Ok(await quanLyLichHenService.LayTrangAsync(maTaiKhoan, soTrang, kichThuocTrang,
                nhom: nhom));
        }
        return Ok(await quanLyLichHenService.LayDanhSachAsync(maTaiKhoan, nhom: nhom));
    }

    [HttpGet("tim-kiem")]
    [Authorize(Roles = "KHACH_HANG,LE_TAN,QUAN_LY,BAC_SI")]
    public async Task<IActionResult> TimKiem([FromQuery] string? trangThai, [FromQuery] DateOnly? ngayHen,
        [FromQuery] string? maKH, [FromQuery] string? tenKH, [FromQuery] string? sdtKH,
        [FromQuery] string? maBacSi, [FromQuery] string? tenBacSi, [FromQuery] string? sdtBacSi,
        [FromQuery] int? page, [FromQuery] int? pageSize, [FromQuery] string? nhom)
    {
        if (!ThuDocTaiKhoan(out var maTaiKhoan)) return Unauthorized();
        if (!QuanLyLichHenService.TrangThaiHopLe(trangThai)) return BadRequest("Trạng thái lịch hẹn không hợp lệ.");
        if (!QuanLyLichHenService.NhomHopLe(nhom)) return BadRequest("Nhóm lịch hẹn không hợp lệ.");
        if (page.HasValue || pageSize.HasValue)
        {
            var soTrang = page ?? 1;
            var kichThuocTrang = pageSize ?? 20;
            if (!PhanTrangHopLe(soTrang, kichThuocTrang)) return BadRequest("Trang hoặc kích thước trang không hợp lệ (page >= 1, pageSize từ 1 đến 50).");
            return Ok(await quanLyLichHenService.LayTrangAsync(maTaiKhoan, soTrang, kichThuocTrang,
                trangThai, ngayHen, maKH, tenKH, sdtKH, maBacSi, tenBacSi, sdtBacSi, nhom));
        }
        return Ok(await quanLyLichHenService.LayDanhSachAsync(maTaiKhoan, trangThai, ngayHen,
            maKH, tenKH, sdtKH, maBacSi, tenBacSi, sdtBacSi, nhom));
    }

    [HttpGet("ca-trong")]
    [Authorize(Roles = "KHACH_HANG,LE_TAN,QUAN_LY,BAC_SI")]
    public async Task<IActionResult> LayCaTrong([FromQuery] string maBacSi, [FromQuery] DateOnly? ngay,
        [FromQuery] int thoiLuongPhut = 30, [FromQuery] string? maKhachHang = null)
    {
        if (!QuanLyLichHenService.ThuDocMa(maBacSi, "BS_", out var idBacSi) || !ngay.HasValue)
            return BadRequest("Mã bác sĩ và ngày phải hợp lệ.");
        if (thoiLuongPhut <= 0 || thoiLuongPhut > ushort.MaxValue)
            return BadRequest("Thời lượng khám không hợp lệ.");
        if (!await quanLyLichHenService.BacSiTonTaiAsync(idBacSi)) return NotFound("Không tìm thấy bác sĩ.");
        ulong? maTaiKhoanKhachHang = null;
        if (User.IsInRole("KHACH_HANG"))
        {
            if (!ThuDocTaiKhoan(out var maTaiKhoan)) return Unauthorized();
            maTaiKhoanKhachHang = maTaiKhoan;
        }
        ulong? idKhachHang = null;
        if (!string.IsNullOrWhiteSpace(maKhachHang) &&
            (User.IsInRole("LE_TAN") || User.IsInRole("QUAN_LY")))
        {
            if (!QuanLyLichHenService.ThuDocMa(maKhachHang, "KH_", out var id))
                return BadRequest("Mã khách hàng không hợp lệ.");
            idKhachHang = id;
        }
        return Ok(await quanLyLichHenService.LayCaTrongAsync(idBacSi, ngay.Value,
            thoiLuongPhut, maTaiKhoanKhachHang, idKhachHang));
    }

    [HttpPut("xac-nhan/{maLichHen}")]
    [Authorize(Roles = "LE_TAN,QUAN_LY,BAC_SI")]
    public async Task<IActionResult> XacNhan(string maLichHen, [FromQuery] string? maPhong)
    {
        if (!ThuDocTaiKhoan(out var maTaiKhoan)) return Unauthorized();
        var ketQua = await quanLyLichHenService.XacNhanAsync(maLichHen, maTaiKhoan, maPhong);
        return ketQua.Loi != null ? Loi(ketQua) : Content("Xác nhận lịch hẹn thành công!", "text/plain");
    }

    [HttpPut("gan-bac-si-va-xac-nhan/{maLichHen}")]
    [Authorize(Roles = "LE_TAN,QUAN_LY")]
    public async Task<IActionResult> GanBacSiVaXacNhan(string maLichHen,
        [FromBody] GanBacSiVaXacNhanLichHenRequest yeuCau)
    {
        if (!ThuDocTaiKhoan(out var maTaiKhoan)) return Unauthorized();
        var ketQua = await quanLyLichHenService.XacNhanAsync(maLichHen, maTaiKhoan,
            yeuCau.MaPhong, yeuCau.MaBacSi);
        return ketQua.Loi != null ? Loi(ketQua) : Content("Đã gán bác sĩ và xác nhận lịch hẹn.", "text/plain");
    }

    [HttpGet("phong-trong/{maLichHen}")]
    [Authorize(Roles = "LE_TAN,QUAN_LY,BAC_SI")]
    public async Task<IActionResult> LayPhongTrong(string maLichHen)
    {
        if (!ThuDocTaiKhoan(out var maTaiKhoan)) return Unauthorized();
        var (ketQua, phongs) = await quanLyLichHenService.LayPhongTrongAsync(maLichHen, maTaiKhoan);
        return ketQua.Loi != null ? Loi(ketQua) : Ok(phongs);
    }

    [HttpGet("bac-si-trong/{maLichHen}")]
    [Authorize(Roles = "LE_TAN,QUAN_LY")]
    public async Task<IActionResult> LayBacSiTrong(string maLichHen)
    {
        var (ketQua, bacSis) = await quanLyLichHenService.LayBacSiTrongAsync(maLichHen);
        return ketQua.Loi != null ? Loi(ketQua) : Ok(bacSis);
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

    private static bool PhanTrangHopLe(int page, int pageSize) =>
        page >= 1 && pageSize is >= 1 and <= 50 && (long)(page - 1) * pageSize <= int.MaxValue;

    private IActionResult Loi(KetQuaLichHen ketQua) => ketQua.KhongTimThay
        ? NotFound(ketQua.Loi)
        : ketQua.KhongCoQuyen ? StatusCode(403, ketQua.Loi) : BadRequest(ketQua.Loi);
}
