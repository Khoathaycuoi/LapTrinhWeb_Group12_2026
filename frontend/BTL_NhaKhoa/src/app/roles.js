export const roles = {
  QUAN_LY: { label: 'Quản lý', base: '/quan-ly' },
  LE_TAN: { label: 'Lễ tân', base: '/le-tan' },
  BAC_SI: { label: 'Bác sĩ', base: '/bac-si' },
  KHACH_HANG: { label: 'Khách hàng', base: '/khach-hang' },
}

export const roleNavigation = {
  QUAN_LY: [
    ['Tổng quan', 'overview'], ['Lịch hẹn', 'lich-hen'], ['Khách hàng', 'khach-hang'],
    ['Nhân sự', 'nhan-su'], ['Dịch vụ', 'dich-vu'], ['Phòng khám', 'phong'],
    ['Chấm công', 'cham-cong'], ['Hóa đơn', 'hoa-don'],
  ],
  LE_TAN: [
    ['Tổng quan', 'overview'], ['Lịch hẹn', 'lich-hen'], ['Khách hàng', 'khach-hang'],
    ['Tiếp nhận', 'tiep-nhan'], ['Hóa đơn', 'hoa-don'],
  ],
  BAC_SI: [
    ['Tổng quan', 'overview'], ['Lịch của tôi', 'lich-hen'], ['Hồ sơ khám', 'ho-so-kham'],
    ['Chấm công', 'cham-cong'],
  ],
  KHACH_HANG: [
    ['Tổng quan', 'overview'], ['Đặt lịch', 'dat-lich'], ['Lịch hẹn của tôi', 'lich-hen'],
    ['Lịch sử khám', 'lich-su-kham'], ['Hóa đơn', 'hoa-don'],
  ],
}

export function roleHome(role) {
  return roles[role] ? `${roles[role].base}/overview` : '/dang-nhap'
}
