using BTL_NhaKhoa.Data;
using BTL_NhaKhoa.DTOs;
using BTL_NhaKhoa.Entities;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace BTL_NhaKhoa.Services;

public sealed record KetQuaDoiTrangThaiTaiKhoan(string? Loi = null, bool KhongTimThay = false);

public sealed class QuanLyNguoiDungService(NhaKhoaDbContext db)
{
    public async Task<IReadOnlyList<DanhSachNguoiDungResponse>> LayDanhSachAsync(
        string loaiNguoiDung, string? trangThai, string? keyword)
    {
        var query = TruyVanTaiKhoan().AsNoTracking();
        query = loaiNguoiDung switch
        {
            "bac-si" => query.Where(x => x.MaVaiTro == "BAC_SI"),
            "nhan-vien" => query.Where(x => x.MaVaiTro == "LE_TAN"),
            "quan-ly" => query.Where(x => x.MaVaiTro == "QUAN_LY"),
            "khach-hang" => query.Where(x => x.MaVaiTro == "KHACH_HANG"),
            _ => throw new ArgumentOutOfRangeException(nameof(loaiNguoiDung))
        };
        if (trangThai != null)
        {
            query = query.Where(x => x.TrangThai == trangThai);
        }

        var taiKhoans = await query.OrderBy(x => x.MaTaiKhoan).ToListAsync();
        var ketQua = taiKhoans.Select(TaoDanhSachResponse).Where(x => x != null).Select(x => x!);
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var tuKhoa = keyword.Trim();
            ketQua = ketQua.Where(x =>
                x.Ten.Contains(tuKhoa, StringComparison.OrdinalIgnoreCase) ||
                x.SoDienThoai.Contains(tuKhoa, StringComparison.OrdinalIgnoreCase) ||
                x.Id.Contains(tuKhoa, StringComparison.OrdinalIgnoreCase));
        }
        return ketQua.ToList();
    }

    public async Task<ThongTinCaNhanResponse?> LayChiTietAsync(string maHoSo)
    {
        if (!ThuDocMaHoSo(maHoSo, out var loai, out var id))
        {
            return null;
        }

        var query = TruyVanTaiKhoan().AsNoTracking();
        query = loai switch
        {
            "KH" => query.Where(x => x.MaVaiTro == "KHACH_HANG" && x.ConNguoi.KhachHang!.MaKhachHang == id),
            "BS" => query.Where(x => x.MaVaiTro == "BAC_SI" && x.ConNguoi.NhanVien!.BacSi!.MaBacSi == id),
            "NV" => query.Where(x => x.MaVaiTro == "LE_TAN" && x.ConNguoi.NhanVien!.MaNhanVien == id),
            "QL" => query.Where(x => x.MaVaiTro == "QUAN_LY" && x.ConNguoi.NhanVien!.MaNhanVien == id),
            _ => query.Where(x => false)
        };
        var taiKhoan = await query.FirstOrDefaultAsync();
        if (taiKhoan == null)
        {
            return null;
        }

        var conNguoi = taiKhoan.ConNguoi;
        var bacSi = conNguoi.NhanVien?.BacSi;
        return new ThongTinCaNhanResponse
        {
            Id = maHoSo,
            Ten = conNguoi.HoTen,
            SoDienThoai = conNguoi.SoDienThoai,
            GioiTinh = conNguoi.GioiTinh,
            Role = taiKhoan.MaVaiTro switch
            {
                "QUAN_LY" => "QUAN_LY",
                "LE_TAN" => "NHAN_VIEN",
                "BAC_SI" => "BAC_SI",
                _ => "KHACH_HANG"
            },
            DiaChi = conNguoi.DiaChi,
            BangCap = bacSi?.BangCap,
            ChungChi = bacSi?.ChungChi,
            NgaySinh = conNguoi.NgaySinh,
            TienSuBenh = null,
            SoNamKinhNghiem = bacSi?.SoNamKinhNghiem,
            TrangThai = HienThiTrangThai(taiKhoan.TrangThai)
        };
    }

    public async Task<(bool TonTai, bool DuocXem, TienSuBenhKhachHangResponse? DuLieu)>
        LayTienSuBenhKhachHangAsync(string maKhachHang, ulong maTaiKhoan)
    {
        if (!QuanLyLichHenService.ThuDocMa(maKhachHang, "KH_", out var idKhachHang))
            return (false, false, null);

        var khachHang = await db.KhachHangs.AsNoTracking()
            .Where(x => x.MaKhachHang == idKhachHang)
            .Select(x => new { x.MaKhachHang, x.TienSuBenh })
            .FirstOrDefaultAsync();
        if (khachHang == null) return (false, false, null);

        var duocPhanCong = await db.LichHens.AsNoTracking().AnyAsync(x =>
            x.MaKhachHang == idKhachHang &&
            (x.TrangThai == "DA_XAC_NHAN" || x.TrangThai == "DA_DEN" ||
                x.TrangThai == "DANG_KHAM") &&
            x.BacSi != null && x.BacSi.NhanVien.ConNguoi.TaiKhoan != null &&
            x.BacSi.NhanVien.ConNguoi.TaiKhoan.MaTaiKhoan == maTaiKhoan &&
            x.BacSi.NhanVien.ConNguoi.TaiKhoan.MaVaiTro == "BAC_SI");
        if (!duocPhanCong) return (true, false, null);

        return (true, true, new TienSuBenhKhachHangResponse
        {
            MaKhachHang = maKhachHang,
            TienSuBenh = khachHang.TienSuBenh
        });
    }

    public async Task<KetQuaDoiTrangThaiTaiKhoan> DoiTrangThaiAsync(
        string soDienThoai, ulong maNguoiThucHien, bool voHieuHoa)
    {
        var soChuanHoa = DangNhapService.ChuanHoaSoDienThoai(soDienThoai);
        await using var giaoDich = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var taiKhoan = await db.TaiKhoans.Include(x => x.ConNguoi)
            .FirstOrDefaultAsync(x => x.ConNguoi.SoDienThoai == soChuanHoa);
        if (taiKhoan == null)
        {
            return new KetQuaDoiTrangThaiTaiKhoan("Không tìm thấy tài khoản.", true);
        }

        if (voHieuHoa)
        {
            if (taiKhoan.MaTaiKhoan == maNguoiThucHien)
            {
                return new KetQuaDoiTrangThaiTaiKhoan("Không thể tự vô hiệu hóa tài khoản của mình.");
            }
            if (taiKhoan.MaVaiTro == "QUAN_LY" && taiKhoan.TrangThai == "HOAT_DONG" &&
                await db.TaiKhoans.CountAsync(x => x.MaVaiTro == "QUAN_LY" && x.TrangThai == "HOAT_DONG") <= 1)
            {
                return new KetQuaDoiTrangThaiTaiKhoan("Không thể vô hiệu hóa Quản lý hoạt động cuối cùng.");
            }
        }

        taiKhoan.TrangThai = voHieuHoa ? "VO_HIEU_HOA" : "HOAT_DONG";
        await db.SaveChangesAsync();
        await giaoDich.CommitAsync();
        return new KetQuaDoiTrangThaiTaiKhoan();
    }

    private IQueryable<TaiKhoan> TruyVanTaiKhoan()
    {
        return db.TaiKhoans
            .Include(x => x.ConNguoi).ThenInclude(x => x.KhachHang)
            .Include(x => x.ConNguoi).ThenInclude(x => x.NhanVien).ThenInclude(x => x!.BacSi);
    }

    private static DanhSachNguoiDungResponse? TaoDanhSachResponse(TaiKhoan taiKhoan)
    {
        var conNguoi = taiKhoan.ConNguoi;
        var maHoSo = taiKhoan.MaVaiTro switch
        {
            "BAC_SI" when conNguoi.NhanVien?.BacSi != null => TaoMaHoSo("BS", conNguoi.NhanVien.BacSi.MaBacSi),
            "LE_TAN" when conNguoi.NhanVien != null => TaoMaHoSo("NV", conNguoi.NhanVien.MaNhanVien),
            "QUAN_LY" when conNguoi.NhanVien != null => TaoMaHoSo("QL", conNguoi.NhanVien.MaNhanVien),
            "KHACH_HANG" when conNguoi.KhachHang != null => TaoMaHoSo("KH", conNguoi.KhachHang.MaKhachHang),
            _ => null
        };
        if (maHoSo == null)
        {
            return null;
        }
        return new DanhSachNguoiDungResponse
        {
            Id = maHoSo,
            Ten = conNguoi.HoTen,
            SoDienThoai = conNguoi.SoDienThoai,
            GioiTinh = conNguoi.GioiTinh,
            VaiTro = taiKhoan.MaVaiTro switch
            {
                "BAC_SI" => "BAC_SI",
                "QUAN_LY" => "QUAN_LY",
                "KHACH_HANG" => "KHACH_HANG",
                _ => "NHAN_VIEN"
            },
            TrangThai = HienThiTrangThai(taiKhoan.TrangThai)
        };
    }

    private static string TaoMaHoSo(string loai, ulong id) => $"{loai}_{id:D4}";

    private static bool ThuDocMaHoSo(string maHoSo, out string loai, out ulong id)
    {
        id = 0;
        var viTri = maHoSo.IndexOf('_');
        loai = viTri > 0 ? maHoSo[..viTri].ToUpperInvariant() : string.Empty;
        return viTri > 0 && ulong.TryParse(maHoSo[(viTri + 1)..], out id);
    }

    private static string HienThiTrangThai(string trangThai) =>
        trangThai == "HOAT_DONG" ? "Hoạt động" : "Không hoạt động";
}
