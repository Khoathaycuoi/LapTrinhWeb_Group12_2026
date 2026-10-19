import { useEffect, useRef, useState } from 'react'
import ConfirmDialog from '../../components/ConfirmDialog'
import ToastNotice from '../../components/ToastNotice'
import { apiAssetUrl } from '../../lib/api'
import { accountApi } from './accountApi'
import './AccountPage.css'

function profileForm(data) {
  return {
    ten: data.ten ?? '',
    gioiTinh: data.gioiTinh ?? '',
    ngaySinh: data.ngaySinh ?? '',
    diaChi: data.diaChi ?? '',
    bangCap: data.bangCap ?? '',
    chungChi: data.chungChi ?? '',
    soNamKinhNghiem: data.soNamKinhNghiem ?? '',
    tienSuBenh: data.tienSuBenh ?? '',
  }
}

function normalizePhone(value) {
  const phone = value.trim().replace(/[\s.-]/g, '')
  return phone.startsWith('+84') ? `0${phone.slice(3)}` : phone
}

function todayLocal() {
  const date = new Date()
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`
}

function displayValue(value) {
  return value === null || value === undefined || value === '' ? 'Chưa cập nhật' : value
}

function displayDate(value) {
  if (!value) return 'Chưa cập nhật'
  const [year, month, day] = value.split('-')
  return `${day}/${month}/${year}`
}

export default function AccountPage({ token, role, onProfileNameChange, onAvatarChange, onPhoneChanged, onLogout }) {
  const [profile, setProfile] = useState(null)
  const [form, setForm] = useState(null)
  const [loadState, setLoadState] = useState({ status: 'loading', error: '' })
  const [reload, setReload] = useState(0)
  const [notice, setNotice] = useState('')
  const [profileError, setProfileError] = useState('')
  const [profileBusy, setProfileBusy] = useState(false)
  const [avatarBusy, setAvatarBusy] = useState(false)
  const [avatarError, setAvatarError] = useState('')
  const avatarInput = useRef(null)
  const [password, setPassword] = useState({ current: '', next: '', confirm: '' })
  const [passwordError, setPasswordError] = useState('')
  const [passwordBusy, setPasswordBusy] = useState(false)
  const [phone, setPhone] = useState({ next: '', currentPassword: '' })
  const [phoneError, setPhoneError] = useState('')
  const [phoneBusy, setPhoneBusy] = useState(false)
  const [mode, setMode] = useState('view')
  const [confirmLogout, setConfirmLogout] = useState(false)

  useEffect(() => {
    const controller = new AbortController()
    queueMicrotask(() => { if (!controller.signal.aborted) setLoadState({ status: 'loading', error: '' }) })
    accountApi.detail(token, controller.signal).then((data) => {
      if (controller.signal.aborted) return
      setProfile(data)
      setForm(profileForm(data))
      setLoadState({ status: 'success', error: '' })
    }).catch((error) => {
      if (!controller.signal.aborted) setLoadState({ status: 'error', error: error.message || 'Chưa tải được thông tin tài khoản.' })
    })
    return () => controller.abort()
  }, [token, reload])

  function updateForm(field, value) {
    setForm((current) => ({ ...current, [field]: value }))
    setProfileError('')
  }

  async function saveProfile(event) {
    event.preventDefault()
    setProfileError('')
    if (!form.ten.trim()) { setProfileError('Vui lòng nhập họ và tên.'); event.currentTarget.elements.namedItem('ten')?.focus(); return }
    if (form.ngaySinh && (form.ngaySinh < '1900-01-01' || form.ngaySinh > todayLocal())) {
      setProfileError('Ngày sinh không hợp lệ.'); event.currentTarget.elements.namedItem('ngaySinh')?.focus(); return
    }
    if (role === 'BAC_SI' && form.soNamKinhNghiem !== '' && (!Number.isInteger(Number(form.soNamKinhNghiem)) || Number(form.soNamKinhNghiem) < 0 || Number(form.soNamKinhNghiem) > 65535)) {
      setProfileError('Số năm kinh nghiệm không hợp lệ.'); event.currentTarget.elements.namedItem('soNamKinhNghiem')?.focus(); return
    }
    setProfileBusy(true)
    try {
      const payload = {
        ten: form.ten.trim(), gioiTinh: form.gioiTinh || null, ngaySinh: form.ngaySinh || null, diaChi: form.diaChi.trim() || null,
        ...(role === 'BAC_SI' ? { bangCap: form.bangCap.trim() || null, chungChi: form.chungChi.trim() || null,
          soNamKinhNghiem: form.soNamKinhNghiem === '' ? null : Number(form.soNamKinhNghiem) } : {}),
        ...(role === 'KHACH_HANG' ? { tienSuBenh: form.tienSuBenh.trim() || null } : {}),
      }
      await accountApi.update(token, payload)
      setProfile((current) => ({ ...current, ...payload }))
      setForm((current) => ({ ...current, ...payload, gioiTinh: payload.gioiTinh ?? '', ngaySinh: payload.ngaySinh ?? '', diaChi: payload.diaChi ?? '',
        ...(role === 'BAC_SI' ? { bangCap: payload.bangCap ?? '', chungChi: payload.chungChi ?? '', soNamKinhNghiem: payload.soNamKinhNghiem ?? '' } : {}),
        ...(role === 'KHACH_HANG' ? { tienSuBenh: payload.tienSuBenh ?? '' } : {}) }))
      onProfileNameChange(payload.ten)
      setNotice('Đã lưu thông tin cá nhân.')
      setMode('view')
    } catch (error) {
      setProfileError(error.message || 'Chưa lưu được thông tin. Vui lòng thử lại.')
    } finally {
      setProfileBusy(false)
    }
  }

  async function changePassword(event) {
    event.preventDefault()
    setPasswordError('')
    if (!password.current) { setPasswordError('Vui lòng nhập mật khẩu hiện tại.'); return }
    if (!/^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d@$!%*?&]{8,}$/.test(password.next)) {
      setPasswordError('Mật khẩu mới phải từ 8 ký tự, có ít nhất một chữ cái và một chữ số.'); return
    }
    if (password.next !== password.confirm) { setPasswordError('Mật khẩu nhập lại chưa khớp.'); return }
    setPasswordBusy(true)
    try {
      await accountApi.changePassword(token, password.current, password.next)
      setPassword({ current: '', next: '', confirm: '' })
      setNotice('Đã đổi mật khẩu.')
      setMode('view')
    } catch (error) {
      setPasswordError(error.message || 'Chưa đổi được mật khẩu. Vui lòng thử lại.')
    } finally {
      setPasswordBusy(false)
    }
  }

  async function changePhone(event) {
    event.preventDefault()
    setPhoneError('')
    const next = normalizePhone(phone.next)
    if (!/^0\d{9}$/.test(next)) { setPhoneError('Số điện thoại phải bắt đầu bằng 0 và có đúng 10 chữ số.'); return }
    if (next === profile.sdt) { setPhoneError('Số điện thoại mới phải khác số hiện tại.'); return }
    if (!phone.currentPassword) { setPhoneError('Vui lòng nhập mật khẩu hiện tại.'); return }
    setPhoneBusy(true)
    try {
      await accountApi.changePhone(token, phone.currentPassword, next)
      onPhoneChanged(next)
    } catch (error) {
      setPhoneError(error.message || 'Chưa đổi được số điện thoại. Vui lòng thử lại.')
    } finally {
      setPhoneBusy(false)
    }
  }

  function openMode(nextMode) {
    setForm(profileForm(profile))
    setProfileError('')
    setAvatarError('')
    setPassword({ current: '', next: '', confirm: '' })
    setPasswordError('')
    setPhone({ next: '', currentPassword: '' })
    setPhoneError('')
    setMode(nextMode)
  }

  async function uploadAvatar(event) {
    const file = event.target.files?.[0]
    event.target.value = ''
    if (!file) return
    setAvatarError('')
    if (!['image/jpeg', 'image/png', 'image/webp'].includes(file.type) || file.size > 2 * 1024 * 1024) {
      setAvatarError('Chọn ảnh JPG, PNG hoặc WebP không quá 2 MB.')
      return
    }
    setAvatarBusy(true)
    try {
      const result = await accountApi.uploadAvatar(token, file)
      setProfile((current) => ({ ...current, anhDaiDienUrl: result.anhDaiDienUrl }))
      onAvatarChange(result.anhDaiDienUrl)
      setNotice('Đã cập nhật ảnh đại diện.')
    } catch (error) {
      setAvatarError(error.message || 'Chưa tải được ảnh. Vui lòng thử lại.')
    } finally {
      setAvatarBusy(false)
    }
  }

  async function deleteAvatar() {
    setAvatarError('')
    setAvatarBusy(true)
    try {
      await accountApi.deleteAvatar(token)
      setProfile((current) => ({ ...current, anhDaiDienUrl: null }))
      onAvatarChange(null)
      setNotice('Đã xóa ảnh đại diện.')
    } catch (error) {
      setAvatarError(error.message || 'Chưa xóa được ảnh. Vui lòng thử lại.')
    } finally {
      setAvatarBusy(false)
    }
  }

  const roleName = role === 'QUAN_LY' ? 'Quản lý' : role === 'LE_TAN' ? 'Lễ tân' : role === 'BAC_SI' ? 'Bác sĩ' : 'Khách hàng'

  return <div className="account-page">
    <div className="page-heading"><div><h1>Tài khoản của tôi</h1><p className="page-description">Thông tin hồ sơ và bảo mật tài khoản.</p></div></div>
    <ToastNotice message={notice} onDismiss={() => setNotice('')} />
    {loadState.status === 'loading' && <div className="account-panel" role="status">Đang tải thông tin tài khoản…</div>}
    {loadState.status === 'error' && <div className="account-panel"><div className="alert alert-error" role="alert">{loadState.error}</div><button type="button" className="secondary-button" onClick={() => setReload((value) => value + 1)}>Thử lại</button></div>}
    {loadState.status === 'success' && form && profile && <section className="account-panel">
      <div className="account-panel-heading">
        <div className="account-panel-title">{mode === 'view' && <span className="account-avatar">{profile.anhDaiDienUrl ? <img src={apiAssetUrl(profile.anhDaiDienUrl)} alt="Ảnh đại diện" /> : (profile.ten?.trim().split(/\s+/).at(-1)?.[0]?.toLocaleUpperCase('vi-VN') || '?')}</span>}<div><h2>{mode === 'view' ? 'Thông tin cá nhân' : mode === 'edit' ? 'Sửa thông tin cá nhân' : mode === 'password' ? 'Đổi mật khẩu' : 'Đổi số điện thoại'}</h2>
          {mode === 'view' && <p>{profile.id} · {roleName}</p>}</div></div>
        {mode === 'view' && <button type="button" className="secondary-button" onClick={() => openMode('edit')}>Sửa thông tin</button>}
      </div>

      {mode === 'view' && <div className="account-overview">
        <dl className="account-details">
          <div><dt>Họ và tên</dt><dd>{displayValue(profile.ten)}</dd></div>
          <div><dt>Số điện thoại</dt><dd>{displayValue(profile.sdt)}</dd></div>
          <div><dt>Giới tính</dt><dd>{displayValue(profile.gioiTinh)}</dd></div>
          <div><dt>Ngày sinh</dt><dd>{displayDate(profile.ngaySinh)}</dd></div>
          <div className="account-wide"><dt>Địa chỉ</dt><dd>{displayValue(profile.diaChi)}</dd></div>
          {profile.maDinhDanh && <div className="account-wide"><dt>Mã định danh</dt><dd>{profile.maDinhDanh}</dd></div>}
          {role === 'BAC_SI' && <>
            <div><dt>Bằng cấp</dt><dd>{displayValue(profile.bangCap)}</dd></div>
            <div><dt>Chứng chỉ</dt><dd>{displayValue(profile.chungChi)}</dd></div>
            <div><dt>Số năm kinh nghiệm</dt><dd>{displayValue(profile.soNamKinhNghiem)}</dd></div>
          </>}
          {role === 'KHACH_HANG' && <div className="account-wide"><dt>Tiền sử bệnh</dt><dd>{displayValue(profile.tienSuBenh)}</dd></div>}
        </dl>
        <aside className="account-footer"><h3>Bảo mật tài khoản</h3><div className="account-settings">
          <button type="button" className="account-setting-button" onClick={() => openMode('password')}>Đổi mật khẩu</button>
          <button type="button" className="account-setting-button" onClick={() => openMode('phone')}>Đổi số điện thoại</button>
          <button type="button" className="account-setting-button account-setting-logout" onClick={() => setConfirmLogout(true)}>Đăng xuất</button>
        </div></aside>
      </div>}

      {mode === 'edit' && <form className="account-form" onSubmit={saveProfile} noValidate>
        <dl className="account-edit-meta">
          <div><dt>Mã tài khoản</dt><dd>{profile.id}</dd></div>
          <div><dt>Vai trò</dt><dd>{roleName}</dd></div>
          <div><dt>Số điện thoại</dt><dd>{profile.sdt}</dd></div>
          <div><dt>Trạng thái</dt><dd>{displayValue(profile.trangThai)}</dd></div>
        </dl>
        <div className="account-avatar-edit">
          <span className="account-avatar account-avatar-edit-preview">{profile.anhDaiDienUrl ? <img src={apiAssetUrl(profile.anhDaiDienUrl)} alt="Ảnh đại diện" /> : (profile.ten?.trim().split(/\s+/).at(-1)?.[0]?.toLocaleUpperCase('vi-VN') || '?')}</span>
          <div className="account-avatar-edit-content">
            <span className="account-avatar-edit-label">Ảnh đại diện</span>
            <div className="account-avatar-edit-actions">
              <input ref={avatarInput} className="account-avatar-input" type="file" accept="image/jpeg,image/png,image/webp" aria-label="Chọn ảnh đại diện" onChange={uploadAvatar} />
              <button type="button" className="secondary-button" disabled={avatarBusy} onClick={() => avatarInput.current?.click()}>{avatarBusy ? 'Đang tải ảnh…' : 'Tải ảnh lên'}</button>
              {profile.anhDaiDienUrl && <button type="button" className="secondary-button" disabled={avatarBusy} onClick={deleteAvatar}>Xóa ảnh</button>}
            </div>
            <small>JPG, PNG hoặc WebP · tối đa 2 MB</small>
          </div>
        </div>
        {avatarError && <div className="alert alert-error" role="alert">{avatarError}</div>}
        <div className="account-grid">
          <label>Họ và tên<input name="ten" value={form.ten} maxLength="150" onChange={(event) => updateForm('ten', event.target.value)} /></label>
          <label>Giới tính<select value={form.gioiTinh} onChange={(event) => updateForm('gioiTinh', event.target.value)}><option value="">Chưa chọn</option><option value="Nam">Nam</option><option value="Nữ">Nữ</option><option value="Khác">Khác</option></select></label>
          <label>Ngày sinh<input name="ngaySinh" type="date" value={form.ngaySinh} onChange={(event) => updateForm('ngaySinh', event.target.value)} /></label>
          <label>Địa chỉ<input value={form.diaChi} maxLength="255" onChange={(event) => updateForm('diaChi', event.target.value)} /></label>
          {role === 'BAC_SI' && <>
            <label>Bằng cấp<input value={form.bangCap} maxLength="255" onChange={(event) => updateForm('bangCap', event.target.value)} /></label>
            <label>Chứng chỉ<input value={form.chungChi} maxLength="255" onChange={(event) => updateForm('chungChi', event.target.value)} /></label>
            <label>Số năm kinh nghiệm<input name="soNamKinhNghiem" type="number" min="0" max="65535" value={form.soNamKinhNghiem} onChange={(event) => updateForm('soNamKinhNghiem', event.target.value)} /></label>
          </>}
          {role === 'KHACH_HANG' && <label className="account-wide">Tiền sử bệnh<textarea value={form.tienSuBenh} rows="3" onChange={(event) => updateForm('tienSuBenh', event.target.value)} /></label>}
        </div>
        {profileError && <div className="alert alert-error" role="alert">{profileError}</div>}
        <div className="account-actions"><button type="button" className="secondary-button" disabled={profileBusy} onClick={() => openMode('view')}>Hủy</button><button type="submit" className="primary-button" disabled={profileBusy}>{profileBusy ? 'Đang lưu…' : 'Lưu thay đổi'}</button></div>
      </form>}

      {mode === 'password' && <form className="account-form account-form-password" onSubmit={changePassword} noValidate>
        <label>Mật khẩu hiện tại<input type="password" autoComplete="current-password" value={password.current} onChange={(event) => { setPassword({ ...password, current: event.target.value }); setPasswordError('') }} /></label>
        <label>Mật khẩu mới<input type="password" autoComplete="new-password" value={password.next} onChange={(event) => { setPassword({ ...password, next: event.target.value }); setPasswordError('') }} /></label>
        <label>Nhập lại mật khẩu mới<input type="password" autoComplete="new-password" value={password.confirm} onChange={(event) => { setPassword({ ...password, confirm: event.target.value }); setPasswordError('') }} /></label>
        {passwordError && <div className="alert alert-error" role="alert">{passwordError}</div>}
        <div className="account-actions"><button type="button" className="secondary-button" disabled={passwordBusy} onClick={() => openMode('view')}>Hủy</button><button type="submit" className="primary-button" disabled={passwordBusy}>{passwordBusy ? 'Đang xử lý…' : 'Đổi mật khẩu'}</button></div>
      </form>}

      {mode === 'phone' && <form className="account-form account-form-phone" onSubmit={changePhone} noValidate>
        <p className="account-helper">Số mới sẽ dùng để đăng nhập. Bạn cần đăng nhập lại sau khi đổi.</p>
        <label>Số điện thoại mới<input inputMode="tel" autoComplete="tel" value={phone.next} onChange={(event) => { setPhone({ ...phone, next: event.target.value }); setPhoneError('') }} placeholder="10 chữ số, bắt đầu bằng 0" /></label>
        <label>Mật khẩu hiện tại<input type="password" autoComplete="current-password" value={phone.currentPassword} onChange={(event) => { setPhone({ ...phone, currentPassword: event.target.value }); setPhoneError('') }} /></label>
        {phoneError && <div className="alert alert-error" role="alert">{phoneError}</div>}
        <div className="account-actions"><button type="button" className="secondary-button" disabled={phoneBusy} onClick={() => openMode('view')}>Hủy</button><button type="submit" className="primary-button" disabled={phoneBusy}>{phoneBusy ? 'Đang xử lý…' : 'Đổi số điện thoại'}</button></div>
      </form>}
    </section>}
    {confirmLogout && <ConfirmDialog title="Xác nhận đăng xuất" message="Bạn có chắc muốn kết thúc phiên đăng nhập hiện tại?" confirmLabel="Đăng xuất" onCancel={() => setConfirmLogout(false)} onConfirm={onLogout} />}
  </div>
}
