using BTL_NhaKhoa.Data;
using BTL_NhaKhoa.DTOs;
using BTL_NhaKhoa.Entities;
using Microsoft.EntityFrameworkCore;

namespace BTL_NhaKhoa.Services;

public sealed class DashboardService(NhaKhoaDbContext db)
{
    private static readonly string[] TrangThaiLichHen =
    [
        "CHO_XAC_NHAN", "DA_XAC_NHAN", "DA_DEN", "DANG_KHAM",
        "DA_HOAN_THANH", "KHONG_DEN", "DA_HUY", "TU_CHOI", "HET_HAN"
    ];

    public async Task<DashboardQuanLyResponse> LayQuanLyAsync()
    {
        var hienTai = DateTime.Now;
        var dauNgay = hienTai.Date;
        var dauNgayMai = dauNgay.AddDays(1);
        var dauThang = new DateTime(hienTai.Year, hienTai.Month, 1);
        var dauThangSau = dauThang.AddMonths(1);
        var dauTuanHienTai = dauNgay.AddDays(-(((int)dauNgay.DayOfWeek + 6) % 7));
        var dauBieuDoDoanhThu = dauTuanHienTai.AddDays(-21);
        var dauBieuDoLich = dauNgay.AddDays(-6);

        var soLichHomNay = await LichDangTheoDoi()
            .CountAsync(x => x.ThoiGianBatDau >= dauNgay && x.ThoiGianBatDau < dauNgayMai);
        var soLichChoXacNhan = await db.LichHens.AsNoTracking()
            .CountAsync(x => x.TrangThai == "CHO_XAC_NHAN" && x.ThoiGianBatDau > hienTai);
        var doanhThuThang = await db.ThanhToans.AsNoTracking()
            .Where(x => x.TrangThai == "DA_XAC_NHAN" && x.HoaDon.TrangThai != "DA_HUY" &&
                x.ThoiDiemThanhToan >= dauThang && x.ThoiDiemThanhToan < dauThangSau)
            .SumAsync(x => (decimal?)x.SoTien) ?? 0m;
        var tongCongNo = await TinhTongCongNoAsync(null);

        var soPhongHoatDong = await db.Phongs.AsNoTracking()
            .CountAsync(x => x.TrangThai == "HOAT_DONG");
        var soPhongTamNgung = await db.Phongs.AsNoTracking()
            .CountAsync(x => x.TrangThai == "TAM_NGUNG");
        var soPhongBaoTri = await db.Phongs.AsNoTracking()
            .CountAsync(x => x.TrangThai == "BAO_TRI");

        var khoanThuBonTuan = await db.ThanhToans.AsNoTracking()
            .Where(x => x.TrangThai == "DA_XAC_NHAN" && x.HoaDon.TrangThai != "DA_HUY" &&
                x.ThoiDiemThanhToan >= dauBieuDoDoanhThu && x.ThoiDiemThanhToan <= hienTai)
            .Select(x => new { x.ThoiDiemThanhToan, x.SoTien }).ToListAsync();
        var doanhThuTheoTuan = khoanThuBonTuan
            .GroupBy(x => x.ThoiDiemThanhToan.Date.AddDays(-(((int)x.ThoiDiemThanhToan.DayOfWeek + 6) % 7)))
            .ToDictionary(x => x.Key, x => x.Sum(y => y.SoTien));
        var bieuDoDoanhThu = Enumerable.Range(0, 4).Select(i =>
        {
            var dauTuan = dauBieuDoDoanhThu.AddDays(i * 7);
            return new DoanhThuTheoTuanResponse
            {
                TuanBatDau = DateOnly.FromDateTime(dauTuan),
                TuanKetThuc = DateOnly.FromDateTime(dauTuan.AddDays(6)),
                SoTien = doanhThuTheoTuan.GetValueOrDefault(dauTuan)
            };
        }).ToList();

        var thongKeLich = await db.LichHens.AsNoTracking()
            .Where(x => x.ThoiGianBatDau >= dauBieuDoLich && x.ThoiGianBatDau < dauNgayMai)
            .GroupBy(x => x.TrangThai)
            .Select(x => new { TrangThai = x.Key, SoLuong = x.Count() })
            .ToListAsync();
        var soLuongTheoTrangThai = thongKeLich.ToDictionary(x => x.TrangThai, x => x.SoLuong);

        return new DashboardQuanLyResponse
        {
            Ngay = DateOnly.FromDateTime(dauNgay),
            SoLichHomNay = soLichHomNay,
            SoLichChoXacNhan = soLichChoXacNhan,
            DoanhThuThang = doanhThuThang,
            TongCongNo = tongCongNo,
            SoPhongHoatDong = soPhongHoatDong,
            SoPhongTamNgung = soPhongTamNgung,
            SoPhongBaoTri = soPhongBaoTri,
            DoanhThuBonTuan = bieuDoDoanhThu,
            LichHenTheoTrangThaiBayNgay = TrangThaiLichHen.Select(trangThai =>
                new LichHenTheoTrangThaiResponse
                {
                    TrangThai = trangThai,
                    SoLuong = soLuongTheoTrangThai.GetValueOrDefault(trangThai)
                }).ToList()
        };
    }

