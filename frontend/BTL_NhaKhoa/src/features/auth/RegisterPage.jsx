import { useState } from 'react'
import { authApi } from '../../lib/api'
import AuthLayout from './AuthLayout'

const emptyRegistration = { ten: '', sdt: '', matKhau: '', maDinhDanh: '', gioiTinh: '', ngaySinh: '', diaChi: '', tienSuBenh: '' }

export default function RegisterPage({ onSuccess, onNavigateLogin }) {
  const [form, setForm] = useState(emptyRegistration)
  const [confirmPassword, setConfirmPassword] = useState('')
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  const update = (key, value) => setForm((current) => ({ ...current, [key]: value }))

  async function submit(event) {
    event.preventDefault()
    setError('')
    if (form.matKhau !== confirmPassword) {
      setError('Mật khẩu nhập lại chưa khớp.')
      return
    }
    setBusy(true)
    try {
      const payload = Object.fromEntries(Object.entries(form).filter(([, value]) => value !== ''))
      const result = await authApi.registerCustomer(payload)
      onSuccess(form.sdt, typeof result === 'string' ? result : 'Đăng ký Khách hàng thành công!')
    } catch (submitError) {
      setError(submitError.message || 'Đã xảy ra lỗi. Vui lòng thử lại.')
    } finally {
      setBusy(false)
    }
  }

  return <AuthLayout>
    <div className="auth-heading"><p className="eyebrow">XIN CHÀO</p><h2>Tạo tài khoản Khách hàng</h2><p>Tạo tài khoản cá nhân để sử dụng dịch vụ phòng khám.</p></div>
    {error && <div className="alert alert-error" role="alert">{error}</div>}
    <form onSubmit={submit} className="auth-form">
      <label>Họ và tên<input value={form.ten} onChange={(event) => update('ten', event.target.value)} maxLength="150" required /></label>
      <label>Số điện thoại<input inputMode="tel" value={form.sdt} onChange={(event) => update('sdt', event.target.value)} placeholder="10 chữ số, bắt đầu bằng 0" required /></label>
      <label>Mật khẩu<input type="password" autoComplete="new-password" value={form.matKhau} onChange={(event) => update('matKhau', event.target.value)} minLength="8" required /><small>Tối thiểu 8 ký tự, gồm chữ cái và chữ số.</small></label>
      <label>Nhập lại mật khẩu<input type="password" autoComplete="new-password" value={confirmPassword} onChange={(event) => setConfirmPassword(event.target.value)} required /></label>
      <details className="optional-fields"><summary>Thông tin bổ sung (không bắt buộc)</summary>
        <div className="optional-grid">
          <label>Mã định danh<input inputMode="numeric" value={form.maDinhDanh} onChange={(event) => update('maDinhDanh', event.target.value)} placeholder="12 chữ số" /></label>
          <label>Giới tính<input value={form.gioiTinh} onChange={(event) => update('gioiTinh', event.target.value)} /></label>
          <label>Ngày sinh<input type="date" value={form.ngaySinh} onChange={(event) => update('ngaySinh', event.target.value)} /></label>
          <label>Địa chỉ<input value={form.diaChi} onChange={(event) => update('diaChi', event.target.value)} maxLength="255" /></label>
          <label className="wide-field">Tiền sử bệnh<input value={form.tienSuBenh} onChange={(event) => update('tienSuBenh', event.target.value)} /></label>
        </div>
      </details>
      <button className="primary-button auth-submit" type="submit" disabled={busy}>{busy ? 'Đang xử lý…' : 'Đăng ký'}</button>
    </form>
    <p className="auth-switch">Đã có tài khoản? <button type="button" className="text-button" onClick={onNavigateLogin}>Đăng nhập</button></p>
    <p className="auth-note">Đăng ký trực tuyến chỉ dành cho Khách hàng.</p>
  </AuthLayout>
}
