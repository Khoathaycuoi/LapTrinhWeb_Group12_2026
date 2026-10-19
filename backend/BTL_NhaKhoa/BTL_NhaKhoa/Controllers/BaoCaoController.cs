using BTL_NhaKhoa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTL_NhaKhoa.Controllers;

[ApiController]
[Authorize(Roles = "QUAN_LY")]
[Route("api/bao-cao")]
public sealed class BaoCaoController(BaoCaoDoanhThuService baoCaoDoanhThuService) : ControllerBase
{
    [HttpGet("doanh-thu")]
    public async Task<IActionResult> LayDoanhThuTheoNam([FromQuery] int? nam)
    {
        var namCanXem = nam ?? DateTime.Now.Year;
        if (namCanXem is < 1900 or > 9998)
            return BadRequest("Năm cần từ 1900 đến 9998.");

        return Ok(await baoCaoDoanhThuService.LayTheoNamAsync(namCanXem));
    }
}
