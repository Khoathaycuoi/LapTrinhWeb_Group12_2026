using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTL_NhaKhoa.Migrations
{
    /// <inheritdoc />
    public partial class TuChoiLichTrung : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "check_lich_hen_trang_thai",
                table: "lich_hen");

            migrationBuilder.AddColumn<string>(
                name: "ly_do_tu_choi",
                table: "lich_hen",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "tu_choi_luc",
                table: "lich_hen",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "check_lich_hen_trang_thai",
                table: "lich_hen",
                sql: "trang_thai IN ('CHO_XAC_NHAN', 'DA_XAC_NHAN', 'DANG_KHAM', 'DA_HOAN_THANH', 'DA_HUY', 'KHONG_DEN', 'TU_CHOI')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "check_lich_hen_trang_thai",
                table: "lich_hen");

            migrationBuilder.DropColumn(
                name: "ly_do_tu_choi",
                table: "lich_hen");

            migrationBuilder.DropColumn(
                name: "tu_choi_luc",
                table: "lich_hen");

            migrationBuilder.AddCheckConstraint(
                name: "check_lich_hen_trang_thai",
                table: "lich_hen",
                sql: "trang_thai IN ('CHO_XAC_NHAN', 'DA_XAC_NHAN', 'DANG_KHAM', 'DA_HOAN_THANH', 'DA_HUY', 'KHONG_DEN')");
        }
    }
}
