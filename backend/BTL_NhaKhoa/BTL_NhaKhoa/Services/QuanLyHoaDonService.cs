using BTL_NhaKhoa.Data;
using BTL_NhaKhoa.DTOs;
using BTL_NhaKhoa.Entities;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace BTL_NhaKhoa.Services;

public sealed record KetQuaHoaDon(string? Loi = null, bool KhongTimThay = false,
    bool KhongCoQuyen = false, string? MaHoaDon = null, string? MaThanhToan = null);

public sealed class QuanLyHoaDonService(NhaKhoaDbContext db)
{
    public async Task<KetQuaHoaDon> LapAsync(LapHoaDonRequest yeuCau, ulong maTaiKhoan)
    {
        if (!QuanLyLichHenService.ThuDocMa(yeuCau.MaHoSoKham, "HSK_", out var maHoSoKham))
            return new KetQuaHoaDon("Mã hồ sơ khám không hợp lệ.");

        await using var giaoDich = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var hoSo = await db.HoSoKhams.Include(x => x.LichHen)
            .Include(x => x.DichVuThucHiens)
            .FirstOrDefaultAsync(x => x.MaHoSoKham == maHoSoKham);
        if (hoSo == null) return new KetQuaHoaDon("Không tìm thấy hồ sơ khám.", KhongTimThay: true);
        if (hoSo.ChotLuc == null || hoSo.LichHen?.TrangThai != "DA_HOAN_THANH")
            return new KetQuaHoaDon("Chỉ lập hóa đơn sau khi hồ sơ đã chốt và lịch khám hoàn thành.");
        if (hoSo.DichVuThucHiens.Count == 0)
            return new KetQuaHoaDon("Hồ sơ chưa có dịch vụ đã thực hiện.");
        if (await db.HoaDons.AnyAsync(x => x.MaHoSoKham == maHoSoKham))
            return new KetQuaHoaDon("Hồ sơ này đã có hóa đơn.");

        var chiTiets = hoSo.DichVuThucHiens.Select(x => new ChiTietHoaDon
        {
            MaDichVuThucHien = x.MaDichVuThucHien,
            TenDichVu = x.TenDichVuTaiThoiDiemThucHien,
            DonGia = x.DonGiaThucTe,
            SoLuong = x.SoLuong
        }).ToList();
        var tongTien = chiTiets.Sum(x => x.DonGia * x.SoLuong);
        if (tongTien > 9999999999999.99m)
            return new KetQuaHoaDon("Tổng tiền vượt giới hạn của hóa đơn.");

        var hoaDon = new HoaDon
        {
            MaHoSoKham = maHoSoKham,
            NguoiLapTaiKhoanId = maTaiKhoan,
            NgayLap = DateTime.Now,
            TongTien = tongTien,
            TrangThai = tongTien == 0 ? "DA_THANH_TOAN" : "CHUA_THANH_TOAN",
            ChiTietHoaDons = chiTiets
        };
        db.HoaDons.Add(hoaDon);
        await db.SaveChangesAsync();
        await giaoDich.CommitAsync();
        return new KetQuaHoaDon(MaHoaDon: TaoMa("HD_", hoaDon.MaHoaDon));
    }

    public async Task<IReadOnlyList<HoaDonResponse>> LayDanhSachAsync(
        ulong maTaiKhoan, string? maKhachHang, bool conNo = false)
    {
        var taiKhoan = await db.TaiKhoans.AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaTaiKhoan == maTaiKhoan);
        if (taiKhoan == null) return [];
        var query = TruyVanHoaDon();
        if (taiKhoan.MaVaiTro == "KHACH_HANG")
            query = query.Where(x => x.HoSoKham.KhachHang.MaConNguoi == taiKhoan.MaConNguoi);
        else if (taiKhoan.MaVaiTro is "LE_TAN" or "QUAN_LY")
        {
            if (!string.IsNullOrWhiteSpace(maKhachHang))
            {
                if (!QuanLyLichHenService.ThuDocMa(maKhachHang, "KH_", out var idKhachHang))
                    return [];
                query = query.Where(x => x.HoSoKham.MaKhachHang == idKhachHang);
            }
        }
        else return [];

        if (conNo)
            query = query.Where(x => x.TrangThai == "CHUA_THANH_TOAN" ||
                x.TrangThai == "THANH_TOAN_MOT_PHAN");

