using BTL_NhaKhoa.DTOs;
using BTL_NhaKhoa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BTL_NhaKhoa.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public sealed class NguoiDungController(
    TaiKhoanCaNhanService taiKhoanCaNhanService,
    QuanLyNguoiDungService quanLyNguoiDungService) : ControllerBase
{
    [HttpGet("bac-si")]
    [Authorize(Roles = "QUAN_LY")]
    public async Task<IActionResult> LayBacSi([FromQuery] string? keyword)
    {
        return Ok(await quanLyNguoiDungService.LayDanhSachAsync("bac-si", null, keyword));
    }

    [HttpGet("bac-si/hoat-dong")]
    public async Task<IActionResult> LayBacSiHoatDong([FromQuery] string? keyword)
    {
        return Ok(await quanLyNguoiDungService.LayDanhSachAsync("bac-si", "HOAT_DONG", keyword));
    }

    [HttpGet("bac-si/khong-hoat-dong")]
    [Authorize(Roles = "QUAN_LY")]
    public async Task<IActionResult> LayBacSiKhongHoatDong([FromQuery] string? keyword)
    {
        return Ok(await quanLyNguoiDungService.LayDanhSachAsync("bac-si", "VO_HIEU_HOA", keyword));
    }

    [HttpGet("nhan-vien")]
    [Authorize(Roles = "QUAN_LY")]
    public async Task<IActionResult> LayNhanVien([FromQuery] string? keyword)
    {
        return Ok(await quanLyNguoiDungService.LayDanhSachAsync("nhan-vien", null, keyword));
    }

    [HttpGet("nhan-vien/hoat-dong")]
    [Authorize(Roles = "QUAN_LY")]
    public async Task<IActionResult> LayNhanVienHoatDong([FromQuery] string? keyword)
    {
        return Ok(await quanLyNguoiDungService.LayDanhSachAsync("nhan-vien", "HOAT_DONG", keyword));
    }

    [HttpGet("nhan-vien/khong-hoat-dong")]
    [Authorize(Roles = "QUAN_LY")]
    public async Task<IActionResult> LayNhanVienKhongHoatDong([FromQuery] string? keyword)
    {
        return Ok(await quanLyNguoiDungService.LayDanhSachAsync("nhan-vien", "VO_HIEU_HOA", keyword));
    }

    [HttpGet("quan-ly")]
    [Authorize(Roles = "QUAN_LY")]
    public async Task<IActionResult> LayQuanLy([FromQuery] string? keyword)
    {
        return Ok(await quanLyNguoiDungService.LayDanhSachAsync("quan-ly", null, keyword));
    }

    [HttpGet("quan-ly/hoat-dong")]
    [Authorize(Roles = "QUAN_LY")]
    public async Task<IActionResult> LayQuanLyHoatDong([FromQuery] string? keyword)
    {
        return Ok(await quanLyNguoiDungService.LayDanhSachAsync("quan-ly", "HOAT_DONG", keyword));
    }

    [HttpGet("quan-ly/khong-hoat-dong")]
    [Authorize(Roles = "QUAN_LY")]
    public async Task<IActionResult> LayQuanLyKhongHoatDong([FromQuery] string? keyword)
    {
        return Ok(await quanLyNguoiDungService.LayDanhSachAsync("quan-ly", "VO_HIEU_HOA", keyword));
    }

    [HttpGet("khach-hang")]
    [Authorize(Roles = "QUAN_LY,LE_TAN")]
    public async Task<IActionResult> LayKhachHang([FromQuery] string? keyword)
    {
        return Ok(await quanLyNguoiDungService.LayDanhSachAsync("khach-hang", null, keyword));
    }

    [HttpGet("khach-hang/hoat-dong")]
    [Authorize(Roles = "QUAN_LY,LE_TAN")]
    public async Task<IActionResult> LayKhachHangHoatDong([FromQuery] string? keyword)
    {
        return Ok(await quanLyNguoiDungService.LayDanhSachAsync("khach-hang", "HOAT_DONG", keyword));
    }

    [HttpGet("khach-hang/khong-hoat-dong")]
    [Authorize(Roles = "QUAN_LY")]
    public async Task<IActionResult> LayKhachHangKhongHoatDong([FromQuery] string? keyword)
    {
        return Ok(await quanLyNguoiDungService.LayDanhSachAsync("khach-hang", "VO_HIEU_HOA", keyword));
    }

    [HttpGet("detail/{id}")]
    [Authorize(Roles = "QUAN_LY,LE_TAN")]
    public async Task<IActionResult> LayChiTietNguoiDung(string id)
    {
        var chiTiet = await quanLyNguoiDungService.LayChiTietAsync(id);
        return chiTiet == null ? NotFound("Không tìm thấy người dùng.") : Ok(chiTiet);
    }

    [HttpPut("{soDienThoai}/xoa")]
    [Authorize(Roles = "QUAN_LY")]
    public async Task<IActionResult> VoHieuHoa(string soDienThoai)
    {
        return await DoiTrangThai(soDienThoai, true);
    }

    [HttpPut("{soDienThoai}/khoi-phuc")]
    [Authorize(Roles = "QUAN_LY")]
    public async Task<IActionResult> KhoiPhuc(string soDienThoai)
    {
        return await DoiTrangThai(soDienThoai, false);
    }

    [HttpGet("me")]
    public async Task<IActionResult> LayThongTinCaNhan()
    {
        if (!ulong.TryParse(User.FindFirstValue("ma_tai_khoan"), out var maTaiKhoan))
        {
            return Unauthorized();
        }

        var thongTin = await taiKhoanCaNhanService.LayThongTinAsync(maTaiKhoan);
        return thongTin == null ? NotFound("Không tìm thấy thông tin người dùng.") : Ok(thongTin);
    }

    [HttpPut("me")]
    public async Task<IActionResult> CapNhatCaNhan([FromBody] CapNhatCaNhanRequest yeuCau)
    {
        if (!ulong.TryParse(User.FindFirstValue("ma_tai_khoan"), out var maTaiKhoan))
        {
            return Unauthorized();
        }

        var ketQua = await taiKhoanCaNhanService.CapNhatAsync(maTaiKhoan, yeuCau);
        if (ketQua.Loi != null)
        {
            return ketQua.KhongTimThay ? NotFound(ketQua.Loi) : BadRequest(ketQua.Loi);
        }
        return Content("Cập nhật thông tin thành công!", "text/plain");
    }

    [HttpPut("me/password")]
    public async Task<IActionResult> DoiMatKhau([FromBody] DoiMatKhauRequest yeuCau)
    {
        if (!ulong.TryParse(User.FindFirstValue("ma_tai_khoan"), out var maTaiKhoan))
        {
            return Unauthorized();
        }

        var ketQua = await taiKhoanCaNhanService.DoiMatKhauAsync(maTaiKhoan, yeuCau);
        if (ketQua.Loi != null)
        {
            return ketQua.KhongTimThay ? NotFound(ketQua.Loi) : BadRequest(ketQua.Loi);
        }
        return Content("Đổi mật khẩu thành công!", "text/plain");
    }

    private async Task<IActionResult> DoiTrangThai(string soDienThoai, bool voHieuHoa)
    {
        if (!ulong.TryParse(User.FindFirstValue("ma_tai_khoan"), out var maNguoiThucHien))
        {
            return Unauthorized();
        }

        var ketQua = await quanLyNguoiDungService.DoiTrangThaiAsync(
            soDienThoai, maNguoiThucHien, voHieuHoa);
        if (ketQua.Loi != null)
        {
            return ketQua.KhongTimThay ? NotFound(ketQua.Loi) : BadRequest(ketQua.Loi);
        }

        return Content(voHieuHoa
            ? "Đã vô hiệu hóa tài khoản thành công!"
            : "Đã khôi phục tài khoản thành công!", "text/plain");
    }
}
