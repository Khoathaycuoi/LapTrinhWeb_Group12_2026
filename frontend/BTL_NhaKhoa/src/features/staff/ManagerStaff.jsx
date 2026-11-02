import { useEffect, useState } from 'react'
import ConfirmDialog from '../../components/ConfirmDialog'
import ListPagination from '../../components/ListPagination'
import PersonDetailDialog from '../../components/PersonDetailDialog'
import ToastNotice from '../../components/ToastNotice'
import PasswordField from '../../components/PasswordField'
import { staffApi } from './staffApi'
import '../../components/ConfirmDialog.css'
import './ManagerStaff.css'

const groups = [
  { key: 'receptionist', label: 'Lễ tân' },
  { key: 'doctor', label: 'Bác sĩ' },
  { key: 'manager', label: 'Quản lý' },
]
const emptyForm = { ten: '', sdt: '', matKhau: '', maDinhDanh: '', gioiTinh: '', ngaySinh: '', diaChi: '', chungChi: '', bangCap: '', namKinhNghiem: '' }

function normalizePhone(value) {
  const phone = String(value || '').replace(/[\s.-]/g, '')
  return phone.startsWith('+84') ? `0${phone.slice(3)}` : phone
}

export default function ManagerStaff({ token, currentPhone, currentAvatarUrl }) {
  const [group, setGroup] = useState('receptionist')
  const [keywordInput, setKeywordInput] = useState('')
  const [keyword, setKeyword] = useState('')
  const [page, setPage] = useState(1)
  const [reload, setReload] = useState(0)
  const [list, setList] = useState({ status: 'loading', items: [], totalItems: 0, error: '' })
  const [detailId, setDetailId] = useState('')
  const [detail, setDetail] = useState({ status: 'idle', data: null, error: '' })
  const [showCreate, setShowCreate] = useState(false)
  const [form, setForm] = useState(emptyForm)
  const [confirmPassword, setConfirmPassword] = useState('')
  const [confirmCreate, setConfirmCreate] = useState(false)
  const [createBusy, setCreateBusy] = useState(false)
  const [createError, setCreateError] = useState('')
  const [statusTarget, setStatusTarget] = useState(null)
  const [statusBusy, setStatusBusy] = useState(false)
  const [statusError, setStatusError] = useState('')
  const [notice, setNotice] = useState('')
  const groupLabel = groups.find((item) => item.key === group)?.label || 'Nhân sự'

  useEffect(() => {
    const controller = new AbortController()
    queueMicrotask(() => { if (!controller.signal.aborted) setList({ status: 'loading', items: [], totalItems: 0, error: '' }) })
    staffApi.list(token, group, keyword, page, controller.signal).then((result) => {
      if (controller.signal.aborted) return
      if (!Array.isArray(result?.items) || !Number.isInteger(result.totalItems)) {
        setList({ status: 'error', items: [], totalItems: 0, error: 'Máy chủ chưa trả danh sách nhân sự.' })
        return
      }
      const lastPage = Math.max(1, Math.ceil(result.totalItems / 10))
      if (page > lastPage) { setPage(lastPage); return }
      setList({ status: 'success', items: result.items, totalItems: result.totalItems, error: '' })
    }).catch((error) => {
      if (!controller.signal.aborted) setList({ status: 'error', items: [], totalItems: 0,
        error: error.status === 0 ? error.message : 'Chưa tải được danh sách nhân sự.' })
    })
    return () => controller.abort()
  }, [token, group, keyword, page, reload])

  useEffect(() => {
    if (!detailId) return undefined
    const controller = new AbortController()
    queueMicrotask(() => { if (!controller.signal.aborted) setDetail({ status: 'loading', data: null, error: '' }) })
    staffApi.detail(token, detailId, controller.signal).then((data) => {
      if (!controller.signal.aborted) setDetail({ status: 'success', data, error: '' })
    }).catch((error) => {
      if (!controller.signal.aborted) setDetail({ status: 'error', data: null,
        error: error.status === 0 ? error.message : 'Chưa xem được thông tin nhân sự.' })
    })
    return () => controller.abort()
  }, [token, detailId, reload])

  useEffect(() => {
    if (!statusTarget) return undefined
    function onKeyDown(event) { if (event.key === 'Escape' && !statusBusy) setStatusTarget(null) }
    window.addEventListener('keydown', onKeyDown)
    return () => window.removeEventListener('keydown', onKeyDown)
  }, [statusTarget, statusBusy])

  function chooseGroup(value) {
    setGroup(value)
    setKeyword('')
    setKeywordInput('')
    setPage(1)
    setShowCreate(false)
    setCreateError('')
  }

  function reviewCreate(event) {
    event.preventDefault()
    setCreateError('')
    if (form.matKhau !== confirmPassword) {
      setCreateError('Mật khẩu nhập lại chưa khớp.')
      return
    }
    setConfirmCreate(true)
  }

  async function createStaff() {
    if (createBusy) return
    setCreateBusy(true)
    setCreateError('')
    try {
      const payload = Object.fromEntries(Object.entries(form)
        .filter(([key, value]) => value !== '' && (group === 'doctor' || !['chungChi', 'bangCap', 'namKinhNghiem'].includes(key))))
      if (payload.namKinhNghiem != null) payload.namKinhNghiem = Number(payload.namKinhNghiem)
      await staffApi.create(token, group, payload)
      setConfirmCreate(false)
      setShowCreate(false)
      setForm(emptyForm)
      setConfirmPassword('')
      setKeywordInput(form.sdt)
      setKeyword(form.sdt)
      setPage(1)
      setReload((value) => value + 1)
      setNotice(`Đã tạo tài khoản ${groupLabel} cho ${form.ten}.`)
    } catch (error) {
      setConfirmCreate(false)
      setCreateError([0, 400, 409].includes(error.status) ? error.message : 'Chưa tạo được tài khoản. Vui lòng thử lại.')
    } finally {
      setCreateBusy(false)
    }
  }

  async function changeStatus() {
    if (!statusTarget || statusBusy) return
    setStatusBusy(true)
    setStatusError('')
    try {
      await staffApi.setActive(token, statusTarget.person.sdt, statusTarget.active)
      setNotice(`${statusTarget.active ? 'Đã khôi phục' : 'Đã vô hiệu hóa'} tài khoản ${statusTarget.person.ten}.`)
      setStatusTarget(null)
      setReload((value) => value + 1)
    } catch (error) {
      setStatusError([0, 400].includes(error.status) ? error.message : 'Chưa cập nhật được tài khoản. Vui lòng thử lại.')
    } finally {
      setStatusBusy(false)
    }
  }

  return <div className="manager-staff">
    <div className="page-heading manager-staff-heading"><div><h1>Nhân sự</h1><p className="page-description">Quản lý tài khoản Lễ tân, Bác sĩ và Quản lý.</p></div>
      <button type="button" className="primary-button" onClick={() => { setShowCreate(true); setCreateError('') }}>Tạo {groupLabel.toLocaleLowerCase('vi-VN')}</button></div>
    <div className="manager-staff-tabs" role="group" aria-label="Nhóm nhân sự">{groups.map((item) => <button key={item.key} type="button"
      className={group === item.key ? 'active' : ''} aria-pressed={group === item.key} onClick={() => chooseGroup(item.key)}>{item.label}</button>)}</div>
    {showCreate && <section className="manager-staff-create"><div className="manager-staff-create-heading"><h2>Tạo tài khoản {groupLabel}</h2><button type="button" className="secondary-button" onClick={() => { setShowCreate(false); setCreateError('') }}>Đóng</button></div>
      <form className="manager-staff-form" onSubmit={reviewCreate}>
        <label>Họ và tên<input required maxLength={150} value={form.ten} onChange={(event) => setForm({ ...form, ten: event.target.value })} /></label>
        <label>Số điện thoại<input required inputMode="tel" placeholder="10 chữ số, bắt đầu bằng 0" value={form.sdt} onChange={(event) => setForm({ ...form, sdt: event.target.value })} /></label>
        <PasswordField label="Mật khẩu" required minLength={8} autoComplete="new-password" value={form.matKhau} onChange={(event) => setForm({ ...form, matKhau: event.target.value })} />
        <PasswordField label="Nhập lại mật khẩu" required autoComplete="new-password" value={confirmPassword} onChange={(event) => setConfirmPassword(event.target.value)} />
        <label>Mã định danh (tùy chọn)<input inputMode="numeric" pattern="[0-9]{12}" maxLength={12} value={form.maDinhDanh} onChange={(event) => setForm({ ...form, maDinhDanh: event.target.value })} /></label>
        <label>Giới tính<select value={form.gioiTinh} onChange={(event) => setForm({ ...form, gioiTinh: event.target.value })}><option value="">Chưa chọn</option><option value="Nam">Nam</option><option value="Nữ">Nữ</option><option value="Khác">Khác</option></select></label>
        <label>Ngày sinh<input type="date" value={form.ngaySinh} onChange={(event) => setForm({ ...form, ngaySinh: event.target.value })} /></label>
        <label>Địa chỉ<input maxLength={255} value={form.diaChi} onChange={(event) => setForm({ ...form, diaChi: event.target.value })} /></label>
        {group === 'doctor' && <><label>Chứng chỉ (tùy chọn)<input maxLength={255} value={form.chungChi} onChange={(event) => setForm({ ...form, chungChi: event.target.value })} /></label>
          <label>Bằng cấp (tùy chọn)<input maxLength={255} value={form.bangCap} onChange={(event) => setForm({ ...form, bangCap: event.target.value })} /></label>
          <label>Năm kinh nghiệm (tùy chọn)<input type="number" min="0" max="65535" value={form.namKinhNghiem} onChange={(event) => setForm({ ...form, namKinhNghiem: event.target.value })} /></label></>}
        {createError && <p className="manager-staff-error" role="alert">{createError}</p>}
        <button type="submit" className="primary-button">Tạo tài khoản</button>
      </form>
    </section>}
    <form className="manager-staff-search" onSubmit={(event) => { event.preventDefault(); setKeyword(keywordInput.trim()); setPage(1); setReload((value) => value + 1) }}>
      <label>Tên, số điện thoại hoặc mã nhân sự<input type="search" value={keywordInput} onChange={(event) => setKeywordInput(event.target.value)} placeholder={`Tìm ${groupLabel.toLocaleLowerCase('vi-VN')}`} /></label>
      <button type="submit" className="secondary-button">Tìm</button>
      {keyword && <button type="button" className="manager-staff-clear" onClick={() => { setKeyword(''); setKeywordInput(''); setPage(1) }}>Xóa bộ lọc</button>}
    </form>
    {list.status === 'loading' && <div className="manager-staff-feedback" role="status">Đang tải danh sách…</div>}
    {list.status === 'error' && <div className="manager-staff-feedback" role="alert"><p>{list.error}</p><button type="button" className="secondary-button" onClick={() => setReload((value) => value + 1)}>Thử lại</button></div>}
    {list.status === 'success' && <><p className="manager-staff-count">{list.totalItems} {groupLabel.toLocaleLowerCase('vi-VN')}</p>
      {list.items.length === 0 && <div className="manager-staff-feedback">Không có nhân sự phù hợp.</div>}
      <div className="manager-staff-list">{list.items.map((person) => <article key={person.id}>
        <div><span>{person.id}</span><h2>{person.ten}</h2><p>{person.sdt} · <span className={`manager-staff-state ${person.trangThai === 'Hoạt động' ? 'manager-staff-state-active' : 'manager-staff-state-inactive'}`}>{person.trangThai}</span></p></div>
        <div className="manager-staff-actions"><button type="button" className="secondary-button" onClick={() => setDetailId(person.id)}>Chi tiết</button>
          {!(person.trangThai === 'Hoạt động' && currentPhone && normalizePhone(person.sdt) === normalizePhone(currentPhone)) &&
            <button type="button" className="secondary-button" onClick={() => { setStatusTarget({ person, active: person.trangThai !== 'Hoạt động' }); setStatusError('') }}>
              {person.trangThai === 'Hoạt động' ? 'Vô hiệu hóa' : 'Khôi phục'}</button>}</div>
      </article>)}</div>
      <ListPagination page={page} pageSize={10} totalItems={list.totalItems} onPageChange={setPage} />
    </>}
    {detailId && <PersonDetailDialog type="staff" id={detailId} detail={detail}
      avatarUrl={detail.data && currentPhone && normalizePhone(detail.data.sdt) === normalizePhone(currentPhone) ? currentAvatarUrl : undefined}
      onClose={() => setDetailId('')} />}
    {confirmCreate && <ConfirmDialog title={`Tạo tài khoản ${groupLabel}?`} message={`Tạo tài khoản cho ${form.ten} với số ${form.sdt}?`}
      confirmLabel={createBusy ? 'Đang tạo…' : 'Tạo tài khoản'} busy={createBusy} onConfirm={createStaff} onCancel={() => setConfirmCreate(false)} />}
    {statusTarget && <div className="dialog-backdrop" onMouseDown={(event) => { if (event.target === event.currentTarget && !statusBusy) setStatusTarget(null) }}><section className="confirm-dialog manager-staff-status" role="alertdialog" aria-modal="true" aria-labelledby="manager-staff-status-title">
      <h2 id="manager-staff-status-title">{statusTarget.active ? 'Khôi phục' : 'Vô hiệu hóa'} tài khoản?</h2>
      <p>{statusTarget.person.ten} · {statusTarget.person.sdt}</p>
      {!statusTarget.active && group === 'doctor' && <p>Lịch đang phân công cho bác sĩ này không tự chuyển sang bác sĩ khác.</p>}
      {statusError && <p className="manager-staff-error" role="alert">{statusError}</p>}
      <div className="confirm-actions"><button type="button" className="secondary-button" disabled={statusBusy} onClick={() => setStatusTarget(null)}>Quay lại</button>
        <button type="button" className="primary-button" disabled={statusBusy} onClick={changeStatus}>{statusBusy ? 'Đang cập nhật…' : 'Xác nhận'}</button></div>
    </section></div>}
    <ToastNotice message={notice} onDismiss={() => setNotice('')} />
  </div>
}
