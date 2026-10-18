import { request } from '../../lib/api'

export const customerRecordsApi = {
  list: (token, page, signal) => request(`/api/ho-so-kham/cua-toi?page=${page}&pageSize=10`, { token, signal }),
  detail: (token, recordId, signal) => request(`/api/ho-so-kham/${encodeURIComponent(recordId)}`, { token, signal }),
}
