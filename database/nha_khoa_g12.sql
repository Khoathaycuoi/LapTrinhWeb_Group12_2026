CREATE DATABASE IF NOT EXISTS `nha_khoa_g12` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE `nha_khoa_g12`;
SET NAMES utf8mb4;

CREATE TABLE `con_nguoi` (
    `ma_con_nguoi` bigint unsigned NOT NULL AUTO_INCREMENT,
    `so_dinh_danh` varchar(20) NULL,
    `ho_ten` varchar(150) NOT NULL,
    `so_dien_thoai` varchar(15) NOT NULL,
    `gioi_tinh` varchar(20) NULL,
    `ngay_sinh` date NULL,
    `dia_chi` varchar(255) NULL,
    `ten_anh_dai_dien` varchar(50) NULL,
    `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`ma_con_nguoi`)
);


CREATE TABLE `dich_vu` (
    `ma_dich_vu` bigint unsigned NOT NULL AUTO_INCREMENT,
    `ten_dich_vu` varchar(150) NOT NULL,
    `mo_ta` text NULL,
    `don_gia` decimal(15,2) NOT NULL,
    `thoi_luong_du_kien_phut` smallint unsigned NOT NULL,
    `trang_thai` varchar(20) NOT NULL DEFAULT 'DANG_HOAT_DONG',
    `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`ma_dich_vu`),
    CONSTRAINT `check_dich_vu_don_gia` CHECK (don_gia >= 0),
    CONSTRAINT `check_dich_vu_thoi_luong` CHECK (thoi_luong_du_kien_phut > 0),
    CONSTRAINT `check_dich_vu_trang_thai` CHECK (trang_thai IN ('DANG_HOAT_DONG', 'NGUNG_HOAT_DONG'))
);


CREATE TABLE `phong` (
    `ma_phong` bigint unsigned NOT NULL AUTO_INCREMENT,
    `ten_phong` varchar(100) NOT NULL,
    `trang_thai` varchar(20) NOT NULL DEFAULT 'HOAT_DONG',
    `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`ma_phong`),
    CONSTRAINT `check_phong_trang_thai` CHECK (trang_thai IN ('HOAT_DONG', 'TAM_NGUNG', 'BAO_TRI'))
);


CREATE TABLE `vai_tro` (
    `ma_vai_tro` varchar(30) NOT NULL,
    `ten_vai_tro` varchar(100) NOT NULL,
    PRIMARY KEY (`ma_vai_tro`)
);


CREATE TABLE `khach_hang` (
    `ma_khach_hang` bigint unsigned NOT NULL AUTO_INCREMENT,
    `ma_con_nguoi` bigint unsigned NOT NULL,
    `ngay_dang_ky` date NOT NULL DEFAULT (CURRENT_DATE),
    `tien_su_benh` text NULL,
    `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`ma_khach_hang`),
    CONSTRAINT `FK_khach_hang_con_nguoi_ma_con_nguoi` FOREIGN KEY (`ma_con_nguoi`) REFERENCES `con_nguoi` (`ma_con_nguoi`) ON DELETE RESTRICT
);


CREATE TABLE `nhan_vien` (
    `ma_nhan_vien` bigint unsigned NOT NULL AUTO_INCREMENT,
    `ma_con_nguoi` bigint unsigned NOT NULL,
    `ngay_vao_lam` date NULL,
    `trang_thai` varchar(20) NOT NULL DEFAULT 'DANG_LAM_VIEC',
    `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`ma_nhan_vien`),
    CONSTRAINT `check_nhan_vien_trang_thai` CHECK (trang_thai IN ('DANG_LAM_VIEC', 'DA_NGHI')),
    CONSTRAINT `FK_nhan_vien_con_nguoi_ma_con_nguoi` FOREIGN KEY (`ma_con_nguoi`) REFERENCES `con_nguoi` (`ma_con_nguoi`) ON DELETE RESTRICT
);


CREATE TABLE `tai_khoan` (
    `ma_tai_khoan` bigint unsigned NOT NULL AUTO_INCREMENT,
    `ma_con_nguoi` bigint unsigned NOT NULL,
    `ma_vai_tro` varchar(30) NOT NULL,
    `mat_khau_hash` varchar(255) NOT NULL,
    `trang_thai` varchar(20) NOT NULL DEFAULT 'HOAT_DONG',
    `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`ma_tai_khoan`),
    CONSTRAINT `check_tai_khoan_trang_thai` CHECK (trang_thai IN ('HOAT_DONG', 'VO_HIEU_HOA')),
    CONSTRAINT `FK_tai_khoan_con_nguoi_ma_con_nguoi` FOREIGN KEY (`ma_con_nguoi`) REFERENCES `con_nguoi` (`ma_con_nguoi`) ON DELETE RESTRICT,
    CONSTRAINT `FK_tai_khoan_vai_tro_ma_vai_tro` FOREIGN KEY (`ma_vai_tro`) REFERENCES `vai_tro` (`ma_vai_tro`) ON DELETE RESTRICT
);


