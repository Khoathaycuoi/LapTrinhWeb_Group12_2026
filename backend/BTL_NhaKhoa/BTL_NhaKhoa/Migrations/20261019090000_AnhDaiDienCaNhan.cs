using BTL_NhaKhoa.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTL_NhaKhoa.Migrations;

[DbContext(typeof(NhaKhoaDbContext))]
[Migration("20261019090000_AnhDaiDienCaNhan")]
public sealed class AnhDaiDienCaNhan : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ten_anh_dai_dien", table: "con_nguoi",
            type: "varchar(50)", maxLength: 50, nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "ten_anh_dai_dien", table: "con_nguoi");
    }
}
