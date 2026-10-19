using BTL_NhaKhoa.Data;
using BTL_NhaKhoa.DTOs;
using BTL_NhaKhoa.Entities;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace BTL_NhaKhoa.Services;

public sealed record KetQuaKhamBenh(string? Loi = null, bool KhongTimThay = false,
    bool KhongCoQuyen = false, string? MaHoSoKham = null, string? MaDonThuoc = null);

public sealed class QuanLyKhamBenhService(NhaKhoaDbContext db)
{
    public async Task<KetQuaKhamBenh> BatDauAsync(string maLichHen, ulong maTaiKhoan)
    {
        if (!QuanLyLichHenService.ThuDocMa(maLichHen, "LH_", out var id))
            return new KetQuaKhamBenh("Mã lịch hẹn không hợp lệ.");
        await using var giaoDich = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var lichHen = await db.LichHens.FirstOrDefaultAsync(x => x.MaLichHen == id);
        if (lichHen == null)
            return new KetQuaKhamBenh("Không tìm thấy lịch hẹn.", KhongTimThay: true);
        if (!lichHen.MaBacSi.HasValue || !await LaBacSiPhuTrachAsync(lichHen.MaBacSi.Value, maTaiKhoan))
            return new KetQuaKhamBenh("Bạn chỉ được bắt đầu lịch khám của chính mình.", KhongCoQuyen: true);
        if (lichHen.TrangThai != "DA_DEN")
            return new KetQuaKhamBenh("Chỉ bắt đầu khám sau khi Lễ tân đã tiếp nhận khách.");
        if (await db.HoSoKhams.AnyAsync(x => x.MaLichHen == id))
            return new KetQuaKhamBenh("Lịch hẹn đã có hồ sơ khám.");

        var lucNay = DateTime.Now;
        var hoSo = new HoSoKham
        {
            MaLichHen = id,
            MaKhachHang = lichHen.MaKhachHang,
            MaBacSi = lichHen.MaBacSi.Value,
            NgayKham = lucNay
        };
        db.HoSoKhams.Add(hoSo);
        lichHen.TrangThai = "DANG_KHAM";
        lichHen.BatDauKhamLuc = lucNay;
        await db.SaveChangesAsync();
        await giaoDich.CommitAsync();
        return new KetQuaKhamBenh(MaHoSoKham: TaoMa("HSK_", hoSo.MaHoSoKham));
    }

    public async Task<KetQuaKhamBenh> CapNhatAsync(string maHoSoKham,
        CapNhatHoSoKhamRequest yeuCau, ulong maTaiKhoan)
    {
        var (ketQua, hoSo) = await LayHoSoDangSuaAsync(maHoSoKham, maTaiKhoan);
        if (ketQua.Loi != null) return ketQua;
        if (yeuCau.NgayTaiKham.HasValue && yeuCau.NgayTaiKham.Value < DateOnly.FromDateTime(DateTime.Today))
            return new KetQuaKhamBenh("Ngày tái khám không được ở quá khứ.");
        hoSo!.TrieuChung = yeuCau.TrieuChung?.Trim();
        hoSo.ChanDoan = yeuCau.ChanDoan?.Trim();
        hoSo.TomTatChoKhach = yeuCau.TomTatChoKhach?.Trim();
        hoSo.GhiChuNoiBo = yeuCau.GhiChuNoiBo?.Trim();
        hoSo.NgayTaiKham = yeuCau.NgayTaiKham;
        await db.SaveChangesAsync();
        return new KetQuaKhamBenh();
    }