    public async Task<DashboardLeTanResponse> LayLeTanAsync()
    {
        var hienTai = DateTime.Now;
        var dauNgay = hienTai.Date;
        var dauNgayMai = dauNgay.AddDays(1);
        var soLichChoXacNhan = await db.LichHens.AsNoTracking()
            .CountAsync(x => x.TrangThai == "CHO_XAC_NHAN" && x.ThoiGianBatDau > hienTai);
        var soLichHomNay = await LichDangTheoDoi()
            .CountAsync(x => x.ThoiGianBatDau >= dauNgay && x.ThoiGianBatDau < dauNgayMai);
        var soKhachChoKham = await db.LichHens.AsNoTracking()
            .CountAsync(x => x.TrangThai == "DA_DEN" &&
                x.ThoiGianBatDau >= dauNgay && x.ThoiGianBatDau < dauNgayMai);
        var soHoaDonConNo = await HoaDonConNo(null).CountAsync();
        var lichCho = await TruyVanLich()
            .Where(x => x.TrangThai == "CHO_XAC_NHAN" && x.ThoiGianBatDau > hienTai)
            .OrderBy(x => x.ThoiGianBatDau).ThenBy(x => x.MaLichHen)
            .Take(5).ToListAsync();
        var lichSapDienRa = await TruyVanLich()
            .Where(x => x.TrangThai == "DA_XAC_NHAN" &&
                x.ThoiGianBatDau >= hienTai && x.ThoiGianBatDau < dauNgayMai)
            .OrderBy(x => x.ThoiGianBatDau).ThenBy(x => x.MaLichHen)
            .Take(5).ToListAsync();

        return new DashboardLeTanResponse
        {
            Ngay = DateOnly.FromDateTime(dauNgay),
            SoLichChoXacNhan = soLichChoXacNhan,
            SoLichHomNay = soLichHomNay,
            SoKhachChoKham = soKhachChoKham,
            SoHoaDonConNo = soHoaDonConNo,
            LichChoXacNhanGanNhat = lichCho.Select(TaoLichResponse).ToList(),
            LichSapDienRa = lichSapDienRa.Select(TaoLichResponse).ToList()
        };
    }

