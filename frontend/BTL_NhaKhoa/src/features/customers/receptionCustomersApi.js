import { request } from '../../lib/api'

export const receptionCustomersApi = {
  list: (token, keyword, page, signal) => {
    const query = new URLSearchParams({ page: String(page), pageSize: '10' })
    if (keyword) query.set('keyword', keyword)
    return request(`/api/users/khach-hang?${query}`, { token, signal })
  },
  detail: (token, id, signal) => request(`/api/users/detail/${encodeURIComponent(id)}`, { token, signal }),
  create: (payload) => request('/api/auth/register/khach-hang', {
    token: null,
    method: 'POST',
    body: JSON.stringify(payload),
  }),
}
