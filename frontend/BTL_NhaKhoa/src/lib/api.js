const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? ''
const TOKEN_KEY = 'nha-khoa.access-token'
const CONNECTION_ERROR = 'Không thể kết nối đến máy chủ. Vui lòng thử lại sau.'

export function apiAssetUrl(path) {
  return path ? `${API_BASE_URL}${path}` : ''
}

export const tokenStore = {
  get: () => localStorage.getItem(TOKEN_KEY),
  set: (token) => localStorage.setItem(TOKEN_KEY, token),
  clear: () => localStorage.removeItem(TOKEN_KEY),
}

export class ApiError extends Error {
  constructor(message, status) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}

export async function request(path, { token = tokenStore.get(), ...options } = {}) {
  const headers = new Headers(options.headers)
  if (options.body && !(options.body instanceof FormData)) headers.set('Content-Type', 'application/json')
  if (token) headers.set('Authorization', `Bearer ${token}`)

  let response
  try {
    response = await fetch(`${API_BASE_URL}${path}`, { ...options, headers })
  } catch {
    throw new ApiError(CONNECTION_ERROR, 0)
  }

  const contentType = response.headers.get('content-type') ?? ''
  const body = response.status === 204 ? '' : await response.text()
  // Vite proxy returns 502 when the backend is unavailable; some proxies return an empty 500.
  if ([502, 503, 504].includes(response.status) ||
      (response.status === 500 && !body.trim())) {
    throw new ApiError(CONNECTION_ERROR, 0)
  }
  if (response.status >= 500) {
    throw new ApiError('Hệ thống đang gặp sự cố. Vui lòng thử lại sau.', response.status)
  }
  if (!response.ok) {
    if (response.status === 401 && token) {
      tokenStore.clear()
      window.dispatchEvent(new Event('nha-khoa:session-expired'))
    }
    let message = body
    if (contentType.includes('application/json') && body) {
      try {
        const data = JSON.parse(body)
        const validationMessage = data.errors && Object.values(data.errors).flat().find(Boolean)
        message = typeof data === 'string'
          ? data
          : validationMessage ?? data.detail ?? data.title ?? data.message ?? body
      } catch {
        // Keep the server response as plain text when it is not valid JSON.
      }
    }
    if (!message && response.status === 401 && path === '/api/auth/login') {
      message = 'Sai số điện thoại hoặc mật khẩu, hoặc tài khoản đã bị vô hiệu hóa.'
    } else if (!message && response.status === 401 && path === '/api/auth/me') {
      message = 'Phiên đăng nhập không còn hợp lệ. Vui lòng đăng nhập lại.'
    }
    throw new ApiError(message || `Yêu cầu ${path} thất bại (${response.status}).`, response.status)
  }
  if (!body) return null
  return contentType.includes('application/json') ? JSON.parse(body) : body
}

export function decodeJwt(token) {
  try {
    const payload = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')
    const normalized = payload.padEnd(Math.ceil(payload.length / 4) * 4, '=')
    return JSON.parse(decodeURIComponent(Array.from(atob(normalized), (char) =>
      `%${char.charCodeAt(0).toString(16).padStart(2, '0')}`).join('')))
  } catch {
    return null
  }
}

export const authApi = {
  login: async (username, password) => {
    const token = await request('/api/auth/login', {
      token: null,
      method: 'POST',
      body: JSON.stringify({ username, password }),
    })
    if (typeof token !== 'string' || token.split('.').length !== 3) {
      throw new ApiError('Đăng nhập chưa thành công. Vui lòng thử lại.', 502)
    }
    return token
  },
  registerCustomer: (payload) => request('/api/auth/register/khach-hang', {
    token: null,
    method: 'POST',
    body: JSON.stringify(payload),
  }),
  me: (token) => request('/api/auth/me', { token }),
  profile: (token) => request('/api/users/me', { token }),
}