    public async Task<DashboardBacSiResponse?> LayBacSiAsync(ulong maTaiKhoan)
    {
        var maConNguoi = await TimMaConNguoiAsync(maTaiKhoan);
        if (!maConNguoi.HasValue) return null;
        var maBacSi = await db.BacSis.AsNoTracking()
            .Where(x => x.NhanVien.MaConNguoi == maConNguoi.Value)
            .Select(x => (ulong?)x.MaBacSi).FirstOrDefaultAsync();
        if (!maBacSi.HasValue) return null;

        var hienTai = DateTime.Now;
        var dauNgay = hienTai.Date;
        var dauNgayMai = dauNgay.AddDays(1);
        var soLichHomNay = await LichDangTheoDoi()
            .CountAsync(x => x.MaBacSi == maBacSi &&
                x.ThoiGianBatDau >= dauNgay && x.ThoiGianBatDau < dauNgayMai);
        var soLichChoXacNhan = await db.LichHens.AsNoTracking()
            .CountAsync(x => x.MaBacSi == maBacSi && x.TrangThai == "CHO_XAC_NHAN" &&
                x.ThoiGianBatDau > hienTai);
        var soKhachChoKham = await db.LichHens.AsNoTracking()
            .CountAsync(x => x.MaBacSi == maBacSi && x.TrangThai == "DA_DEN" &&
                x.ThoiGianBatDau >= dauNgay && x.ThoiGianBatDau < dauNgayMai);
        var soCaDangKham = await db.LichHens.AsNoTracking()
            .CountAsync(x => x.MaBacSi == maBacSi && x.TrangThai == "DANG_KHAM");
        var soHoSoChuaChot = await db.HoSoKhams.AsNoTracking()
            .CountAsync(x => x.MaBacSi == maBacSi && x.ChotLuc == null &&
                x.LichHen != null && x.LichHen.TrangThai == "DANG_KHAM");
        var lichHomNay = await TruyVanLich()
            .Where(x => x.MaBacSi == maBacSi &&
                x.TrangThai != "DA_HUY" && x.TrangThai != "TU_CHOI" &&
                x.TrangThai != "HET_HAN" &&
                (x.TrangThai != "CHO_XAC_NHAN" || x.ThoiGianBatDau > hienTai) &&
                x.ThoiGianBatDau >= dauNgay && x.ThoiGianBatDau < dauNgayMai)
            .OrderBy(x => x.ThoiGianBatDau).ThenBy(x => x.MaLichHen)
            .Take(20).ToListAsync();

        return new DashboardBacSiResponse
        {
            Ngay = DateOnly.FromDateTime(dauNgay),
            SoLichHomNay = soLichHomNay,
            SoLichChoXacNhan = soLichChoXacNhan,
            SoKhachChoKham = soKhachChoKham,
            SoCaDangKham = soCaDangKham,
            SoHoSoChuaChot = soHoSoChuaChot,
            LichHomNay = lichHomNay.Select(TaoLichResponse).ToList()
        };
    }

    public async Task<DashboardKhachHangResponse?> LayKhachHangAsync(ulong maTaiKhoan)
    {
        var maConNguoi = await TimMaConNguoiAsync(maTaiKhoan);
        if (!maConNguoi.HasValue) return null;
        var maKhachHang = await db.KhachHangs.AsNoTracking()
            .Where(x => x.MaConNguoi == maConNguoi.Value)
            .Select(x => (ulong?)x.MaKhachHang).FirstOrDefaultAsync();
        if (!maKhachHang.HasValue) return null;

        var hienTai = DateTime.Now;
        var lichSapToi = TruyVanLich().Where(x => x.MaKhachHang == maKhachHang &&
            x.TrangThai == "DA_XAC_NHAN" &&
            x.ThoiGianBatDau >= hienTai);
        var soLichSapToi = await lichSapToi.CountAsync();
        var lichGanNhat = await lichSapToi.OrderBy(x => x.ThoiGianBatDau)
            .ThenBy(x => x.MaLichHen).FirstOrDefaultAsync();

        var hoaDonConNo = HoaDonConNo(maKhachHang.Value);
        var soHoaDonConNo = await hoaDonConNo.CountAsync();
        var tongCongNo = await TinhTongCongNoAsync(maKhachHang.Value);
        var hoaDonGanNhat = await hoaDonConNo
            .Include(x => x.ThanhToans)
            .OrderByDescending(x => x.NgayLap).ThenByDescending(x => x.MaHoaDon)
            .Take(3)
            .AsSplitQuery().ToListAsync();
        var lanKhamGanNhat = await db.HoSoKhams.AsNoTracking()
            .Where(x => x.MaKhachHang == maKhachHang && x.ChotLuc != null)
            .OrderByDescending(x => x.NgayKham).ThenByDescending(x => x.MaHoSoKham)
            .Select(x => new
            {
                x.MaHoSoKham,
                x.NgayKham,
                x.MaBacSi,
                x.TomTatChoKhach,
                x.ChotLuc
            }).FirstOrDefaultAsync();

        return new DashboardKhachHangResponse
        {
            Ngay = DateOnly.FromDateTime(hienTai),
            SoLichSapToi = soLichSapToi,
            LichSapToiGanNhat = lichGanNhat == null ? null : TaoLichResponse(lichGanNhat),
            SoHoaDonConNo = soHoaDonConNo,
            TongCongNo = tongCongNo,
            HoaDonConNoGanNhat = hoaDonGanNhat.Select(x => new HoaDonDashboardResponse
            {
                MaHoaDon = TaoMa("HD_", x.MaHoaDon),
                NgayLap = x.NgayLap,
                TongTien = x.TongTien,
                DaThanhToan = x.ThanhToans.Where(t => t.TrangThai == "DA_XAC_NHAN")
                    .Sum(t => t.SoTien),
                ConNo = x.TongTien - x.ThanhToans.Where(t => t.TrangThai == "DA_XAC_NHAN")
                    .Sum(t => t.SoTien)
            }).ToList(),
            LanKhamGanNhat = lanKhamGanNhat == null ? null : new TomTatHoSoKhamResponse
            {
                MaHoSoKham = TaoMa("HSK_", lanKhamGanNhat.MaHoSoKham),
                NgayKham = lanKhamGanNhat.NgayKham,
                MaBacSi = TaoMa("BS_", lanKhamGanNhat.MaBacSi),
                TomTat = lanKhamGanNhat.TomTatChoKhach ?? string.Empty,
                ChotLuc = lanKhamGanNhat.ChotLuc
            }
        };
    }

