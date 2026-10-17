using BTL_NhaKhoa.Data;
using BTL_NhaKhoa.DTOs;
using BTL_NhaKhoa.Entities;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace BTL_NhaKhoa.Services;

public sealed record KetQuaLichHen(string? Loi = null, bool KhongTimThay = false,
    bool KhongCoQuyen = false, string? MaLichHen = null);

public sealed class QuanLyLichHenService(NhaKhoaDbContext db)
{
    private static readonly (TimeOnly BatDau, TimeOnly KetThuc)[] CaLamViec =
    [
        (new TimeOnly(7, 0), new TimeOnly(11, 30)),
        (new TimeOnly(14, 0), new TimeOnly(17, 30))
    ];

    public Task<bool> BacSiTonTaiAsync(ulong maBacSi) =>
        db.BacSis.AnyAsync(x => x.MaBacSi == maBacSi &&
            x.NhanVien.TrangThai == "DANG_LAM_VIEC" &&
            x.NhanVien.ConNguoi.TaiKhoan != null &&
            x.NhanVien.ConNguoi.TaiKhoan.TrangThai == "HOAT_DONG");

    public static bool TrangThaiHopLe(string? trangThai) =>
        string.IsNullOrWhiteSpace(trangThai) || ChuanHoaTrangThai(trangThai) != null;

    public async Task<KetQuaLichHen> DatAsync(DatLichRequest yeuCau, ulong maTaiKhoan)
    {
        var batDau = yeuCau.NgayHen!.Value.ToDateTime(yeuCau.GioHen!.Value);
        var taiKhoan = await db.TaiKhoans.AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaTaiKhoan == maTaiKhoan);
        if (taiKhoan == null)
        {
            return new KetQuaLichHen("Không tìm thấy tài khoản.", KhongTimThay: true);
        }