CREATE TABLE `bac_si` (
    `ma_bac_si` bigint unsigned NOT NULL AUTO_INCREMENT,
    `ma_nhan_vien` bigint unsigned NOT NULL,
    `chung_chi` varchar(255) NULL,
    `bang_cap` varchar(255) NULL,
    `so_nam_kinh_nghiem` smallint unsigned NULL,
    `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`ma_bac_si`),
    CONSTRAINT `FK_bac_si_nhan_vien_ma_nhan_vien` FOREIGN KEY (`ma_nhan_vien`) REFERENCES `nhan_vien` (`ma_nhan_vien`) ON DELETE RESTRICT
);


CREATE TABLE `cham_cong` (
    `ma_cham_cong` bigint unsigned NOT NULL AUTO_INCREMENT,
    `ma_nhan_vien` bigint unsigned NOT NULL,
    `ngay_cham_cong` date NOT NULL,
    `gio_vao_sang` datetime NULL,
    `gio_ra_sang` datetime NULL,
    `gio_vao_chieu` datetime NULL,
    `gio_ra_chieu` datetime NULL,
    `tinh_trang_sang` varchar(25) NOT NULL DEFAULT 'CHUA_XAC_DINH',
    `tinh_trang_chieu` varchar(25) NOT NULL DEFAULT 'CHUA_XAC_DINH',
    `ly_do_sang` varchar(500) NULL,
    `ly_do_chieu` varchar(500) NULL,
    `trang_thai_duyet` varchar(20) NOT NULL DEFAULT 'CHUA_HOAN_TAT',
    `nguoi_duyet_tai_khoan_id` bigint unsigned NULL,
    `duyet_luc` datetime NULL,
    `nguoi_tu_choi_tai_khoan_id` bigint unsigned NULL,
    `tu_choi_luc` datetime NULL,
    `ly_do_tu_choi` varchar(500) NULL,
    PRIMARY KEY (`ma_cham_cong`),
    CONSTRAINT `check_cham_cong_duyet` CHECK (trang_thai_duyet IN ('CHUA_HOAN_TAT', 'CHO_DUYET', 'DA_DUYET', 'TU_CHOI')),
    CONSTRAINT `check_cham_cong_gio_chieu` CHECK (gio_vao_chieu IS NULL OR gio_ra_chieu IS NULL OR gio_ra_chieu >= gio_vao_chieu),
    CONSTRAINT `check_cham_cong_gio_sang` CHECK (gio_vao_sang IS NULL OR gio_ra_sang IS NULL OR gio_ra_sang >= gio_vao_sang),
    CONSTRAINT `check_cham_cong_tinh_trang_chieu` CHECK (tinh_trang_chieu IN ('CHUA_XAC_DINH', 'LAM_VIEC', 'NGHI_PHEP', 'VANG_KHONG_PHEP')),
    CONSTRAINT `check_cham_cong_tinh_trang_sang` CHECK (tinh_trang_sang IN ('CHUA_XAC_DINH', 'LAM_VIEC', 'NGHI_PHEP', 'VANG_KHONG_PHEP')),
    CONSTRAINT `FK_cham_cong_nhan_vien_ma_nhan_vien` FOREIGN KEY (`ma_nhan_vien`) REFERENCES `nhan_vien` (`ma_nhan_vien`) ON DELETE RESTRICT,
    CONSTRAINT `FK_cham_cong_tai_khoan_nguoi_duyet_tai_khoan_id` FOREIGN KEY (`nguoi_duyet_tai_khoan_id`) REFERENCES `tai_khoan` (`ma_tai_khoan`) ON DELETE RESTRICT,
    CONSTRAINT `FK_cham_cong_tai_khoan_nguoi_tu_choi_tai_khoan_id` FOREIGN KEY (`nguoi_tu_choi_tai_khoan_id`) REFERENCES `tai_khoan` (`ma_tai_khoan`) ON DELETE RESTRICT
);


CREATE TABLE `lich_hen` (
    `ma_lich_hen` bigint unsigned NOT NULL AUTO_INCREMENT,
    `ma_khach_hang` bigint unsigned NOT NULL,
    `ma_bac_si` bigint unsigned NULL,
    `ma_phong` bigint unsigned NULL,
    `nguoi_tao_tai_khoan_id` bigint unsigned NOT NULL,
    `nguoi_xac_nhan_tai_khoan_id` bigint unsigned NULL,
    `nguoi_huy_tai_khoan_id` bigint unsigned NULL,
    `nguoi_tiep_nhan_tai_khoan_id` bigint unsigned NULL,
    `nguoi_danh_dau_khong_den_tai_khoan_id` bigint unsigned NULL,
    `thoi_gian_bat_dau` datetime NOT NULL,
    `thoi_luong_du_kien_phut` smallint unsigned NOT NULL,
    `trang_thai` varchar(30) NOT NULL DEFAULT 'CHO_XAC_NHAN',
    `ghi_chu` text NULL,
    `ly_do_huy` text NULL,
    `ly_do_tu_choi` text NULL,
    `xac_nhan_luc` datetime NULL,
    `den_luc` datetime NULL,
    `bat_dau_kham_luc` datetime NULL,
    `hoan_thanh_luc` datetime NULL,
    `khong_den_luc` datetime NULL,
    `ghi_chu_khong_den` text NULL,
    `huy_luc` datetime NULL,
    `tu_choi_luc` datetime NULL,
    `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`ma_lich_hen`),
    CONSTRAINT `check_lich_hen_thoi_luong` CHECK (thoi_luong_du_kien_phut > 0),
    CONSTRAINT `check_lich_hen_trang_thai` CHECK (trang_thai IN ('CHO_XAC_NHAN', 'HET_HAN', 'DA_XAC_NHAN', 'DA_DEN', 'DANG_KHAM', 'DA_HOAN_THANH', 'DA_HUY', 'KHONG_DEN', 'TU_CHOI')),
    CONSTRAINT `FK_lich_hen_bac_si_ma_bac_si` FOREIGN KEY (`ma_bac_si`) REFERENCES `bac_si` (`ma_bac_si`) ON DELETE RESTRICT,
    CONSTRAINT `FK_lich_hen_khach_hang_ma_khach_hang` FOREIGN KEY (`ma_khach_hang`) REFERENCES `khach_hang` (`ma_khach_hang`) ON DELETE RESTRICT,
    CONSTRAINT `FK_lich_hen_phong_ma_phong` FOREIGN KEY (`ma_phong`) REFERENCES `phong` (`ma_phong`) ON DELETE RESTRICT,
    CONSTRAINT `FK_lich_hen_tai_khoan_nguoi_danh_dau_khong_den_tai_khoan_id` FOREIGN KEY (`nguoi_danh_dau_khong_den_tai_khoan_id`) REFERENCES `tai_khoan` (`ma_tai_khoan`) ON DELETE RESTRICT,
    CONSTRAINT `FK_lich_hen_tai_khoan_nguoi_huy_tai_khoan_id` FOREIGN KEY (`nguoi_huy_tai_khoan_id`) REFERENCES `tai_khoan` (`ma_tai_khoan`) ON DELETE RESTRICT,
    CONSTRAINT `FK_lich_hen_tai_khoan_nguoi_tao_tai_khoan_id` FOREIGN KEY (`nguoi_tao_tai_khoan_id`) REFERENCES `tai_khoan` (`ma_tai_khoan`) ON DELETE RESTRICT,
    CONSTRAINT `FK_lich_hen_tai_khoan_nguoi_tiep_nhan_tai_khoan_id` FOREIGN KEY (`nguoi_tiep_nhan_tai_khoan_id`) REFERENCES `tai_khoan` (`ma_tai_khoan`) ON DELETE RESTRICT,
    CONSTRAINT `FK_lich_hen_tai_khoan_nguoi_xac_nhan_tai_khoan_id` FOREIGN KEY (`nguoi_xac_nhan_tai_khoan_id`) REFERENCES `tai_khoan` (`ma_tai_khoan`) ON DELETE RESTRICT
);


