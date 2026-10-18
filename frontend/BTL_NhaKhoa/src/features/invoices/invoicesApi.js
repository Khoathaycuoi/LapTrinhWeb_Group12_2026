import { request } from '../../lib/api'

export const invoicesApi = {
  list: (token, { page, pageSize, paymentFilter, customerId, fromDate, toDate }, signal) => {
    const query = new URLSearchParams({ page: String(page), pageSize: String(pageSize) })
    if (paymentFilter === 'unpaid') query.set('conNo', 'true')
    if (paymentFilter === 'paid') query.set('trangThai', 'DA_THANH_TOAN')
    if (customerId) query.set('maKhachHang', customerId)
    if (fromDate) query.set('tuNgay', fromDate)
    if (toDate) query.set('denNgay', toDate)
    return request(`/api/hoa-don?${query}`, { token, signal })
  },
  detail: (token, invoiceId, signal) => request(`/api/hoa-don/${encodeURIComponent(invoiceId)}`, { token, signal }),
  recordPayment: (token, invoiceId, payment) => request(`/api/hoa-don/${encodeURIComponent(invoiceId)}/thanh-toan`, {
    token,
    method: 'POST',
    body: JSON.stringify(payment),
  }),
  eligibleExams: (token, page, signal) => request(`/api/hoa-don/cho-lap?page=${page}&pageSize=10`, { token, signal }),
  create: (token, examId) => request('/api/hoa-don', {
    token,
    method: 'POST',
    body: JSON.stringify({ maHoSoKham: examId }),
  }),
}
