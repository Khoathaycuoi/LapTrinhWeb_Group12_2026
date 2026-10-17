import { request } from '../../lib/api'

export const appointmentsApi = {
  myAppointments: (token, { date }, signal) => {
    const query = new URLSearchParams()
    if (date) query.set('ngayHen', date)
    const suffix = query.size ? `?${query}` : ''
    return request(`/api/lich-hen/tim-kiem${suffix}`, { token, signal })
  },
  cancel: (token, maLichHen, lyDo) => request('/api/lich-hen/huy-lich', {
    token,
    method: 'PUT',
    body: JSON.stringify({ maLichHen, lyDo }),
  }),
}
