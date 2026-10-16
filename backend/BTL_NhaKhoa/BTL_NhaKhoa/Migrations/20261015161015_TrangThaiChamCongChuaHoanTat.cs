using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTL_NhaKhoa.Migrations
{
    /// <inheritdoc />
    public partial class TrangThaiChamCongChuaHoanTat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "check_cham_cong_duyet",
                table: "cham_cong");

            migrationBuilder.AddCheckConstraint(
                name: "check_cham_cong_duyet",
                table: "cham_cong",
                sql: "trang_thai_duyet IN ('CHUA_HOAN_TAT', 'CHO_DUYET', 'DA_DUYET', 'TU_CHOI')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "check_cham_cong_duyet",
                table: "cham_cong");

            migrationBuilder.AddCheckConstraint(
                name: "check_cham_cong_duyet",
                table: "cham_cong",
                sql: "trang_thai_duyet IN ('CHO_DUYET', 'DA_DUYET', 'TU_CHOI')");
        }
    }
}