        var hoaDons = await query.OrderByDescending(x => x.NgayLap).ToListAsync();
        return hoaDons.Select(TaoResponse).ToList();
    }

    public async Task<(KetQuaHoaDon KetQua, HoaDonResponse? DuLieu)> LayChiTietAsync(
        string maHoaDon, ulong maTaiKhoan)
    {
        if (!QuanLyLichHenService.ThuDocMa(maHoaDon, "HD_", out var id))
            return (new KetQuaHoaDon("Mã hóa đơn không hợp lệ."), null);
        var hoaDon = await TruyVanHoaDon().FirstOrDefaultAsync(x => x.MaHoaDon == id);
        if (hoaDon == null)
            return (new KetQuaHoaDon("Không tìm thấy hóa đơn.", KhongTimThay: true), null);
        var taiKhoan = await db.TaiKhoans.AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaTaiKhoan == maTaiKhoan);
        if (taiKhoan == null) return (new KetQuaHoaDon("Không tìm thấy tài khoản.", KhongTimThay: true), null);
        if (taiKhoan.MaVaiTro is not ("LE_TAN" or "QUAN_LY") &&
            (taiKhoan.MaVaiTro != "KHACH_HANG" ||
             hoaDon.HoSoKham.KhachHang.MaConNguoi != taiKhoan.MaConNguoi))
            return (new KetQuaHoaDon("Bạn không có quyền xem hóa đơn này.", KhongCoQuyen: true), null);
        return (new KetQuaHoaDon(), TaoResponse(hoaDon));
    }

    public async Task<KetQuaHoaDon> GhiNhanThanhToanAsync(
        string maHoaDon, GhiNhanThanhToanRequest yeuCau, ulong maTaiKhoan)
    {
        if (!QuanLyLichHenService.ThuDocMa(maHoaDon, "HD_", out var id))
            return new KetQuaHoaDon("Mã hóa đơn không hợp lệ.");
        if (yeuCau.SoTien <= 0 || decimal.Round(yeuCau.SoTien, 2) != yeuCau.SoTien)
            return new KetQuaHoaDon("Số tiền phải lớn hơn 0 và có tối đa hai chữ số thập phân.");
        var phuongThuc = yeuCau.PhuongThuc.Trim().ToUpperInvariant();
        if (phuongThuc is not ("TIEN_MAT" or "CHUYEN_KHOAN" or "KHAC"))
            return new KetQuaHoaDon("Phương thức phải là TIEN_MAT, CHUYEN_KHOAN hoặc KHAC.");

        await using var giaoDich = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var hoaDon = await db.HoaDons.Include(x => x.ThanhToans)
            .FirstOrDefaultAsync(x => x.MaHoaDon == id);
        if (hoaDon == null) return new KetQuaHoaDon("Không tìm thấy hóa đơn.", KhongTimThay: true);
        if (hoaDon.TrangThai == "DA_HUY") return new KetQuaHoaDon("Hóa đơn đã hủy.");
        var daThu = hoaDon.ThanhToans.Where(x => x.TrangThai == "DA_XAC_NHAN").Sum(x => x.SoTien);
        if (yeuCau.SoTien > hoaDon.TongTien - daThu)
            return new KetQuaHoaDon("Số tiền thu vượt quá số còn nợ của hóa đơn.");

        var thanhToan = new ThanhToan
        {
            MaHoaDon = id,
            NguoiXacNhanTaiKhoanId = maTaiKhoan,
            SoTien = yeuCau.SoTien,
            PhuongThuc = phuongThuc,
            TrangThai = "DA_XAC_NHAN",
            ThoiDiemThanhToan = DateTime.Now,
            GhiChu = yeuCau.GhiChu?.Trim()
        };
        db.ThanhToans.Add(thanhToan);
        hoaDon.TrangThai = daThu + yeuCau.SoTien == hoaDon.TongTien
            ? "DA_THANH_TOAN" : "THANH_TOAN_MOT_PHAN";
        await db.SaveChangesAsync();
        await giaoDich.CommitAsync();
        return new KetQuaHoaDon(MaThanhToan: TaoMa("TT_", thanhToan.MaThanhToan));
    }

    public async Task<KetQuaHoaDon> HuyThanhToanAsync(
        string maHoaDon, string maThanhToan, HuyThanhToanRequest yeuCau, ulong maTaiKhoan)
    {
        if (!QuanLyLichHenService.ThuDocMa(maHoaDon, "HD_", out var idHoaDon) ||
            !QuanLyLichHenService.ThuDocMa(maThanhToan, "TT_", out var idThanhToan))
            return new KetQuaHoaDon("Mã hóa đơn hoặc mã thanh toán không hợp lệ.");
        var lyDo = yeuCau.LyDo?.Trim();
        if (string.IsNullOrWhiteSpace(lyDo)) return new KetQuaHoaDon("Phải nhập lý do hủy khoản thu.");

        await using var giaoDich = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var hoaDon = await db.HoaDons.Include(x => x.ThanhToans)
            .FirstOrDefaultAsync(x => x.MaHoaDon == idHoaDon);
        if (hoaDon == null) return new KetQuaHoaDon("Không tìm thấy hóa đơn.", KhongTimThay: true);
        var thanhToan = hoaDon.ThanhToans.FirstOrDefault(x => x.MaThanhToan == idThanhToan);
        if (thanhToan == null) return new KetQuaHoaDon("Không tìm thấy khoản thu.", KhongTimThay: true);
        if (thanhToan.TrangThai != "DA_XAC_NHAN")
            return new KetQuaHoaDon("Khoản thu đã được hủy trước đó.");

        thanhToan.TrangThai = "DA_HUY";
        thanhToan.LyDoHuy = lyDo;
        thanhToan.NguoiHuyTaiKhoanId = maTaiKhoan;
        thanhToan.HuyLuc = DateTime.Now;
        var daThu = hoaDon.ThanhToans.Where(x => x.TrangThai == "DA_XAC_NHAN").Sum(x => x.SoTien);
        hoaDon.TrangThai = daThu == 0 ? "CHUA_THANH_TOAN"
            : daThu >= hoaDon.TongTien ? "DA_THANH_TOAN" : "THANH_TOAN_MOT_PHAN";
        await db.SaveChangesAsync();
        await giaoDich.CommitAsync();
        return new KetQuaHoaDon();
    }

    private IQueryable<HoaDon> TruyVanHoaDon() => db.HoaDons.AsNoTracking()
        .Include(x => x.HoSoKham).ThenInclude(x => x.KhachHang)
        .Include(x => x.ChiTietHoaDons)
        .Include(x => x.ThanhToans).ThenInclude(x => x.NguoiXacNhanTaiKhoan)
            .ThenInclude(x => x.ConNguoi)
        .Include(x => x.ThanhToans).ThenInclude(x => x.NguoiHuyTaiKhoan)
            .ThenInclude(x => x!.ConNguoi)
        .AsSplitQuery();

    private static HoaDonResponse TaoResponse(HoaDon hoaDon)
    {
        var daThu = hoaDon.ThanhToans.Where(x => x.TrangThai == "DA_XAC_NHAN")
            .Sum(x => x.SoTien);
        return new HoaDonResponse
        {
            MaHoaDon = TaoMa("HD_", hoaDon.MaHoaDon),
            MaHoSoKham = TaoMa("HSK_", hoaDon.MaHoSoKham),
            MaKhachHang = TaoMa("KH_", hoaDon.HoSoKham.MaKhachHang),
            NgayLap = hoaDon.NgayLap,
            TongTien = hoaDon.TongTien,
            DaThanhToan = daThu,
            ConNo = hoaDon.TongTien - daThu,
            TrangThai = hoaDon.TrangThai,
            ChiTietHoaDons = hoaDon.ChiTietHoaDons.OrderBy(x => x.MaChiTietHoaDon)
                .Select(x => new ChiTietHoaDonResponse
                {
                    MaDichVuThucHien = TaoMa("DVTH_", x.MaDichVuThucHien),
                    TenDichVu = x.TenDichVu,
                    DonGia = x.DonGia,
                    SoLuong = x.SoLuong,
                    ThanhTien = x.ThanhTien
                }).ToList(),
            ThanhToans = hoaDon.ThanhToans.OrderBy(x => x.MaThanhToan)
                .Select(x => new ThanhToanResponse
                {
                    MaThanhToan = TaoMa("TT_", x.MaThanhToan),
                    SoTien = x.SoTien,
                    PhuongThuc = x.PhuongThuc,
                    TrangThai = x.TrangThai,
                    ThoiDiemThanhToan = x.ThoiDiemThanhToan,
                    NguoiXacNhan = x.NguoiXacNhanTaiKhoan.ConNguoi.HoTen,
                    GhiChu = x.GhiChu,
                    LyDoHuy = x.LyDoHuy,
                    HuyLuc = x.HuyLuc,
                    NguoiHuy = x.NguoiHuyTaiKhoan?.ConNguoi.HoTen
                }).ToList()
        };
    }

    private static string TaoMa(string tienTo, ulong id) => $"{tienTo}{id:D4}";
}
