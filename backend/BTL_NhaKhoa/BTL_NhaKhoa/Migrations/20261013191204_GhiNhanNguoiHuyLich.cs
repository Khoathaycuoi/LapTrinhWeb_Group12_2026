using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTL_NhaKhoa.Migrations
{
    /// <inheritdoc />
    public partial class GhiNhanNguoiHuyLich : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "huy_luc",
                table: "lich_hen",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "nguoi_huy_tai_khoan_id",
                table: "lich_hen",
                type: "bigint unsigned",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_lich_hen_nguoi_huy_tai_khoan_id",
                table: "lich_hen",
                column: "nguoi_huy_tai_khoan_id");

            migrationBuilder.AddForeignKey(
                name: "FK_lich_hen_tai_khoan_nguoi_huy_tai_khoan_id",
                table: "lich_hen",
                column: "nguoi_huy_tai_khoan_id",
                principalTable: "tai_khoan",
                principalColumn: "ma_tai_khoan",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_lich_hen_tai_khoan_nguoi_huy_tai_khoan_id",
                table: "lich_hen");

            migrationBuilder.DropIndex(
                name: "IX_lich_hen_nguoi_huy_tai_khoan_id",
                table: "lich_hen");

            migrationBuilder.DropColumn(
                name: "huy_luc",
                table: "lich_hen");

            migrationBuilder.DropColumn(
                name: "nguoi_huy_tai_khoan_id",
                table: "lich_hen");
        }
    }
}