    private Task<ulong?> TimMaConNguoiAsync(ulong maTaiKhoan) =>
        db.TaiKhoans.AsNoTracking().Where(x => x.MaTaiKhoan == maTaiKhoan)
            .Select(x => (ulong?)x.MaConNguoi).FirstOrDefaultAsync();

    private IQueryable<LichHen> LichDangTheoDoi()
    {
        var lucNay = DateTime.Now;
        return db.LichHens.AsNoTracking()
            .Where(x => x.TrangThai != "DA_HUY" && x.TrangThai != "TU_CHOI" &&
                x.TrangThai != "HET_HAN" &&
                (x.TrangThai != "CHO_XAC_NHAN" || x.ThoiGianBatDau > lucNay));
    }

    private IQueryable<LichHen> TruyVanLich() => db.LichHens.AsNoTracking()
        .Include(x => x.KhachHang).ThenInclude(x => x.ConNguoi)
        .Include(x => x.BacSi).ThenInclude(x => x!.NhanVien).ThenInclude(x => x.ConNguoi);

    private IQueryable<HoaDon> HoaDonConNo(ulong? maKhachHang)
    {
        var query = db.HoaDons.AsNoTracking()
            .Where(x => x.TrangThai == "CHUA_THANH_TOAN" ||
                x.TrangThai == "THANH_TOAN_MOT_PHAN");
        return maKhachHang.HasValue
            ? query.Where(x => x.HoSoKham.MaKhachHang == maKhachHang.Value)
            : query;
    }

    private async Task<decimal> TinhTongCongNoAsync(ulong? maKhachHang)
    {
        var hoaDons = HoaDonConNo(maKhachHang);
        var tongHoaDon = await hoaDons.SumAsync(x => (decimal?)x.TongTien) ?? 0m;
        var daThu = await db.ThanhToans.AsNoTracking()
            .Where(x => x.TrangThai == "DA_XAC_NHAN" &&
                (x.HoaDon.TrangThai == "CHUA_THANH_TOAN" ||
                 x.HoaDon.TrangThai == "THANH_TOAN_MOT_PHAN") &&
                (!maKhachHang.HasValue ||
                 x.HoaDon.HoSoKham.MaKhachHang == maKhachHang.Value))
            .SumAsync(x => (decimal?)x.SoTien) ?? 0m;
        return tongHoaDon - daThu;
    }

    private static LichHenDashboardResponse TaoLichResponse(LichHen lich) => new()
    {
        MaLichHen = TaoMa("LH_", lich.MaLichHen),
        ThoiGianBatDau = lich.ThoiGianBatDau,
        TrangThai = lich.TrangThai,
        MaKhachHang = TaoMa("KH_", lich.MaKhachHang),
        TenKhachHang = lich.KhachHang.ConNguoi.HoTen,
        MaBacSi = lich.MaBacSi.HasValue ? TaoMa("BS_", lich.MaBacSi.Value) : null,
        TenBacSi = lich.BacSi?.NhanVien.ConNguoi.HoTen
    };

    private static string TaoMa(string tienTo, ulong id) => $"{tienTo}{id:D4}";
}
