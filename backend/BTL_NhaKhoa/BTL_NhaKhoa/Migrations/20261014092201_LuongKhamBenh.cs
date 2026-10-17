using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTL_NhaKhoa.Migrations
{
    /// <inheritdoc />
    public partial class LuongKhamBenh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "check_lich_hen_trang_thai",
                table: "lich_hen");

            migrationBuilder.AddColumn<DateTime>(
                name: "bat_dau_kham_luc",
                table: "lich_hen",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "den_luc",
                table: "lich_hen",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ghi_chu_khong_den",
                table: "lich_hen",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "hoan_thanh_luc",
                table: "lich_hen",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "khong_den_luc",
                table: "lich_hen",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "nguoi_danh_dau_khong_den_tai_khoan_id",
                table: "lich_hen",
                type: "bigint unsigned",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "nguoi_tiep_nhan_tai_khoan_id",
                table: "lich_hen",
                type: "bigint unsigned",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "chot_luc",
                table: "ho_so_kham",
                type: "datetime",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_lich_hen_nguoi_danh_dau_khong_den_tai_khoan_id",
                table: "lich_hen",
                column: "nguoi_danh_dau_khong_den_tai_khoan_id");

            migrationBuilder.CreateIndex(
                name: "IX_lich_hen_nguoi_tiep_nhan_tai_khoan_id",
                table: "lich_hen",
                column: "nguoi_tiep_nhan_tai_khoan_id");

            migrationBuilder.AddCheckConstraint(
                name: "check_lich_hen_trang_thai",
                table: "lich_hen",
                sql: "trang_thai IN ('CHO_XAC_NHAN', 'DA_XAC_NHAN', 'DA_DEN', 'DANG_KHAM', 'DA_HOAN_THANH', 'DA_HUY', 'KHONG_DEN', 'TU_CHOI')");

            migrationBuilder.AddForeignKey(
                name: "FK_lich_hen_tai_khoan_nguoi_danh_dau_khong_den_tai_khoan_id",
                table: "lich_hen",
                column: "nguoi_danh_dau_khong_den_tai_khoan_id",
                principalTable: "tai_khoan",
                principalColumn: "ma_tai_khoan",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_lich_hen_tai_khoan_nguoi_tiep_nhan_tai_khoan_id",
                table: "lich_hen",
                column: "nguoi_tiep_nhan_tai_khoan_id",
                principalTable: "tai_khoan",
                principalColumn: "ma_tai_khoan",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_lich_hen_tai_khoan_nguoi_danh_dau_khong_den_tai_khoan_id",
                table: "lich_hen");

            migrationBuilder.DropForeignKey(
                name: "FK_lich_hen_tai_khoan_nguoi_tiep_nhan_tai_khoan_id",
                table: "lich_hen");

            migrationBuilder.DropIndex(
                name: "IX_lich_hen_nguoi_danh_dau_khong_den_tai_khoan_id",
                table: "lich_hen");

            migrationBuilder.DropIndex(
                name: "IX_lich_hen_nguoi_tiep_nhan_tai_khoan_id",
                table: "lich_hen");

            migrationBuilder.DropCheckConstraint(
                name: "check_lich_hen_trang_thai",
                table: "lich_hen");

            migrationBuilder.DropColumn(
                name: "bat_dau_kham_luc",
                table: "lich_hen");

            migrationBuilder.DropColumn(
                name: "den_luc",
                table: "lich_hen");

            migrationBuilder.DropColumn(
                name: "ghi_chu_khong_den",
                table: "lich_hen");

            migrationBuilder.DropColumn(
                name: "hoan_thanh_luc",
                table: "lich_hen");

            migrationBuilder.DropColumn(
                name: "khong_den_luc",
                table: "lich_hen");

            migrationBuilder.DropColumn(
                name: "nguoi_danh_dau_khong_den_tai_khoan_id",
                table: "lich_hen");

            migrationBuilder.DropColumn(
                name: "nguoi_tiep_nhan_tai_khoan_id",
                table: "lich_hen");

            migrationBuilder.DropColumn(
                name: "chot_luc",
                table: "ho_so_kham");

            migrationBuilder.AddCheckConstraint(
                name: "check_lich_hen_trang_thai",
                table: "lich_hen",
                sql: "trang_thai IN ('CHO_XAC_NHAN', 'DA_XAC_NHAN', 'DANG_KHAM', 'DA_HOAN_THANH', 'DA_HUY', 'KHONG_DEN', 'TU_CHOI')");
        }
    }
}
