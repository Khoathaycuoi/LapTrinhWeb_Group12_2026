using BTL_NhaKhoa.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTL_NhaKhoa.Migrations;

[DbContext(typeof(NhaKhoaDbContext))]
[Migration("20261017180000_HetHanLichCho")]
public sealed class HetHanLichCho : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropCheckConstraint(
            name: "check_lich_hen_trang_thai", table: "lich_hen");
        migrationBuilder.AddCheckConstraint(
            name: "check_lich_hen_trang_thai", table: "lich_hen",
            sql: "trang_thai IN ('CHO_XAC_NHAN', 'HET_HAN', 'DA_XAC_NHAN', 'DA_DEN', 'DANG_KHAM', 'DA_HOAN_THANH', 'DA_HUY', 'KHONG_DEN', 'TU_CHOI')");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("UPDATE lich_hen SET trang_thai = 'CHO_XAC_NHAN' WHERE trang_thai = 'HET_HAN'");
        migrationBuilder.DropCheckConstraint(
            name: "check_lich_hen_trang_thai", table: "lich_hen");
        migrationBuilder.AddCheckConstraint(
            name: "check_lich_hen_trang_thai", table: "lich_hen",
            sql: "trang_thai IN ('CHO_XAC_NHAN', 'DA_XAC_NHAN', 'DA_DEN', 'DANG_KHAM', 'DA_HOAN_THANH', 'DA_HUY', 'KHONG_DEN', 'TU_CHOI')");
    }
}