CREATE TABLE `lich_su_chinh_sua_cham_cong` (
    `ma_lich_su` bigint unsigned NOT NULL AUTO_INCREMENT,
    `ma_cham_cong` bigint unsigned NOT NULL,
    `nguoi_sua_tai_khoan_id` bigint unsigned NOT NULL,
    `buoi` varchar(10) NOT NULL DEFAULT 'CA_NGAY',
    `tinh_trang_cu` varchar(25) NULL,
    `tinh_trang_moi` varchar(25) NULL,
    `gio_vao_cu` datetime NULL,
    `gio_ra_cu` datetime NULL,
    `gio_vao_moi` datetime NULL,
    `gio_ra_moi` datetime NULL,
    `ly_do` varchar(500) NOT NULL,
    `thoi_diem_sua` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`ma_lich_su`),
    CONSTRAINT `FK_lich_su_chinh_sua_cham_cong_cham_cong_ma_cham_cong` FOREIGN KEY (`ma_cham_cong`) REFERENCES `cham_cong` (`ma_cham_cong`) ON DELETE RESTRICT,
    CONSTRAINT `FK_lich_su_chinh_sua_cham_cong_tai_khoan_nguoi_sua_tai_khoan_id` FOREIGN KEY (`nguoi_sua_tai_khoan_id`) REFERENCES `tai_khoan` (`ma_tai_khoan`) ON DELETE RESTRICT
);