        ulong maKhachHang;
        var nhanVienDatHo = taiKhoan.MaVaiTro is "LE_TAN" or "QUAN_LY";
        var lucNay = DateTime.Now;
        if (batDau < lucNay && !(nhanVienDatHo &&
            batDau.Date == lucNay.Date && batDau.AddMinutes(5) >= lucNay))
        {
            return new KetQuaLichHen("Thời gian hẹn không được ở quá khứ.");
        }
        if (taiKhoan.MaVaiTro == "KHACH_HANG")
        {
            var khachHang = await db.KhachHangs.AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaConNguoi == taiKhoan.MaConNguoi);
            if (khachHang == null)
            {
                return new KetQuaLichHen("Không tìm thấy thông tin khách hàng.", KhongTimThay: true);
            }
            maKhachHang = khachHang.MaKhachHang;
        }
        else if (nhanVienDatHo)
        {
            if (!ThuDocMa(yeuCau.MaKhachHang, "KH_", out maKhachHang) ||
                !await db.KhachHangs.AnyAsync(x => x.MaKhachHang == maKhachHang))
            {
                return new KetQuaLichHen("Vui lòng chọn mã khách hàng hợp lệ.");
            }
        }
        else
        {
            return new KetQuaLichHen("Bạn không có quyền đặt lịch.", KhongCoQuyen: true);
        }

        ulong? maBacSi = null;
        if (!string.IsNullOrWhiteSpace(yeuCau.MaBacSi))
        {
            if (!ThuDocMa(yeuCau.MaBacSi, "BS_", out var idBacSi) ||
                !await BacSiTonTaiAsync(idBacSi))
            {
                return new KetQuaLichHen("Không tìm thấy bác sĩ.");
            }
            maBacSi = idBacSi;
        }

        var maDichVus = new List<ulong>();
        foreach (var ma in yeuCau.DanhSachMaDichVu)
        {
            if (!ThuDocMa(ma, "DV_", out var id))
            {
                return new KetQuaLichHen($"Mã dịch vụ không hợp lệ: {ma}");
            }
            maDichVus.Add(id);
        }
        if (maDichVus.Count != maDichVus.Distinct().Count())
        {
            return new KetQuaLichHen("Một dịch vụ không được chọn nhiều lần trong cùng lịch hẹn.");
        }
        var dichVus = await db.DichVus.AsNoTracking()
            .Where(x => maDichVus.Contains(x.MaDichVu)).ToListAsync();
        if (dichVus.Count != maDichVus.Count || dichVus.Any(x => x.TrangThai != "DANG_HOAT_DONG"))
        {
            return new KetQuaLichHen("Có dịch vụ không tồn tại hoặc đã ngừng hoạt động.");
        }
        var tongPhut = dichVus.Sum(x => (int)x.ThoiLuongDuKienPhut);
        if (tongPhut == 0 || tongPhut > ushort.MaxValue)
        {
            return new KetQuaLichHen("Tổng thời lượng dịch vụ không hợp lệ.");
        }
        if (!NamTrongCaLamViec(batDau, tongPhut))
        {
            return new KetQuaLichHen("Lịch hẹn phải nằm trọn trong ca 07:00–11:30 hoặc 14:00–17:30.");
        }
        await using var giaoDich = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        if (maBacSi.HasValue && await BacSiBanAsync(maBacSi.Value, batDau, tongPhut))
        {
            return new KetQuaLichHen("Bác sĩ đã có lịch bận trong khoảng thời gian này.");
        }

        var maPhong = maBacSi.HasValue ? await TimPhongTrongAsync(batDau, tongPhut) : null;
        var xacNhanNgay = nhanVienDatHo && maBacSi.HasValue && maPhong.HasValue;
        var lichHen = new LichHen
        {
            MaKhachHang = maKhachHang,
            MaBacSi = maBacSi,
            MaPhong = xacNhanNgay ? maPhong : null,
            NguoiTaoTaiKhoanId = maTaiKhoan,
            NguoiXacNhanTaiKhoanId = xacNhanNgay ? maTaiKhoan : null,
            XacNhanLuc = xacNhanNgay ? DateTime.Now : null,
            ThoiGianBatDau = batDau,
            ThoiLuongDuKienPhut = (ushort)tongPhut,
            TrangThai = xacNhanNgay ? "DA_XAC_NHAN" : "CHO_XAC_NHAN",
            ChiTietLichHens = dichVus.Select(x => new ChiTietLichHen
            {
                MaDichVu = x.MaDichVu,
                DonGiaTaiThoiDiemDat = x.DonGia,
                ThoiLuongTaiThoiDiemDatPhut = x.ThoiLuongDuKienPhut
            }).ToList()
        };
        db.LichHens.Add(lichHen);
        if (xacNhanNgay)
        {
            await TuChoiLichTrungAsync(maBacSi!.Value, batDau, tongPhut, null);
        }
        await db.SaveChangesAsync();
        await giaoDich.CommitAsync();
        return new KetQuaLichHen(MaLichHen: TaoMa("LH_", lichHen.MaLichHen));
    }

    public async Task<IReadOnlyList<LichHenResponse>> LayDanhSachAsync(ulong maTaiKhoan,
        string? trangThai = null, DateOnly? ngayHen = null, string? maKhachHang = null,
        string? tenKhachHang = null, string? soDienThoaiKhachHang = null,
        string? maBacSi = null, string? tenBacSi = null, string? soDienThoaiBacSi = null)
    {
        var taiKhoan = await db.TaiKhoans.AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaTaiKhoan == maTaiKhoan);
        if (taiKhoan == null) return [];

        var query = db.LichHens.AsNoTracking()
            .Include(x => x.KhachHang).ThenInclude(x => x.ConNguoi)
            .Include(x => x.BacSi).ThenInclude(x => x!.NhanVien).ThenInclude(x => x.ConNguoi)
            .Include(x => x.NguoiHuyTaiKhoan).ThenInclude(x => x!.ConNguoi)
            .Include(x => x.NguoiDanhDauKhongDenTaiKhoan).ThenInclude(x => x!.ConNguoi)
            .Include(x => x.ChiTietLichHens).ThenInclude(x => x.DichVu)
            .Include(x => x.ChiTietLichHens).ThenInclude(x => x.DichVuThucHien)
            .AsSplitQuery()
            .AsQueryable();

        if (taiKhoan.MaVaiTro == "KHACH_HANG")
        {
            query = query.Where(x => x.KhachHang.MaConNguoi == taiKhoan.MaConNguoi);
            maKhachHang = null;
            tenKhachHang = null;
            soDienThoaiKhachHang = null;
        }
        else if (taiKhoan.MaVaiTro == "BAC_SI")
        {
            query = query.Where(x => x.MaBacSi != null &&
                x.BacSi!.NhanVien.MaConNguoi == taiKhoan.MaConNguoi);
            maBacSi = null;
            tenBacSi = null;
            soDienThoaiBacSi = null;
        }
        else if (taiKhoan.MaVaiTro is not ("QUAN_LY" or "LE_TAN"))
        {
            return [];
        }

        if (!string.IsNullOrWhiteSpace(trangThai))
        {
            var maTrangThai = ChuanHoaTrangThai(trangThai);
            query = query.Where(x => x.TrangThai == maTrangThai);
        }
        if (ngayHen.HasValue)
        {
            var dauNgay = ngayHen.Value.ToDateTime(TimeOnly.MinValue);
            var cuoiNgay = dauNgay.AddDays(1);
            query = query.Where(x => x.ThoiGianBatDau >= dauNgay && x.ThoiGianBatDau < cuoiNgay);
        }
        if (!string.IsNullOrWhiteSpace(maKhachHang))
        {
            var idKhachHang = DocMaHoacKhong(maKhachHang, "KH_");
            query = query.Where(x => x.MaKhachHang == idKhachHang);
        }
        if (!string.IsNullOrWhiteSpace(maBacSi))
        {
            var idBacSi = DocMaHoacKhong(maBacSi, "BS_");
            query = query.Where(x => x.MaBacSi == idBacSi);
        }
        if (!string.IsNullOrWhiteSpace(tenKhachHang))
        {
            query = query.Where(x => x.KhachHang.ConNguoi.HoTen.Contains(tenKhachHang.Trim()));
        }
        if (!string.IsNullOrWhiteSpace(soDienThoaiKhachHang))
        {
            query = query.Where(x => x.KhachHang.ConNguoi.SoDienThoai.Contains(soDienThoaiKhachHang.Trim()));
        }
        if (!string.IsNullOrWhiteSpace(tenBacSi))
        {
            query = query.Where(x => x.BacSi != null && x.BacSi.NhanVien.ConNguoi.HoTen.Contains(tenBacSi.Trim()));
        }
        if (!string.IsNullOrWhiteSpace(soDienThoaiBacSi))
        {
            query = query.Where(x => x.BacSi != null && x.BacSi.NhanVien.ConNguoi.SoDienThoai.Contains(soDienThoaiBacSi.Trim()));
        }

        var danhSach = await query.OrderByDescending(x => x.ThoiGianBatDau).ToListAsync();
        return danhSach.Select(TaoResponse).ToList();
    }

    public async Task<KetQuaLichHen> XacNhanAsync(string maLichHen, ulong maTaiKhoan,
        string? maPhongDuocChon = null)
    {
        if (!ThuDocMa(maLichHen, "LH_", out var id)) return new KetQuaLichHen("Mã lịch hẹn không hợp lệ.");
        await using var giaoDich = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var lichHen = await db.LichHens.FirstOrDefaultAsync(x => x.MaLichHen == id);
        if (lichHen == null) return new KetQuaLichHen("Không tìm thấy lịch hẹn.", KhongTimThay: true);
        if (lichHen.TrangThai != "CHO_XAC_NHAN") return new KetQuaLichHen("Chỉ xác nhận được lịch đang chờ xác nhận.");

        var taiKhoan = await db.TaiKhoans.AsNoTracking().FirstOrDefaultAsync(x => x.MaTaiKhoan == maTaiKhoan);
        if (taiKhoan == null) return new KetQuaLichHen("Không tìm thấy tài khoản.", KhongTimThay: true);
        if (taiKhoan.MaVaiTro == "BAC_SI")
        {
            if (!lichHen.MaBacSi.HasValue || !await db.BacSis.AnyAsync(x =>
                x.MaBacSi == lichHen.MaBacSi.Value && x.NhanVien.MaConNguoi == taiKhoan.MaConNguoi))
            {
                return new KetQuaLichHen("Bạn chỉ được xác nhận lịch của chính mình.", KhongCoQuyen: true);
            }
        }
        else if (taiKhoan.MaVaiTro is not ("QUAN_LY" or "LE_TAN"))
        {
            return new KetQuaLichHen("Bạn không có quyền xác nhận lịch.", KhongCoQuyen: true);
        }
        if (lichHen.MaBacSi.HasValue && await BacSiBanAsync(lichHen.MaBacSi.Value,
            lichHen.ThoiGianBatDau, lichHen.ThoiLuongDuKienPhut, lichHen.MaLichHen))
        {
            return new KetQuaLichHen("Bác sĩ đã có lịch bận trong khoảng thời gian này.");
        }
        if (!lichHen.MaBacSi.HasValue)
        {
            return new KetQuaLichHen("Cần gán bác sĩ trước khi xác nhận lịch hẹn.");
        }
        if (!await BacSiTonTaiAsync(lichHen.MaBacSi.Value))
        {
            return new KetQuaLichHen("Bác sĩ đã ngừng hoạt động.");
        }
        ulong? maPhong = null;
        if (!string.IsNullOrWhiteSpace(maPhongDuocChon))
        {
            if (!ThuDocMa(maPhongDuocChon, "PH_", out var idPhong))
                return new KetQuaLichHen("Mã phòng không hợp lệ.");
            var phongTrong = await LayDanhSachPhongTrongAsync(lichHen.ThoiGianBatDau,
                lichHen.ThoiLuongDuKienPhut);
            if (!phongTrong.Contains(idPhong))
                return new KetQuaLichHen("Phòng được chọn không hoạt động hoặc đã có lịch trong khoảng thời gian này.");
            maPhong = idPhong;
        }
        else
        {
            maPhong = await TimPhongTrongAsync(lichHen.ThoiGianBatDau,
                lichHen.ThoiLuongDuKienPhut);
        }
        if (!maPhong.HasValue)
        {
            return new KetQuaLichHen("Không còn phòng hoạt động trống trong khoảng thời gian này.");
        }

        lichHen.TrangThai = "DA_XAC_NHAN";
        lichHen.MaPhong = maPhong;
        lichHen.NguoiXacNhanTaiKhoanId = maTaiKhoan;
        lichHen.XacNhanLuc = DateTime.Now;
        await TuChoiLichTrungAsync(lichHen.MaBacSi.Value, lichHen.ThoiGianBatDau,
            lichHen.ThoiLuongDuKienPhut, lichHen.MaLichHen);
        await db.SaveChangesAsync();
        await giaoDich.CommitAsync();
        return new KetQuaLichHen();
    }

    public async Task<KetQuaLichHen> TiepNhanAsync(string maLichHen, ulong maTaiKhoan)
    {
        if (!ThuDocMa(maLichHen, "LH_", out var id))
            return new KetQuaLichHen("Mã lịch hẹn không hợp lệ.");
        await using var giaoDich = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var lichHen = await db.LichHens.FirstOrDefaultAsync(x => x.MaLichHen == id);
        if (lichHen == null) return new KetQuaLichHen("Không tìm thấy lịch hẹn.", KhongTimThay: true);
        if (lichHen.TrangThai != "DA_XAC_NHAN")
            return new KetQuaLichHen("Chỉ tiếp nhận được lịch đã xác nhận.");
        if (lichHen.ThoiGianBatDau.Date != DateTime.Today)
            return new KetQuaLichHen("Chỉ tiếp nhận khách vào ngày hẹn.");
        var taiKhoan = await db.TaiKhoans.AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaTaiKhoan == maTaiKhoan);
        if (taiKhoan?.MaVaiTro is not ("LE_TAN" or "QUAN_LY"))
            return new KetQuaLichHen("Bạn không có quyền tiếp nhận khách.", KhongCoQuyen: true);
        lichHen.TrangThai = "DA_DEN";
        lichHen.DenLuc = DateTime.Now;
        lichHen.NguoiTiepNhanTaiKhoanId = maTaiKhoan;
        await db.SaveChangesAsync();
        await giaoDich.CommitAsync();
        return new KetQuaLichHen();
    }

    public async Task<KetQuaLichHen> DanhDauKhongDenAsync(string maLichHen,
        string? ghiChu, ulong maTaiKhoan)
    {
        if (!ThuDocMa(maLichHen, "LH_", out var id))
            return new KetQuaLichHen("Mã lịch hẹn không hợp lệ.");
        await using var giaoDich = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var lichHen = await db.LichHens.FirstOrDefaultAsync(x => x.MaLichHen == id);
        if (lichHen == null) return new KetQuaLichHen("Không tìm thấy lịch hẹn.", KhongTimThay: true);
        if (lichHen.TrangThai != "DA_XAC_NHAN")
            return new KetQuaLichHen("Chỉ đánh dấu không đến cho lịch đã xác nhận và chưa tiếp nhận.");
        if (DateTime.Now < lichHen.ThoiGianBatDau.AddMinutes(30))
            return new KetQuaLichHen("Chỉ đánh dấu không đến sau giờ hẹn ít nhất 30 phút.");
        var taiKhoan = await db.TaiKhoans.AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaTaiKhoan == maTaiKhoan);
        if (taiKhoan?.MaVaiTro is not ("LE_TAN" or "QUAN_LY"))
            return new KetQuaLichHen("Bạn không có quyền đánh dấu không đến.", KhongCoQuyen: true);
        lichHen.TrangThai = "KHONG_DEN";
        lichHen.KhongDenLuc = DateTime.Now;
        lichHen.GhiChuKhongDen = ghiChu?.Trim();
        lichHen.NguoiDanhDauKhongDenTaiKhoanId = maTaiKhoan;
        await db.SaveChangesAsync();
        await giaoDich.CommitAsync();
        return new KetQuaLichHen();
    }

    public async Task<(KetQuaLichHen KetQua, object? Phongs)> LayPhongTrongAsync(
        string maLichHen, ulong maTaiKhoan)
    {
        if (!ThuDocMa(maLichHen, "LH_", out var id))
            return (new KetQuaLichHen("Mã lịch hẹn không hợp lệ."), null);
        var lichHen = await db.LichHens.AsNoTracking()
            .Where(x => x.MaLichHen == id)
            .Select(x => new { x.MaBacSi, x.ThoiGianBatDau, x.ThoiLuongDuKienPhut, x.TrangThai })
            .FirstOrDefaultAsync();
        if (lichHen == null)
            return (new KetQuaLichHen("Không tìm thấy lịch hẹn.", KhongTimThay: true), null);
        if (lichHen.TrangThai != "CHO_XAC_NHAN")
            return (new KetQuaLichHen("Chỉ xem phòng trống cho lịch đang chờ xác nhận."), null);
        var taiKhoan = await db.TaiKhoans.AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaTaiKhoan == maTaiKhoan);
        if (taiKhoan == null)
            return (new KetQuaLichHen("Không tìm thấy tài khoản.", KhongTimThay: true), null);
        if (taiKhoan.MaVaiTro == "BAC_SI")
        {
            if (!lichHen.MaBacSi.HasValue || !await db.BacSis.AnyAsync(x =>
                x.MaBacSi == lichHen.MaBacSi.Value && x.NhanVien.MaConNguoi == taiKhoan.MaConNguoi))
                return (new KetQuaLichHen("Bạn chỉ được xem phòng cho lịch của chính mình.",
                    KhongCoQuyen: true), null);
        }
        else if (taiKhoan.MaVaiTro is not ("LE_TAN" or "QUAN_LY"))
            return (new KetQuaLichHen("Bạn không có quyền xem phòng trống.", KhongCoQuyen: true), null);

        var maPhongs = await LayDanhSachPhongTrongAsync(lichHen.ThoiGianBatDau,
            lichHen.ThoiLuongDuKienPhut);
        var phongs = await db.Phongs.AsNoTracking()
            .Where(x => maPhongs.Contains(x.MaPhong))
            .OrderBy(x => x.MaPhong)
            .Select(x => new { x.MaPhong, x.TenPhong })
            .ToListAsync();
        return (new KetQuaLichHen(), phongs.Select(x => new
        {
            MaPhong = TaoMa("PH_", x.MaPhong), x.TenPhong
        }).ToList());
    }

    public async Task<KetQuaLichHen> GanBacSiAsync(string maLichHen, string maBacSi)
    {
        if (!ThuDocMa(maLichHen, "LH_", out var idLichHen) ||
            !ThuDocMa(maBacSi, "BS_", out var idBacSi))
        {
            return new KetQuaLichHen("Mã lịch hẹn hoặc mã bác sĩ không hợp lệ.");
        }
        var lichHen = await db.LichHens.FirstOrDefaultAsync(x => x.MaLichHen == idLichHen);
        if (lichHen == null) return new KetQuaLichHen("Không tìm thấy lịch hẹn.", KhongTimThay: true);
        if (lichHen.TrangThai != "CHO_XAC_NHAN")
            return new KetQuaLichHen("Chỉ gán bác sĩ cho lịch đang chờ xác nhận.");
        if (!await BacSiTonTaiAsync(idBacSi))
            return new KetQuaLichHen("Không tìm thấy bác sĩ.", KhongTimThay: true);
        if (await BacSiBanAsync(idBacSi, lichHen.ThoiGianBatDau,
            lichHen.ThoiLuongDuKienPhut, lichHen.MaLichHen))
            return new KetQuaLichHen("Bác sĩ đã có lịch bận trong khoảng thời gian này.");
        lichHen.MaBacSi = idBacSi;
        await db.SaveChangesAsync();
        return new KetQuaLichHen();
    }

    public async Task<KetQuaLichHen> HoanThanhAsync(string maLichHen, ulong maTaiKhoan)
    {
        if (!ThuDocMa(maLichHen, "LH_", out var id)) return new KetQuaLichHen("Mã lịch hẹn không hợp lệ.");
        var lichHen = await db.LichHens.FirstOrDefaultAsync(x => x.MaLichHen == id);
        if (lichHen == null) return new KetQuaLichHen("Không tìm thấy lịch hẹn.", KhongTimThay: true);
        if (lichHen.TrangThai != "DANG_KHAM") return new KetQuaLichHen("Chỉ hoàn thành được lịch đang khám.");
        if (!lichHen.BatDauKhamLuc.HasValue)
            return new KetQuaLichHen("Buổi khám chưa được bắt đầu.");
        if (!await db.HoSoKhams.AnyAsync(x => x.MaLichHen == id && x.ChotLuc != null))
            return new KetQuaLichHen("Bác sĩ cần chốt hồ sơ khám trước khi hoàn thành lịch.");

        var taiKhoan = await db.TaiKhoans.AsNoTracking().FirstOrDefaultAsync(x => x.MaTaiKhoan == maTaiKhoan);
        if (taiKhoan == null) return new KetQuaLichHen("Không tìm thấy tài khoản.", KhongTimThay: true);
        if (taiKhoan.MaVaiTro == "BAC_SI")
        {
            if (!lichHen.MaBacSi.HasValue || !await db.BacSis.AnyAsync(x =>
                x.MaBacSi == lichHen.MaBacSi.Value && x.NhanVien.MaConNguoi == taiKhoan.MaConNguoi))
            {
                return new KetQuaLichHen("Bạn chỉ được hoàn thành lịch của chính mình.", KhongCoQuyen: true);
            }
        }
        else if (taiKhoan.MaVaiTro is not ("QUAN_LY" or "LE_TAN"))
        {
            return new KetQuaLichHen("Bạn không có quyền hoàn thành lịch.", KhongCoQuyen: true);
        }
        lichHen.TrangThai = "DA_HOAN_THANH";
        lichHen.HoanThanhLuc = DateTime.Now;
        await db.SaveChangesAsync();
        return new KetQuaLichHen();
    }

    public async Task<IReadOnlyList<TimeOnly>> LayCaTrongAsync(ulong maBacSi, DateOnly ngay,
        int thoiLuongPhut = 30)
    {
        var dauNgay = ngay.ToDateTime(TimeOnly.MinValue);
        var cuoiNgay = dauNgay.AddDays(1);
        var lichDaDat = await db.LichHens.AsNoTracking()
            .Where(x => x.MaBacSi == maBacSi && x.ThoiGianBatDau >= dauNgay &&
                x.ThoiGianBatDau < cuoiNgay &&
                (x.TrangThai == "DA_XAC_NHAN" || x.TrangThai == "DA_DEN" || x.TrangThai == "DANG_KHAM"))
            .Select(x => new { x.ThoiGianBatDau, x.ThoiLuongDuKienPhut }).ToListAsync();

        var ketQua = new List<TimeOnly>();
        foreach (var (gioBatDau, gioKetThuc) in CaLamViec)
        {
            for (var gio = gioBatDau; gio < gioKetThuc; gio = gio.AddMinutes(30))
            {
                var thoiDiem = ngay.ToDateTime(gio);
                var ketThuc = thoiDiem.AddMinutes(thoiLuongPhut);
                if (ketThuc > ngay.ToDateTime(gioKetThuc)) continue;
                if (thoiDiem < DateTime.Now) continue;
                if (lichDaDat.Any(x => GiaoNhau(thoiDiem, ketThuc,
                    x.ThoiGianBatDau, x.ThoiGianBatDau.AddMinutes(x.ThoiLuongDuKienPhut)))) continue;
                if ((await TimPhongTrongAsync(thoiDiem, thoiLuongPhut)) is null) continue;
                ketQua.Add(gio);
            }
        }
        return ketQua;
    }

    private async Task<bool> BacSiBanAsync(ulong maBacSi, DateTime batDau, int soPhut,
        ulong? boQuaLichHen = null)
    {
        var dauNgay = batDau.Date;
        var cuoiNgay = dauNgay.AddDays(1);
        var lichTrongNgay = await db.LichHens.AsNoTracking()
            .Where(x => x.MaBacSi == maBacSi && x.ThoiGianBatDau >= dauNgay &&
                x.ThoiGianBatDau < cuoiNgay &&
                (x.TrangThai == "DA_XAC_NHAN" || x.TrangThai == "DA_DEN" || x.TrangThai == "DANG_KHAM") &&
                (!boQuaLichHen.HasValue || x.MaLichHen != boQuaLichHen.Value))
            .Select(x => new { x.ThoiGianBatDau, x.ThoiLuongDuKienPhut }).ToListAsync();
        return lichTrongNgay.Any(x => GiaoNhau(batDau, batDau.AddMinutes(soPhut),
            x.ThoiGianBatDau, x.ThoiGianBatDau.AddMinutes(x.ThoiLuongDuKienPhut)));
    }

    private static bool GiaoNhau(DateTime batDauA, DateTime ketThucA, DateTime batDauB, DateTime ketThucB) =>
        batDauA < ketThucB && batDauB < ketThucA;

    private static bool NamTrongCaLamViec(DateTime batDau, int thoiLuongPhut) =>
        CaLamViec.Any(ca =>
            batDau >= batDau.Date.Add(ca.BatDau.ToTimeSpan()) &&
            batDau.AddMinutes(thoiLuongPhut) <= batDau.Date.Add(ca.KetThuc.ToTimeSpan()));

    private async Task<ulong?> TimPhongTrongAsync(DateTime batDau, int thoiLuongPhut)
    {
        var maPhong = (await LayDanhSachPhongTrongAsync(batDau, thoiLuongPhut)).FirstOrDefault();
        return maPhong == 0 ? null : maPhong;
    }

    private async Task<List<ulong>> LayDanhSachPhongTrongAsync(DateTime batDau, int thoiLuongPhut)
    {
        var phongHoatDong = await db.Phongs.AsNoTracking()
            .Where(x => x.TrangThai == "HOAT_DONG")
            .OrderBy(x => x.MaPhong).Select(x => x.MaPhong).ToListAsync();
        if (phongHoatDong.Count == 0) return [];
        var dauNgay = batDau.Date;
        var cuoiNgay = dauNgay.AddDays(1);
        var lichDaXacNhan = await db.LichHens.AsNoTracking()
            .Where(x => x.MaPhong != null && x.ThoiGianBatDau >= dauNgay &&
                x.ThoiGianBatDau < cuoiNgay &&
                (x.TrangThai == "DA_XAC_NHAN" || x.TrangThai == "DA_DEN" || x.TrangThai == "DANG_KHAM"))
            .Select(x => new { x.MaPhong, x.ThoiGianBatDau, x.ThoiLuongDuKienPhut })
            .ToListAsync();
        return phongHoatDong.Where(id => !lichDaXacNhan.Any(x => x.MaPhong == id &&
            GiaoNhau(batDau, batDau.AddMinutes(thoiLuongPhut),
                x.ThoiGianBatDau, x.ThoiGianBatDau.AddMinutes(x.ThoiLuongDuKienPhut)))).ToList();
    }

    private async Task TuChoiLichTrungAsync(ulong maBacSi, DateTime batDau,
        int thoiLuongPhut, ulong? boQuaLichHen)
    {
        var dauNgay = batDau.Date;
        var cuoiNgay = dauNgay.AddDays(1);
        var lichCho = await db.LichHens
            .Where(x => x.MaBacSi == maBacSi && x.TrangThai == "CHO_XAC_NHAN" &&
                x.ThoiGianBatDau >= dauNgay && x.ThoiGianBatDau < cuoiNgay &&
                (!boQuaLichHen.HasValue || x.MaLichHen != boQuaLichHen.Value))
            .ToListAsync();
        foreach (var lich in lichCho.Where(x => GiaoNhau(batDau, batDau.AddMinutes(thoiLuongPhut),
            x.ThoiGianBatDau, x.ThoiGianBatDau.AddMinutes(x.ThoiLuongDuKienPhut))))
        {
            lich.TrangThai = "TU_CHOI";
            lich.LyDoTuChoi = "Ca của bác sĩ đã được xác nhận cho lịch hẹn khác.";
            lich.TuChoiLuc = DateTime.Now;
        }
    }

    public static bool ThuDocMa(string? ma, string tienTo, out ulong id)
    {
        id = 0;
        return ma != null && ma.StartsWith(tienTo, StringComparison.OrdinalIgnoreCase) &&
            ulong.TryParse(ma.AsSpan(tienTo.Length), out id) && id > 0;
    }

    private static ulong DocMaHoacKhong(string ma, string tienTo) => ThuDocMa(ma, tienTo, out var id) ? id : 0;
    private static string TaoMa(string tienTo, ulong id) => $"{tienTo}{id:D4}";

    private static string? ChuanHoaTrangThai(string trangThai) => trangThai.Trim() switch
    {
        "Chờ xác nhận" or "CHO_XAC_NHAN" => "CHO_XAC_NHAN",
        "Đã xác nhận" or "DA_XAC_NHAN" => "DA_XAC_NHAN",
        "Đã đến" or "DA_DEN" => "DA_DEN",
        "Đang khám" or "DANG_KHAM" => "DANG_KHAM",
        "Đã hoàn thành" or "DA_HOAN_THANH" => "DA_HOAN_THANH",
        "Đã hủy" or "DA_HUY" => "DA_HUY",
        "Không đến" or "KHONG_DEN" => "KHONG_DEN",
        "Từ chối" or "TU_CHOI" => "TU_CHOI",
        _ => null
    };

    private static LichHenResponse TaoResponse(LichHen lichHen) => new()
    {
        MaLichHen = TaoMa("LH_", lichHen.MaLichHen),
        NgayHen = DateOnly.FromDateTime(lichHen.ThoiGianBatDau),
        GioHen = TimeOnly.FromDateTime(lichHen.ThoiGianBatDau),
        TrangThai = lichHen.TrangThai switch
        {
            "CHO_XAC_NHAN" => "Chờ xác nhận",
            "DA_XAC_NHAN" => "Đã xác nhận",
            "DA_DEN" => "Đã đến",
            "DANG_KHAM" => "Đang khám",
            "DA_HOAN_THANH" => "Đã hoàn thành",
            "DA_HUY" => "Đã hủy",
            "KHONG_DEN" => "Không đến",
            "TU_CHOI" => "Từ chối",
            _ => lichHen.TrangThai
        },
        MaKH = TaoMa("KH_", lichHen.MaKhachHang),
        TenKH = lichHen.KhachHang.ConNguoi.HoTen,
        MaBacSi = lichHen.MaBacSi.HasValue ? TaoMa("BS_", lichHen.MaBacSi.Value) : null,
        TenBacSi = lichHen.BacSi?.NhanVien.ConNguoi.HoTen,
        MaPhong = lichHen.MaPhong.HasValue ? TaoMa("PH_", lichHen.MaPhong.Value) : null,
        GhiChu = lichHen.GhiChu,
        CreatedAt = lichHen.CreatedAt,
        UpdatedAt = lichHen.UpdatedAt,
        ThoiLuongDuKienPhut = lichHen.ThoiLuongDuKienPhut,
        LyDoHuy = lichHen.LyDoHuy,
        HuyLuc = lichHen.HuyLuc,
        LyDoTuChoi = lichHen.LyDoTuChoi,
        TuChoiLuc = lichHen.TuChoiLuc,
        NguoiHuy = lichHen.NguoiHuyTaiKhoan?.ConNguoi.HoTen,
        DenLuc = lichHen.DenLuc,
        BatDauKhamLuc = lichHen.BatDauKhamLuc,
        HoanThanhLuc = lichHen.HoanThanhLuc,
        KhongDenLuc = lichHen.KhongDenLuc,
        GhiChuKhongDen = lichHen.GhiChuKhongDen,
        NguoiDanhDauKhongDen = lichHen.NguoiDanhDauKhongDenTaiKhoan?.ConNguoi.HoTen,
        DichVusDuKien = lichHen.ChiTietLichHens.Select(x => new ChiTietLichHenResponse
        {
            MaDichVu = TaoMa("DV_", x.MaDichVu),
            TenDichVu = x.DichVu.TenDichVu,
            DonGiaTaiThoiDiemDat = x.DonGiaTaiThoiDiemDat,
            ThoiLuongTaiThoiDiemDatPhut = x.ThoiLuongTaiThoiDiemDatPhut,
            DaThucHien = x.DichVuThucHien != null
        }).ToList()
    };
}
