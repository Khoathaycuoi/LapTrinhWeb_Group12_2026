using BTL_NhaKhoa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BTL_NhaKhoa.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public sealed class DashboardController(DashboardService dashboardService) : ControllerBase
{
    [HttpGet("quan-ly")]
    [Authorize(Roles = "QUAN_LY")]
    public async Task<IActionResult> LayQuanLy() =>
        Ok(await dashboardService.LayQuanLyAsync());

    [HttpGet("le-tan")]
    [Authorize(Roles = "LE_TAN")]
    public async Task<IActionResult> LayLeTan() =>
        Ok(await dashboardService.LayLeTanAsync());

    [HttpGet("bac-si")]
    [Authorize(Roles = "BAC_SI")]
    public async Task<IActionResult> LayBacSi()
    {
        if (!ThuDocTaiKhoan(out var maTaiKhoan)) return Unauthorized();
        var duLieu = await dashboardService.LayBacSiAsync(maTaiKhoan);
        return duLieu == null ? NotFound("Không tìm thấy hồ sơ bác sĩ.") : Ok(duLieu);
    }

    [HttpGet("khach-hang")]
    [Authorize(Roles = "KHACH_HANG")]
    public async Task<IActionResult> LayKhachHang()
    {
        if (!ThuDocTaiKhoan(out var maTaiKhoan)) return Unauthorized();
        var duLieu = await dashboardService.LayKhachHangAsync(maTaiKhoan);
        return duLieu == null ? NotFound("Không tìm thấy hồ sơ khách hàng.") : Ok(duLieu);
    }

    private bool ThuDocTaiKhoan(out ulong maTaiKhoan) =>
        ulong.TryParse(User.FindFirstValue("ma_tai_khoan"), out maTaiKhoan);
}
