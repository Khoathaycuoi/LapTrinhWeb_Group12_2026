using BTL_NhaKhoa.Data;
using BTL_NhaKhoa.DTOs;
using BTL_NhaKhoa.Entities;
using Microsoft.EntityFrameworkCore;

namespace BTL_NhaKhoa.Services;

public sealed record KetQuaThaoTacDichVu(string? Loi = null, bool KhongTimThay = false, bool TrungDuLieu = false);

public sealed class QuanLyDichVuService(NhaKhoaDbContext db)
{
    public async Task<KetQuaThaoTacDichVu> ThemAsync(ThemDichVuRequest yeuCau)
    {
        var ten = yeuCau.TenDichVu.Trim();
        var loi = KiemTraThongTin(ten, yeuCau.DonGia, yeuCau.ThoiLuongDuKienPhut);
        if (loi != null)
        {
            return new KetQuaThaoTacDichVu(loi);
        }
        if (await db.DichVus.AnyAsync(x => x.TenDichVu == ten))
        {
            return new KetQuaThaoTacDichVu("Tên dịch vụ đã tồn tại.", TrungDuLieu: true);
        }

        db.DichVus.Add(new DichVu
        {
            TenDichVu = ten,
            MoTa = yeuCau.MoTa?.Trim(),
            DonGia = yeuCau.DonGia!.Value,
            ThoiLuongDuKienPhut = yeuCau.ThoiLuongDuKienPhut ?? 30,
            TrangThai = "DANG_HOAT_DONG"
        });
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            if (await db.DichVus.AnyAsync(x => x.TenDichVu == ten))
            {
                return new KetQuaThaoTacDichVu("Tên dịch vụ đã tồn tại.", TrungDuLieu: true);
            }
            throw;
        }
        return new KetQuaThaoTacDichVu();
    }

    public async Task<KetQuaThaoTacDichVu> CapNhatAsync(CapNhatDichVuRequest yeuCau)
    {
        var ten = yeuCau.TenDichVu.Trim();
        var loi = KiemTraThongTin(ten, yeuCau.DonGia, yeuCau.ThoiLuongDuKienPhut);
        if (loi != null)
        {
            return new KetQuaThaoTacDichVu(loi);
        }

        var dichVu = await db.DichVus.FirstOrDefaultAsync(x => x.TenDichVu == ten);
        if (dichVu == null)
        {
            return new KetQuaThaoTacDichVu("Không tìm thấy dịch vụ.", KhongTimThay: true);
        }

        dichVu.MoTa = yeuCau.MoTa?.Trim();
        dichVu.DonGia = yeuCau.DonGia!.Value;
        if (yeuCau.ThoiLuongDuKienPhut.HasValue)
        {
            dichVu.ThoiLuongDuKienPhut = yeuCau.ThoiLuongDuKienPhut.Value;
        }
        await db.SaveChangesAsync();
        return new KetQuaThaoTacDichVu();
    }

    public async Task<KetQuaThaoTacDichVu> DoiTrangThaiAsync(string maDichVu, bool voHieuHoa)
    {
        if (!ThuDocMaDichVu(maDichVu, out var id))
        {
            return new KetQuaThaoTacDichVu("Mã dịch vụ không hợp lệ.", KhongTimThay: true);
        }
        var dichVu = await db.DichVus.FirstOrDefaultAsync(x => x.MaDichVu == id);
        if (dichVu == null)
        {
            return new KetQuaThaoTacDichVu("Không tìm thấy dịch vụ.", KhongTimThay: true);
        }

        dichVu.TrangThai = voHieuHoa ? "NGUNG_HOAT_DONG" : "DANG_HOAT_DONG";
        await db.SaveChangesAsync();
        return new KetQuaThaoTacDichVu();
    }

    public async Task<IReadOnlyList<DichVuResponse>> LayTatCaAsync()
    {
        var danhSach = await db.DichVus.AsNoTracking().OrderBy(x => x.MaDichVu).ToListAsync();
        return danhSach.Select(TaoResponse).ToList();
    }

    public async Task<IReadOnlyList<DichVuResponse>> TimKiemAsync(
        string? ten, string? trangThai, decimal? giaTu, decimal? giaDen)
    {
        var query = db.DichVus.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(ten))
        {
            var tuKhoa = ten.Trim();
            query = query.Where(x => x.TenDichVu.Contains(tuKhoa));
        }
        if (!string.IsNullOrWhiteSpace(trangThai))
        {
            var maTrangThai = ChuanHoaTrangThai(trangThai);
            query = query.Where(x => x.TrangThai == maTrangThai);
        }
        if (giaTu.HasValue)
        {
            query = query.Where(x => x.DonGia >= giaTu.Value);
        }
        if (giaDen.HasValue)
        {
            query = query.Where(x => x.DonGia <= giaDen.Value);
        }
        var danhSach = await query.OrderBy(x => x.MaDichVu).ToListAsync();
        return danhSach.Select(TaoResponse).ToList();
    }

    public static bool TrangThaiHopLe(string? trangThai) =>
        string.IsNullOrWhiteSpace(trangThai) || ChuanHoaTrangThai(trangThai) != null;

    private static string? ChuanHoaTrangThai(string trangThai) => trangThai.Trim() switch
    {
        "Hoạt động" or "DANG_HOAT_DONG" => "DANG_HOAT_DONG",
        "Ngừng hoạt động" or "NGUNG_HOAT_DONG" => "NGUNG_HOAT_DONG",
        _ => null
    };

    private static string? KiemTraThongTin(string ten, decimal? donGia, ushort? thoiLuong)
    {
        if (ten.Length == 0 || ten.Length > 150)
        {
            return "Tên dịch vụ phải có từ 1 đến 150 ký tự.";
        }
        if (donGia is null or < 0 or > 9999999999999.99m)
        {
            return "Đơn giá phải từ 0 đến 9.999.999.999.999,99.";
        }
        if (thoiLuong == 0)
        {
            return "Thời lượng dự kiến phải lớn hơn 0 phút.";
        }
        return null;
    }

    private static bool ThuDocMaDichVu(string ma, out ulong id)
    {
        id = 0;
        return ma.StartsWith("DV_", StringComparison.OrdinalIgnoreCase) &&
            ulong.TryParse(ma.AsSpan(3), out id) && id > 0;
    }

    private static DichVuResponse TaoResponse(DichVu dichVu) => new()
    {
        MaDichVu = $"DV_{dichVu.MaDichVu:D4}",
        TenDichVu = dichVu.TenDichVu,
        MoTa = dichVu.MoTa,
        DonGia = dichVu.DonGia,
        TrangThai = dichVu.TrangThai == "DANG_HOAT_DONG" ? "Hoạt động" : "Ngừng hoạt động",
        ThoiLuongDuKienPhut = dichVu.ThoiLuongDuKienPhut
    };
}
