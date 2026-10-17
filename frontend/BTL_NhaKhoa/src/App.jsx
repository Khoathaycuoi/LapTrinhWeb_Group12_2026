import { useEffect, useState } from 'react'
import AppShell from './components/AppShell'
import AuthScreen from './components/AuthScreen'
import ConfirmDialog from './components/ConfirmDialog'
import { AuthProvider } from './features/auth/AuthContext'
import { useAuth } from './features/auth/useAuth'
import { roleHome, roleNavigation, roles } from './app/roles'
import './App.css'

function PageContent({ role, path }) {
  const key = path.split('/').filter(Boolean).at(-1)
  const navItem = roleNavigation[role].find(([, route]) => route === key)
  const known = Boolean(navItem)
  const label = navItem?.[0] ?? key.replaceAll('-', ' ')
  return <>
    <div className="page-heading"><div><h1>{label === 'overview' ? 'Tổng quan' : label}</h1><p className="page-description">Chào mừng bạn đến với Nha Khoa G12.</p></div></div>
    {key === 'overview' ? <section className="welcome-card">
      <div className="welcome-icon">✧</div><div><h2>Chào mừng đến với Nha Khoa G12</h2>
        <p>Các thông tin tổng quan sẽ được cập nhật tại đây.</p></div>
    </section> : <section className="placeholder-card"><div className="placeholder-mark">◌</div><h2>{known ? 'Tính năng đang được hoàn thiện' : 'Không tìm thấy trang'}</h2><p>{known ? 'Mục này chưa thể sử dụng. Vui lòng quay lại sau.' : 'Đường dẫn bạn mở không thuộc khu vực này.'}</p></section>}
  </>
}

function UnsupportedRole({ logout, navigate }) {
  const [confirmLogout, setConfirmLogout] = useState(false)
  return <div className="auth-error">
    <h1>Vai trò chưa được hỗ trợ</h1>
    <p>Tài khoản hiện tại không có vai trò thuộc hệ thống.</p>
    <button className="primary-button" onClick={() => setConfirmLogout(true)}>Đăng xuất</button>
    {confirmLogout && <ConfirmDialog title="Xác nhận đăng xuất" message="Bạn có chắc muốn kết thúc phiên đăng nhập hiện tại?" confirmLabel="Đăng xuất" onCancel={() => setConfirmLogout(false)} onConfirm={() => { logout(); navigate('/dang-nhap', true) }} />}
  </div>
}

function AppContent() {
  const { session, ready, login, logout } = useAuth()
  const [path, setPath] = useState(window.location.pathname)

  useEffect(() => {
    const onPopState = () => setPath(window.location.pathname)
    window.addEventListener('popstate', onPopState)
    return () => window.removeEventListener('popstate', onPopState)
  }, [])

  function navigate(nextPath, replace = false) {
    if (replace) window.history.replaceState({}, '', nextPath)
    else window.history.pushState({}, '', nextPath)
    setPath(nextPath)
  }

  const currentPath = session
    ? path.startsWith(`${roles[session.role]?.base}/`) ? path : roleHome(session.role)
    : path === '/dang-ky' ? path : '/dang-nhap'

  useEffect(() => {
    if (ready && window.location.pathname !== currentPath) {
      window.history.replaceState({}, '', currentPath)
    }
  }, [ready, currentPath])

  if (!ready) return <div className="loading-screen"><span className="loading-mark">✦</span><p>Đang kiểm tra phiên đăng nhập…</p></div>
  if (!session) return <AuthScreen mode={currentPath === '/dang-ky' ? 'register' : 'login'} onModeChange={(mode) => navigate(mode === 'register' ? '/dang-ky' : '/dang-nhap')} onLogin={async (phone, password) => {
    const authenticated = await login(phone, password)
    navigate(roleHome(authenticated.role), true)
  }} />

  const allowed = roles[session.role]
  if (!allowed) return <UnsupportedRole logout={logout} navigate={navigate} />
  return <AppShell role={session.role} name={session.name} path={currentPath} onNavigate={navigate} onLogout={() => { logout(); navigate('/dang-nhap', true) }}>
    <PageContent role={session.role} path={currentPath} />
  </AppShell>
}

export default function App() {
  return <AuthProvider><AppContent /></AuthProvider>
}
