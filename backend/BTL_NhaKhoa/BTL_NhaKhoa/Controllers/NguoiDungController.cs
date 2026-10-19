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
    QuanLyNguoiDungService quanLyNguoiDungService,
    IWebHostEnvironment moiTruong) : ControllerBase
{
    [HttpGet("anh-dai-dien/{tenFile}")]
    [AllowAnonymous]
    public IActionResult LayAnhDaiDien(string tenFile)
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(tenFile, "^[a-f0-9]{32}\\.(jpg|png|webp)$"))
            return NotFound();
        var duongDan = Path.Combine(moiTruong.ContentRootPath, "wwwroot", "avatars", tenFile);
        if (!System.IO.File.Exists(duongDan)) return NotFound();
        var loaiAnh = Path.GetExtension(tenFile) switch
        {
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "image/jpeg"
        };
        return PhysicalFile(duongDan, loaiAnh);
    }

    [HttpGet("bac-si")]
    [Authorize(Roles = "QUAN_LY")]
    public async Task<IActionResult> LayBacSi([FromQuery] string? keyword,
        [FromQuery] PhanTrangRequest? phanTrang = null)
    {
        if (phanTrang?.CoPhanTrang == true)
        {
            if (!phanTrang.HopLe) return BadRequest("Trang hoặc kích thước trang không hợp lệ.");
            return Ok(await quanLyNguoiDungService.LayTrangAsync("bac-si", null, keyword,
                phanTrang.SoTrang, phanTrang.KichThuocTrang));
        }
        return Ok(await quanLyNguoiDungService.LayDanhSachAsync("bac-si", null, keyword));
    }

    [HttpGet("bac-si/hoat-dong")]
    public async Task<IActionResult> LayBacSiHoatDong([FromQuery] string? keyword,
        [FromQuery] PhanTrangRequest? phanTrang = null)
    {
        if (phanTrang?.CoPhanTrang == true)
        {
            if (!phanTrang.HopLe) return BadRequest("Trang hoặc kích thước trang không hợp lệ.");
            return Ok(await quanLyNguoiDungService.LayTrangAsync("bac-si", "HOAT_DONG", keyword,
                phanTrang.SoTrang, phanTrang.KichThuocTrang));
        }
        return Ok(await quanLyNguoiDungService.LayDanhSachAsync("bac-si", "HOAT_DONG", keyword));
    }

    [HttpGet("bac-si/khong-hoat-dong")]
    [Authorize(Roles = "QUAN_LY")]
    public async Task<IActionResult> LayBacSiKhongHoatDong([FromQuery] string? keyword,
        [FromQuery] PhanTrangRequest? phanTrang = null)
    {
        if (phanTrang?.CoPhanTrang == true)
        {
            if (!phanTrang.HopLe) return BadRequest("Trang hoặc kích thước trang không hợp lệ.");
            return Ok(await quanLyNguoiDungService.LayTrangAsync("bac-si", "VO_HIEU_HOA", keyword,
                phanTrang.SoTrang, phanTrang.KichThuocTrang));
        }
        return Ok(await quanLyNguoiDungService.LayDanhSachAsync("bac-si", "VO_HIEU_HOA", keyword));
    }

    [HttpGet("nhan-vien")]
    [Authorize(Roles = "QUAN_LY")]
    public async Task<IActionResult> LayNhanVien([FromQuery] string? keyword,
        [FromQuery] PhanTrangRequest? phanTrang = null)
    {
        if (phanTrang?.CoPhanTrang == true)
        {
            if (!phanTrang.HopLe) return BadRequest("Trang hoặc kích thước trang không hợp lệ.");
            return Ok(await quanLyNguoiDungService.LayTrangAsync("nhan-vien", null, keyword,
                phanTrang.SoTrang, phanTrang.KichThuocTrang));
        }
        return Ok(await quanLyNguoiDungService.LayDanhSachAsync("nhan-vien", null, keyword));
    }

    [HttpGet("nhan-vien/hoat-dong")]
    [Authorize(Roles = "QUAN_LY")]
    public async Task<IActionResult> LayNhanVienHoatDong([FromQuery] string? keyword,
        [FromQuery] PhanTrangRequest? phanTrang = null)
    {
        if (phanTrang?.CoPhanTrang == true)
        {
            if (!phanTrang.HopLe) return BadRequest("Trang hoặc kích thước trang không hợp lệ.");
            return Ok(await quanLyNguoiDungService.LayTrangAsync("nhan-vien", "HOAT_DONG", keyword,
                phanTrang.SoTrang, phanTrang.KichThuocTrang));
        }
        return Ok(await quanLyNguoiDungService.LayDanhSachAsync("nhan-vien", "HOAT_DONG", keyword));
    }

    [HttpGet("nhan-vien/khong-hoat-dong")]
    [Authorize(Roles = "QUAN_LY")]
    public async Task<IActionResult> LayNhanVienKhongHoatDong([FromQuery] string? keyword,
        [FromQuery] PhanTrangRequest? phanTrang = null)
    {
        if (phanTrang?.CoPhanTrang == true)
        {
            if (!phanTrang.HopLe) return BadRequest("Trang hoặc kích thước trang không hợp lệ.");
            return Ok(await quanLyNguoiDungService.LayTrangAsync("nhan-vien", "VO_HIEU_HOA", keyword,
                phanTrang.SoTrang, phanTrang.KichThuocTrang));
        }
        return Ok(await quanLyNguoiDungService.LayDanhSachAsync("nhan-vien", "VO_HIEU_HOA", keyword));
    }

    [HttpGet("quan-ly")]
    [Authorize(Roles = "QUAN_LY")]
    public async Task<IActionResult> LayQuanLy([FromQuery] string? keyword,
        [FromQuery] PhanTrangRequest? phanTrang = null)
    {
        if (phanTrang?.CoPhanTrang == true)
        {
            if (!phanTrang.HopLe) return BadRequest("Trang hoặc kích thước trang không hợp lệ.");
            return Ok(await quanLyNguoiDungService.LayTrangAsync("quan-ly", null, keyword,
                phanTrang.SoTrang, phanTrang.KichThuocTrang));
        }
        return Ok(await quanLyNguoiDungService.LayDanhSachAsync("quan-ly", null, keyword));
    }

    [HttpGet("quan-ly/hoat-dong")]
    [Authorize(Roles = "QUAN_LY")]
    public async Task<IActionResult> LayQuanLyHoatDong([FromQuery] string? keyword,
        [FromQuery] PhanTrangRequest? phanTrang = null)
    {
        if (phanTrang?.CoPhanTrang == true)
        {
            if (!phanTrang.HopLe) return BadRequest("Trang hoặc kích thước trang không hợp lệ.");
            return Ok(await quanLyNguoiDungService.LayTrangAsync("quan-ly", "HOAT_DONG", keyword,
                phanTrang.SoTrang, phanTrang.KichThuocTrang));
        }
        return Ok(await quanLyNguoiDungService.LayDanhSachAsync("quan-ly", "HOAT_DONG", keyword));
    }

    [HttpGet("quan-ly/khong-hoat-dong")]
    [Authorize(Roles = "QUAN_LY")]
    public async Task<IActionResult> LayQuanLyKhongHoatDong([FromQuery] string? keyword,
        [FromQuery] PhanTrangRequest? phanTrang = null)
    {
        if (phanTrang?.CoPhanTrang == true)
        {
            if (!phanTrang.HopLe) return BadRequest("Trang hoặc kích thước trang không hợp lệ.");
            return Ok(await quanLyNguoiDungService.LayTrangAsync("quan-ly", "VO_HIEU_HOA", keyword,
                phanTrang.SoTrang, phanTrang.KichThuocTrang));
        }
        return Ok(await quanLyNguoiDungService.LayDanhSachAsync("quan-ly", "VO_HIEU_HOA", keyword));
    }

    [HttpGet("khach-hang")]
    [Authorize(Roles = "QUAN_LY,LE_TAN")]
    public async Task<IActionResult> LayKhachHang([FromQuery] string? keyword,
        [FromQuery] PhanTrangRequest? phanTrang = null)
    {
        if (phanTrang?.CoPhanTrang == true)
        {
            if (!phanTrang.HopLe) return BadRequest("Trang hoặc kích thước trang không hợp lệ.");
            return Ok(await quanLyNguoiDungService.LayTrangAsync("khach-hang", null, keyword,
                phanTrang.SoTrang, phanTrang.KichThuocTrang));
        }
        return Ok(await quanLyNguoiDungService.LayDanhSachAsync("khach-hang", null, keyword));
    }

    [HttpGet("khach-hang/hoat-dong")]
    [Authorize(Roles = "QUAN_LY,LE_TAN")]
    public async Task<IActionResult> LayKhachHangHoatDong([FromQuery] string? keyword,
        [FromQuery] PhanTrangRequest? phanTrang = null)
    {
        if (phanTrang?.CoPhanTrang == true)
        {
            if (!phanTrang.HopLe) return BadRequest("Trang hoặc kích thước trang không hợp lệ.");
            return Ok(await quanLyNguoiDungService.LayTrangAsync("khach-hang", "HOAT_DONG", keyword,
                phanTrang.SoTrang, phanTrang.KichThuocTrang));
        }
        return Ok(await quanLyNguoiDungService.LayDanhSachAsync("khach-hang", "HOAT_DONG", keyword));
    }

    [HttpGet("khach-hang/khong-hoat-dong")]
    [Authorize(Roles = "QUAN_LY")]
    public async Task<IActionResult> LayKhachHangKhongHoatDong([FromQuery] string? keyword,
        [FromQuery] PhanTrangRequest? phanTrang = null)
    {
        if (phanTrang?.CoPhanTrang == true)
        {
            if (!phanTrang.HopLe) return BadRequest("Trang hoặc kích thước trang không hợp lệ.");
            return Ok(await quanLyNguoiDungService.LayTrangAsync("khach-hang", "VO_HIEU_HOA", keyword,
                phanTrang.SoTrang, phanTrang.KichThuocTrang));
        }
        return Ok(await quanLyNguoiDungService.LayDanhSachAsync("khach-hang", "VO_HIEU_HOA", keyword));
    }

    [HttpGet("detail/{id}")]
    [Authorize(Roles = "QUAN_LY,LE_TAN")]
    public async Task<IActionResult> LayChiTietNguoiDung(string id)
    {
        var chiTiet = await quanLyNguoiDungService.LayChiTietAsync(id);
        return chiTiet == null ? NotFound("Không tìm thấy người dùng.") : Ok(chiTiet);
    }

    [HttpGet("khach-hang/{maKhachHang}/tien-su-benh")]
    [Authorize(Roles = "BAC_SI")]
    public async Task<IActionResult> LayTienSuBenhKhachHang(string maKhachHang)
    {
        if (!ulong.TryParse(User.FindFirstValue("ma_tai_khoan"), out var maTaiKhoan))
            return Unauthorized();

        var (tonTai, duocXem, duLieu) = await quanLyNguoiDungService
            .LayTienSuBenhKhachHangAsync(maKhachHang, maTaiKhoan);
        if (!tonTai) return NotFound("Không tìm thấy khách hàng.");
        return duocXem ? Ok(duLieu) : Forbid();
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

    [HttpPost("me/anh-dai-dien")]
    [RequestSizeLimit(3 * 1024 * 1024)]
    public async Task<IActionResult> TaiAnhDaiDien([FromForm] IFormFile? anh)
    {
        if (!ulong.TryParse(User.FindFirstValue("ma_tai_khoan"), out var maTaiKhoan))
            return Unauthorized();
        var (loi, duongDan) = await taiKhoanCaNhanService.LuuAnhAsync(maTaiKhoan, anh);
        if (loi != null) return BadRequest(loi);
        return Ok(new { anhDaiDienUrl = duongDan });
    }

    [HttpDelete("me/anh-dai-dien")]
    public async Task<IActionResult> XoaAnhDaiDien()
    {
        if (!ulong.TryParse(User.FindFirstValue("ma_tai_khoan"), out var maTaiKhoan))
            return Unauthorized();
        return await taiKhoanCaNhanService.XoaAnhAsync(maTaiKhoan) ? NoContent() : NotFound("Không tìm thấy tài khoản.");
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

    [HttpPut("me/so-dien-thoai")]
    public async Task<IActionResult> DoiSoDienThoai([FromBody] DoiSoDienThoaiRequest yeuCau)
    {
        if (!ulong.TryParse(User.FindFirstValue("ma_tai_khoan"), out var maTaiKhoan))
        {
            return Unauthorized();
        }

        var ketQua = await taiKhoanCaNhanService.DoiSoDienThoaiAsync(maTaiKhoan, yeuCau);
        if (ketQua.Loi != null)
        {
            if (ketQua.KhongTimThay) return NotFound(ketQua.Loi);
            return ketQua.TrungDuLieu ? Conflict(ketQua.Loi) : BadRequest(ketQua.Loi);
        }
        return Content("Đổi số điện thoại thành công! Vui lòng đăng nhập lại.", "text/plain");
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
