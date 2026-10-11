using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BTL_NhaKhoa.Migrations
{
    /// <inheritdoc />
    public partial class KhoiTao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "con_nguoi",
                columns: table => new
                {
                    ma_con_nguoi = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    so_dinh_danh = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    ho_ten = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    so_dien_thoai = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false),
                    gioi_tinh = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    ngay_sinh = table.Column<DateOnly>(type: "date", nullable: true),
                    dia_chi = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_con_nguoi", x => x.ma_con_nguoi);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "dich_vu",
                columns: table => new
                {
                    ma_dich_vu = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ten_dich_vu = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    mo_ta = table.Column<string>(type: "text", nullable: true),
                    don_gia = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: false),
                    thoi_luong_du_kien_phut = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    trang_thai = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "DANG_HOAT_DONG"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dich_vu", x => x.ma_dich_vu);
                    table.CheckConstraint("check_dich_vu_don_gia", "don_gia >= 0");
                    table.CheckConstraint("check_dich_vu_thoi_luong", "thoi_luong_du_kien_phut > 0");
                    table.CheckConstraint("check_dich_vu_trang_thai", "trang_thai IN ('DANG_HOAT_DONG', 'NGUNG_HOAT_DONG')");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "phong",
                columns: table => new
                {
                    ma_phong = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ten_phong = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    trang_thai = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "HOAT_DONG"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_phong", x => x.ma_phong);
                    table.CheckConstraint("check_phong_trang_thai", "trang_thai IN ('HOAT_DONG', 'TAM_NGUNG', 'BAO_TRI')");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "vai_tro",
                columns: table => new
                {
                    ma_vai_tro = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    ten_vai_tro = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vai_tro", x => x.ma_vai_tro);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "khach_hang",
                columns: table => new
                {
                    ma_khach_hang = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ma_con_nguoi = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ngay_dang_ky = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "(CURRENT_DATE)"),
                    tien_su_benh = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_khach_hang", x => x.ma_khach_hang);
                    table.ForeignKey(
                        name: "FK_khach_hang_con_nguoi_ma_con_nguoi",
                        column: x => x.ma_con_nguoi,
                        principalTable: "con_nguoi",
                        principalColumn: "ma_con_nguoi",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "nhan_vien",
                columns: table => new
                {
                    ma_nhan_vien = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ma_con_nguoi = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ngay_vao_lam = table.Column<DateOnly>(type: "date", nullable: true),
                    trang_thai = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "DANG_LAM_VIEC"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nhan_vien", x => x.ma_nhan_vien);
                    table.CheckConstraint("check_nhan_vien_trang_thai", "trang_thai IN ('DANG_LAM_VIEC', 'DA_NGHI')");
                    table.ForeignKey(
                        name: "FK_nhan_vien_con_nguoi_ma_con_nguoi",
                        column: x => x.ma_con_nguoi,
                        principalTable: "con_nguoi",
                        principalColumn: "ma_con_nguoi",
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
                    ten_thiet_bi = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    loai_thiet_bi = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    ngay_mua = table.Column<DateOnly>(type: "date", nullable: true),
                    tinh_trang = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "HOAT_DONG"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                name: "tai_khoan",
                columns: table => new
                {
                    ma_tai_khoan = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ma_con_nguoi = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ma_vai_tro = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    mat_khau_hash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    trang_thai = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "HOAT_DONG"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tai_khoan", x => x.ma_tai_khoan);
                    table.CheckConstraint("check_tai_khoan_trang_thai", "trang_thai IN ('HOAT_DONG', 'VO_HIEU_HOA')");
                    table.ForeignKey(
                        name: "FK_tai_khoan_con_nguoi_ma_con_nguoi",
                        column: x => x.ma_con_nguoi,
                        principalTable: "con_nguoi",
                        principalColumn: "ma_con_nguoi",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tai_khoan_vai_tro_ma_vai_tro",
                        column: x => x.ma_vai_tro,
                        principalTable: "vai_tro",
                        principalColumn: "ma_vai_tro",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "bac_si",
                columns: table => new
                {
                    ma_bac_si = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ma_nhan_vien = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    chung_chi = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    bang_cap = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    so_nam_kinh_nghiem = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bac_si", x => x.ma_bac_si);
                    table.ForeignKey(
                        name: "FK_bac_si_nhan_vien_ma_nhan_vien",
                        column: x => x.ma_nhan_vien,
                        principalTable: "nhan_vien",
                        principalColumn: "ma_nhan_vien",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "bao_tri",
                columns: table => new
                {
                    ma_bao_tri = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    nguoi_bao_tai_khoan_id = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    nguoi_xu_ly_tai_khoan_id = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    ngay_bao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ngay_hoan_tat = table.Column<DateTime>(type: "datetime", nullable: true),
                    noi_dung = table.Column<string>(type: "text", nullable: false),
                    chi_phi = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: true),
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
                name: "cham_cong",
                columns: table => new
                {
                    ma_cham_cong = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ma_nhan_vien = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ngay_cham_cong = table.Column<DateOnly>(type: "date", nullable: false),
                    gio_vao = table.Column<DateTime>(type: "datetime", nullable: true),
                    gio_ra = table.Column<DateTime>(type: "datetime", nullable: true),
                    so_phut_tre = table.Column<ushort>(type: "smallint unsigned", nullable: false, defaultValue: (ushort)0),
                    trang_thai = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "DI_LAM"),
                    trang_thai_duyet = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "CHO_DUYET"),
                    nguoi_duyet_tai_khoan_id = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    duyet_luc = table.Column<DateTime>(type: "datetime", nullable: true),
                    ghi_chu = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cham_cong", x => x.ma_cham_cong);
                    table.CheckConstraint("check_cham_cong_duyet", "trang_thai_duyet IN ('CHO_DUYET', 'DA_DUYET', 'TU_CHOI')");
                    table.CheckConstraint("check_cham_cong_gio", "gio_vao IS NULL OR gio_ra IS NULL OR gio_ra >= gio_vao");
                    table.CheckConstraint("check_cham_cong_trang_thai", "trang_thai IN ('DI_LAM', 'NGHI_PHEP', 'NGHI_KHONG_PHEP')");
                    table.ForeignKey(
                        name: "FK_cham_cong_nhan_vien_ma_nhan_vien",
                        column: x => x.ma_nhan_vien,
                        principalTable: "nhan_vien",
                        principalColumn: "ma_nhan_vien",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_cham_cong_tai_khoan_nguoi_duyet_tai_khoan_id",
                        column: x => x.nguoi_duyet_tai_khoan_id,
                        principalTable: "tai_khoan",
                        principalColumn: "ma_tai_khoan",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "lich_hen",
                columns: table => new
                {
                    ma_lich_hen = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ma_khach_hang = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ma_bac_si = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    ma_phong = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    nguoi_tao_tai_khoan_id = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    nguoi_xac_nhan_tai_khoan_id = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    thoi_gian_bat_dau = table.Column<DateTime>(type: "datetime", nullable: false),
                    thoi_luong_du_kien_phut = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    trang_thai = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "CHO_XAC_NHAN"),
                    ghi_chu = table.Column<string>(type: "text", nullable: true),
                    ly_do_huy = table.Column<string>(type: "text", nullable: true),
                    xac_nhan_luc = table.Column<DateTime>(type: "datetime", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lich_hen", x => x.ma_lich_hen);
                    table.CheckConstraint("check_lich_hen_thoi_luong", "thoi_luong_du_kien_phut > 0");
                    table.CheckConstraint("check_lich_hen_trang_thai", "trang_thai IN ('CHO_XAC_NHAN', 'DA_XAC_NHAN', 'DANG_KHAM', 'DA_HOAN_THANH', 'DA_HUY', 'KHONG_DEN')");
                    table.ForeignKey(
                        name: "FK_lich_hen_bac_si_ma_bac_si",
                        column: x => x.ma_bac_si,
                        principalTable: "bac_si",
                        principalColumn: "ma_bac_si",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lich_hen_khach_hang_ma_khach_hang",
                        column: x => x.ma_khach_hang,
                        principalTable: "khach_hang",
                        principalColumn: "ma_khach_hang",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lich_hen_phong_ma_phong",
                        column: x => x.ma_phong,
                        principalTable: "phong",
                        principalColumn: "ma_phong",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lich_hen_tai_khoan_nguoi_tao_tai_khoan_id",
                        column: x => x.nguoi_tao_tai_khoan_id,
                        principalTable: "tai_khoan",
                        principalColumn: "ma_tai_khoan",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lich_hen_tai_khoan_nguoi_xac_nhan_tai_khoan_id",
                        column: x => x.nguoi_xac_nhan_tai_khoan_id,
                        principalTable: "tai_khoan",
                        principalColumn: "ma_tai_khoan",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "lich_lam_viec_bac_si",
                columns: table => new
                {
                    ma_lich_lam_viec = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ma_bac_si = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    thu_trong_tuan = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    gio_bat_dau = table.Column<TimeOnly>(type: "time", nullable: false),
                    gio_ket_thuc = table.Column<TimeOnly>(type: "time", nullable: false),
                    ngay_hieu_luc_tu = table.Column<DateOnly>(type: "date", nullable: false),
                    ngay_hieu_luc_den = table.Column<DateOnly>(type: "date", nullable: true),
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

            migrationBuilder.CreateTable(
                name: "lich_su_chinh_sua_cham_cong",
                columns: table => new
                {
                    ma_lich_su = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ma_cham_cong = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    nguoi_sua_tai_khoan_id = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    gio_vao_cu = table.Column<DateTime>(type: "datetime", nullable: true),
                    gio_ra_cu = table.Column<DateTime>(type: "datetime", nullable: true),
                    gio_vao_moi = table.Column<DateTime>(type: "datetime", nullable: true),
                    gio_ra_moi = table.Column<DateTime>(type: "datetime", nullable: true),
                    ly_do = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    thoi_diem_sua = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lich_su_chinh_sua_cham_cong", x => x.ma_lich_su);
                    table.ForeignKey(
                        name: "FK_lich_su_chinh_sua_cham_cong_cham_cong_ma_cham_cong",
                        column: x => x.ma_cham_cong,
                        principalTable: "cham_cong",
                        principalColumn: "ma_cham_cong",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lich_su_chinh_sua_cham_cong_tai_khoan_nguoi_sua_tai_khoan_id",
                        column: x => x.nguoi_sua_tai_khoan_id,
                        principalTable: "tai_khoan",
                        principalColumn: "ma_tai_khoan",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "chi_tiet_lich_hen",
                columns: table => new
                {
                    ma_chi_tiet_lich_hen = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ma_lich_hen = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ma_dich_vu = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    don_gia_tai_thoi_diem_dat = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: false),
                    thoi_luong_tai_thoi_diem_dat_phut = table.Column<ushort>(type: "smallint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chi_tiet_lich_hen", x => x.ma_chi_tiet_lich_hen);
                    table.CheckConstraint("check_chi_tiet_lich_hen_gia", "don_gia_tai_thoi_diem_dat >= 0");
                    table.CheckConstraint("check_chi_tiet_lich_hen_thoi_luong", "thoi_luong_tai_thoi_diem_dat_phut > 0");
                    table.ForeignKey(
                        name: "FK_chi_tiet_lich_hen_dich_vu_ma_dich_vu",
                        column: x => x.ma_dich_vu,
                        principalTable: "dich_vu",
                        principalColumn: "ma_dich_vu",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_chi_tiet_lich_hen_lich_hen_ma_lich_hen",
                        column: x => x.ma_lich_hen,
                        principalTable: "lich_hen",
                        principalColumn: "ma_lich_hen",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ho_so_kham",
                columns: table => new
                {
                    ma_ho_so_kham = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ma_khach_hang = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ma_bac_si = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ma_lich_hen = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    ngay_kham = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    trieu_chung = table.Column<string>(type: "text", nullable: true),
                    chan_doan = table.Column<string>(type: "text", nullable: true),
                    tom_tat_cho_khach = table.Column<string>(type: "text", nullable: true),
                    ghi_chu_noi_bo = table.Column<string>(type: "text", nullable: true),
                    ngay_tai_kham = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ho_so_kham", x => x.ma_ho_so_kham);
                    table.ForeignKey(
                        name: "FK_ho_so_kham_bac_si_ma_bac_si",
                        column: x => x.ma_bac_si,
                        principalTable: "bac_si",
                        principalColumn: "ma_bac_si",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ho_so_kham_khach_hang_ma_khach_hang",
                        column: x => x.ma_khach_hang,
                        principalTable: "khach_hang",
                        principalColumn: "ma_khach_hang",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ho_so_kham_lich_hen_ma_lich_hen",
                        column: x => x.ma_lich_hen,
                        principalTable: "lich_hen",
                        principalColumn: "ma_lich_hen",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "dich_vu_thuc_hien",
                columns: table => new
                {
                    ma_dich_vu_thuc_hien = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ma_ho_so_kham = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ma_chi_tiet_lich_hen = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    ma_dich_vu = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ten_dich_vu_tai_thoi_diem_thuc_hien = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    don_gia_thuc_te = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: false),
                    so_luong = table.Column<ushort>(type: "smallint unsigned", nullable: false, defaultValue: (ushort)1),
                    thoi_diem_thuc_hien = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ghi_chu = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dich_vu_thuc_hien", x => x.ma_dich_vu_thuc_hien);
                    table.CheckConstraint("check_dich_vu_thuc_hien_gia", "don_gia_thuc_te >= 0");
                    table.CheckConstraint("check_dich_vu_thuc_hien_so_luong", "so_luong > 0");
                    table.ForeignKey(
                        name: "FK_dich_vu_thuc_hien_chi_tiet_lich_hen_ma_chi_tiet_lich_hen",
                        column: x => x.ma_chi_tiet_lich_hen,
                        principalTable: "chi_tiet_lich_hen",
                        principalColumn: "ma_chi_tiet_lich_hen",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_dich_vu_thuc_hien_dich_vu_ma_dich_vu",
                        column: x => x.ma_dich_vu,
                        principalTable: "dich_vu",
                        principalColumn: "ma_dich_vu",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_dich_vu_thuc_hien_ho_so_kham_ma_ho_so_kham",
                        column: x => x.ma_ho_so_kham,
                        principalTable: "ho_so_kham",
                        principalColumn: "ma_ho_so_kham",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "don_thuoc",
                columns: table => new
                {
                    ma_don_thuoc = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ma_ho_so_kham = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ngay_ke = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ghi_chu = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_don_thuoc", x => x.ma_don_thuoc);
                    table.ForeignKey(
                        name: "FK_don_thuoc_ho_so_kham_ma_ho_so_kham",
                        column: x => x.ma_ho_so_kham,
                        principalTable: "ho_so_kham",
                        principalColumn: "ma_ho_so_kham",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "hoa_don",
                columns: table => new
                {
                    ma_hoa_don = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ma_ho_so_kham = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ngay_lap = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    tong_tien = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: false, defaultValue: 0m),
                    trang_thai = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "CHUA_THANH_TOAN"),
                    nguoi_lap_tai_khoan_id = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hoa_don", x => x.ma_hoa_don);
                    table.CheckConstraint("check_hoa_don_tong_tien", "tong_tien >= 0");
                    table.CheckConstraint("check_hoa_don_trang_thai", "trang_thai IN ('CHUA_THANH_TOAN', 'THANH_TOAN_MOT_PHAN', 'DA_THANH_TOAN', 'DA_HUY')");
                    table.ForeignKey(
                        name: "FK_hoa_don_ho_so_kham_ma_ho_so_kham",
                        column: x => x.ma_ho_so_kham,
                        principalTable: "ho_so_kham",
                        principalColumn: "ma_ho_so_kham",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hoa_don_tai_khoan_nguoi_lap_tai_khoan_id",
                        column: x => x.nguoi_lap_tai_khoan_id,
                        principalTable: "tai_khoan",
                        principalColumn: "ma_tai_khoan",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "chi_tiet_don_thuoc",
                columns: table => new
                {
                    ma_chi_tiet_don_thuoc = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ma_don_thuoc = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ten_thuoc = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    ham_luong = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    lieu_dung = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    so_lan_moi_ngay = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    so_ngay_dung = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    so_luong = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    huong_dan = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chi_tiet_don_thuoc", x => x.ma_chi_tiet_don_thuoc);
                    table.CheckConstraint("check_chi_tiet_don_thuoc_so_luong", "so_luong > 0");
                    table.ForeignKey(
                        name: "FK_chi_tiet_don_thuoc_don_thuoc_ma_don_thuoc",
                        column: x => x.ma_don_thuoc,
                        principalTable: "don_thuoc",
                        principalColumn: "ma_don_thuoc",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "chi_tiet_hoa_don",
                columns: table => new
                {
                    ma_chi_tiet_hoa_don = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ma_hoa_don = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ma_dich_vu_thuc_hien = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ten_dich_vu = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    don_gia = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: false),
                    so_luong = table.Column<ushort>(type: "smallint unsigned", nullable: false, defaultValue: (ushort)1),
                    thanh_tien = table.Column<decimal>(type: "decimal(17,2)", precision: 17, scale: 2, nullable: false, computedColumnSql: "don_gia * so_luong", stored: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chi_tiet_hoa_don", x => x.ma_chi_tiet_hoa_don);
                    table.CheckConstraint("check_chi_tiet_hoa_don_gia", "don_gia >= 0");
                    table.CheckConstraint("check_chi_tiet_hoa_don_so_luong", "so_luong > 0");
                    table.ForeignKey(
                        name: "FK_chi_tiet_hoa_don_dich_vu_thuc_hien_ma_dich_vu_thuc_hien",
                        column: x => x.ma_dich_vu_thuc_hien,
                        principalTable: "dich_vu_thuc_hien",
                        principalColumn: "ma_dich_vu_thuc_hien",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_chi_tiet_hoa_don_hoa_don_ma_hoa_don",
                        column: x => x.ma_hoa_don,
                        principalTable: "hoa_don",
                        principalColumn: "ma_hoa_don",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "thanh_toan",
                columns: table => new
                {
                    ma_thanh_toan = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ma_hoa_don = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    nguoi_xac_nhan_tai_khoan_id = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    so_tien = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: false),
                    phuong_thuc = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    trang_thai = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "DA_XAC_NHAN"),
                    thoi_diem_thanh_toan = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ly_do_huy = table.Column<string>(type: "text", nullable: true),
                    ghi_chu = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_thanh_toan", x => x.ma_thanh_toan);
                    table.CheckConstraint("check_thanh_toan_phuong_thuc", "phuong_thuc IN ('TIEN_MAT', 'CHUYEN_KHOAN', 'KHAC')");
                    table.CheckConstraint("check_thanh_toan_so_tien", "so_tien > 0");
                    table.CheckConstraint("check_thanh_toan_trang_thai", "trang_thai IN ('DA_XAC_NHAN', 'DA_HUY')");
                    table.ForeignKey(
                        name: "FK_thanh_toan_hoa_don_ma_hoa_don",
                        column: x => x.ma_hoa_don,
                        principalTable: "hoa_don",
                        principalColumn: "ma_hoa_don",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_thanh_toan_tai_khoan_nguoi_xac_nhan_tai_khoan_id",
                        column: x => x.nguoi_xac_nhan_tai_khoan_id,
                        principalTable: "tai_khoan",
                        principalColumn: "ma_tai_khoan",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.InsertData(
                table: "vai_tro",
                columns: new[] { "ma_vai_tro", "ten_vai_tro" },
                values: new object[,]
                {
                    { "BAC_SI", "Bác sĩ" },
                    { "KHACH_HANG", "Khách hàng" },
                    { "LE_TAN", "Lễ tân" },
                    { "QUAN_LY", "Quản lý" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_bac_si_ma_nhan_vien",
                table: "bac_si",
                column: "ma_nhan_vien",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_bao_tri_nguoi_bao_tai_khoan_id",
                table: "bao_tri",
                column: "nguoi_bao_tai_khoan_id");

            migrationBuilder.CreateIndex(
                name: "IX_bao_tri_nguoi_xu_ly_tai_khoan_id",
                table: "bao_tri",
                column: "nguoi_xu_ly_tai_khoan_id");

            migrationBuilder.CreateIndex(
                name: "IX_cham_cong_ma_nhan_vien_ngay_cham_cong",
                table: "cham_cong",
                columns: new[] { "ma_nhan_vien", "ngay_cham_cong" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cham_cong_ngay_cham_cong_trang_thai_duyet",
                table: "cham_cong",
                columns: new[] { "ngay_cham_cong", "trang_thai_duyet" });

            migrationBuilder.CreateIndex(
                name: "IX_cham_cong_nguoi_duyet_tai_khoan_id",
                table: "cham_cong",
                column: "nguoi_duyet_tai_khoan_id");

            migrationBuilder.CreateIndex(
                name: "IX_chi_tiet_don_thuoc_ma_don_thuoc",
                table: "chi_tiet_don_thuoc",
                column: "ma_don_thuoc");

            migrationBuilder.CreateIndex(
                name: "IX_chi_tiet_hoa_don_ma_dich_vu_thuc_hien",
                table: "chi_tiet_hoa_don",
                column: "ma_dich_vu_thuc_hien",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_chi_tiet_hoa_don_ma_hoa_don",
                table: "chi_tiet_hoa_don",
                column: "ma_hoa_don");

            migrationBuilder.CreateIndex(
                name: "IX_chi_tiet_lich_hen_ma_dich_vu",
                table: "chi_tiet_lich_hen",
                column: "ma_dich_vu");

            migrationBuilder.CreateIndex(
                name: "IX_chi_tiet_lich_hen_ma_lich_hen_ma_dich_vu",
                table: "chi_tiet_lich_hen",
                columns: new[] { "ma_lich_hen", "ma_dich_vu" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_con_nguoi_so_dien_thoai",
                table: "con_nguoi",
                column: "so_dien_thoai",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_con_nguoi_so_dinh_danh",
                table: "con_nguoi",
                column: "so_dinh_danh",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_dich_vu_ten_dich_vu",
                table: "dich_vu",
                column: "ten_dich_vu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_dich_vu_thuc_hien_ma_chi_tiet_lich_hen",
                table: "dich_vu_thuc_hien",
                column: "ma_chi_tiet_lich_hen",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_dich_vu_thuc_hien_ma_dich_vu",
                table: "dich_vu_thuc_hien",
                column: "ma_dich_vu");

            migrationBuilder.CreateIndex(
                name: "IX_dich_vu_thuc_hien_ma_ho_so_kham",
                table: "dich_vu_thuc_hien",
                column: "ma_ho_so_kham");

            migrationBuilder.CreateIndex(
                name: "IX_don_thuoc_ma_ho_so_kham",
                table: "don_thuoc",
                column: "ma_ho_so_kham");

            migrationBuilder.CreateIndex(
                name: "IX_ho_so_kham_ma_bac_si",
                table: "ho_so_kham",
                column: "ma_bac_si");

            migrationBuilder.CreateIndex(
                name: "IX_ho_so_kham_ma_khach_hang_ngay_kham",
                table: "ho_so_kham",
                columns: new[] { "ma_khach_hang", "ngay_kham" });

            migrationBuilder.CreateIndex(
                name: "IX_ho_so_kham_ma_lich_hen",
                table: "ho_so_kham",
                column: "ma_lich_hen",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hoa_don_ma_ho_so_kham",
                table: "hoa_don",
                column: "ma_ho_so_kham",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hoa_don_nguoi_lap_tai_khoan_id",
                table: "hoa_don",
                column: "nguoi_lap_tai_khoan_id");

            migrationBuilder.CreateIndex(
                name: "IX_khach_hang_ma_con_nguoi",
                table: "khach_hang",
                column: "ma_con_nguoi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lich_hen_ma_bac_si_thoi_gian_bat_dau",
                table: "lich_hen",
                columns: new[] { "ma_bac_si", "thoi_gian_bat_dau" });

            migrationBuilder.CreateIndex(
                name: "IX_lich_hen_ma_khach_hang_thoi_gian_bat_dau",
                table: "lich_hen",
                columns: new[] { "ma_khach_hang", "thoi_gian_bat_dau" });

            migrationBuilder.CreateIndex(
                name: "IX_lich_hen_ma_phong_thoi_gian_bat_dau",
                table: "lich_hen",
                columns: new[] { "ma_phong", "thoi_gian_bat_dau" });

            migrationBuilder.CreateIndex(
                name: "IX_lich_hen_nguoi_tao_tai_khoan_id",
                table: "lich_hen",
                column: "nguoi_tao_tai_khoan_id");

            migrationBuilder.CreateIndex(
                name: "IX_lich_hen_nguoi_xac_nhan_tai_khoan_id",
                table: "lich_hen",
                column: "nguoi_xac_nhan_tai_khoan_id");

            migrationBuilder.CreateIndex(
                name: "IX_lich_hen_trang_thai",
                table: "lich_hen",
                column: "trang_thai");

            migrationBuilder.CreateIndex(
                name: "IX_lich_lam_viec_bac_si_ma_bac_si_thu_trong_tuan_ngay_hieu_luc_~",
                table: "lich_lam_viec_bac_si",
                columns: new[] { "ma_bac_si", "thu_trong_tuan", "ngay_hieu_luc_tu", "ngay_hieu_luc_den" });

            migrationBuilder.CreateIndex(
                name: "IX_lich_su_chinh_sua_cham_cong_ma_cham_cong_thoi_diem_sua",
                table: "lich_su_chinh_sua_cham_cong",
                columns: new[] { "ma_cham_cong", "thoi_diem_sua" });

            migrationBuilder.CreateIndex(
                name: "IX_lich_su_chinh_sua_cham_cong_nguoi_sua_tai_khoan_id",
                table: "lich_su_chinh_sua_cham_cong",
                column: "nguoi_sua_tai_khoan_id");

            migrationBuilder.CreateIndex(
                name: "IX_nhan_vien_ma_con_nguoi",
                table: "nhan_vien",
                column: "ma_con_nguoi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_phong_ten_phong",
                table: "phong",
                column: "ten_phong",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tai_khoan_ma_con_nguoi",
                table: "tai_khoan",
                column: "ma_con_nguoi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tai_khoan_ma_vai_tro",
                table: "tai_khoan",
                column: "ma_vai_tro");

            migrationBuilder.CreateIndex(
                name: "IX_thanh_toan_ma_hoa_don",
                table: "thanh_toan",
                column: "ma_hoa_don");

            migrationBuilder.CreateIndex(
                name: "IX_thanh_toan_nguoi_xac_nhan_tai_khoan_id",
                table: "thanh_toan",
                column: "nguoi_xac_nhan_tai_khoan_id");

            migrationBuilder.CreateIndex(
                name: "IX_thiet_bi_ma_phong",
                table: "thiet_bi",
                column: "ma_phong");

            migrationBuilder.CreateIndex(
                name: "IX_thuc_hien_bao_tri_ma_thiet_bi",
                table: "thuc_hien_bao_tri",
                column: "ma_thiet_bi");

            migrationBuilder.CreateIndex(
                name: "IX_vai_tro_ten_vai_tro",
                table: "vai_tro",
                column: "ten_vai_tro",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "chi_tiet_don_thuoc");

            migrationBuilder.DropTable(
                name: "chi_tiet_hoa_don");

            migrationBuilder.DropTable(
                name: "lich_lam_viec_bac_si");

            migrationBuilder.DropTable(
                name: "lich_su_chinh_sua_cham_cong");

            migrationBuilder.DropTable(
                name: "thanh_toan");

            migrationBuilder.DropTable(
                name: "thuc_hien_bao_tri");

            migrationBuilder.DropTable(
                name: "don_thuoc");

            migrationBuilder.DropTable(
                name: "dich_vu_thuc_hien");

            migrationBuilder.DropTable(
                name: "cham_cong");

            migrationBuilder.DropTable(
                name: "hoa_don");

            migrationBuilder.DropTable(
                name: "bao_tri");

            migrationBuilder.DropTable(
                name: "thiet_bi");

            migrationBuilder.DropTable(
                name: "chi_tiet_lich_hen");

            migrationBuilder.DropTable(
                name: "ho_so_kham");

            migrationBuilder.DropTable(
                name: "dich_vu");

            migrationBuilder.DropTable(
                name: "lich_hen");

            migrationBuilder.DropTable(
                name: "bac_si");

            migrationBuilder.DropTable(
                name: "khach_hang");

            migrationBuilder.DropTable(
                name: "phong");

            migrationBuilder.DropTable(
                name: "tai_khoan");

            migrationBuilder.DropTable(
                name: "nhan_vien");

            migrationBuilder.DropTable(
                name: "vai_tro");

            migrationBuilder.DropTable(
                name: "con_nguoi");
        }
    }
}
