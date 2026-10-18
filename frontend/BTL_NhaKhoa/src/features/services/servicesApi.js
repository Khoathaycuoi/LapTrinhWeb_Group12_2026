import { request } from '../../lib/api'

export const servicesApi = {
  list: (token, filters, page, signal) => {
    const query = new URLSearchParams({ page: String(page), pageSize: '10' })
    if (filters.name) query.set('ten', filters.name)
    if (filters.status) query.set('status', filters.status)
    return request(`/api/dichvu/search-all?${query}`, { token, signal })
  },
  create: (token, data) => request('/api/dichvu/add', {
    token, method: 'POST', body: JSON.stringify(data),
  }),
  update: (token, data) => request('/api/dichvu/dv', {
    token, method: 'PUT', body: JSON.stringify(data),
  }),
  setActive: (token, id, active) => request(`/api/dichvu/${encodeURIComponent(id)}/${active ? 'khoi-phuc' : 'xoa'}`, {
    token, method: 'PUT',
  }),
}
