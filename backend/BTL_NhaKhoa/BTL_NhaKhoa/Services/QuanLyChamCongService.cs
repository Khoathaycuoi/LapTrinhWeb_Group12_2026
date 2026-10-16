using BTL_NhaKhoa.Data;
using BTL_NhaKhoa.DTOs;
using BTL_NhaKhoa.Entities;
using Microsoft.EntityFrameworkCore;

namespace BTL_NhaKhoa.Services;

public sealed record KetQuaChamCong(string? Loi = null, bool KhongTimThay = false,
    bool XungDot = false, ChamCongResponse? DuLieu = null);

public sealed class QuanLyChamCongService(NhaKhoaDbContext db)
{
    private static readonly TimeOnly BatDauSang = new(7, 0);
    private static readonly TimeOnly KetThucSang = new(11, 30);
    private static readonly TimeOnly BatDauChieu = new(14, 0);
    private static readonly TimeOnly KetThucChieu = new(17, 30);
    private static readonly TimeOnly MoCheckInSang = new(6, 30);
    private static readonly TimeOnly MoCheckInChieu = new(13, 30);

    public async Task<KetQuaChamCong> CheckInAsync(ulong maTaiKhoan, string buoi)
    {
        buoi = buoi.Trim().ToUpperInvariant();
        if (!BuoiHopLe(buoi)) return new("Buổi phải là SANG hoặc CHIEU.");
        var nhanVien = await TimNhanVienAsync(maTaiKhoan);
        if (nhanVien == null) return new("Không tìm thấy hồ sơ Lễ tân/Bác sĩ đang làm việc.", KhongTimThay: true);
        var bayGio = DateTime.Now;
        if (!TrongCuaSoCheckIn(buoi, TimeOnly.FromDateTime(bayGio)))
            return new("Đã ngoài thời gian check-in của buổi này.");

        var ngay = DateOnly.FromDateTime(bayGio);
        var chamCong = await db.ChamCongs.FirstOrDefaultAsync(x =>
            x.MaNhanVien == nhanVien.MaNhanVien && x.NgayChamCong == ngay);
        if (chamCong == null)
        {
            chamCong = new ChamCong { MaNhanVien = nhanVien.MaNhanVien, NgayChamCong = ngay };
            db.ChamCongs.Add(chamCong);
        }
        else if (chamCong.TrangThaiDuyet is "DA_DUYET" or "TU_CHOI")
            return new("Bản chấm công đã được Quản lý xử lý.", XungDot: true);

        if (buoi == "SANG")
        {
            if (chamCong.TinhTrangSang != "CHUA_XAC_DINH" || chamCong.GioVaoSang != null)
                return new("Buổi sáng đã được ghi nhận.", XungDot: true);
            chamCong.GioVaoSang = bayGio;
            chamCong.TinhTrangSang = "LAM_VIEC";
        }
        else
        {
            if (chamCong.TinhTrangChieu != "CHUA_XAC_DINH" || chamCong.GioVaoChieu != null)
                return new("Buổi chiều đã được ghi nhận.", XungDot: true);
            chamCong.GioVaoChieu = bayGio;
            chamCong.TinhTrangChieu = "LAM_VIEC";
        }
        CapNhatTrangThai(chamCong);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is MySql.Data.MySqlClient.MySqlException { Number: 1062 })
        {
            return new("Bản chấm công hôm nay vừa được tạo hoặc thay đổi. Hãy tải lại trang.", XungDot: true);
        }
        return await LayChiTietAsync(chamCong.MaChamCong);
    }

    public async Task<KetQuaChamCong> CheckOutAsync(ulong maTaiKhoan, string buoi)
    {
        buoi = buoi.Trim().ToUpperInvariant();
        if (!BuoiHopLe(buoi)) return new("Buổi phải là SANG hoặc CHIEU.");
        var nhanVien = await TimNhanVienAsync(maTaiKhoan);
        if (nhanVien == null) return new("Không tìm thấy hồ sơ Lễ tân/Bác sĩ đang làm việc.", KhongTimThay: true);
        var bayGio = DateTime.Now;
        var ngay = DateOnly.FromDateTime(bayGio);
        var chamCong = await db.ChamCongs.FirstOrDefaultAsync(x =>
            x.MaNhanVien == nhanVien.MaNhanVien && x.NgayChamCong == ngay);
        if (chamCong == null) return new("Hôm nay chưa có bản chấm công.");
        if (chamCong.TrangThaiDuyet is "DA_DUYET" or "TU_CHOI")
            return new("Bản chấm công đã được Quản lý xử lý.", XungDot: true);
        if (buoi == "SANG")
        {
            if (chamCong.GioVaoSang == null || chamCong.TinhTrangSang != "LAM_VIEC")
                return new("Chưa check-in buổi sáng.");
            if (chamCong.GioRaSang != null) return new("Đã check-out buổi sáng.", XungDot: true);
            if (TimeOnly.FromDateTime(bayGio) >= MoCheckInChieu)
                return new("Đã hết thời gian check-out sáng; liên hệ Quản lý để bổ sung giờ.");
            if (bayGio < chamCong.GioVaoSang)
                return new("Giờ vào lớn hơn giờ server; liên hệ Quản lý để chỉnh sửa.");
            chamCong.GioRaSang = bayGio;
        }
        else
        {
            if (chamCong.GioVaoChieu == null || chamCong.TinhTrangChieu != "LAM_VIEC")
                return new("Chưa check-in buổi chiều.");
            if (chamCong.GioRaChieu != null) return new("Đã check-out buổi chiều.", XungDot: true);
            if (bayGio < chamCong.GioVaoChieu)
                return new("Giờ vào lớn hơn giờ server; liên hệ Quản lý để chỉnh sửa.");
            chamCong.GioRaChieu = bayGio;
        }
        CapNhatTrangThai(chamCong);
        await db.SaveChangesAsync();
        return await LayChiTietAsync(chamCong.MaChamCong);
    }

    public async Task<ChamCongHomNayResponse?> LayHomNayAsync(ulong maTaiKhoan)
    {
        var nhanVien = await TimNhanVienAsync(maTaiKhoan);
        if (nhanVien == null) return null;
        var bayGio = DateTime.Now;
        var ngay = DateOnly.FromDateTime(bayGio);
        var chamCong = await TruyVan().FirstOrDefaultAsync(x =>
            x.MaNhanVien == nhanVien.MaNhanVien && x.NgayChamCong == ngay);
        var coTheSua = chamCong == null || chamCong.TrangThaiDuyet is not ("DA_DUYET" or "TU_CHOI");
        var gio = TimeOnly.FromDateTime(bayGio);
        return new ChamCongHomNayResponse
        {
            GioServer = bayGio,
            ChamCong = chamCong == null ? null : TaoResponse(chamCong),
            CoTheCheckInSang = coTheSua && TrongCuaSoCheckIn("SANG", gio) &&
                (chamCong == null || (chamCong.TinhTrangSang == "CHUA_XAC_DINH" &&
                    chamCong.GioVaoSang == null)),
            CoTheCheckOutSang = coTheSua && gio < MoCheckInChieu &&
                chamCong?.TinhTrangSang == "LAM_VIEC" && chamCong.GioVaoSang != null &&
                chamCong.GioRaSang == null,
            CoTheCheckInChieu = coTheSua && TrongCuaSoCheckIn("CHIEU", gio) &&
                (chamCong == null || (chamCong.TinhTrangChieu == "CHUA_XAC_DINH" &&
                    chamCong.GioVaoChieu == null)),
            CoTheCheckOutChieu = coTheSua && chamCong?.TinhTrangChieu == "LAM_VIEC" &&
                chamCong.GioVaoChieu != null &&
                chamCong.GioRaChieu == null
        };
    }

    public async Task<IReadOnlyList<ChamCongResponse>> LayCuaToiAsync(ulong maTaiKhoan,
        DateOnly? tuNgay, DateOnly? denNgay)
    {
        var nhanVien = await TimNhanVienAsync(maTaiKhoan);
        if (nhanVien == null) return [];
        var query = TruyVan().Where(x => x.MaNhanVien == nhanVien.MaNhanVien);
        if (tuNgay.HasValue) query = query.Where(x => x.NgayChamCong >= tuNgay.Value);
        if (denNgay.HasValue) query = query.Where(x => x.NgayChamCong <= denNgay.Value);
        return (await query.OrderByDescending(x => x.NgayChamCong).ToListAsync())
            .Select(TaoResponse).ToList();
    }

    public async Task<IReadOnlyList<ChamCongResponse>> LayDanhSachAsync(
        string? maNhanVien, DateOnly? tuNgay, DateOnly? denNgay, string? trangThaiDuyet)
    {
        var query = TruyVan();
        if (!string.IsNullOrWhiteSpace(maNhanVien))
        {
            if (!QuanLyLichHenService.ThuDocMa(maNhanVien, "NV_", out var id)) return [];
            query = query.Where(x => x.MaNhanVien == id);
        }
        if (tuNgay.HasValue) query = query.Where(x => x.NgayChamCong >= tuNgay.Value);
        if (denNgay.HasValue) query = query.Where(x => x.NgayChamCong <= denNgay.Value);
        if (!string.IsNullOrWhiteSpace(trangThaiDuyet))
            query = query.Where(x => x.TrangThaiDuyet == trangThaiDuyet.Trim().ToUpperInvariant());
        return (await query.OrderByDescending(x => x.NgayChamCong)
            .ThenBy(x => x.MaNhanVien).ToListAsync()).Select(TaoResponse).ToList();
    }

    public async Task<IReadOnlyList<TongHopChamCongNgayResponse>> LayTongHopNgayAsync(DateOnly ngay)
    {
        var nhanViens = await db.NhanViens.AsNoTracking()
            .Include(x => x.ConNguoi).ThenInclude(x => x.TaiKhoan)
            .Where(x => x.TrangThai == "DANG_LAM_VIEC" &&
                x.ConNguoi.TaiKhoan != null && x.ConNguoi.TaiKhoan.TrangThai == "HOAT_DONG" &&
                (x.ConNguoi.TaiKhoan.MaVaiTro == "LE_TAN" ||
                 x.ConNguoi.TaiKhoan.MaVaiTro == "BAC_SI"))
            .OrderBy(x => x.ConNguoi.HoTen).ToListAsync();
        var chamCongs = (await TruyVan().Where(x => x.NgayChamCong == ngay).ToListAsync())
            .ToDictionary(x => x.MaNhanVien);
        return nhanViens.Select(x =>
        {
            chamCongs.TryGetValue(x.MaNhanVien, out var chamCong);
            return new TongHopChamCongNgayResponse
            {
                MaNhanVien = $"NV_{x.MaNhanVien:D4}",
                HoTen = x.ConNguoi.HoTen,
                VaiTro = x.ConNguoi.TaiKhoan!.MaVaiTro,
                NgayChamCong = ngay,
                TrangThai = chamCong?.TrangThaiDuyet ?? "CHUA_CO_DU_LIEU",
                ChamCong = chamCong == null ? null : TaoResponse(chamCong)
            };
        }).ToList();
    }

    public async Task<KetQuaChamCong> LayChiTietAsync(ulong maChamCong)
    {
        var chamCong = await TruyVan().FirstOrDefaultAsync(x => x.MaChamCong == maChamCong);
        return chamCong == null
            ? new("Không tìm thấy bản chấm công.", KhongTimThay: true)
            : new(DuLieu: TaoResponse(chamCong));
    }

    public async Task<KetQuaChamCong> ThemThuCongAsync(ThemChamCongThuCongRequest yeuCau,
        ulong maNguoiSua)
    {
        if (!QuanLyLichHenService.ThuDocMa(yeuCau.MaNhanVien, "NV_", out var maNhanVien))
            return new("Mã nhân viên không hợp lệ.");
        if (yeuCau.NgayChamCong == default || yeuCau.NgayChamCong > DateOnly.FromDateTime(DateTime.Now))
            return new("Ngày chấm công không hợp lệ.");
        var nhanVien = await db.NhanViens.AsNoTracking()
            .Include(x => x.ConNguoi).ThenInclude(x => x.TaiKhoan)
            .FirstOrDefaultAsync(x => x.MaNhanVien == maNhanVien);
        if (nhanVien == null || nhanVien.ConNguoi.TaiKhoan?.MaVaiTro is not ("LE_TAN" or "BAC_SI"))
            return new("Không tìm thấy Lễ tân/Bác sĩ.", KhongTimThay: true);
        if (await db.ChamCongs.AnyAsync(x => x.MaNhanVien == maNhanVien &&
            x.NgayChamCong == yeuCau.NgayChamCong))
            return new("Ngày này đã có bản chấm công; dùng API chỉnh sửa.", XungDot: true);

        var chamCong = new ChamCong { MaNhanVien = maNhanVien, NgayChamCong = yeuCau.NgayChamCong };
        var loi = ApDungChinhSua(chamCong, yeuCau.Buoi, yeuCau.GioVao,
            yeuCau.GioRa, yeuCau.TinhTrang, yeuCau.LyDo, maNguoiSua);
        if (loi != null) return new(loi);
        db.ChamCongs.Add(chamCong);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is MySql.Data.MySqlClient.MySqlException { Number: 1062 })
        {
            return new("Ngày này vừa được ghi chấm công. Hãy tải lại trang.", XungDot: true);
        }
        return await LayChiTietAsync(chamCong.MaChamCong);
    }

    public async Task<KetQuaChamCong> ChinhSuaAsync(ulong maChamCong,
        ChinhSuaChamCongRequest yeuCau, ulong maNguoiSua)
    {
        var chamCong = await db.ChamCongs.FirstOrDefaultAsync(x => x.MaChamCong == maChamCong);
        if (chamCong == null) return new("Không tìm thấy bản chấm công.", KhongTimThay: true);
        if (chamCong.TrangThaiDuyet is "DA_DUYET" or "TU_CHOI")
            return new("Bản chấm công đã duyệt hoặc từ chối; không thể chỉnh sửa qua API này.", XungDot: true);
        var loi = ApDungChinhSua(chamCong, yeuCau.Buoi, yeuCau.GioVao,
            yeuCau.GioRa, yeuCau.TinhTrang, yeuCau.LyDo, maNguoiSua);
        if (loi != null) return new(loi);
        await db.SaveChangesAsync();
        return await LayChiTietAsync(maChamCong);
    }

    public async Task<KetQuaChamCong> DuyetAsync(ulong maChamCong, ulong maNguoiDuyet)
    {
        var chamCong = await db.ChamCongs.FirstOrDefaultAsync(x => x.MaChamCong == maChamCong);
        if (chamCong == null) return new("Không tìm thấy bản chấm công.", KhongTimThay: true);
        if (chamCong.TrangThaiDuyet != "CHO_DUYET" || !DaHoanTatCaNgay(chamCong))
            return new("Chỉ duyệt khi cả hai buổi đã có giờ đầy đủ hoặc được phân loại.", XungDot: true);
        chamCong.TrangThaiDuyet = "DA_DUYET";
        chamCong.NguoiDuyetTaiKhoanId = maNguoiDuyet;
        chamCong.DuyetLuc = DateTime.Now;
        await db.SaveChangesAsync();
        return await LayChiTietAsync(maChamCong);
    }

    public async Task<KetQuaChamCong> TuChoiAsync(ulong maChamCong,
        TuChoiChamCongRequest yeuCau, ulong maNguoiTuChoi)
    {
        var lyDo = yeuCau.LyDo?.Trim();
        if (string.IsNullOrWhiteSpace(lyDo)) return new("Phải nhập lý do từ chối.");
        var chamCong = await db.ChamCongs.FirstOrDefaultAsync(x => x.MaChamCong == maChamCong);
        if (chamCong == null) return new("Không tìm thấy bản chấm công.", KhongTimThay: true);
        if (chamCong.TrangThaiDuyet is "DA_DUYET" or "TU_CHOI")
            return new("Bản chấm công đã được xử lý.", XungDot: true);
        chamCong.TrangThaiDuyet = "TU_CHOI";
        chamCong.LyDoTuChoi = lyDo;
        chamCong.NguoiTuChoiTaiKhoanId = maNguoiTuChoi;
        chamCong.TuChoiLuc = DateTime.Now;
        await db.SaveChangesAsync();
        return await LayChiTietAsync(maChamCong);
    }

    private string? ApDungChinhSua(ChamCong chamCong, string buoi, DateTime? gioVao,
        DateTime? gioRa, string? tinhTrang, string? lyDoNhap, ulong maNguoiSua)
    {
        buoi = buoi?.Trim().ToUpperInvariant() ?? string.Empty;
        if (!BuoiHopLe(buoi)) return "Buổi phải là SANG hoặc CHIEU.";
        var lyDo = lyDoNhap?.Trim();
        if (string.IsNullOrWhiteSpace(lyDo) || lyDo.Length > 500)
            return "Lý do phải có từ 1 đến 500 ký tự.";
        var phanLoai = tinhTrang?.Trim().ToUpperInvariant();
        if (phanLoai != null && phanLoai is not ("NGHI_PHEP" or "VANG_KHONG_PHEP"))
            return "Tình trạng chỉ được là NGHI_PHEP hoặc VANG_KHONG_PHEP.";
        if (phanLoai == null && gioVao == null && gioRa == null)
            return "Phải nhập giờ hoặc phân loại buổi làm việc.";
        if (phanLoai != null && (gioVao != null || gioRa != null))
            return "Không nhập giờ làm cho buổi nghỉ/vắng.";

        var vaoCu = buoi == "SANG" ? chamCong.GioVaoSang : chamCong.GioVaoChieu;
        var raCu = buoi == "SANG" ? chamCong.GioRaSang : chamCong.GioRaChieu;
        var tinhTrangCu = buoi == "SANG" ? chamCong.TinhTrangSang : chamCong.TinhTrangChieu;
        var vaoMoi = gioVao ?? vaoCu;
        var raMoi = gioRa ?? raCu;
        var tinhTrangMoi = phanLoai ?? "LAM_VIEC";
        if (phanLoai != null)
        {
            if (vaoCu != null || raCu != null) return "Buổi này đã có giờ; hãy từ chối bản ghi sai để xử lý riêng.";
            vaoMoi = null;
            raMoi = null;
            if (chamCong.NgayChamCong == DateOnly.FromDateTime(DateTime.Now) &&
                TimeOnly.FromDateTime(DateTime.Now) < (buoi == "SANG" ? KetThucSang : KetThucChieu))
                return "Chưa kết thúc buổi; không thể phân loại nghỉ/vắng.";
        }
        else
        {
            if (tinhTrangCu is "NGHI_PHEP" or "VANG_KHONG_PHEP")
                return "Buổi đã được phân loại nghỉ/vắng; không thể thêm giờ làm qua API này.";
            if (vaoMoi == null) return "Phải có giờ vào trước khi ghi giờ ra.";
            if (DateOnly.FromDateTime(vaoMoi.Value) != chamCong.NgayChamCong ||
                (raMoi != null && DateOnly.FromDateTime(raMoi.Value) != chamCong.NgayChamCong))
                return "Giờ vào/ra phải thuộc ngày chấm công.";
            if (!TrongCuaSoCheckIn(buoi, TimeOnly.FromDateTime(vaoMoi.Value)))
                return "Giờ vào nằm ngoài khung check-in của buổi.";
            if (raMoi < vaoMoi) return "Giờ ra không được trước giờ vào.";
            if (raMoi != null && buoi == "SANG" && TimeOnly.FromDateTime(raMoi.Value) >= MoCheckInChieu)
                return "Giờ ra sáng phải trước 13:30.";
            if (vaoMoi > DateTime.Now || raMoi > DateTime.Now)
                return "Không thể ghi giờ chấm công trong tương lai.";
        }
        if (vaoMoi == vaoCu && raMoi == raCu && tinhTrangMoi == tinhTrangCu)
            return "Dữ liệu chấm công không thay đổi.";

        db.LichSuChinhSuaChamCongs.Add(new LichSuChinhSuaChamCong
        {
            ChamCong = chamCong,
            NguoiSuaTaiKhoanId = maNguoiSua,
            Buoi = buoi,
            GioVaoCu = vaoCu,
            GioRaCu = raCu,
            GioVaoMoi = vaoMoi,
            GioRaMoi = raMoi,
            TinhTrangCu = tinhTrangCu,
            TinhTrangMoi = tinhTrangMoi,
            LyDo = lyDo,
            ThoiDiemSua = DateTime.Now
        });
        if (buoi == "SANG")
        {
            chamCong.GioVaoSang = vaoMoi;
            chamCong.GioRaSang = raMoi;
            chamCong.TinhTrangSang = tinhTrangMoi;
            chamCong.LyDoSang = phanLoai == null ? null : lyDo;
        }
        else
        {
            chamCong.GioVaoChieu = vaoMoi;
            chamCong.GioRaChieu = raMoi;
            chamCong.TinhTrangChieu = tinhTrangMoi;
            chamCong.LyDoChieu = phanLoai == null ? null : lyDo;
        }
        CapNhatTrangThai(chamCong);
        return null;
    }

    private static void CapNhatTrangThai(ChamCong chamCong) =>
        chamCong.TrangThaiDuyet = DaHoanTatCaNgay(chamCong) ? "CHO_DUYET" : "CHUA_HOAN_TAT";

    private static bool DaHoanTatCaNgay(ChamCong chamCong) =>
        BuoiDaXuLy(chamCong.TinhTrangSang, chamCong.GioVaoSang, chamCong.GioRaSang) &&
        BuoiDaXuLy(chamCong.TinhTrangChieu, chamCong.GioVaoChieu, chamCong.GioRaChieu);

    private static bool BuoiDaXuLy(string tinhTrang, DateTime? vao, DateTime? ra) =>
        tinhTrang is "NGHI_PHEP" or "VANG_KHONG_PHEP" ||
        (tinhTrang == "LAM_VIEC" && vao != null && ra != null);

    private static bool BuoiHopLe(string? buoi) => buoi is "SANG" or "CHIEU";

    private static bool TrongCuaSoCheckIn(string buoi, TimeOnly gio) => buoi == "SANG"
        ? gio >= MoCheckInSang && gio < KetThucSang
        : gio >= MoCheckInChieu && gio < KetThucChieu;

    private async Task<NhanVien?> TimNhanVienAsync(ulong maTaiKhoan)
    {
        var taiKhoan = await db.TaiKhoans.AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaTaiKhoan == maTaiKhoan);
        if (taiKhoan?.MaVaiTro is not ("LE_TAN" or "BAC_SI")) return null;
        return await db.NhanViens.AsNoTracking().FirstOrDefaultAsync(x =>
            x.MaConNguoi == taiKhoan.MaConNguoi && x.TrangThai == "DANG_LAM_VIEC");
    }

    private IQueryable<ChamCong> TruyVan() => db.ChamCongs.AsNoTracking()
        .Include(x => x.NhanVien).ThenInclude(x => x.ConNguoi)
        .Include(x => x.NguoiDuyetTaiKhoan).ThenInclude(x => x!.ConNguoi)
        .Include(x => x.NguoiTuChoiTaiKhoan).ThenInclude(x => x!.ConNguoi)
        .Include(x => x.LichSuChinhSuas).ThenInclude(x => x.NguoiSuaTaiKhoan)
            .ThenInclude(x => x.ConNguoi)
        .AsSplitQuery();

    private static int SoPhutTre(DateTime? vao, DateOnly ngay, TimeOnly batDau) => vao == null
        ? 0 : Math.Max(0, (int)(vao.Value - ngay.ToDateTime(batDau)).TotalMinutes);

    private static int SoPhutVeSom(DateTime? ra, DateOnly ngay, TimeOnly ketThuc) => ra == null
        ? 0 : Math.Max(0, (int)(ngay.ToDateTime(ketThuc) - ra.Value).TotalMinutes);

    private static int SoPhutLam(DateTime? vao, DateTime? ra, DateOnly ngay,
        TimeOnly batDau, TimeOnly ketThuc)
    {
        if (vao == null || ra == null) return 0;
        var tu = vao.Value > ngay.ToDateTime(batDau) ? vao.Value : ngay.ToDateTime(batDau);
        var den = ra.Value < ngay.ToDateTime(ketThuc) ? ra.Value : ngay.ToDateTime(ketThuc);
        return Math.Max(0, (int)(den - tu).TotalMinutes);
    }

    private static ChamCongResponse TaoResponse(ChamCong chamCong) => new()
    {
        MaChamCong = $"CC_{chamCong.MaChamCong:D4}",
        MaNhanVien = $"NV_{chamCong.MaNhanVien:D4}",
        HoTen = chamCong.NhanVien.ConNguoi.HoTen,
        NgayChamCong = chamCong.NgayChamCong,
        GioVaoSang = chamCong.GioVaoSang,
        GioRaSang = chamCong.GioRaSang,
        GioVaoChieu = chamCong.GioVaoChieu,
        GioRaChieu = chamCong.GioRaChieu,
        TinhTrangSang = chamCong.TinhTrangSang,
        TinhTrangChieu = chamCong.TinhTrangChieu,
        LyDoSang = chamCong.LyDoSang,
        LyDoChieu = chamCong.LyDoChieu,
        SoPhutTreSang = SoPhutTre(chamCong.GioVaoSang, chamCong.NgayChamCong, BatDauSang),
        SoPhutTreChieu = SoPhutTre(chamCong.GioVaoChieu, chamCong.NgayChamCong, BatDauChieu),
        SoPhutVeSomSang = SoPhutVeSom(chamCong.GioRaSang, chamCong.NgayChamCong, KetThucSang),
        SoPhutVeSomChieu = SoPhutVeSom(chamCong.GioRaChieu, chamCong.NgayChamCong, KetThucChieu),
        TongPhutLamViec = SoPhutLam(chamCong.GioVaoSang, chamCong.GioRaSang,
            chamCong.NgayChamCong, BatDauSang, KetThucSang) +
            SoPhutLam(chamCong.GioVaoChieu, chamCong.GioRaChieu,
                chamCong.NgayChamCong, BatDauChieu, KetThucChieu),
        TrangThaiDuyet = chamCong.TrangThaiDuyet,
        NguoiDuyet = chamCong.NguoiDuyetTaiKhoan?.ConNguoi.HoTen,
        DuyetLuc = chamCong.DuyetLuc,
        NguoiTuChoi = chamCong.NguoiTuChoiTaiKhoan?.ConNguoi.HoTen,
        TuChoiLuc = chamCong.TuChoiLuc,
        LyDoTuChoi = chamCong.LyDoTuChoi,
        LichSuChinhSua = chamCong.LichSuChinhSuas.OrderBy(x => x.ThoiDiemSua)
            .Select(x => new LichSuChinhSuaChamCongResponse
            {
                Buoi = x.Buoi,
                TinhTrangCu = x.TinhTrangCu,
                TinhTrangMoi = x.TinhTrangMoi,
                GioVaoCu = x.GioVaoCu,
                GioRaCu = x.GioRaCu,
                GioVaoMoi = x.GioVaoMoi,
                GioRaMoi = x.GioRaMoi,
                LyDo = x.LyDo,
                NguoiSua = x.NguoiSuaTaiKhoan.ConNguoi.HoTen,
                ThoiDiemSua = x.ThoiDiemSua
            }).ToList()
    };
}
