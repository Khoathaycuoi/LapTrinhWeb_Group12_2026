import { request } from '../../lib/api'

export const revenueApi = {
  byYear: (token, year, signal) => request(`/api/bao-cao/doanh-thu?nam=${year}`, { token, signal }),
}
