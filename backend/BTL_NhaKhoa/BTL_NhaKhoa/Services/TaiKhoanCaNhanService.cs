using BTL_NhaKhoa.Data;
using BTL_NhaKhoa.DTOs;
using BTL_NhaKhoa.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace BTL_NhaKhoa.Services;

public sealed record KetQuaThaoTacCaNhan(string? Loi = null, bool KhongTimThay = false,
    bool TrungDuLieu = false);

public sealed class TaiKhoanCaNhanService(NhaKhoaDbContext db, IWebHostEnvironment moiTruong)
{
    private readonly PasswordHasher<TaiKhoan> _maHoaMatKhau = new();
    private const long DungLuongAnhToiDa = 2 * 1024 * 1024;
    public static string? TaoDuongDanAnh(string? tenFile) =>
        tenFile == null ? null : $"/api/users/anh-dai-dien/{tenFile}";

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
            MaDinhDanh = conNguoi.SoDinhDanh,
            AnhDaiDienUrl = TaoDuongDanAnh(conNguoi.TenAnhDaiDien),
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

    public async Task<(string? Loi, string? DuongDan)> LuuAnhAsync(ulong maTaiKhoan, IFormFile? anh)
    {
        if (anh == null || anh.Length == 0 || anh.Length > DungLuongAnhToiDa)
            return ("Vui lòng chọn ảnh có dung lượng không quá 2 MB.", null);

        await using var boNho = new MemoryStream();
        await anh.CopyToAsync(boNho);
        var duLieu = boNho.ToArray();
        var duoiFile = XacDinhDinhDangAnh(duLieu);
        if (duoiFile == null)
            return ("Chỉ chấp nhận ảnh JPG, PNG hoặc WebP hợp lệ.", null);

        var conNguoi = await db.TaiKhoans.Where(x => x.MaTaiKhoan == maTaiKhoan)
            .Select(x => x.ConNguoi).FirstOrDefaultAsync();
        if (conNguoi == null) return ("Không tìm thấy tài khoản.", null);

        var thuMuc = Path.Combine(moiTruong.ContentRootPath, "wwwroot", "avatars");
        Directory.CreateDirectory(thuMuc);
        var tenFileMoi = $"{Guid.NewGuid():N}.{duoiFile}";
        var duongDanMoi = Path.Combine(thuMuc, tenFileMoi);
        await File.WriteAllBytesAsync(duongDanMoi, duLieu);
        var tenFileCu = conNguoi.TenAnhDaiDien;
        try
        {
            conNguoi.TenAnhDaiDien = tenFileMoi;
            await db.SaveChangesAsync();
        }
        catch
        {
            File.Delete(duongDanMoi);
            throw;
        }
        XoaFileCu(thuMuc, tenFileCu);
        return (null, TaoDuongDanAnh(tenFileMoi));
    }

    public async Task<bool> XoaAnhAsync(ulong maTaiKhoan)
    {
        var conNguoi = await db.TaiKhoans.Where(x => x.MaTaiKhoan == maTaiKhoan)
            .Select(x => x.ConNguoi).FirstOrDefaultAsync();
        if (conNguoi == null) return false;
        var tenFileCu = conNguoi.TenAnhDaiDien;
        conNguoi.TenAnhDaiDien = null;
        await db.SaveChangesAsync();
        XoaFileCu(Path.Combine(moiTruong.ContentRootPath, "wwwroot", "avatars"), tenFileCu);
        return true;
    }

    private static void XoaFileCu(string thuMuc, string? tenFile)
    {
        if (tenFile != null && System.Text.RegularExpressions.Regex.IsMatch(tenFile, "^[a-f0-9]{32}\\.(jpg|png|webp)$"))
        {
            var duongDan = Path.Combine(thuMuc, tenFile);
            try
            {
                if (File.Exists(duongDan)) File.Delete(duongDan);
            }
            catch (IOException) { /* Ảnh cũ không còn được tham chiếu; có thể dọn sau. */ }
            catch (UnauthorizedAccessException) { /* Không làm hỏng thao tác đã lưu trong DB. */ }
        }
    }

    private static string? XacDinhDinhDangAnh(byte[] duLieu)
    {
        if (duLieu.Length >= 3 && duLieu[0] == 0xFF && duLieu[1] == 0xD8 && duLieu[2] == 0xFF) return "jpg";
        if (duLieu.Length >= 8 && duLieu.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })) return "png";
        if (duLieu.Length >= 12 && duLieu.AsSpan(0, 4).SequenceEqual("RIFF"u8) && duLieu.AsSpan(8, 4).SequenceEqual("WEBP"u8)) return "webp";
        return null;
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

    public async Task<KetQuaThaoTacCaNhan> DoiSoDienThoaiAsync(ulong maTaiKhoan,
        DoiSoDienThoaiRequest yeuCau)
    {
        if (string.IsNullOrWhiteSpace(yeuCau.MatKhauHienTai))
        {
            return new KetQuaThaoTacCaNhan("Vui lòng nhập mật khẩu hiện tại.");
        }
        if (string.IsNullOrWhiteSpace(yeuCau.SoDienThoaiMoi))
        {
            return new KetQuaThaoTacCaNhan("Vui lòng nhập số điện thoại mới.");
        }

        var taiKhoan = await db.TaiKhoans.Include(x => x.ConNguoi)
            .FirstOrDefaultAsync(x => x.MaTaiKhoan == maTaiKhoan);
        if (taiKhoan == null)
        {
            return new KetQuaThaoTacCaNhan("Không tìm thấy tài khoản.", KhongTimThay: true);
        }
        if (_maHoaMatKhau.VerifyHashedPassword(taiKhoan, taiKhoan.MatKhauHash,
            yeuCau.MatKhauHienTai) == PasswordVerificationResult.Failed)
        {
            return new KetQuaThaoTacCaNhan("Mật khẩu hiện tại không chính xác.");
        }

        var soDienThoaiMoi = DangNhapService.ChuanHoaSoDienThoai(yeuCau.SoDienThoaiMoi);
        if (!Regex.IsMatch(soDienThoaiMoi, @"^0\d{9}$"))
        {
            return new KetQuaThaoTacCaNhan("Số điện thoại phải bắt đầu bằng 0 và có đúng 10 chữ số.");
        }
        if (taiKhoan.ConNguoi.SoDienThoai == soDienThoaiMoi)
        {
            return new KetQuaThaoTacCaNhan("Số điện thoại mới phải khác số hiện tại.");
        }
        if (await db.ConNguois.AnyAsync(x => x.SoDienThoai == soDienThoaiMoi))
        {
            return new KetQuaThaoTacCaNhan("Số điện thoại đã được sử dụng.", TrungDuLieu: true);
        }

        taiKhoan.ConNguoi.SoDienThoai = soDienThoaiMoi;
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            if (await db.ConNguois.AnyAsync(x => x.SoDienThoai == soDienThoaiMoi))
            {
                return new KetQuaThaoTacCaNhan("Số điện thoại đã được sử dụng.", TrungDuLieu: true);
            }
            throw;
        }
        return new KetQuaThaoTacCaNhan();
    }
}
