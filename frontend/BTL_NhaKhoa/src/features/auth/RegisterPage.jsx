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

  function invalidField() {
    if (!form.ten.trim()) return ['ten', 'Vui lòng nhập họ và tên.']
    const phone = form.sdt.trim().replace(/[ .-]/g, '').replace(/^\+84/, '0')
    if (!phone) return ['sdt', 'Vui lòng nhập số điện thoại.']
    if (!/^0\d{9}$/.test(phone)) return ['sdt', 'Số điện thoại phải bắt đầu bằng 0 và có đúng 10 chữ số.']
    if (!form.matKhau) return ['matKhau', 'Vui lòng nhập mật khẩu.']
    if (!/^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d@$!%*?&]{8,}$/.test(form.matKhau)) {
      return ['matKhau', 'Mật khẩu phải từ 8 ký tự, có ít nhất một chữ cái và một chữ số.']
    }
    if (!confirmPassword) return ['confirmPassword', 'Vui lòng nhập lại mật khẩu.']
    if (form.matKhau !== confirmPassword) return ['confirmPassword', 'Mật khẩu nhập lại chưa khớp.']
    if (form.maDinhDanh && !/^\d{12}$/.test(form.maDinhDanh)) return ['maDinhDanh', 'Mã định danh phải có đúng 12 chữ số.']
    const today = new Date()
    const localToday = `${today.getFullYear()}-${String(today.getMonth() + 1).padStart(2, '0')}-${String(today.getDate()).padStart(2, '0')}`
    if (form.ngaySinh && (form.ngaySinh < '1900-01-01' || form.ngaySinh > localToday)) {
      return ['ngaySinh', 'Ngày sinh không hợp lệ.']
    }
    return null
  }

  async function submit(event) {
    event.preventDefault()
    setError('')
    const invalid = event.currentTarget.elements.namedItem('ngaySinh')?.validity.badInput
      ? ['ngaySinh', 'Ngày sinh không hợp lệ.'] : invalidField()
    if (invalid) {
      setError(invalid[1])
      if (['maDinhDanh', 'ngaySinh'].includes(invalid[0])) event.currentTarget.querySelector('.optional-fields').open = true
      event.currentTarget.elements.namedItem(invalid[0])?.focus()
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

  return <AuthLayout variant="register">
    <div className="auth-heading"><p className="eyebrow">XIN CHÀO</p><h2>Tạo tài khoản Khách hàng</h2><p>Tạo tài khoản cá nhân để sử dụng dịch vụ phòng khám.</p></div>
    {error && <div className="alert alert-error" role="alert">{error}</div>}
    <form onSubmit={submit} className="auth-form" noValidate>
      <label>Họ và tên<input name="ten" value={form.ten} onChange={(event) => { update('ten', event.target.value); setError('') }} maxLength="150" required /></label>
      <label>Số điện thoại<input name="sdt" inputMode="tel" value={form.sdt} onChange={(event) => { update('sdt', event.target.value); setError('') }} placeholder="10 chữ số, bắt đầu bằng 0" required /></label>
      <label>Mật khẩu<input name="matKhau" type="password" autoComplete="new-password" value={form.matKhau} onChange={(event) => { update('matKhau', event.target.value); setError('') }} minLength="8" required /><small>Tối thiểu 8 ký tự, gồm chữ cái và chữ số.</small></label>
      <label>Nhập lại mật khẩu<input name="confirmPassword" type="password" autoComplete="new-password" value={confirmPassword} onChange={(event) => { setConfirmPassword(event.target.value); setError('') }} required /></label>
      <details className="optional-fields"><summary>Thông tin bổ sung (không bắt buộc)</summary>
        <div className="optional-grid">
          <label>Mã định danh<input name="maDinhDanh" inputMode="numeric" value={form.maDinhDanh} onChange={(event) => { update('maDinhDanh', event.target.value); setError('') }} placeholder="12 chữ số" /></label>
          <label>Giới tính<input value={form.gioiTinh} onChange={(event) => update('gioiTinh', event.target.value)} /></label>
          <label>Ngày sinh<input name="ngaySinh" type="date" value={form.ngaySinh} onChange={(event) => { update('ngaySinh', event.target.value); setError('') }} /></label>
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
