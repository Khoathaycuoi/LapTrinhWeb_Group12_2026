import { request } from '../../lib/api'

export const dashboardApi = {
  manager: (token, signal) => request('/api/dashboard/quan-ly', { token, signal }),
  customer: (token, signal) => request('/api/dashboard/khach-hang', { token, signal }),
  reception: (token, signal) => request('/api/dashboard/le-tan', { token, signal }),
  doctor: (token, signal) => request('/api/dashboard/bac-si', { token, signal }),
}
