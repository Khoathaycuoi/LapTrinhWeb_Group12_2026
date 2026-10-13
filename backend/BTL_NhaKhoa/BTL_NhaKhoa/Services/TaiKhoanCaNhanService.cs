using BTL_NhaKhoa.Data;
using BTL_NhaKhoa.DTOs;
using BTL_NhaKhoa.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BTL_NhaKhoa.Services;

public sealed record KetQuaThaoTacCaNhan(string? Loi = null, bool KhongTimThay = false);

public sealed class TaiKhoanCaNhanService(NhaKhoaDbContext db)
{
    private readonly PasswordHasher<TaiKhoan> _maHoaMatKhau = new();

    public async Task<ThongTinCaNhanResponse?> LayThongTinAsync(ulong maTaiKhoan)
    {
        var taiKhoan = await db.TaiKhoans.AsNoTracking()
            .Include(x => x.ConNguoi)
                .ThenInclude(x => x.NhanVien)
                    .ThenInclude(x => x!.BacSi)
            .Include(x => x.ConNguoi)
                .ThenInclude(x => x.KhachHang)
            .FirstOrDefaultAsync(x => x.MaTaiKhoan == maTaiKhoan);
        if (taiKhoan == null)
        {
            return null;
        }

        var conNguoi = taiKhoan.ConNguoi;
        var nhanVien = conNguoi.NhanVien;
        var bacSi = nhanVien?.BacSi;
        var khachHang = conNguoi.KhachHang;
        var id = taiKhoan.MaVaiTro switch
        {
            "QUAN_LY" when nhanVien != null => $"QL_{nhanVien.MaNhanVien:D4}",
            "LE_TAN" when nhanVien != null => $"NV_{nhanVien.MaNhanVien:D4}",
            "BAC_SI" when bacSi != null => $"BS_{bacSi.MaBacSi:D4}",
            "KHACH_HANG" when khachHang != null => $"KH_{khachHang.MaKhachHang:D4}",
            _ => null
        };
        var role = taiKhoan.MaVaiTro switch
        {
            "QUAN_LY" => "ROLE_ADMIN",
            "LE_TAN" => "ROLE_NHANVIEN",
            "BAC_SI" => "ROLE_BACSI",
            "KHACH_HANG" => "ROLE_USER",
            _ => string.Empty
        };

        if (id == null || role.Length == 0)
        {
            return null;
        }

        return new ThongTinCaNhanResponse
        {
            Id = id,
            Ten = conNguoi.HoTen,
            SoDienThoai = conNguoi.SoDienThoai,
            GioiTinh = conNguoi.GioiTinh,
            Role = role,
            DiaChi = conNguoi.DiaChi,
            BangCap = taiKhoan.MaVaiTro == "BAC_SI" ? bacSi?.BangCap : null,
            ChungChi = taiKhoan.MaVaiTro == "BAC_SI" ? bacSi?.ChungChi : null,
            NgaySinh = conNguoi.NgaySinh,
            TienSuBenh = taiKhoan.MaVaiTro == "KHACH_HANG" ? khachHang?.TienSuBenh : null,
            SoNamKinhNghiem = taiKhoan.MaVaiTro == "BAC_SI" ? bacSi?.SoNamKinhNghiem : null,
            TrangThai = taiKhoan.TrangThai == "HOAT_DONG" ? "Hoạt động" : "Không hoạt động"
        };
    }

    public async Task<KetQuaThaoTacCaNhan> CapNhatAsync(ulong maTaiKhoan, CapNhatCaNhanRequest yeuCau)
    {
        if (string.IsNullOrWhiteSpace(yeuCau.Ten))
        {
            return new KetQuaThaoTacCaNhan("Vui lòng nhập họ tên.");
        }
        if (yeuCau.NgaySinh is { } ngaySinh &&
            (ngaySinh.Year < 1900 || ngaySinh > DateOnly.FromDateTime(DateTime.Today)))
        {
            return new KetQuaThaoTacCaNhan("Ngày sinh không hợp lệ.");
        }

        var taiKhoan = await db.TaiKhoans.Include(x => x.ConNguoi)
            .FirstOrDefaultAsync(x => x.MaTaiKhoan == maTaiKhoan);
        if (taiKhoan == null)
        {
            return new KetQuaThaoTacCaNhan("Không tìm thấy tài khoản.", true);
        }

        BacSi? bacSi = null;
        KhachHang? khachHang = null;
        if (taiKhoan.MaVaiTro == "BAC_SI")
        {
            bacSi = await db.BacSis.FirstOrDefaultAsync(x => x.NhanVien.MaConNguoi == taiKhoan.MaConNguoi);
            if (bacSi == null)
            {
                return new KetQuaThaoTacCaNhan("Không tìm thấy hồ sơ bác sĩ.", true);
            }
        }
        else if (taiKhoan.MaVaiTro == "KHACH_HANG")
        {
            khachHang = await db.KhachHangs.FirstOrDefaultAsync(x => x.MaConNguoi == taiKhoan.MaConNguoi);
            if (khachHang == null)
            {
                return new KetQuaThaoTacCaNhan("Không tìm thấy hồ sơ khách hàng.", true);
            }
        }

        var conNguoi = taiKhoan.ConNguoi;
        conNguoi.HoTen = yeuCau.Ten.Trim();
        conNguoi.GioiTinh = yeuCau.GioiTinh?.Trim();
        conNguoi.NgaySinh = yeuCau.NgaySinh;
        conNguoi.DiaChi = yeuCau.DiaChi?.Trim();

        if (bacSi != null)
        {
            bacSi.BangCap = yeuCau.BangCap?.Trim();
            bacSi.ChungChi = yeuCau.ChungChi?.Trim();
            bacSi.SoNamKinhNghiem = yeuCau.SoNamKinhNghiem;
        }
        if (khachHang != null)
        {
            khachHang.TienSuBenh = yeuCau.TienSuBenh?.Trim();
        }

        await db.SaveChangesAsync();
        return new KetQuaThaoTacCaNhan();
    }

    public async Task<KetQuaThaoTacCaNhan> DoiMatKhauAsync(ulong maTaiKhoan, DoiMatKhauRequest yeuCau)
    {
        var taiKhoan = await db.TaiKhoans.FirstOrDefaultAsync(x => x.MaTaiKhoan == maTaiKhoan);
        if (taiKhoan == null)
        {
            return new KetQuaThaoTacCaNhan("Không tìm thấy tài khoản.", true);
        }

        if (_maHoaMatKhau.VerifyHashedPassword(taiKhoan, taiKhoan.MatKhauHash, yeuCau.MatKhauCu)
            == PasswordVerificationResult.Failed)
        {
            return new KetQuaThaoTacCaNhan("Mật khẩu cũ không chính xác.");
        }
        if (_maHoaMatKhau.VerifyHashedPassword(taiKhoan, taiKhoan.MatKhauHash, yeuCau.MatKhauMoi)
            != PasswordVerificationResult.Failed)
        {
            return new KetQuaThaoTacCaNhan("Mật khẩu mới không được trùng với mật khẩu cũ.");
        }

        taiKhoan.MatKhauHash = _maHoaMatKhau.HashPassword(taiKhoan, yeuCau.MatKhauMoi);
        await db.SaveChangesAsync();
        return new KetQuaThaoTacCaNhan();
    }
}
