import { request } from '../../lib/api'

export const doctorAppointmentsApi = {
  list: (token, { tab, date, keyword, page }, signal) => {
    const query = new URLSearchParams({ page: String(page), pageSize: '10' })
    if (tab === 'today') {
      query.set('ngayHen', date)
      query.set('nhom', 'tracked')
    } else if (tab === 'pending') {
      query.set('trangThai', 'CHO_XAC_NHAN')
      query.set('nhom', 'upcoming')
    } else if (tab === 'arrived') query.set('trangThai', 'DA_DEN')
    else if (tab === 'examining') query.set('trangThai', 'DANG_KHAM')
    if (keyword) query.set(/^\d+$/.test(keyword) ? 'sdtKH' : 'tenKH', keyword)
    return request(`/api/lich-hen/tim-kiem?${query}`, { token, signal })
  },
  rooms: (token, id, signal) => request(`/api/lich-hen/phong-trong/${encodeURIComponent(id)}`, { token, signal }),
  confirm: (token, id, roomId) => request(`/api/lich-hen/xac-nhan/${encodeURIComponent(id)}${roomId ? `?${new URLSearchParams({ maPhong: roomId })}` : ''}`, { token, method: 'PUT' }),
  reject: (token, id, reason) => request(`/api/lich-hen/tu-choi/${encodeURIComponent(id)}`, { token, method: 'PUT', body: JSON.stringify({ lyDo: reason }) }),
  cancel: (token, id, reason) => request('/api/lich-hen/huy-lich', { token, method: 'PUT', body: JSON.stringify({ maLichHen: id, lyDo: reason }) }),
  start: (token, id) => request(`/api/lich-hen/bat-dau-kham/${encodeURIComponent(id)}`, { token, method: 'PUT' }),
}