    public async Task<KetQuaKhamBenh> GhiNhanDichVuAsync(string maHoSoKham,
        GhiNhanDichVuThucHienRequest yeuCau, ulong maTaiKhoan)
    {
        var (ketQua, hoSo) = await LayHoSoDangSuaAsync(maHoSoKham, maTaiKhoan);
        if (ketQua.Loi != null) return ketQua;
        if (!QuanLyLichHenService.ThuDocMa(yeuCau.MaDichVu, "DV_", out var maDichVu))
            return new KetQuaKhamBenh("Mã dịch vụ không hợp lệ.");
        var chiTietLich = await db.ChiTietLichHens
            .Include(x => x.DichVu)
            .FirstOrDefaultAsync(x => x.MaLichHen == hoSo!.MaLichHen && x.MaDichVu == maDichVu);
        if (chiTietLich == null)
            return new KetQuaKhamBenh("Dịch vụ không có trong lịch hẹn; cần đặt lịch điều trị riêng nếu phát sinh thủ thuật.");
        if (await db.DichVuThucHiens.AnyAsync(x => x.MaChiTietLichHen == chiTietLich.MaChiTietLichHen))
            return new KetQuaKhamBenh("Dịch vụ này đã được ghi nhận thực hiện.");

        db.DichVuThucHiens.Add(new DichVuThucHien
        {
            MaHoSoKham = hoSo!.MaHoSoKham,
            MaChiTietLichHen = chiTietLich.MaChiTietLichHen,
            MaDichVu = maDichVu,
            TenDichVuTaiThoiDiemThucHien = chiTietLich.DichVu.TenDichVu,
            DonGiaThucTe = chiTietLich.DonGiaTaiThoiDiemDat,
            SoLuong = yeuCau.SoLuong,
            ThoiDiemThucHien = DateTime.Now,
            GhiChu = yeuCau.GhiChu?.Trim()
        });
        await db.SaveChangesAsync();
        return new KetQuaKhamBenh();
    }

    public async Task<KetQuaKhamBenh> KeDonAsync(string maHoSoKham,
        KeDonThuocRequest yeuCau, ulong maTaiKhoan)
    {
        var (ketQua, hoSo) = await LayHoSoDangSuaAsync(maHoSoKham, maTaiKhoan);
        if (ketQua.Loi != null) return ketQua;
        if (yeuCau.DanhSachThuoc == null || yeuCau.DanhSachThuoc.Count == 0 || yeuCau.DanhSachThuoc.Any(x =>
            x == null || string.IsNullOrWhiteSpace(x.TenThuoc) || x.TenThuoc.Trim().Length > 255 ||
            string.IsNullOrWhiteSpace(x.LieuDung) || x.LieuDung.Trim().Length > 255 ||
            x.HamLuong?.Length > 100 || x.SoLuong <= 0 ||
            x.SoLanMoiNgay == 0 || x.SoNgayDung == 0))
            return new KetQuaKhamBenh("Thông tin thuốc hoặc liều dùng không hợp lệ.");

        var donThuoc = new DonThuoc
        {
            MaHoSoKham = hoSo!.MaHoSoKham,
            NgayKe = DateTime.Now,
            GhiChu = yeuCau.GhiChu?.Trim(),
            ChiTietDonThuocs = yeuCau.DanhSachThuoc.Select(x => new ChiTietDonThuoc
            {
                TenThuoc = x.TenThuoc.Trim(),
                HamLuong = x.HamLuong?.Trim(),
                LieuDung = x.LieuDung.Trim(),
                SoLanMoiNgay = x.SoLanMoiNgay,
                SoNgayDung = x.SoNgayDung,
                SoLuong = x.SoLuong,
                HuongDan = x.HuongDan?.Trim()
            }).ToList()
        };
        db.DonThuocs.Add(donThuoc);
        await db.SaveChangesAsync();
        return new KetQuaKhamBenh(MaDonThuoc: TaoMa("DT_", donThuoc.MaDonThuoc));
    }

    public async Task<KetQuaKhamBenh> BoDichVuThucHienAsync(string maHoSoKham,
        string maDichVu, ulong maTaiKhoan)
    {
        var (ketQua, hoSo) = await LayHoSoDangSuaAsync(maHoSoKham, maTaiKhoan);
        if (ketQua.Loi != null) return ketQua;
        if (!QuanLyLichHenService.ThuDocMa(maDichVu, "DV_", out var idDichVu))
            return new KetQuaKhamBenh("Mã dịch vụ không hợp lệ.");
        var dichVu = await db.DichVuThucHiens.FirstOrDefaultAsync(x =>
            x.MaHoSoKham == hoSo!.MaHoSoKham && x.MaDichVu == idDichVu);
        if (dichVu == null)
            return new KetQuaKhamBenh("Dịch vụ chưa được ghi nhận thực hiện.", KhongTimThay: true);
        db.DichVuThucHiens.Remove(dichVu);
        await db.SaveChangesAsync();
        return new KetQuaKhamBenh();
    }

