import { useState } from 'react'
import { authApi } from '../lib/api'

const emptyRegistration = { ten: '', sdt: '', matKhau: '', maDinhDanh: '', gioiTinh: '', ngaySinh: '', diaChi: '', tienSuBenh: '' }

export default function AuthScreen({ onLogin, mode, onModeChange }) {
  const [form, setForm] = useState(emptyRegistration)
  const [confirmPassword, setConfirmPassword] = useState('')
  const [phone, setPhone] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [notice, setNotice] = useState('')
  const [busy, setBusy] = useState(false)

  const update = (key, value) => setForm((current) => ({ ...current, [key]: value }))
  async function submit(event) {
    event.preventDefault()
    setError(''); setNotice('')
    if (mode === 'register' && form.matKhau !== confirmPassword) {
      setError('Mật khẩu nhập lại chưa khớp.')
      return
    }
    setBusy(true)
    try {
      if (mode === 'login') {
        await onLogin(phone, password)
      } else {
        const payload = Object.fromEntries(Object.entries(form).filter(([, value]) => value !== ''))
        const result = await authApi.registerCustomer(payload)
        setNotice(typeof result === 'string' ? result : 'Đăng ký Khách hàng thành công!')
        onModeChange('login')
        setPhone(form.sdt)
        setPassword('')
        setForm(emptyRegistration)
        setConfirmPassword('')
      }
    } catch (submitError) {
      setError(submitError.message || 'Đã xảy ra lỗi. Vui lòng thử lại.')
    } finally {
      setBusy(false)
    }
  }

  return <main className="auth-page">
    <section className="auth-art" aria-label="Phòng khám nha khoa">
      <div className="brand-mark"><img src="/tooth-favicon.png" alt="" /></div>
      <p className="eyebrow">NHA KHOA G12</p>
      <h1>Chăm sóc nụ cười,<br />bắt đầu từ sự an tâm.</h1>
      <p className="art-copy">Một không gian chăm sóc sức khỏe răng miệng tận tâm cho cả gia đình.</p>
      <div className="art-decoration" aria-hidden="true"><span>✧</span><span>＋</span><span>◌</span></div>
      <div className="art-footer">Hệ thống quản lý phòng khám</div>
    </section>
    <section className="auth-panel">
      <div className="auth-form-wrap">
        <div className="mobile-brand"><span><img src="/tooth-favicon.png" alt="" /></span> Nha Khoa G12</div>
        <div className="auth-heading">
          <p className="eyebrow">XIN CHÀO</p>
          <h2>{mode === 'login' ? 'Đăng nhập' : 'Tạo tài khoản Khách hàng'}</h2>
          <p>{mode === 'login' ? 'Đăng nhập để tiếp tục vào hệ thống.' : 'Tạo tài khoản cá nhân để sử dụng dịch vụ phòng khám.'}</p>
        </div>
        {error && <div className="alert alert-error" role="alert">{error}</div>}
        {notice && <div className="alert alert-success" role="status">{notice}</div>}
        <form onSubmit={submit} className="auth-form">
          {mode === 'login' ? <>
            <label>Số điện thoại<input autoComplete="username" inputMode="tel" value={phone} onChange={(event) => setPhone(event.target.value)} placeholder="Ví dụ: 0912345678" required /></label>
            <label>Mật khẩu<input type="password" autoComplete="current-password" value={password} onChange={(event) => setPassword(event.target.value)} placeholder="Nhập mật khẩu" required /></label>
          </> : <>
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
          </>}
          <button className="primary-button auth-submit" type="submit" disabled={busy}>{busy ? 'Đang xử lý…' : mode === 'login' ? 'Đăng nhập' : 'Đăng ký'}</button>
        </form>
        <p className="auth-switch">{mode === 'login' ? 'Chưa có tài khoản?' : 'Đã có tài khoản?'}{' '}
          <button type="button" className="text-button" onClick={() => { onModeChange(mode === 'login' ? 'register' : 'login'); setError(''); setNotice('') }}>{mode === 'login' ? 'Đăng ký Khách hàng' : 'Đăng nhập'}</button>
        </p>
        <p className="auth-note">Đăng ký trực tuyến chỉ dành cho Khách hàng.</p>
      </div>
    </section>
  </main>
}
