using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTL_NhaKhoa.Migrations
{
    /// <inheritdoc />
    public partial class ChamCongHaiBuoi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "check_cham_cong_gio",
                table: "cham_cong");

            migrationBuilder.DropCheckConstraint(
                name: "check_cham_cong_trang_thai",
                table: "cham_cong");

            migrationBuilder.DropColumn(
                name: "ghi_chu",
                table: "cham_cong");

            migrationBuilder.DropColumn(
                name: "so_phut_tre",
                table: "cham_cong");

            migrationBuilder.DropColumn(
                name: "trang_thai",
                table: "cham_cong");

            migrationBuilder.AddColumn<string>(
                name: "buoi",
                table: "lich_su_chinh_sua_cham_cong",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "CA_NGAY");

            migrationBuilder.AddColumn<string>(
                name: "tinh_trang_cu",
                table: "lich_su_chinh_sua_cham_cong",
                type: "varchar(25)",
                maxLength: 25,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tinh_trang_moi",
                table: "lich_su_chinh_sua_cham_cong",
                type: "varchar(25)",
                maxLength: 25,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "trang_thai_duyet",
                table: "cham_cong",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "CHUA_HOAN_TAT",
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "CHO_DUYET");

            migrationBuilder.AddColumn<DateTime>(
                name: "gio_ra_chieu",
                table: "cham_cong",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "gio_vao_sang",
                table: "cham_cong",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "gio_ra_sang",
                table: "cham_cong",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "gio_vao_chieu",
                table: "cham_cong",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ly_do_chieu",
                table: "cham_cong",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ly_do_sang",
                table: "cham_cong",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ly_do_tu_choi",
                table: "cham_cong",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "tu_choi_luc",
                table: "cham_cong",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "nguoi_tu_choi_tai_khoan_id",
                table: "cham_cong",
                type: "bigint unsigned",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tinh_trang_chieu",
                table: "cham_cong",
                type: "varchar(25)",
                maxLength: 25,
                nullable: false,
                defaultValue: "CHUA_XAC_DINH");

            migrationBuilder.AddColumn<string>(
                name: "tinh_trang_sang",
                table: "cham_cong",
                type: "varchar(25)",
                maxLength: 25,
                nullable: false,
                defaultValue: "CHUA_XAC_DINH");

            migrationBuilder.Sql("""
                UPDATE cham_cong SET
                    gio_vao_sang = CASE WHEN gio_vao IS NOT NULL AND TIME(gio_vao) < '11:30:00' THEN gio_vao ELSE NULL END,
                    gio_ra_sang = CASE WHEN gio_ra IS NOT NULL AND TIME(gio_ra) < '13:30:00' THEN gio_ra ELSE NULL END,
                    gio_vao_chieu = CASE WHEN gio_vao IS NOT NULL AND TIME(gio_vao) >= '13:30:00' THEN gio_vao ELSE NULL END,
                    gio_ra_chieu = CASE WHEN gio_ra IS NOT NULL AND TIME(gio_ra) >= '13:30:00' THEN gio_ra ELSE NULL END,
                    tinh_trang_sang = CASE WHEN (gio_vao IS NOT NULL AND TIME(gio_vao) < '11:30:00') OR (gio_ra IS NOT NULL AND TIME(gio_ra) < '13:30:00') THEN 'LAM_VIEC' ELSE 'CHUA_XAC_DINH' END,
                    tinh_trang_chieu = CASE WHEN (gio_vao IS NOT NULL AND TIME(gio_vao) >= '13:30:00') OR (gio_ra IS NOT NULL AND TIME(gio_ra) >= '13:30:00') THEN 'LAM_VIEC' ELSE 'CHUA_XAC_DINH' END,
                    trang_thai_duyet = CASE WHEN trang_thai_duyet = 'CHO_DUYET' THEN 'CHUA_HOAN_TAT' ELSE trang_thai_duyet END
                """);

            migrationBuilder.DropColumn(name: "gio_vao", table: "cham_cong");
            migrationBuilder.DropColumn(name: "gio_ra", table: "cham_cong");

            migrationBuilder.CreateIndex(
                name: "IX_cham_cong_nguoi_tu_choi_tai_khoan_id",
                table: "cham_cong",
                column: "nguoi_tu_choi_tai_khoan_id");

            migrationBuilder.AddCheckConstraint(
                name: "check_cham_cong_gio_chieu",
                table: "cham_cong",
                sql: "gio_vao_chieu IS NULL OR gio_ra_chieu IS NULL OR gio_ra_chieu >= gio_vao_chieu");

            migrationBuilder.AddCheckConstraint(
                name: "check_cham_cong_gio_sang",
                table: "cham_cong",
                sql: "gio_vao_sang IS NULL OR gio_ra_sang IS NULL OR gio_ra_sang >= gio_vao_sang");

            migrationBuilder.AddCheckConstraint(
                name: "check_cham_cong_tinh_trang_chieu",
                table: "cham_cong",
                sql: "tinh_trang_chieu IN ('CHUA_XAC_DINH', 'LAM_VIEC', 'NGHI_PHEP', 'VANG_KHONG_PHEP')");

            migrationBuilder.AddCheckConstraint(
                name: "check_cham_cong_tinh_trang_sang",
                table: "cham_cong",
                sql: "tinh_trang_sang IN ('CHUA_XAC_DINH', 'LAM_VIEC', 'NGHI_PHEP', 'VANG_KHONG_PHEP')");

            migrationBuilder.AddForeignKey(
                name: "FK_cham_cong_tai_khoan_nguoi_tu_choi_tai_khoan_id",
                table: "cham_cong",
                column: "nguoi_tu_choi_tai_khoan_id",
                principalTable: "tai_khoan",
                principalColumn: "ma_tai_khoan",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cham_cong_tai_khoan_nguoi_tu_choi_tai_khoan_id",
                table: "cham_cong");

            migrationBuilder.DropIndex(
                name: "IX_cham_cong_nguoi_tu_choi_tai_khoan_id",
                table: "cham_cong");

            migrationBuilder.DropCheckConstraint(
                name: "check_cham_cong_gio_chieu",
                table: "cham_cong");

            migrationBuilder.DropCheckConstraint(
                name: "check_cham_cong_gio_sang",
                table: "cham_cong");

            migrationBuilder.DropCheckConstraint(
                name: "check_cham_cong_tinh_trang_chieu",
                table: "cham_cong");

            migrationBuilder.DropCheckConstraint(
                name: "check_cham_cong_tinh_trang_sang",
                table: "cham_cong");

            migrationBuilder.AddColumn<DateTime>(
                name: "gio_vao",
                table: "cham_cong",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "gio_ra",
                table: "cham_cong",
                type: "datetime",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE cham_cong SET
                    gio_vao = COALESCE(gio_vao_sang, gio_vao_chieu),
                    gio_ra = COALESCE(gio_ra_chieu, gio_ra_sang)
                """);

            migrationBuilder.DropColumn(
                name: "buoi",
                table: "lich_su_chinh_sua_cham_cong");

            migrationBuilder.DropColumn(
                name: "tinh_trang_cu",
                table: "lich_su_chinh_sua_cham_cong");

            migrationBuilder.DropColumn(
                name: "tinh_trang_moi",
                table: "lich_su_chinh_sua_cham_cong");

            migrationBuilder.DropColumn(
                name: "gio_ra_chieu",
                table: "cham_cong");

            migrationBuilder.DropColumn(name: "gio_vao_sang", table: "cham_cong");

            migrationBuilder.DropColumn(
                name: "gio_ra_sang",
                table: "cham_cong");

            migrationBuilder.DropColumn(
                name: "gio_vao_chieu",
                table: "cham_cong");

            migrationBuilder.DropColumn(
                name: "ly_do_chieu",
                table: "cham_cong");

            migrationBuilder.DropColumn(
                name: "ly_do_sang",
                table: "cham_cong");

            migrationBuilder.DropColumn(
                name: "ly_do_tu_choi",
                table: "cham_cong");

            migrationBuilder.DropColumn(name: "tu_choi_luc", table: "cham_cong");

            migrationBuilder.DropColumn(
                name: "nguoi_tu_choi_tai_khoan_id",
                table: "cham_cong");

            migrationBuilder.DropColumn(
                name: "tinh_trang_chieu",
                table: "cham_cong");

            migrationBuilder.DropColumn(
                name: "tinh_trang_sang",
                table: "cham_cong");

            migrationBuilder.AlterColumn<string>(
                name: "trang_thai_duyet",
                table: "cham_cong",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "CHO_DUYET",
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "CHUA_HOAN_TAT");

            migrationBuilder.AddColumn<string>(
                name: "ghi_chu",
                table: "cham_cong",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<ushort>(
                name: "so_phut_tre",
                table: "cham_cong",
                type: "smallint unsigned",
                nullable: false,
                defaultValue: (ushort)0);

            migrationBuilder.AddColumn<string>(
                name: "trang_thai",
                table: "cham_cong",
                type: "varchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "DI_LAM");

            migrationBuilder.AddCheckConstraint(
                name: "check_cham_cong_gio",
                table: "cham_cong",
                sql: "gio_vao IS NULL OR gio_ra IS NULL OR gio_ra >= gio_vao");

            migrationBuilder.AddCheckConstraint(
                name: "check_cham_cong_trang_thai",
                table: "cham_cong",
                sql: "trang_thai IN ('DI_LAM', 'NGHI_PHEP', 'NGHI_KHONG_PHEP')");
        }
    }
}