    public async Task<KetQuaKhamBenh> BoDonThuocAsync(string maHoSoKham,
        string maDonThuoc, ulong maTaiKhoan)
    {
        var (ketQua, hoSo) = await LayHoSoDangSuaAsync(maHoSoKham, maTaiKhoan);
        if (ketQua.Loi != null) return ketQua;
        if (!QuanLyLichHenService.ThuDocMa(maDonThuoc, "DT_", out var idDonThuoc))
            return new KetQuaKhamBenh("Mã đơn thuốc không hợp lệ.");
        var donThuoc = await db.DonThuocs.Include(x => x.ChiTietDonThuocs)
            .FirstOrDefaultAsync(x => x.MaDonThuoc == idDonThuoc &&
                x.MaHoSoKham == hoSo!.MaHoSoKham);
        if (donThuoc == null)
            return new KetQuaKhamBenh("Không tìm thấy đơn thuốc.", KhongTimThay: true);
        db.ChiTietDonThuocs.RemoveRange(donThuoc.ChiTietDonThuocs);
        db.DonThuocs.Remove(donThuoc);
        await db.SaveChangesAsync();
        return new KetQuaKhamBenh();
    }

    public async Task<KetQuaKhamBenh> ChotAsync(string maHoSoKham, ulong maTaiKhoan)
    {
        var (ketQua, hoSo) = await LayHoSoDangSuaAsync(maHoSoKham, maTaiKhoan);
        if (ketQua.Loi != null) return ketQua;
        if (string.IsNullOrWhiteSpace(hoSo!.ChanDoan) ||
            string.IsNullOrWhiteSpace(hoSo.TomTatChoKhach))
            return new KetQuaKhamBenh("Cần nhập chẩn đoán và tóm tắt cho khách trước khi chốt hồ sơ.");
        hoSo.ChotLuc = DateTime.Now;
        await db.SaveChangesAsync();
        return new KetQuaKhamBenh();
    }

