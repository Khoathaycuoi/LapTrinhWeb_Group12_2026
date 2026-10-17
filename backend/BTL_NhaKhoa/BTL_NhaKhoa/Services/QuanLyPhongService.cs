using BTL_NhaKhoa.Data;
using BTL_NhaKhoa.DTOs;
using BTL_NhaKhoa.Entities;
using Microsoft.EntityFrameworkCore;

namespace BTL_NhaKhoa.Services;

public sealed class QuanLyPhongService(NhaKhoaDbContext db)
{
    public async Task<(string? Loi, string? MaPhong)> ThemAsync(ThemPhongRequest yeuCau)
    {
        var ten = yeuCau.TenPhong.Trim();
        if (ten.Length == 0) return ("Tên phòng không được để trống.", null);
        if (await db.Phongs.AnyAsync(x => x.TenPhong == ten))
            return ("Tên phòng đã tồn tại.", null);
        var phong = new Phong { TenPhong = ten };
        db.Phongs.Add(phong);
        await db.SaveChangesAsync();
        return (null, $"PH_{phong.MaPhong:D4}");
    }

    public async Task<IReadOnlyList<DanhSachPhongResponse>> LayDanhSachAsync()
    {
        var phongs = await db.Phongs.AsNoTracking()
            .OrderBy(x => x.MaPhong).ToListAsync();
        return phongs.Select(TaoPhongResponse).ToList();
    }

    public async Task<DanhSachPhanTrangResponse<DanhSachPhongResponse>> LayTrangAsync(int page, int pageSize)
    {
        var query = db.Phongs.AsNoTracking();
        var totalItems = await query.CountAsync();
        var phongs = await query.OrderBy(x => x.MaPhong)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new DanhSachPhanTrangResponse<DanhSachPhongResponse>(
            phongs.Select(TaoPhongResponse).ToList(), totalItems, page, pageSize);
    }

    private static DanhSachPhongResponse TaoPhongResponse(Phong phong) => new()
    {
        MaPhong = $"PH_{phong.MaPhong:D4}",
        TenPhong = phong.TenPhong,
        TrangThai = phong.TrangThai
    };

    public async Task<(string? Loi, bool KhongTimThay, IReadOnlyList<LichSuPhongResponse>? DuLieu)>
        LayLichSuAsync(string maPhong, DateOnly? tuNgay, DateOnly? denNgay)
    {
        if (!QuanLyLichHenService.ThuDocMa(maPhong, "PH_", out var id))
            return ("Mã phòng không hợp lệ.", false, null);
        if (tuNgay.HasValue && denNgay.HasValue && tuNgay > denNgay)
            return ("Từ ngày không được sau đến ngày.", false, null);
        if (!await db.Phongs.AnyAsync(x => x.MaPhong == id))
            return ("Không tìm thấy phòng.", true, null);

        var query = db.LichHens.AsNoTracking()
            .Where(x => x.MaPhong == id);
        if (tuNgay.HasValue)
        {
            var batDau = tuNgay.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(x => x.ThoiGianBatDau >= batDau);
        }
        if (denNgay.HasValue)
        {
            var ketThuc = denNgay.Value.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(x => x.ThoiGianBatDau <= ketThuc);
        }

        var lichHens = await query
            .Include(x => x.KhachHang).ThenInclude(x => x.ConNguoi)
            .Include(x => x.BacSi).ThenInclude(x => x!.NhanVien).ThenInclude(x => x.ConNguoi)
            .Include(x => x.NguoiHuyTaiKhoan).ThenInclude(x => x!.ConNguoi)
            .OrderByDescending(x => x.ThoiGianBatDau)
            .ThenByDescending(x => x.MaLichHen)
            .ToListAsync();
        return (null, false, lichHens.Select(x => TaoLichSuResponse(x, id)).ToList());
    }

