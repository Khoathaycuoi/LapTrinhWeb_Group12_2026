import { request } from '../../lib/api'

export const accountApi = {
  detail: (token, signal) => request('/api/users/me', { token, signal }),
  update: (token, payload) => request('/api/users/me', { token, method: 'PUT', body: JSON.stringify(payload) }),
  uploadAvatar: (token, file) => {
    const body = new FormData()
    body.append('anh', file)
    return request('/api/users/me/anh-dai-dien', { token, method: 'POST', body })
  },
  deleteAvatar: (token) => request('/api/users/me/anh-dai-dien', { token, method: 'DELETE' }),
  changePassword: (token, oldPassword, newPassword) => request('/api/users/me/password', {
    token, method: 'PUT', body: JSON.stringify({ oldPassword, newPassword }),
  }),
  changePhone: (token, matKhauHienTai, soDienThoaiMoi) => request('/api/users/me/so-dien-thoai', {
    token, method: 'PUT', body: JSON.stringify({ matKhauHienTai, soDienThoaiMoi }),
  }),
}
