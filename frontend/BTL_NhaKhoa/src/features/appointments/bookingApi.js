import { request } from '../../lib/api'

export const bookingApi = {
  services: (token, signal) => request('/api/dichvu/search-all?status=DANG_HOAT_DONG', { token, signal }),
  doctors: (token, signal) => request('/api/users/bac-si/hoat-dong', { token, signal }),
  availableSlots: (token, doctorId, date, duration, signal) => {
    const query = new URLSearchParams({ maBacSi: doctorId, ngay: date, thoiLuongPhut: String(duration) })
    return request(`/api/lich-hen/ca-trong?${query}`, { token, signal })
  },
  book: (token, payload) => request('/api/lich-hen/dat-lich', {
    token,
    method: 'POST',
    body: JSON.stringify(payload),
  }),
}
