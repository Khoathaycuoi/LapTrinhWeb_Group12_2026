using BTL_NhaKhoa.Data;
using BTL_NhaKhoa.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BTL_NhaKhoa.Services;

public sealed class BaoCaoDoanhThuService(NhaKhoaDbContext db)
{
    public async Task<BaoCaoDoanhThuNamResponse> LayTheoNamAsync(int nam)
    {
        var dauNam = new DateTime(nam, 1, 1);
        var dauNamSau = dauNam.AddYears(1);
        var theoThang = await db.ThanhToans.AsNoTracking()
            .Where(x => x.TrangThai == "DA_XAC_NHAN" && x.HoaDon.TrangThai != "DA_HUY" &&
                x.ThoiDiemThanhToan >= dauNam && x.ThoiDiemThanhToan < dauNamSau)
            .GroupBy(x => x.ThoiDiemThanhToan.Month)
            .Select(nhom => new DoanhThuTheoThangResponse
            {
                Thang = nhom.Key,
                SoTien = nhom.Sum(x => x.SoTien),
                SoKhoanThu = nhom.Count()
            })
            .ToListAsync();
        var daCo = theoThang.ToDictionary(x => x.Thang);
        var cacThang = Enumerable.Range(1, 12)
            .Select(thang => daCo.GetValueOrDefault(thang) ?? new DoanhThuTheoThangResponse { Thang = thang })
            .ToList();

        return new BaoCaoDoanhThuNamResponse
        {
            Nam = nam,
            TongDoanhThu = cacThang.Sum(x => x.SoTien),
            TongKhoanThu = cacThang.Sum(x => x.SoKhoanThu),
            CacThang = cacThang
        };
    }
}
