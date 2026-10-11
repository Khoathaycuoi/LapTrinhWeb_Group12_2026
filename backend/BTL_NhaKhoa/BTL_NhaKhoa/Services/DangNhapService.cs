using BTL_NhaKhoa.Data;
using BTL_NhaKhoa.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BTL_NhaKhoa.Services;

public sealed class DangNhapService(NhaKhoaDbContext db, CauHinhJwt cauHinhJwt)
{
    private readonly PasswordHasher<TaiKhoan> _maHoaMatKhau = new();

    public async Task<string?> DangNhapAsync(string soDienThoai, string matKhau)
    {
        var soChuanHoa = ChuanHoaSoDienThoai(soDienThoai);
        var taiKhoan = await db.TaiKhoans.Include(x => x.ConNguoi)
            .FirstOrDefaultAsync(x => x.ConNguoi.SoDienThoai == soChuanHoa);

        if (taiKhoan == null || taiKhoan.TrangThai != "HOAT_DONG" ||
            _maHoaMatKhau.VerifyHashedPassword(taiKhoan, taiKhoan.MatKhauHash, matKhau)
                == PasswordVerificationResult.Failed)
        {
            return null;
        }

        var roleCu = taiKhoan.MaVaiTro switch
        {
            "QUAN_LY" => "ROLE_ADMIN",
            "LE_TAN" => "ROLE_NHANVIEN",
            "BAC_SI" => "ROLE_BACSI",
            "KHACH_HANG" => "ROLE_USER",
            _ => null
        };
        if (roleCu == null)
        {
            return null;
        }

        var hienTai = DateTime.UtcNow;
        var token = new JwtSecurityToken(
            issuer: cauHinhJwt.NhaPhatHanh,
            audience: cauHinhJwt.DoiTuongNhan,
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, soChuanHoa),
                new Claim("ma_tai_khoan", taiKhoan.MaTaiKhoan.ToString()),
                new Claim("vai_tro", taiKhoan.MaVaiTro),
                new Claim("role", roleCu)
            ],
            notBefore: hienTai,
            expires: hienTai.AddHours(24),
            signingCredentials: new SigningCredentials(cauHinhJwt.KhoaKy, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static string ChuanHoaSoDienThoai(string soDienThoai)
    {
        var so = soDienThoai.Trim().Replace(" ", "").Replace("-", "").Replace(".", "");
        return so.StartsWith("+84", StringComparison.Ordinal)
            ? "0" + so[3..]
            : so;
    }
}
