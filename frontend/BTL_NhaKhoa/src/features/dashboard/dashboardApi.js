import { request } from '../../lib/api'

export const dashboardApi = {
  customer: (token, signal) => request('/api/dashboard/khach-hang', { token, signal }),
  reception: (token, signal) => request('/api/dashboard/le-tan', { token, signal }),
  doctor: (token, signal) => request('/api/dashboard/bac-si', { token, signal }),
}
