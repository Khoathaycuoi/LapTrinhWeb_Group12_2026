import { request } from '../../lib/api'

export const receptionAppointmentsApi = {
  list: (token, filters, signal) => {
    const query = new URLSearchParams()
    if (filters.status) query.set('trangThai', filters.status)
    if (filters.date) query.set('ngayHen', filters.date)
    if (filters.customer) query.set('tenKH', filters.customer)
    return request(`/api/lich-hen/tim-kiem?${query}`, { token, signal })
  },
  doctors: (token, signal) => request('/api/users/bac-si/hoat-dong', { token, signal }),
  availableRooms: (token, appointmentId, signal) => request(`/api/lich-hen/phong-trong/${encodeURIComponent(appointmentId)}`, { token, signal }),
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
