using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace BTL_NhaKhoa.Migrations
{
    /// <inheritdoc />
    public partial class BoThietBiVaBaoTri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "thuc_hien_bao_tri");

            migrationBuilder.DropTable(
                name: "bao_tri");

            migrationBuilder.DropTable(
                name: "thiet_bi");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bao_tri",
                columns: table => new
                {
                    ma_bao_tri = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    nguoi_bao_tai_khoan_id = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    nguoi_xu_ly_tai_khoan_id = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    chi_phi = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: true),
                    ngay_bao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ngay_hoan_tat = table.Column<DateTime>(type: "datetime", nullable: true),
                    noi_dung = table.Column<string>(type: "text", nullable: false),
                    trang_thai = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "MOI_TIEP_NHAN")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bao_tri", x => x.ma_bao_tri);
                    table.CheckConstraint("check_bao_tri_chi_phi", "chi_phi IS NULL OR chi_phi >= 0");
                    table.CheckConstraint("check_bao_tri_trang_thai", "trang_thai IN ('MOI_TIEP_NHAN', 'DANG_XU_LY', 'DA_HOAN_TAT', 'DA_HUY')");
                    table.ForeignKey(
                        name: "FK_bao_tri_tai_khoan_nguoi_bao_tai_khoan_id",
                        column: x => x.nguoi_bao_tai_khoan_id,
                        principalTable: "tai_khoan",
                        principalColumn: "ma_tai_khoan",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_bao_tri_tai_khoan_nguoi_xu_ly_tai_khoan_id",
                        column: x => x.nguoi_xu_ly_tai_khoan_id,
                        principalTable: "tai_khoan",
                        principalColumn: "ma_tai_khoan",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "thiet_bi",
                columns: table => new
                {
                    ma_thiet_bi = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ma_phong = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    loai_thiet_bi = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    ngay_mua = table.Column<DateTime>(type: "date", nullable: true),
                    ten_thiet_bi = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    tinh_trang = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "HOAT_DONG"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_thiet_bi", x => x.ma_thiet_bi);
                    table.CheckConstraint("check_thiet_bi_tinh_trang", "tinh_trang IN ('HOAT_DONG', 'HONG', 'DANG_BAO_TRI', 'NGUNG_SU_DUNG')");
                    table.ForeignKey(
                        name: "FK_thiet_bi_phong_ma_phong",
                        column: x => x.ma_phong,
                        principalTable: "phong",
                        principalColumn: "ma_phong",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "thuc_hien_bao_tri",
                columns: table => new
                {
                    ma_bao_tri = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ma_thiet_bi = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ghi_chu = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_thuc_hien_bao_tri", x => new { x.ma_bao_tri, x.ma_thiet_bi });
                    table.ForeignKey(
                        name: "FK_thuc_hien_bao_tri_bao_tri_ma_bao_tri",
                        column: x => x.ma_bao_tri,
                        principalTable: "bao_tri",
                        principalColumn: "ma_bao_tri",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_thuc_hien_bao_tri_thiet_bi_ma_thiet_bi",
                        column: x => x.ma_thiet_bi,
                        principalTable: "thiet_bi",
                        principalColumn: "ma_thiet_bi",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_bao_tri_nguoi_bao_tai_khoan_id",
                table: "bao_tri",
                column: "nguoi_bao_tai_khoan_id");

            migrationBuilder.CreateIndex(
                name: "IX_bao_tri_nguoi_xu_ly_tai_khoan_id",
                table: "bao_tri",
                column: "nguoi_xu_ly_tai_khoan_id");

            migrationBuilder.CreateIndex(
                name: "IX_thiet_bi_ma_phong",
                table: "thiet_bi",
                column: "ma_phong");

            migrationBuilder.CreateIndex(
                name: "IX_thuc_hien_bao_tri_ma_thiet_bi",
                table: "thuc_hien_bao_tri",
                column: "ma_thiet_bi");
        }
    }
}