CREATE TABLE `chi_tiet_lich_hen` (
    `ma_chi_tiet_lich_hen` bigint unsigned NOT NULL AUTO_INCREMENT,
    `ma_lich_hen` bigint unsigned NOT NULL,
    `ma_dich_vu` bigint unsigned NOT NULL,
    `don_gia_tai_thoi_diem_dat` decimal(15,2) NOT NULL,
    `thoi_luong_tai_thoi_diem_dat_phut` smallint unsigned NOT NULL,
    PRIMARY KEY (`ma_chi_tiet_lich_hen`),
    CONSTRAINT `check_chi_tiet_lich_hen_gia` CHECK (don_gia_tai_thoi_diem_dat >= 0),
    CONSTRAINT `check_chi_tiet_lich_hen_thoi_luong` CHECK (thoi_luong_tai_thoi_diem_dat_phut > 0),
    CONSTRAINT `FK_chi_tiet_lich_hen_dich_vu_ma_dich_vu` FOREIGN KEY (`ma_dich_vu`) REFERENCES `dich_vu` (`ma_dich_vu`) ON DELETE RESTRICT,
    CONSTRAINT `FK_chi_tiet_lich_hen_lich_hen_ma_lich_hen` FOREIGN KEY (`ma_lich_hen`) REFERENCES `lich_hen` (`ma_lich_hen`) ON DELETE RESTRICT
);


CREATE TABLE `ho_so_kham` (
    `ma_ho_so_kham` bigint unsigned NOT NULL AUTO_INCREMENT,
    `ma_khach_hang` bigint unsigned NOT NULL,
    `ma_bac_si` bigint unsigned NOT NULL,
    `ma_lich_hen` bigint unsigned NULL,
    `ngay_kham` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `trieu_chung` text NULL,
    `chan_doan` text NULL,
    `tom_tat_cho_khach` text NULL,
    `ghi_chu_noi_bo` text NULL,
    `ngay_tai_kham` date NULL,
    `chot_luc` datetime NULL,
    `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`ma_ho_so_kham`),
    CONSTRAINT `FK_ho_so_kham_bac_si_ma_bac_si` FOREIGN KEY (`ma_bac_si`) REFERENCES `bac_si` (`ma_bac_si`) ON DELETE RESTRICT,
    CONSTRAINT `FK_ho_so_kham_khach_hang_ma_khach_hang` FOREIGN KEY (`ma_khach_hang`) REFERENCES `khach_hang` (`ma_khach_hang`) ON DELETE RESTRICT,
    CONSTRAINT `FK_ho_so_kham_lich_hen_ma_lich_hen` FOREIGN KEY (`ma_lich_hen`) REFERENCES `lich_hen` (`ma_lich_hen`) ON DELETE RESTRICT
);


CREATE TABLE `dich_vu_thuc_hien` (
    `ma_dich_vu_thuc_hien` bigint unsigned NOT NULL AUTO_INCREMENT,
    `ma_ho_so_kham` bigint unsigned NOT NULL,
    `ma_chi_tiet_lich_hen` bigint unsigned NULL,
    `ma_dich_vu` bigint unsigned NOT NULL,
    `ten_dich_vu_tai_thoi_diem_thuc_hien` varchar(150) NOT NULL,
    `don_gia_thuc_te` decimal(15,2) NOT NULL,
    `so_luong` smallint unsigned NOT NULL DEFAULT 1,
    `thoi_diem_thuc_hien` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `ghi_chu` text NULL,
    PRIMARY KEY (`ma_dich_vu_thuc_hien`),
    CONSTRAINT `check_dich_vu_thuc_hien_gia` CHECK (don_gia_thuc_te >= 0),
    CONSTRAINT `check_dich_vu_thuc_hien_so_luong` CHECK (so_luong > 0),
    CONSTRAINT `FK_dich_vu_thuc_hien_chi_tiet_lich_hen_ma_chi_tiet_lich_hen` FOREIGN KEY (`ma_chi_tiet_lich_hen`) REFERENCES `chi_tiet_lich_hen` (`ma_chi_tiet_lich_hen`) ON DELETE RESTRICT,
    CONSTRAINT `FK_dich_vu_thuc_hien_dich_vu_ma_dich_vu` FOREIGN KEY (`ma_dich_vu`) REFERENCES `dich_vu` (`ma_dich_vu`) ON DELETE RESTRICT,
    CONSTRAINT `FK_dich_vu_thuc_hien_ho_so_kham_ma_ho_so_kham` FOREIGN KEY (`ma_ho_so_kham`) REFERENCES `ho_so_kham` (`ma_ho_so_kham`) ON DELETE RESTRICT
);


CREATE TABLE `don_thuoc` (
    `ma_don_thuoc` bigint unsigned NOT NULL AUTO_INCREMENT,
    `ma_ho_so_kham` bigint unsigned NOT NULL,
    `ngay_ke` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `ghi_chu` text NULL,
    PRIMARY KEY (`ma_don_thuoc`),
    CONSTRAINT `FK_don_thuoc_ho_so_kham_ma_ho_so_kham` FOREIGN KEY (`ma_ho_so_kham`) REFERENCES `ho_so_kham` (`ma_ho_so_kham`) ON DELETE RESTRICT
);


CREATE TABLE `hoa_don` (
    `ma_hoa_don` bigint unsigned NOT NULL AUTO_INCREMENT,
    `ma_ho_so_kham` bigint unsigned NOT NULL,
    `ngay_lap` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `tong_tien` decimal(15,2) NOT NULL DEFAULT 0.0,
    `trang_thai` varchar(30) NOT NULL DEFAULT 'CHUA_THANH_TOAN',
    `nguoi_lap_tai_khoan_id` bigint unsigned NOT NULL,
    `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`ma_hoa_don`),
    CONSTRAINT `check_hoa_don_tong_tien` CHECK (tong_tien >= 0),
    CONSTRAINT `check_hoa_don_trang_thai` CHECK (trang_thai IN ('CHUA_THANH_TOAN', 'THANH_TOAN_MOT_PHAN', 'DA_THANH_TOAN', 'DA_HUY')),
    CONSTRAINT `FK_hoa_don_ho_so_kham_ma_ho_so_kham` FOREIGN KEY (`ma_ho_so_kham`) REFERENCES `ho_so_kham` (`ma_ho_so_kham`) ON DELETE RESTRICT,
    CONSTRAINT `FK_hoa_don_tai_khoan_nguoi_lap_tai_khoan_id` FOREIGN KEY (`nguoi_lap_tai_khoan_id`) REFERENCES `tai_khoan` (`ma_tai_khoan`) ON DELETE RESTRICT
);


