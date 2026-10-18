import { request } from '../../lib/api'

export const doctorRecordsApi = {
  list: (token, page, unfinalized, signal) => request(`/api/ho-so-kham/cua-toi?page=${page}&pageSize=10${unfinalized ? '&chuaChot=true' : ''}`, { token, signal }),
  detail: (token, id, signal) => request(`/api/ho-so-kham/${encodeURIComponent(id)}`, { token, signal }),
  fromAppointment: (token, id, signal) => request(`/api/ho-so-kham/theo-lich/${encodeURIComponent(id)}`, { token, signal }),
  medicalHistory: (token, customerId, signal) => request(`/api/users/khach-hang/${encodeURIComponent(customerId)}/tien-su-benh`, { token, signal }),
  save: (token, id, payload) => request(`/api/ho-so-kham/${encodeURIComponent(id)}`, { token, method: 'PUT', body: JSON.stringify(payload) }),
  performService: (token, id, payload) => request(`/api/ho-so-kham/${encodeURIComponent(id)}/dich-vu-thuc-hien`, { token, method: 'POST', body: JSON.stringify(payload) }),
  removeService: (token, id, serviceId) => request(`/api/ho-so-kham/${encodeURIComponent(id)}/dich-vu-thuc-hien/${encodeURIComponent(serviceId)}`, { token, method: 'DELETE' }),
  prescribe: (token, id, payload) => request(`/api/ho-so-kham/${encodeURIComponent(id)}/don-thuoc`, { token, method: 'POST', body: JSON.stringify(payload) }),
  removePrescription: (token, id, prescriptionId) => request(`/api/ho-so-kham/${encodeURIComponent(id)}/don-thuoc/${encodeURIComponent(prescriptionId)}`, { token, method: 'DELETE' }),
  finalize: (token, id) => request(`/api/ho-so-kham/${encodeURIComponent(id)}/chot`, { token, method: 'PUT' }),
  completeAppointment: (token, appointmentId) => request(`/api/lich-hen/hoan-thanh/${encodeURIComponent(appointmentId)}`, { token, method: 'PUT' }),
}
