using BTL_NhaKhoa.Data;
using BTL_NhaKhoa.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace BTL_NhaKhoa.Services;

public sealed record KetQuaHuyLich(string? Loi = null, bool KhongTimThay = false, bool KhongCoQuyen = false);

public sealed class HuyLichHenService(NhaKhoaDbContext db)
{
    public async Task<KetQuaHuyLich> HuyAsync(HuyLichRequest yeuCau, ulong maTaiKhoan)
    {
        if (!ThuDocMaLichHen(yeuCau.MaLichHen, out var maLichHen))
        {
            return new KetQuaHuyLich("Mã lịch hẹn không hợp lệ.");
        }

        var lyDo = yeuCau.LyDo.Trim();
        if (lyDo.Length == 0)
        {
            return new KetQuaHuyLich("Vui lòng nhập lý do hủy.");
        }

        await using var giaoDich = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var lichHen = await db.LichHens.FirstOrDefaultAsync(x => x.MaLichHen == maLichHen);
        if (lichHen == null)
        {
            return new KetQuaHuyLich("Không tìm thấy lịch hẹn.", KhongTimThay: true);
        }
        if (lichHen.TrangThai is not ("CHO_XAC_NHAN" or "DA_XAC_NHAN"))
        {
            return new KetQuaHuyLich("Chỉ có thể hủy lịch đang chờ hoặc đã xác nhận.");
        }

        var taiKhoan = await db.TaiKhoans.Include(x => x.ConNguoi)
            .FirstOrDefaultAsync(x => x.MaTaiKhoan == maTaiKhoan);
        if (taiKhoan == null || taiKhoan.TrangThai != "HOAT_DONG")
        {
            return new KetQuaHuyLich("Tài khoản không còn hiệu lực.", KhongCoQuyen: true);
        }

        var lucHuy = DateTime.Now;
        if (lichHen.TrangThai == "CHO_XAC_NHAN" && lichHen.ThoiGianBatDau <= lucHuy)
        {
            return new KetQuaHuyLich("Lịch chờ xác nhận đã quá giờ hẹn và không thể hủy.");
        }
        string tenVaiTro;
        switch (taiKhoan.MaVaiTro)
        {
            case "KHACH_HANG":
                if (!await db.KhachHangs.AnyAsync(x =>
                    x.MaKhachHang == lichHen.MaKhachHang && x.MaConNguoi == taiKhoan.MaConNguoi))
                {
                    return new KetQuaHuyLich("Bạn không có quyền hủy lịch hẹn của người khác.", KhongCoQuyen: true);
                }
                if (lichHen.TrangThai == "DA_XAC_NHAN" &&
                    lichHen.ThoiGianBatDau - lucHuy < TimeSpan.FromHours(12))
                {
                    return new KetQuaHuyLich("Lịch đã xác nhận chỉ được tự hủy trước giờ hẹn ít nhất 12 tiếng. Vui lòng liên hệ Lễ tân để được hỗ trợ.");
                }
                tenVaiTro = "Khách hàng";
                break;

            case "BAC_SI":
                if (lichHen.MaBacSi == null || !await db.BacSis.AnyAsync(x =>
                    x.MaBacSi == lichHen.MaBacSi && x.NhanVien.MaConNguoi == taiKhoan.MaConNguoi))
                {
                    return new KetQuaHuyLich("Bạn chỉ được hủy lịch hẹn do chính mình phụ trách.", KhongCoQuyen: true);
                }
                tenVaiTro = "Bác sĩ";
                break;

            case "LE_TAN":
                tenVaiTro = "Lễ tân";
                break;

            case "QUAN_LY":
                tenVaiTro = "Quản lý";
                break;

            default:
                return new KetQuaHuyLich("Bạn không có quyền hủy lịch hẹn.", KhongCoQuyen: true);
        }

        lichHen.TrangThai = "DA_HUY";
        lichHen.LyDoHuy = lyDo;
        lichHen.NguoiHuyTaiKhoanId = taiKhoan.MaTaiKhoan;
        lichHen.HuyLuc = lucHuy;
        var ghiChuHuy = $"[Hủy bởi: {taiKhoan.ConNguoi.HoTen} - {tenVaiTro}] Lý do: {lyDo}";
        lichHen.GhiChu = string.IsNullOrWhiteSpace(lichHen.GhiChu)
            ? ghiChuHuy
            : $"{lichHen.GhiChu}\n{ghiChuHuy}";

        await db.SaveChangesAsync();
        await giaoDich.CommitAsync();
        return new KetQuaHuyLich();
    }

    private static bool ThuDocMaLichHen(string maLichHen, out ulong id)
    {
        id = 0;
        return maLichHen.StartsWith("LH_", StringComparison.OrdinalIgnoreCase) &&
            ulong.TryParse(maLichHen.AsSpan(3), out id) && id > 0;
    }
}