CREATE TABLE `chi_tiet_don_thuoc` (
    `ma_chi_tiet_don_thuoc` bigint unsigned NOT NULL AUTO_INCREMENT,
    `ma_don_thuoc` bigint unsigned NOT NULL,
    `ten_thuoc` varchar(255) NOT NULL,
    `ham_luong` varchar(100) NULL,
    `lieu_dung` varchar(255) NOT NULL,
    `so_lan_moi_ngay` smallint unsigned NULL,
    `so_ngay_dung` smallint unsigned NULL,
    `so_luong` decimal(10,2) NOT NULL,
    `huong_dan` text NULL,
    PRIMARY KEY (`ma_chi_tiet_don_thuoc`),
    CONSTRAINT `check_chi_tiet_don_thuoc_so_luong` CHECK (so_luong > 0),
    CONSTRAINT `FK_chi_tiet_don_thuoc_don_thuoc_ma_don_thuoc` FOREIGN KEY (`ma_don_thuoc`) REFERENCES `don_thuoc` (`ma_don_thuoc`) ON DELETE RESTRICT
);


CREATE TABLE `chi_tiet_hoa_don` (
    `ma_chi_tiet_hoa_don` bigint unsigned NOT NULL AUTO_INCREMENT,
    `ma_hoa_don` bigint unsigned NOT NULL,
    `ma_dich_vu_thuc_hien` bigint unsigned NOT NULL,
    `ten_dich_vu` varchar(150) NOT NULL,
    `don_gia` decimal(15,2) NOT NULL,
    `so_luong` smallint unsigned NOT NULL DEFAULT 1,
    `thanh_tien` decimal(17,2) AS (don_gia * so_luong),
    PRIMARY KEY (`ma_chi_tiet_hoa_don`),
    CONSTRAINT `check_chi_tiet_hoa_don_gia` CHECK (don_gia >= 0),
    CONSTRAINT `check_chi_tiet_hoa_don_so_luong` CHECK (so_luong > 0),
    CONSTRAINT `FK_chi_tiet_hoa_don_dich_vu_thuc_hien_ma_dich_vu_thuc_hien` FOREIGN KEY (`ma_dich_vu_thuc_hien`) REFERENCES `dich_vu_thuc_hien` (`ma_dich_vu_thuc_hien`) ON DELETE RESTRICT,
    CONSTRAINT `FK_chi_tiet_hoa_don_hoa_don_ma_hoa_don` FOREIGN KEY (`ma_hoa_don`) REFERENCES `hoa_don` (`ma_hoa_don`) ON DELETE RESTRICT
);