    public async Task<(KetQuaKhamBenh KetQua, HoSoKhamResponse? HoSo)> LayChiTietAsync(
        string maHoSoKham, ulong maTaiKhoan)
    {
        if (!QuanLyLichHenService.ThuDocMa(maHoSoKham, "HSK_", out var id))
            return (new KetQuaKhamBenh("Mã hồ sơ khám không hợp lệ."), null);
        var hoSo = await TaoTruyVanHoSo().FirstOrDefaultAsync(x => x.MaHoSoKham == id);
        if (hoSo == null)
            return (new KetQuaKhamBenh("Không tìm thấy hồ sơ khám.", KhongTimThay: true), null);
        var taiKhoan = await db.TaiKhoans.AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaTaiKhoan == maTaiKhoan);
        if (taiKhoan == null)
            return (new KetQuaKhamBenh("Không tìm thấy tài khoản.", KhongTimThay: true), null);
        if (taiKhoan.MaVaiTro == "BAC_SI" &&
            await LaBacSiPhuTrachAsync(hoSo.MaBacSi, maTaiKhoan))
            return (new KetQuaKhamBenh(), TaoResponse(hoSo, true));
        if (taiKhoan.MaVaiTro == "BAC_SI" && hoSo.ChotLuc.HasValue &&
            await DuocPhanCongKhachHangAsync(hoSo.MaKhachHang, maTaiKhoan))
            return (new KetQuaKhamBenh(), TaoResponse(hoSo, true));
        if (taiKhoan.MaVaiTro == "KHACH_HANG" && hoSo.ChotLuc.HasValue &&
            await db.KhachHangs.AnyAsync(x => x.MaKhachHang == hoSo.MaKhachHang &&
                x.MaConNguoi == taiKhoan.MaConNguoi))
            return (new KetQuaKhamBenh(), TaoResponse(hoSo, false));
        return (new KetQuaKhamBenh("Bạn không có quyền xem hồ sơ này.", KhongCoQuyen: true), null);
    }

    public async Task<(KetQuaKhamBenh KetQua, DanhSachPhanTrangResponse<TomTatHoSoKhamResponse>? DanhSach)>
        LayLichSuKhachHangAsync(string maKhachHang, ulong maTaiKhoan, int page, int pageSize)
    {
        if (!QuanLyLichHenService.ThuDocMa(maKhachHang, "KH_", out var idKhachHang))
            return (new KetQuaKhamBenh("Mã khách hàng không hợp lệ."), null);
        if (!await db.KhachHangs.AnyAsync(x => x.MaKhachHang == idKhachHang))
            return (new KetQuaKhamBenh("Không tìm thấy khách hàng.", KhongTimThay: true), null);
        if (!await DuocPhanCongKhachHangAsync(idKhachHang, maTaiKhoan))
            return (new KetQuaKhamBenh("Bạn không có quyền xem lịch sử khám của khách hàng này.", KhongCoQuyen: true), null);

        var query = db.HoSoKhams.AsNoTracking().Where(x =>
            x.MaKhachHang == idKhachHang && x.ChotLuc != null);
        var totalItems = await query.CountAsync();
        var items = await query.OrderByDescending(x => x.NgayKham)
            .ThenByDescending(x => x.MaHoSoKham)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new TomTatHoSoKhamResponse
            {
                MaHoSoKham = TaoMa("HSK_", x.MaHoSoKham),
                NgayKham = x.NgayKham,
                MaBacSi = TaoMa("BS_", x.MaBacSi),
                TenBacSi = x.BacSi.NhanVien.ConNguoi.HoTen,
                TomTat = x.TomTatChoKhach ?? string.Empty,
                ChotLuc = x.ChotLuc
            }).ToListAsync();
        return (new KetQuaKhamBenh(),
            new DanhSachPhanTrangResponse<TomTatHoSoKhamResponse>(items, totalItems, page, pageSize));
    }

    public async Task<(KetQuaKhamBenh KetQua, HoSoKhamResponse? HoSo)> LayTheoLichAsync(
        string maLichHen, ulong maTaiKhoan)
    {
        if (!QuanLyLichHenService.ThuDocMa(maLichHen, "LH_", out var id))
            return (new KetQuaKhamBenh("Mã lịch hẹn không hợp lệ."), null);
        var maHoSoKham = await db.HoSoKhams.AsNoTracking()
            .Where(x => x.MaLichHen == id)
            .Select(x => (ulong?)x.MaHoSoKham).FirstOrDefaultAsync();
        if (!maHoSoKham.HasValue)
            return (new KetQuaKhamBenh("Lịch hẹn chưa có hồ sơ khám.", KhongTimThay: true), null);
        return await LayChiTietAsync(TaoMa("HSK_", maHoSoKham.Value), maTaiKhoan);
    }

    public async Task<IReadOnlyList<TomTatHoSoKhamResponse>> LayCuaToiAsync(ulong maTaiKhoan)
    {
        var taiKhoan = await db.TaiKhoans.AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaTaiKhoan == maTaiKhoan);
        if (taiKhoan == null) return [];
        IQueryable<HoSoKham> query = db.HoSoKhams.AsNoTracking()
            .Include(x => x.BacSi).ThenInclude(x => x.NhanVien).ThenInclude(x => x.ConNguoi);
        if (taiKhoan.MaVaiTro == "BAC_SI")
        {
            query = query.Where(x => x.BacSi.NhanVien.MaConNguoi == taiKhoan.MaConNguoi);
        }
        else if (taiKhoan.MaVaiTro == "KHACH_HANG")
        {
            query = query.Where(x => x.KhachHang.MaConNguoi == taiKhoan.MaConNguoi &&
                x.ChotLuc != null);
        }
        else return [];
        var hoSos = await query.OrderByDescending(x => x.NgayKham).ToListAsync();
        return hoSos.Select(x => new TomTatHoSoKhamResponse
        {
            MaHoSoKham = TaoMa("HSK_", x.MaHoSoKham),
            NgayKham = x.NgayKham,
            MaBacSi = TaoMa("BS_", x.MaBacSi),
            TenBacSi = x.BacSi.NhanVien.ConNguoi.HoTen,
            TomTat = x.TomTatChoKhach ?? string.Empty,
            ChotLuc = x.ChotLuc
        }).ToList();
    }

    public async Task<DanhSachPhanTrangResponse<TomTatHoSoKhamResponse>> LayTrangCuaToiAsync(
        ulong maTaiKhoan, int page, int pageSize, bool chuaChot = false)
    {
        var taiKhoan = await db.TaiKhoans.AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaTaiKhoan == maTaiKhoan);
        if (taiKhoan == null)
            return new DanhSachPhanTrangResponse<TomTatHoSoKhamResponse>([], 0, page, pageSize);
        IQueryable<HoSoKham> query = db.HoSoKhams.AsNoTracking()
            .Include(x => x.BacSi).ThenInclude(x => x.NhanVien).ThenInclude(x => x.ConNguoi);
        if (taiKhoan.MaVaiTro == "BAC_SI")
        {
            query = query.Where(x => x.BacSi.NhanVien.MaConNguoi == taiKhoan.MaConNguoi);
            if (chuaChot)
                query = query.Where(x => x.ChotLuc == null && x.LichHen != null &&
                    x.LichHen.TrangThai == "DANG_KHAM");
        }
        else if (taiKhoan.MaVaiTro == "KHACH_HANG")
            query = query.Where(x => x.KhachHang.MaConNguoi == taiKhoan.MaConNguoi &&
                x.ChotLuc != null);
        else
            return new DanhSachPhanTrangResponse<TomTatHoSoKhamResponse>([], 0, page, pageSize);

        var totalItems = await query.CountAsync();
        var hoSos = await query.OrderByDescending(x => x.NgayKham)
            .ThenByDescending(x => x.MaHoSoKham)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        var items = hoSos.Select(x => new TomTatHoSoKhamResponse
        {
            MaHoSoKham = TaoMa("HSK_", x.MaHoSoKham),
            NgayKham = x.NgayKham,
            MaBacSi = TaoMa("BS_", x.MaBacSi),
            TenBacSi = x.BacSi.NhanVien.ConNguoi.HoTen,
            TomTat = x.TomTatChoKhach ?? string.Empty,
            ChotLuc = x.ChotLuc
        }).ToList();
        return new DanhSachPhanTrangResponse<TomTatHoSoKhamResponse>(
            items, totalItems, page, pageSize);
    }

    private async Task<(KetQuaKhamBenh KetQua, HoSoKham? HoSo)> LayHoSoDangSuaAsync(
        string maHoSoKham, ulong maTaiKhoan)
    {
        if (!QuanLyLichHenService.ThuDocMa(maHoSoKham, "HSK_", out var id))
            return (new KetQuaKhamBenh("Mã hồ sơ khám không hợp lệ."), null);
        var hoSo = await db.HoSoKhams.Include(x => x.LichHen)
            .FirstOrDefaultAsync(x => x.MaHoSoKham == id);
        if (hoSo == null)
            return (new KetQuaKhamBenh("Không tìm thấy hồ sơ khám.", KhongTimThay: true), null);
        if (!await LaBacSiPhuTrachAsync(hoSo.MaBacSi, maTaiKhoan))
            return (new KetQuaKhamBenh("Bạn chỉ được sửa hồ sơ khám của chính mình.", KhongCoQuyen: true), null);
        if (hoSo.ChotLuc.HasValue || hoSo.LichHen?.TrangThai != "DANG_KHAM")
            return (new KetQuaKhamBenh("Chỉ sửa hồ sơ đang khám và chưa chốt."), null);
        return (new KetQuaKhamBenh(), hoSo);
    }

    private async Task<bool> LaBacSiPhuTrachAsync(ulong maBacSi, ulong maTaiKhoan)
    {
        var maConNguoi = await db.TaiKhoans.AsNoTracking()
            .Where(x => x.MaTaiKhoan == maTaiKhoan && x.MaVaiTro == "BAC_SI")
            .Select(x => (ulong?)x.MaConNguoi).FirstOrDefaultAsync();
        return maConNguoi.HasValue && await db.BacSis.AnyAsync(x =>
            x.MaBacSi == maBacSi && x.NhanVien.MaConNguoi == maConNguoi.Value);
    }

    private Task<bool> DuocPhanCongKhachHangAsync(ulong maKhachHang, ulong maTaiKhoan) =>
        db.LichHens.AsNoTracking().AnyAsync(x =>
            x.MaKhachHang == maKhachHang &&
            (x.TrangThai == "CHO_XAC_NHAN" || x.TrangThai == "DA_XAC_NHAN" || x.TrangThai == "DA_DEN" ||
                x.TrangThai == "DANG_KHAM") &&
            x.BacSi != null && x.BacSi.NhanVien.ConNguoi.TaiKhoan != null &&
            x.BacSi.NhanVien.ConNguoi.TaiKhoan.MaTaiKhoan == maTaiKhoan &&
            x.BacSi.NhanVien.ConNguoi.TaiKhoan.MaVaiTro == "BAC_SI");

    private IQueryable<HoSoKham> TaoTruyVanHoSo() => db.HoSoKhams.AsNoTracking()
        .Include(x => x.BacSi).ThenInclude(x => x.NhanVien).ThenInclude(x => x.ConNguoi)
        .Include(x => x.DichVuThucHiens)
        .Include(x => x.LichHen).ThenInclude(x => x!.ChiTietLichHens).ThenInclude(x => x.DichVu)
        .Include(x => x.DonThuocs).ThenInclude(x => x.ChiTietDonThuocs)
        .AsSplitQuery();

    private static HoSoKhamResponse TaoResponse(HoSoKham hoSo, bool laBacSi) => new()
    {
        MaHoSoKham = TaoMa("HSK_", hoSo.MaHoSoKham),
        MaLichHen = hoSo.MaLichHen.HasValue ? TaoMa("LH_", hoSo.MaLichHen.Value) : null,
        TrangThaiLichHen = hoSo.LichHen?.TrangThai,
        NgayKham = hoSo.NgayKham,
        MaKhachHang = TaoMa("KH_", hoSo.MaKhachHang),
        MaBacSi = TaoMa("BS_", hoSo.MaBacSi),
        TenBacSi = hoSo.BacSi.NhanVien.ConNguoi.HoTen,
        TrieuChung = hoSo.TrieuChung,
        ChanDoan = hoSo.ChanDoan,
        TomTatChoKhach = hoSo.TomTatChoKhach,
        GhiChuNoiBo = laBacSi ? hoSo.GhiChuNoiBo : null,
        NgayTaiKham = hoSo.NgayTaiKham,
        ChotLuc = hoSo.ChotLuc,
        DichVusDuKien = hoSo.LichHen?.ChiTietLichHens.Select(x => new ChiTietLichHenResponse
        {
            MaDichVu = TaoMa("DV_", x.MaDichVu),
            TenDichVu = x.DichVu.TenDichVu,
            DonGiaTaiThoiDiemDat = x.DonGiaTaiThoiDiemDat,
            ThoiLuongTaiThoiDiemDatPhut = x.ThoiLuongTaiThoiDiemDatPhut,
            DaThucHien = hoSo.DichVuThucHiens.Any(d => d.MaDichVu == x.MaDichVu)
        }).ToList() ?? [],
        DichVuThucHiens = hoSo.DichVuThucHiens.Select(x => new DichVuThucHienResponse
        {
            MaDichVuThucHien = TaoMa("DVTH_", x.MaDichVuThucHien),
            MaDichVu = TaoMa("DV_", x.MaDichVu),
            TenDichVu = x.TenDichVuTaiThoiDiemThucHien,
            DonGia = x.DonGiaThucTe,
            SoLuong = x.SoLuong,
            ThoiDiemThucHien = x.ThoiDiemThucHien,
            GhiChu = x.GhiChu
        }).ToList(),
        DonThuocs = hoSo.DonThuocs.Select(x => new DonThuocResponse
        {
            MaDonThuoc = TaoMa("DT_", x.MaDonThuoc),
            NgayKe = x.NgayKe,
            GhiChu = x.GhiChu,
            ChiTietDonThuocs = x.ChiTietDonThuocs.Select(y => new ChiTietDonThuocResponse
            {
                TenThuoc = y.TenThuoc,
                HamLuong = y.HamLuong,
                LieuDung = y.LieuDung,
                SoLanMoiNgay = y.SoLanMoiNgay,
                SoNgayDung = y.SoNgayDung,
                SoLuong = y.SoLuong,
                HuongDan = y.HuongDan
            }).ToList()
        }).ToList()
    };

    private static string TaoMa(string tienTo, ulong id) => $"{tienTo}{id:D4}";
}
