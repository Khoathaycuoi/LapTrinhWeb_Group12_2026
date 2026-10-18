import { request } from '../../lib/api'

export const roomsApi = {
  list: (token, page, signal) => request(`/api/phong?${new URLSearchParams({ page: String(page), pageSize: '10' })}`, { token, signal }),
  create: (token, name) => request('/api/phong', { token, method: 'POST', body: JSON.stringify({ tenPhong: name }) }),
  setStatus: (token, id, status) => request(`/api/phong/${encodeURIComponent(id)}/trang-thai`, {
    token, method: 'PUT', body: JSON.stringify({ trangThai: status }),
  }),
  history: (token, id, dates, page, signal) => {
    const query = new URLSearchParams({ page: String(page), pageSize: '10' })
    if (dates.from) query.set('tuNgay', dates.from)
    if (dates.to) query.set('denNgay', dates.to)
    return request(`/api/phong/${encodeURIComponent(id)}/lich-su?${query}`, { token, signal })
  },
}
