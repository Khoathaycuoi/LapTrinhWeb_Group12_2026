import { useEffect, useMemo, useState } from 'react'
import { authApi, decodeJwt, tokenStore } from '../../lib/api'
import { AuthContext } from './auth-context'

async function loadSession(token, claims) {
  const identity = await authApi.me(token)
  let name = identity.ten ?? identity.Ten

  // Older running backend instances may not yet include ten in /api/auth/me.
  if (!name?.trim()) {
    const profile = await authApi.profile(token)
    name = profile.ten ?? profile.Ten
  }
  if (!name?.trim()) {
    throw new Error('Không thể tải tên tài khoản. Vui lòng thử lại sau.')
  }

  return {
    token,
    role: identity.vaiTro ?? identity.VaiTro ?? claims.vai_tro,
    name: name.trim(),
    phone: identity.soDienThoai ?? identity.SoDienThoai ?? claims.sub ?? '',
  }
}

export function AuthProvider({ children }) {
  const [session, setSession] = useState(null)
  const [ready, setReady] = useState(() => {
    const token = tokenStore.get()
    if (!token) return true
    const claims = decodeJwt(token)
    return !claims?.vai_tro || Number(claims.exp ?? 0) * 1000 <= Date.now()
  })

  useEffect(() => {
    const expireSession = () => setSession(null)
    window.addEventListener('nha-khoa:session-expired', expireSession)
    let active = true
    const token = tokenStore.get()
    if (!token) {
      return () => { active = false; window.removeEventListener('nha-khoa:session-expired', expireSession) }
    }

    const claims = decodeJwt(token)
    const role = claims?.vai_tro
    const expiration = Number(claims?.exp ?? 0) * 1000
    if (!role || expiration <= Date.now()) {
      tokenStore.clear()
      return () => { active = false; window.removeEventListener('nha-khoa:session-expired', expireSession) }
    }

    loadSession(token, claims)
      .then((restored) => { if (active) setSession(restored) })
      .catch(() => tokenStore.clear())
      .finally(() => { if (active) setReady(true) })
    return () => { active = false; window.removeEventListener('nha-khoa:session-expired', expireSession) }
  }, [])

  const value = useMemo(() => ({
    session,
    ready,
    async login(phone, password) {
      const token = await authApi.login(phone, password)
      const claims = decodeJwt(token)
      if (!claims?.vai_tro) throw new Error('Token thiếu thông tin vai trò. Vui lòng liên hệ quản trị hệ thống.')
      try {
        const nextSession = await loadSession(token, claims)
        tokenStore.set(token)
        setSession(nextSession)
        return nextSession
      } catch (error) {
        tokenStore.clear()
        if (error.status === 401) {
          throw new Error('Không thể xác thực phiên đăng nhập. Vui lòng thử đăng nhập lại; nếu lỗi vẫn tiếp diễn, hãy liên hệ quản trị viên.', { cause: error })
        }
        throw error
      }
    },
    logout() {
      tokenStore.clear()
      setSession(null)
    },
  }), [session, ready])

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
