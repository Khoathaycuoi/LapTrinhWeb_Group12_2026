import { request } from '../../lib/api'

export const attendanceApi = {
  today: (token, signal) => request('/api/cham-cong/hom-nay', { token, signal }),
  history: (token, page, { fromDate, toDate }, signal) => {
    const query = new URLSearchParams({ page: String(page), pageSize: '10' })
    if (fromDate) query.set('tuNgay', fromDate)
    if (toDate) query.set('denNgay', toDate)
    return request(`/api/cham-cong/me?${query}`, { token, signal })
  },
  action: (token, type, period) => request(`/api/cham-cong/${type}/${period}`, { token, method: 'POST' }),
}
