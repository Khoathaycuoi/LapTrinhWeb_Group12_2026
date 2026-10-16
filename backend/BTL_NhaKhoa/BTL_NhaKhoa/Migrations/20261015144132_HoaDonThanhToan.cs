using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTL_NhaKhoa.Migrations
{
    /// <inheritdoc />
    public partial class HoaDonThanhToan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "huy_luc",
                table: "thanh_toan",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "nguoi_huy_tai_khoan_id",
                table: "thanh_toan",
                type: "bigint unsigned",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_thanh_toan_nguoi_huy_tai_khoan_id",
                table: "thanh_toan",
                column: "nguoi_huy_tai_khoan_id");

            migrationBuilder.AddForeignKey(
                name: "FK_thanh_toan_tai_khoan_nguoi_huy_tai_khoan_id",
                table: "thanh_toan",
                column: "nguoi_huy_tai_khoan_id",
                principalTable: "tai_khoan",
                principalColumn: "ma_tai_khoan",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_thanh_toan_tai_khoan_nguoi_huy_tai_khoan_id",
                table: "thanh_toan");

            migrationBuilder.DropIndex(
                name: "IX_thanh_toan_nguoi_huy_tai_khoan_id",
                table: "thanh_toan");

            migrationBuilder.DropColumn(
                name: "huy_luc",
                table: "thanh_toan");

            migrationBuilder.DropColumn(
                name: "nguoi_huy_tai_khoan_id",
                table: "thanh_toan");
        }
    }
}