    public async Task<(string? Loi, bool KhongTimThay,
        DanhSachPhanTrangResponse<LichSuPhongResponse>? DuLieu)> LayTrangLichSuAsync(
        string maPhong, DateOnly? tuNgay, DateOnly? denNgay, int page, int pageSize)
    {
        if (!QuanLyLichHenService.ThuDocMa(maPhong, "PH_", out var id))
            return ("Mã phòng không hợp lệ.", false, null);
        if (tuNgay.HasValue && denNgay.HasValue && tuNgay > denNgay)
            return ("Từ ngày không được sau đến ngày.", false, null);
        if (!await db.Phongs.AnyAsync(x => x.MaPhong == id))
            return ("Không tìm thấy phòng.", true, null);

        var query = db.LichHens.AsNoTracking().Where(x => x.MaPhong == id);
        if (tuNgay.HasValue)
        {
            var batDau = tuNgay.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(x => x.ThoiGianBatDau >= batDau);
        }
        if (denNgay.HasValue)
        {
            var ketThuc = denNgay.Value.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(x => x.ThoiGianBatDau <= ketThuc);
        }
        var totalItems = await query.CountAsync();
        var lichHens = await query
            .Include(x => x.KhachHang).ThenInclude(x => x.ConNguoi)
            .Include(x => x.BacSi).ThenInclude(x => x!.NhanVien).ThenInclude(x => x.ConNguoi)
            .Include(x => x.NguoiHuyTaiKhoan).ThenInclude(x => x!.ConNguoi)
            .OrderByDescending(x => x.ThoiGianBatDau).ThenByDescending(x => x.MaLichHen)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (null, false, new DanhSachPhanTrangResponse<LichSuPhongResponse>(
            lichHens.Select(x => TaoLichSuResponse(x, id)).ToList(), totalItems, page, pageSize));
    }

    private static LichSuPhongResponse TaoLichSuResponse(LichHen x, ulong id) => new()
        {
            MaLichHen = $"LH_{x.MaLichHen:D4}",
            MaPhong = $"PH_{id:D4}",
            ThoiGianBatDau = x.ThoiGianBatDau,
            ThoiGianKetThucDuKien = x.ThoiGianBatDau.AddMinutes(x.ThoiLuongDuKienPhut),
            TrangThai = x.TrangThai,
            MaKhachHang = $"KH_{x.MaKhachHang:D4}",
            TenKhachHang = x.KhachHang.ConNguoi.HoTen,
            MaBacSi = x.MaBacSi.HasValue ? $"BS_{x.MaBacSi.Value:D4}" : null,
            TenBacSi = x.BacSi?.NhanVien.ConNguoi.HoTen,
            XacNhanLuc = x.XacNhanLuc,
            HuyLuc = x.HuyLuc,
            LyDoHuy = x.LyDoHuy,
            NguoiHuy = x.NguoiHuyTaiKhoan?.ConNguoi.HoTen,
            KhongDenLuc = x.KhongDenLuc,
            GhiChuKhongDen = x.GhiChuKhongDen
        };

    public async Task<string?> DoiTrangThaiAsync(string maPhong, string trangThai)
    {
        if (!QuanLyLichHenService.ThuDocMa(maPhong, "PH_", out var id))
            return "Mã phòng không hợp lệ.";
        var trangThaiMoi = trangThai.Trim().ToUpperInvariant();
        if (trangThaiMoi is not ("HOAT_DONG" or "TAM_NGUNG" or "BAO_TRI"))
            return "Trạng thái phòng không hợp lệ.";
        var phong = await db.Phongs.FirstOrDefaultAsync(x => x.MaPhong == id);
        if (phong == null) return "Không tìm thấy phòng.";
        if (trangThaiMoi != "HOAT_DONG")
        {
            var lucNay = DateTime.Now;
            var lichLienQuan = await db.LichHens.AsNoTracking()
                .Where(x => x.MaPhong == id && x.ThoiGianBatDau >= lucNay.Date &&
                    (x.TrangThai == "DA_XAC_NHAN" || x.TrangThai == "DA_DEN" || x.TrangThai == "DANG_KHAM"))
                .Select(x => new { x.ThoiGianBatDau, x.ThoiLuongDuKienPhut }).ToListAsync();
            if (lichLienQuan.Any(x => x.ThoiGianBatDau.AddMinutes(x.ThoiLuongDuKienPhut) > lucNay))
                return "Phòng còn lịch đã xác nhận; cần xử lý các lịch đó trước khi tạm ngừng hoặc bảo trì.";
        }
        phong.TrangThai = trangThaiMoi;
        await db.SaveChangesAsync();
        return null;
    }
}
