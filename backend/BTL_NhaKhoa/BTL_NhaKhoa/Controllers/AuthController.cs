using BTL_NhaKhoa.DTOs;
using BTL_NhaKhoa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BTL_NhaKhoa.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(DangNhapService dangNhapService, DangKyService dangKyService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register/khach-hang")]
    public async Task<IActionResult> DangKyKhachHang([FromBody] DangKyKhachHangRequest yeuCau)
    {
        return await DangKy(yeuCau, "KHACH_HANG", "Đăng ký Khách hàng thành công!");
    }

    [Authorize(Policy = "QuanLy")]
    [HttpPost("register/le-tan")]
    [HttpPost("register/nhan-vien")]
    public async Task<IActionResult> DangKyLeTan([FromBody] DangKyRequest yeuCau)
    {
        return await DangKy(yeuCau, "LE_TAN", "Đăng ký Lễ tân thành công!");
    }

    [Authorize(Policy = "QuanLy")]
    [HttpPost("register/bac-si")]
    public async Task<IActionResult> DangKyBacSi([FromBody] DangKyBacSiRequest yeuCau)
    {
        return await DangKy(yeuCau, "BAC_SI", "Đăng ký Bác sĩ thành công!");
    }

    [Authorize(Policy = "QuanLy")]
    [HttpPost("register/quan-ly")]
    public async Task<IActionResult> DangKyQuanLy([FromBody] DangKyRequest yeuCau)
    {
        return await DangKy(yeuCau, "QUAN_LY", "Đăng ký Quản lý thành công!");
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> DangNhap([FromBody] DangNhapRequest yeuCau)
    {
        var token = await dangNhapService.DangNhapAsync(yeuCau.SoDienThoai, yeuCau.MatKhau);
        return token == null
            ? Unauthorized("Sai số điện thoại hoặc mật khẩu, hoặc tài khoản đã bị vô hiệu hóa.")
            : Content(token, "text/plain");
    }

    [HttpGet("me")]
    public IActionResult ThongTinDangNhap()
    {
        return Ok(new
        {
            MaTaiKhoan = User.FindFirstValue("ma_tai_khoan"),
            SoDienThoai = User.FindFirstValue("sub"),
            VaiTro = User.FindFirstValue("vai_tro")
        });
    }

    private async Task<IActionResult> DangKy(DangKyRequest yeuCau, string maVaiTro, string thongBao)
    {
        var ketQua = await dangKyService.DangKyAsync(yeuCau, maVaiTro);
        if (ketQua.Loi == null)
        {
            return Content(thongBao, "text/plain");
        }

        return ketQua.TrungDuLieu ? Conflict(ketQua.Loi) : BadRequest(ketQua.Loi);
    }
}
