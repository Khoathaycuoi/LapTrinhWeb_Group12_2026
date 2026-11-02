import { useState } from 'react'
import AuthLayout from './AuthLayout'
import ToastNotice from '../../components/ToastNotice'
import PasswordField from '../../components/PasswordField'

export default function LoginPage({ onLogin, onNavigateRegister, onNoticeDismiss, registeredPhone = '', notice = '' }) {
  const [phone, setPhone] = useState(registeredPhone)
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [showPasswordHelp, setShowPasswordHelp] = useState(false)
  const [busy, setBusy] = useState(false)

  async function submit(event) {
    event.preventDefault()
    setError('')
    if (!phone.trim()) {
      setError('Vui lòng nhập số điện thoại.')
      event.currentTarget.elements.namedItem('phone')?.focus()
      return
    }
    if (!password) {
      setError('Vui lòng nhập mật khẩu.')
      event.currentTarget.elements.namedItem('password')?.focus()
      return
    }
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
    <ToastNotice message={notice} onDismiss={onNoticeDismiss} />
    <form onSubmit={submit} className="auth-form" noValidate>
      <label>Số điện thoại<input name="phone" autoComplete="username" inputMode="tel" value={phone} onChange={(event) => { setPhone(event.target.value); setError('') }} placeholder="Ví dụ: 0912345678" required /></label>
      <PasswordField label="Mật khẩu" name="password" autoComplete="current-password" value={password} onChange={(event) => { setPassword(event.target.value); setError('') }} placeholder="Nhập mật khẩu" required />
      <div className="auth-help-actions">
        <button type="button" className="text-button" onClick={() => setShowPasswordHelp(true)}>Quên mật khẩu?</button>
      </div>
      {showPasswordHelp && <p className="auth-help-message" role="status">Chức năng đặt lại mật khẩu chưa được hỗ trợ. Vui lòng gọi hotline <strong>19000000</strong> để được hỗ trợ.</p>}
      <button className="primary-button auth-submit" type="submit" disabled={busy}>{busy ? 'Đang xử lý…' : 'Đăng nhập'}</button>
    </form>
    <p className="auth-switch">Chưa có tài khoản? <button type="button" className="text-button" onClick={onNavigateRegister}>Đăng ký Khách hàng</button></p>
    <p className="auth-note">Đăng ký trực tuyến chỉ dành cho Khách hàng.</p>
  </AuthLayout>
}
