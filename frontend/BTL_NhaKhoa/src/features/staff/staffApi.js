import { request } from '../../lib/api'

const paths = { manager: 'quan-ly', receptionist: 'nhan-vien', doctor: 'bac-si' }

export const staffApi = {
  list: (token, group, keyword, page, signal) => {
    const query = new URLSearchParams({ page: String(page), pageSize: '10' })
    if (keyword) query.set('keyword', keyword)
    return request(`/api/users/${paths[group]}?${query}`, { token, signal })
  },
  detail: (token, id, signal) => request(`/api/users/detail/${encodeURIComponent(id)}`, { token, signal }),
  create: (token, group, payload) => request(`/api/auth/register/${group === 'receptionist' ? 'le-tan' : paths[group]}`, {
    token,
    method: 'POST',
    body: JSON.stringify(payload),
  }),
  setActive: (token, phone, active) => request(`/api/users/${encodeURIComponent(phone)}/${active ? 'khoi-phuc' : 'xoa'}`, {
    token,
    method: 'PUT',
  }),
}
