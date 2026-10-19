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
  managerList: (token, page, filters, signal) => {
    const query = new URLSearchParams({ page: String(page), pageSize: '10' })
    if (filters.employee) query.set('maNhanVien', filters.employee)
    if (filters.fromDate) query.set('tuNgay', filters.fromDate)
    if (filters.toDate) query.set('denNgay', filters.toDate)
    if (filters.status) query.set('trangThaiDuyet', filters.status)
    return request(`/api/cham-cong?${query}`, { token, signal })
  },
  managerDay: (token, date, signal) => request(`/api/cham-cong/ngay/${encodeURIComponent(date)}`, { token, signal }),
  managerDetail: (token, id, signal) => request(`/api/cham-cong/${encodeURIComponent(id)}`, { token, signal }),
  managerCreate: (token, payload) => request('/api/cham-cong/thu-cong', { token, method: 'POST', body: JSON.stringify(payload) }),
  managerEdit: (token, id, payload) => request(`/api/cham-cong/${encodeURIComponent(id)}/chinh-sua`, { token, method: 'PUT', body: JSON.stringify(payload) }),
  managerApprove: (token, id) => request(`/api/cham-cong/${encodeURIComponent(id)}/duyet`, { token, method: 'PUT' }),
  managerReject: (token, id, reason) => request(`/api/cham-cong/${encodeURIComponent(id)}/tu-choi`, {
    token, method: 'PUT', body: JSON.stringify({ lyDo: reason }),
  }),
}