CREATE TABLE `thanh_toan` (
    `ma_thanh_toan` bigint unsigned NOT NULL AUTO_INCREMENT,
    `ma_hoa_don` bigint unsigned NOT NULL,
    `nguoi_xac_nhan_tai_khoan_id` bigint unsigned NOT NULL,
    `so_tien` decimal(15,2) NOT NULL,
    `phuong_thuc` varchar(30) NOT NULL,
    `trang_thai` varchar(20) NOT NULL DEFAULT 'DA_XAC_NHAN',
    `thoi_diem_thanh_toan` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `ly_do_huy` text NULL,
    `nguoi_huy_tai_khoan_id` bigint unsigned NULL,
    `huy_luc` datetime NULL,
    `ghi_chu` varchar(255) NULL,
    PRIMARY KEY (`ma_thanh_toan`),
    CONSTRAINT `check_thanh_toan_phuong_thuc` CHECK (phuong_thuc IN ('TIEN_MAT', 'CHUYEN_KHOAN', 'KHAC')),
    CONSTRAINT `check_thanh_toan_so_tien` CHECK (so_tien > 0),
    CONSTRAINT `check_thanh_toan_trang_thai` CHECK (trang_thai IN ('DA_XAC_NHAN', 'DA_HUY')),
    CONSTRAINT `FK_thanh_toan_hoa_don_ma_hoa_don` FOREIGN KEY (`ma_hoa_don`) REFERENCES `hoa_don` (`ma_hoa_don`) ON DELETE RESTRICT,
    CONSTRAINT `FK_thanh_toan_tai_khoan_nguoi_huy_tai_khoan_id` FOREIGN KEY (`nguoi_huy_tai_khoan_id`) REFERENCES `tai_khoan` (`ma_tai_khoan`) ON DELETE RESTRICT,
    CONSTRAINT `FK_thanh_toan_tai_khoan_nguoi_xac_nhan_tai_khoan_id` FOREIGN KEY (`nguoi_xac_nhan_tai_khoan_id`) REFERENCES `tai_khoan` (`ma_tai_khoan`) ON DELETE RESTRICT
);


INSERT INTO `vai_tro` (`ma_vai_tro`, `ten_vai_tro`)
VALUES ('BAC_SI', 'Bác sĩ');
SELECT ROW_COUNT();

INSERT INTO `vai_tro` (`ma_vai_tro`, `ten_vai_tro`)
VALUES ('KHACH_HANG', 'Khách hàng');
SELECT ROW_COUNT();

INSERT INTO `vai_tro` (`ma_vai_tro`, `ten_vai_tro`)
VALUES ('LE_TAN', 'Lễ tân');
SELECT ROW_COUNT();

INSERT INTO `vai_tro` (`ma_vai_tro`, `ten_vai_tro`)
VALUES ('QUAN_LY', 'Quản lý');
SELECT ROW_COUNT();



CREATE UNIQUE INDEX `IX_bac_si_ma_nhan_vien` ON `bac_si` (`ma_nhan_vien`);


CREATE UNIQUE INDEX `IX_cham_cong_ma_nhan_vien_ngay_cham_cong` ON `cham_cong` (`ma_nhan_vien`, `ngay_cham_cong`);


CREATE INDEX `IX_cham_cong_ngay_cham_cong_trang_thai_duyet` ON `cham_cong` (`ngay_cham_cong`, `trang_thai_duyet`);


CREATE INDEX `IX_cham_cong_nguoi_duyet_tai_khoan_id` ON `cham_cong` (`nguoi_duyet_tai_khoan_id`);


CREATE INDEX `IX_cham_cong_nguoi_tu_choi_tai_khoan_id` ON `cham_cong` (`nguoi_tu_choi_tai_khoan_id`);


CREATE INDEX `IX_chi_tiet_don_thuoc_ma_don_thuoc` ON `chi_tiet_don_thuoc` (`ma_don_thuoc`);


CREATE UNIQUE INDEX `IX_chi_tiet_hoa_don_ma_dich_vu_thuc_hien` ON `chi_tiet_hoa_don` (`ma_dich_vu_thuc_hien`);


CREATE INDEX `IX_chi_tiet_hoa_don_ma_hoa_don` ON `chi_tiet_hoa_don` (`ma_hoa_don`);


CREATE INDEX `IX_chi_tiet_lich_hen_ma_dich_vu` ON `chi_tiet_lich_hen` (`ma_dich_vu`);


CREATE UNIQUE INDEX `IX_chi_tiet_lich_hen_ma_lich_hen_ma_dich_vu` ON `chi_tiet_lich_hen` (`ma_lich_hen`, `ma_dich_vu`);


CREATE UNIQUE INDEX `IX_con_nguoi_so_dien_thoai` ON `con_nguoi` (`so_dien_thoai`);


CREATE UNIQUE INDEX `IX_con_nguoi_so_dinh_danh` ON `con_nguoi` (`so_dinh_danh`);


CREATE UNIQUE INDEX `IX_dich_vu_ten_dich_vu` ON `dich_vu` (`ten_dich_vu`);


