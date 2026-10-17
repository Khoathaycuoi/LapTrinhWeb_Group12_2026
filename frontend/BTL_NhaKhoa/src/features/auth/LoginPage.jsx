import { useState } from 'react'
import AuthLayout from './AuthLayout'

export default function LoginPage({ onLogin, onNavigateRegister, registeredPhone = '', notice = '' }) {
  const [phone, setPhone] = useState(registeredPhone)
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  async function submit(event) {
    event.preventDefault()
    setError('')
    setBusy(true)
    try {
      await onLogin(phone, password)
    } catch (submitError) {
      setError(submitError.message || 'Đã xảy ra lỗi. Vui lòng thử lại.')
    } finally {
      setBusy(false)
    }
  }

  return <AuthLayout>
    <div className="auth-heading"><p className="eyebrow">XIN CHÀO</p><h2>Đăng nhập</h2><p>Đăng nhập để tiếp tục vào hệ thống.</p></div>
    {error && <div className="alert alert-error" role="alert">{error}</div>}
    {notice && <div className="alert alert-success" role="status">{notice}</div>}
    <form onSubmit={submit} className="auth-form">
      <label>Số điện thoại<input autoComplete="username" inputMode="tel" value={phone} onChange={(event) => setPhone(event.target.value)} placeholder="Ví dụ: 0912345678" required /></label>
      <label>Mật khẩu<input type="password" autoComplete="current-password" value={password} onChange={(event) => setPassword(event.target.value)} placeholder="Nhập mật khẩu" required /></label>
      <button className="primary-button auth-submit" type="submit" disabled={busy}>{busy ? 'Đang xử lý…' : 'Đăng nhập'}</button>
    </form>
    <p className="auth-switch">Chưa có tài khoản? <button type="button" className="text-button" onClick={onNavigateRegister}>Đăng ký Khách hàng</button></p>
    <p className="auth-note">Đăng ký trực tuyến chỉ dành cho Khách hàng.</p>
  </AuthLayout>
}
