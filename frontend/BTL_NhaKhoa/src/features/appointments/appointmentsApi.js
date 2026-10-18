import { request } from '../../lib/api'

export const appointmentsApi = {
  myAppointments: (token, { date, group, status, page, pageSize }, signal) => {
    const query = new URLSearchParams()
    if (date) query.set('ngayHen', date)
    if (group && group !== 'all') query.set('nhom', group)
    if (status) query.set('trangThai', status)
    query.set('page', page)
    query.set('pageSize', pageSize)
    return request(`/api/lich-hen/tim-kiem?${query}`, { token, signal })
  },
  cancel: (token, maLichHen, lyDo) => request('/api/lich-hen/huy-lich', {
    token,
    method: 'PUT',
    body: JSON.stringify({ maLichHen, lyDo }),
  }),
}
