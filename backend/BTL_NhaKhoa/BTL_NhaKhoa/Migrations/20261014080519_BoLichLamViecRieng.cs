using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace BTL_NhaKhoa.Migrations
{
    /// <inheritdoc />
    public partial class BoLichLamViecRieng : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "lich_lam_viec_bac_si");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "lich_lam_viec_bac_si",
                columns: table => new
                {
                    ma_lich_lam_viec = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ma_bac_si = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    gio_bat_dau = table.Column<TimeSpan>(type: "time", nullable: false),
                    gio_ket_thuc = table.Column<TimeSpan>(type: "time", nullable: false),
                    ngay_hieu_luc_den = table.Column<DateTime>(type: "date", nullable: true),
                    ngay_hieu_luc_tu = table.Column<DateTime>(type: "date", nullable: false),
                    thu_trong_tuan = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    trang_thai = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "HOAT_DONG")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lich_lam_viec_bac_si", x => x.ma_lich_lam_viec);
                    table.CheckConstraint("check_lich_lam_viec_gio", "gio_bat_dau < gio_ket_thuc");
                    table.CheckConstraint("check_lich_lam_viec_ngay", "ngay_hieu_luc_den IS NULL OR ngay_hieu_luc_den >= ngay_hieu_luc_tu");
                    table.CheckConstraint("check_lich_lam_viec_thu", "thu_trong_tuan BETWEEN 1 AND 7");
                    table.CheckConstraint("check_lich_lam_viec_trang_thai", "trang_thai IN ('HOAT_DONG', 'NGUNG_HOAT_DONG')");
                    table.ForeignKey(
                        name: "FK_lich_lam_viec_bac_si_bac_si_ma_bac_si",
                        column: x => x.ma_bac_si,
                        principalTable: "bac_si",
                        principalColumn: "ma_bac_si",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_lich_lam_viec_bac_si_ma_bac_si_thu_trong_tuan_ngay_hieu_luc_~",
                table: "lich_lam_viec_bac_si",
                columns: new[] { "ma_bac_si", "thu_trong_tuan", "ngay_hieu_luc_tu", "ngay_hieu_luc_den" });
        }
    }
}
