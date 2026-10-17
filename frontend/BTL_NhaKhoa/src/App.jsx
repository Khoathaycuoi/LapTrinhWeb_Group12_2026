import { useEffect, useState } from 'react'
import AppShell from './components/AppShell'
import { AuthProvider } from './features/auth/AuthContext'
import { useAuth } from './features/auth/useAuth'
import LoginPage from './features/auth/LoginPage'
import RegisterPage from './features/auth/RegisterPage'
import RolePage from './app/RolePage'
import { roleHome, roles } from './app/roles'
import SessionLoadingPage from './pages/SessionLoadingPage'
import UnsupportedRolePage from './pages/UnsupportedRolePage'
import './App.css'

function AppContent() {
  const { session, ready, login, logout } = useAuth()
  const [path, setPath] = useState(window.location.pathname + window.location.search)
  const [registrationResult, setRegistrationResult] = useState(null)

  useEffect(() => {
    const onPopState = () => setPath(window.location.pathname + window.location.search)
    window.addEventListener('popstate', onPopState)
    return () => window.removeEventListener('popstate', onPopState)
  }, [])

  function navigate(nextPath, replace = false, navigationState = {}) {
    if (replace) window.history.replaceState(navigationState, '', nextPath)
    else window.history.pushState(navigationState, '', nextPath)
    setPath(nextPath)
  }

  function signOut() {
    logout()
    navigate('/dang-nhap', true)
  }

  const currentPath = session
    ? path.startsWith(`${roles[session.role]?.base}/`) ? path : roleHome(session.role)
    : path === '/dang-ky' ? path : '/dang-nhap'
  const [routePath, query = ''] = currentPath.split('?')

  useEffect(() => {
    if (ready && window.location.pathname + window.location.search !== currentPath) {
      window.history.replaceState({}, '', currentPath)
    }
  }, [ready, currentPath])

  if (!ready) return <SessionLoadingPage />

  if (!session) {
    if (routePath === '/dang-ky') {
      return <RegisterPage onNavigateLogin={() => navigate('/dang-nhap')}
        onSuccess={(phone, message) => { setRegistrationResult({ phone, message }); navigate('/dang-nhap') }} />
    }
    return <LoginPage registeredPhone={registrationResult?.phone} notice={registrationResult?.message}
      onNavigateRegister={() => { setRegistrationResult(null); navigate('/dang-ky') }}
      onLogin={async (phone, password) => {
        const authenticated = await login(phone, password)
        setRegistrationResult(null)
        navigate(roleHome(authenticated.role), true)
      }} />
  }

  if (!roles[session.role]) return <UnsupportedRolePage onLogout={signOut} />

  return <AppShell role={session.role} name={session.name} path={routePath} onNavigate={navigate} onLogout={signOut}>
    <RolePage session={session} routePath={routePath} query={query} navigationState={window.history.state} onNavigate={navigate} />
  </AppShell>
}

export default function App() {
  return <AuthProvider><AppContent /></AuthProvider>
}
