using BTL_NhaKhoa.Data;
using BTL_NhaKhoa.DTOs;
using BTL_NhaKhoa.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace BTL_NhaKhoa.Services;

public sealed record KetQuaDangKy(string? Loi = null, bool TrungDuLieu = false);

public sealed class DangKyService(NhaKhoaDbContext db)
{
    private readonly PasswordHasher<TaiKhoan> _maHoaMatKhau = new();

    public async Task<KetQuaDangKy> DangKyAsync(YeuCauDangKy yeuCau, string maVaiTro)
    {
        if (string.IsNullOrWhiteSpace(yeuCau.HoTen))
        {
            return new KetQuaDangKy("Vui lòng nhập họ tên.");
        }
        if (yeuCau.HoTen.Trim().Length > 150)
        {
            return new KetQuaDangKy("Họ tên không được vượt quá 150 ký tự.");
        }
        if (!Regex.IsMatch(yeuCau.MatKhau,
            @"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d@$!%*?&]{8,}$"))
        {
            return new KetQuaDangKy("Mật khẩu phải từ 8 ký tự, có ít nhất một chữ cái và một chữ số.");
        }
        if (yeuCau.NgaySinh is { } ngaySinh &&
            (ngaySinh.Year < 1900 || ngaySinh > DateOnly.FromDateTime(DateTime.Today)))
        {
            return new KetQuaDangKy("Ngày sinh không hợp lệ.");
        }

        var soDienThoai = DangNhapService.ChuanHoaSoDienThoai(yeuCau.SoDienThoai);
        if (!Regex.IsMatch(soDienThoai, @"^0\d{9}$"))
        {
            return new KetQuaDangKy("Số điện thoại phải bắt đầu bằng 0 và có đúng 10 chữ số.");
        }

        if (await db.ConNguois.AnyAsync(x => x.SoDienThoai == soDienThoai))
        {
            return new KetQuaDangKy("Số điện thoại đã được sử dụng.", true);
        }

        var soDinhDanh = string.IsNullOrWhiteSpace(yeuCau.SoDinhDanh)
            ? null : yeuCau.SoDinhDanh.Trim();
        if (soDinhDanh != null && await db.ConNguois.AnyAsync(x => x.SoDinhDanh == soDinhDanh))
        {
            return new KetQuaDangKy("Mã định danh đã tồn tại.", true);
        }

        var conNguoi = new ConNguoi
        {
            SoDinhDanh = soDinhDanh,
            HoTen = yeuCau.HoTen.Trim(),
            SoDienThoai = soDienThoai,
            GioiTinh = yeuCau.GioiTinh?.Trim(),
            NgaySinh = yeuCau.NgaySinh,
            DiaChi = yeuCau.DiaChi?.Trim()
        };
        var taiKhoan = new TaiKhoan
        {
            ConNguoi = conNguoi,
            MaVaiTro = maVaiTro
        };
        taiKhoan.MatKhauHash = _maHoaMatKhau.HashPassword(taiKhoan, yeuCau.MatKhau);
        db.TaiKhoans.Add(taiKhoan);

        if (maVaiTro == "KHACH_HANG")
        {
            db.KhachHangs.Add(new KhachHang
            {
                ConNguoi = conNguoi,
                NgayDangKy = DateOnly.FromDateTime(DateTime.UtcNow),
                TienSuBenh = (yeuCau as YeuCauDangKyKhachHang)?.TienSuBenh
            });
        }
        else if (maVaiTro is "QUAN_LY" or "LE_TAN" or "BAC_SI")
        {
            var nhanVien = new NhanVien { ConNguoi = conNguoi };
            db.NhanViens.Add(nhanVien);

            if (maVaiTro == "BAC_SI")
            {
                var thongTinBacSi = yeuCau as YeuCauDangKyBacSi;
                db.BacSis.Add(new BacSi
                {
                    NhanVien = nhanVien,
                    ChungChi = thongTinBacSi?.ChungChi,
                    BangCap = thongTinBacSi?.BangCap,
                    SoNamKinhNghiem = thongTinBacSi?.SoNamKinhNghiem
                });
            }
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(maVaiTro));
        }

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            if (await db.ConNguois.AnyAsync(x => x.SoDienThoai == soDienThoai))
            {
                return new KetQuaDangKy("Số điện thoại đã được sử dụng.", true);
            }
            if (soDinhDanh != null && await db.ConNguois.AnyAsync(x => x.SoDinhDanh == soDinhDanh))
            {
                return new KetQuaDangKy("Mã định danh đã tồn tại.", true);
            }
            throw;
        }
        return new KetQuaDangKy();
    }
}