CREATE UNIQUE INDEX `IX_dich_vu_thuc_hien_ma_chi_tiet_lich_hen` ON `dich_vu_thuc_hien` (`ma_chi_tiet_lich_hen`);


CREATE INDEX `IX_dich_vu_thuc_hien_ma_dich_vu` ON `dich_vu_thuc_hien` (`ma_dich_vu`);


CREATE INDEX `IX_dich_vu_thuc_hien_ma_ho_so_kham` ON `dich_vu_thuc_hien` (`ma_ho_so_kham`);


CREATE INDEX `IX_don_thuoc_ma_ho_so_kham` ON `don_thuoc` (`ma_ho_so_kham`);


CREATE INDEX `IX_ho_so_kham_ma_bac_si` ON `ho_so_kham` (`ma_bac_si`);


CREATE INDEX `IX_ho_so_kham_ma_khach_hang_ngay_kham` ON `ho_so_kham` (`ma_khach_hang`, `ngay_kham`);


CREATE UNIQUE INDEX `IX_ho_so_kham_ma_lich_hen` ON `ho_so_kham` (`ma_lich_hen`);


CREATE UNIQUE INDEX `IX_hoa_don_ma_ho_so_kham` ON `hoa_don` (`ma_ho_so_kham`);


CREATE INDEX `IX_hoa_don_nguoi_lap_tai_khoan_id` ON `hoa_don` (`nguoi_lap_tai_khoan_id`);


CREATE UNIQUE INDEX `IX_khach_hang_ma_con_nguoi` ON `khach_hang` (`ma_con_nguoi`);


CREATE INDEX `IX_lich_hen_ma_bac_si_thoi_gian_bat_dau` ON `lich_hen` (`ma_bac_si`, `thoi_gian_bat_dau`);


CREATE INDEX `IX_lich_hen_ma_khach_hang_thoi_gian_bat_dau` ON `lich_hen` (`ma_khach_hang`, `thoi_gian_bat_dau`);


CREATE INDEX `IX_lich_hen_ma_phong_thoi_gian_bat_dau` ON `lich_hen` (`ma_phong`, `thoi_gian_bat_dau`);


CREATE INDEX `IX_lich_hen_nguoi_danh_dau_khong_den_tai_khoan_id` ON `lich_hen` (`nguoi_danh_dau_khong_den_tai_khoan_id`);


CREATE INDEX `IX_lich_hen_nguoi_huy_tai_khoan_id` ON `lich_hen` (`nguoi_huy_tai_khoan_id`);


CREATE INDEX `IX_lich_hen_nguoi_tao_tai_khoan_id` ON `lich_hen` (`nguoi_tao_tai_khoan_id`);


CREATE INDEX `IX_lich_hen_nguoi_tiep_nhan_tai_khoan_id` ON `lich_hen` (`nguoi_tiep_nhan_tai_khoan_id`);


CREATE INDEX `IX_lich_hen_nguoi_xac_nhan_tai_khoan_id` ON `lich_hen` (`nguoi_xac_nhan_tai_khoan_id`);


CREATE INDEX `IX_lich_hen_trang_thai` ON `lich_hen` (`trang_thai`);


CREATE INDEX `IX_lich_su_chinh_sua_cham_cong_ma_cham_cong_thoi_diem_sua` ON `lich_su_chinh_sua_cham_cong` (`ma_cham_cong`, `thoi_diem_sua`);


CREATE INDEX `IX_lich_su_chinh_sua_cham_cong_nguoi_sua_tai_khoan_id` ON `lich_su_chinh_sua_cham_cong` (`nguoi_sua_tai_khoan_id`);


CREATE UNIQUE INDEX `IX_nhan_vien_ma_con_nguoi` ON `nhan_vien` (`ma_con_nguoi`);


CREATE UNIQUE INDEX `IX_phong_ten_phong` ON `phong` (`ten_phong`);


CREATE UNIQUE INDEX `IX_tai_khoan_ma_con_nguoi` ON `tai_khoan` (`ma_con_nguoi`);


CREATE INDEX `IX_tai_khoan_ma_vai_tro` ON `tai_khoan` (`ma_vai_tro`);


CREATE INDEX `IX_thanh_toan_ma_hoa_don` ON `thanh_toan` (`ma_hoa_don`);


CREATE INDEX `IX_thanh_toan_nguoi_huy_tai_khoan_id` ON `thanh_toan` (`nguoi_huy_tai_khoan_id`);


CREATE INDEX `IX_thanh_toan_nguoi_xac_nhan_tai_khoan_id` ON `thanh_toan` (`nguoi_xac_nhan_tai_khoan_id`);


CREATE UNIQUE INDEX `IX_vai_tro_ten_vai_tro` ON `vai_tro` (`ten_vai_tro`);

