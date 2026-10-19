import { request } from '../../lib/api'

export const accountApi = {
  detail: (token, signal) => request('/api/users/me', { token, signal }),
  update: (token, payload) => request('/api/users/me', { token, method: 'PUT', body: JSON.stringify(payload) }),
  changePassword: (token, oldPassword, newPassword) => request('/api/users/me/password', {
    token, method: 'PUT', body: JSON.stringify({ oldPassword, newPassword }),
  }),
  changePhone: (token, matKhauHienTai, soDienThoaiMoi) => request('/api/users/me/so-dien-thoai', {
    token, method: 'PUT', body: JSON.stringify({ matKhauHienTai, soDienThoaiMoi }),
  }),
}
