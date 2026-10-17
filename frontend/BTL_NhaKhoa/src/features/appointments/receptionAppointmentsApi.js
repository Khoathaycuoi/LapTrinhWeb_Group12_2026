import { request } from '../../lib/api'

export const receptionAppointmentsApi = {
  intakeList: (token, { date, tab, keyword, page, pageSize }, signal) => {
    const query = new URLSearchParams({ ngayHen: date, page, pageSize })
    if (tab === 'waiting') query.set('trangThai', 'DA_XAC_NHAN')
    else if (tab === 'active') query.set('nhom', 'active')
    else query.set('trangThai', 'KHONG_DEN')
    if (keyword) {
      if (/^\d+$/.test(keyword)) query.set('sdtKH', keyword)
      else query.set('tenKH', keyword)
    }
    return request(`/api/lich-hen/tim-kiem?${query}`, { token, signal })
  },
  receive: (token, appointmentId) => request(`/api/lich-hen/tiep-nhan/${encodeURIComponent(appointmentId)}`, {
    token, method: 'PUT',
  }),
  markNoShow: (token, appointmentId, note) => request(`/api/lich-hen/khong-den/${encodeURIComponent(appointmentId)}`, {
    token, method: 'PUT', body: JSON.stringify({ ghiChu: note || null }),
  }),
  list: (token, filters, signal) => {
    const query = new URLSearchParams()
    if (filters.status) query.set('trangThai', filters.status)
    if (filters.upcoming) query.set('nhom', 'upcoming')
    if (filters.date) query.set('ngayHen', filters.date)
    if (filters.customer) query.set('tenKH', filters.customer)
    query.set('page', filters.page)
    query.set('pageSize', filters.pageSize)
    return request(`/api/lich-hen/tim-kiem?${query}`, { token, signal })
  },
  doctors: (token, signal) => request('/api/users/bac-si/hoat-dong', { token, signal }),
  availableRooms: (token, appointmentId, signal) => request(`/api/lich-hen/phong-trong/${encodeURIComponent(appointmentId)}`, { token, signal }),
  availableDoctors: (token, appointmentId, signal) => request(`/api/lich-hen/bac-si-trong/${encodeURIComponent(appointmentId)}`, { token, signal }),
  assignAndConfirm: (token, appointmentId, doctorId, roomId) => request(`/api/lich-hen/gan-bac-si-va-xac-nhan/${encodeURIComponent(appointmentId)}`, {
    token, method: 'PUT', body: JSON.stringify({ maBacSi: doctorId, ...(roomId ? { maPhong: roomId } : {}) }),
  }),
  confirm: (token, appointmentId, roomId) => {
    const query = roomId ? `?${new URLSearchParams({ maPhong: roomId })}` : ''
    return request(`/api/lich-hen/xac-nhan/${encodeURIComponent(appointmentId)}${query}`, { token, method: 'PUT' })
  },
  cancel: (token, appointmentId, reason) => request('/api/lich-hen/huy-lich', {
    token, method: 'PUT', body: JSON.stringify({ maLichHen: appointmentId, lyDo: reason }),
  }),
}
