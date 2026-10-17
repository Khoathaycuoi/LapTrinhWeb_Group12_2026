import { request } from '../../lib/api'

export const dashboardApi = {
  customer: (token, signal) => request('/api/dashboard/khach-hang', { token, signal }),
}