CREATE TABLE `__EFMigrationsHistory` (
    `MigrationId` varchar(150) NOT NULL,
    `ProductVersion` varchar(32) NOT NULL,
    PRIMARY KEY (`MigrationId`)
);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`) VALUES
('20261010112618_KhoiTao', '10.0.12'),
('20261013191204_GhiNhanNguoiHuyLich', '10.0.12'),
('20261013200258_TuChoiLichTrung', '10.0.12'),
('20261014080519_BoLichLamViecRieng', '10.0.12'),
('20261014085400_BoThietBiVaBaoTri', '10.0.12'),
('20261014092201_LuongKhamBenh', '10.0.12'),
('20261015144132_HoaDonThanhToan', '10.0.12'),
('20261015161015_TrangThaiChamCongChuaHoanTat', '10.0.12'),
('20261015172913_ChamCongHaiBuoi', '10.0.12'),
('20261017180000_HetHanLichCho', '10.0.12'),
('20261019090000_AnhDaiDienCaNhan', '10.0.12');

-- Four demo accounts supplied by the project owner. Passwords are not recoverable from these hashes.
START TRANSACTION;

INSERT INTO `con_nguoi` (`ho_ten`, `so_dien_thoai`)
VALUES ('Quản lý Demo', '0900000002');
SET @demo_quan_ly_con_nguoi = LAST_INSERT_ID();
INSERT INTO `tai_khoan` (`ma_con_nguoi`, `ma_vai_tro`, `mat_khau_hash`, `trang_thai`)
VALUES (@demo_quan_ly_con_nguoi, 'QUAN_LY',
        'AQAAAAIAAYagAAAAEE+J5HAgxvohTuVcM1EBi5APPfWuFTn8+qQ/NUg0aBpMYjPrvFUZRlr8gxsB/0zmwg==',
        'HOAT_DONG');
INSERT INTO `nhan_vien` (`ma_con_nguoi`, `trang_thai`)
VALUES (@demo_quan_ly_con_nguoi, 'DANG_LAM_VIEC');

INSERT INTO `con_nguoi` (`ho_ten`, `so_dien_thoai`)
VALUES ('Lễ tân Demo', '0900000003');
SET @demo_le_tan_con_nguoi = LAST_INSERT_ID();
INSERT INTO `tai_khoan` (`ma_con_nguoi`, `ma_vai_tro`, `mat_khau_hash`, `trang_thai`)
VALUES (@demo_le_tan_con_nguoi, 'LE_TAN',
        'AQAAAAIAAYagAAAAEGg2ogDPhoujZnTDEQxItRKcnwS+n7nCQOCEKd6JOWz872BRr7PedL8SGsGeO0dFQQ==',
        'HOAT_DONG');
INSERT INTO `nhan_vien` (`ma_con_nguoi`, `trang_thai`)
VALUES (@demo_le_tan_con_nguoi, 'DANG_LAM_VIEC');

INSERT INTO `con_nguoi` (`ho_ten`, `so_dien_thoai`)
VALUES ('Bác sĩ Demo', '0900000004');
SET @demo_bac_si_con_nguoi = LAST_INSERT_ID();
INSERT INTO `tai_khoan` (`ma_con_nguoi`, `ma_vai_tro`, `mat_khau_hash`, `trang_thai`)
VALUES (@demo_bac_si_con_nguoi, 'BAC_SI',
        'AQAAAAIAAYagAAAAEHLGlsFYb+v6W/ziu4Wr0dBMyvK+9FD6IIItfkIiD2cYKHmEkS/6rLqM8vFq/1KGCw==',
        'HOAT_DONG');
INSERT INTO `nhan_vien` (`ma_con_nguoi`, `trang_thai`)
VALUES (@demo_bac_si_con_nguoi, 'DANG_LAM_VIEC');
SET @demo_bac_si_nhan_vien = LAST_INSERT_ID();
INSERT INTO `bac_si` (`ma_nhan_vien`)
VALUES (@demo_bac_si_nhan_vien);

INSERT INTO `con_nguoi` (`ho_ten`, `so_dien_thoai`)
VALUES ('Khách hàng Demo', '0900000005');
SET @demo_khach_hang_con_nguoi = LAST_INSERT_ID();
INSERT INTO `tai_khoan` (`ma_con_nguoi`, `ma_vai_tro`, `mat_khau_hash`, `trang_thai`)
VALUES (@demo_khach_hang_con_nguoi, 'KHACH_HANG',
        'AQAAAAIAAYagAAAAEMJAD4ULqOGL9YzUBslklDnzAcRW3AAUDhTVHIv6QTwYkPSdNT+461jISIYj4O5NhA==',
        'HOAT_DONG');
INSERT INTO `khach_hang` (`ma_con_nguoi`, `ngay_dang_ky`)
VALUES (@demo_khach_hang_con_nguoi, CURRENT_DATE());

COMMIT;
