import { useEffect, useState } from 'react'
import ConfirmDialog from '../../components/ConfirmDialog'
import ListPagination from '../../components/ListPagination'
import ToastNotice from '../../components/ToastNotice'
import { receptionCustomersApi } from './receptionCustomersApi'
import './ReceptionCustomers.css'

const emptyForm = { ten: '', sdt: '', matKhau: '', gioiTinh: '', ngaySinh: '', diaChi: '' }

export default function ReceptionCustomers({ token, onNavigate }) {
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
  const [busy, setBusy] = useState(false)
  const [formError, setFormError] = useState('')
  const [notice, setNotice] = useState('')

  useEffect(() => {
    const controller = new AbortController()
    setList({ status: 'loading', items: [], totalItems: 0, error: '' })
    receptionCustomersApi.list(token, keyword, page, controller.signal).then((result) => {
      if (controller.signal.aborted) return
      if (!Array.isArray(result?.items) || !Number.isInteger(result.totalItems)) {
        setList({ status: 'error', items: [], totalItems: 0, error: 'Máy chủ chưa trả danh sách khách hàng.' })
        return
      }
      const lastPage = Math.max(1, Math.ceil(result.totalItems / 10))
      if (page > lastPage) { setPage(lastPage); return }
      setList({ status: 'success', items: result.items, totalItems: result.totalItems, error: '' })
    }).catch((error) => {
      if (!controller.signal.aborted) setList({ status: 'error', items: [], totalItems: 0,
        error: error.status === 0 ? error.message : 'Chưa tải được danh sách khách hàng.' })
    })
    return () => controller.abort()
  }, [token, keyword, page, reload])

  useEffect(() => {
    if (!detailId) return undefined
    const controller = new AbortController()
    setDetail({ status: 'loading', data: null, error: '' })
    receptionCustomersApi.detail(token, detailId, controller.signal).then((data) => {
      if (!controller.signal.aborted) setDetail({ status: 'success', data, error: '' })
    }).catch((error) => {
      if (!controller.signal.aborted) setDetail({ status: 'error', data: null,
        error: error.status === 0 ? error.message : 'Chưa xem được khách hàng này.' })
    })
    return () => controller.abort()
  }, [token, detailId, reload])

  function reviewCreate(event) {
    event.preventDefault()
    setFormError('')
    if (form.matKhau !== confirmPassword) {
      setFormError('Mật khẩu nhập lại chưa khớp.')
      return
    }
    setConfirmCreate(true)
  }

  async function createCustomer() {
    if (busy) return
    setBusy(true)
    try {
      const payload = Object.fromEntries(Object.entries(form).filter(([, value]) => value !== ''))
      await receptionCustomersApi.create(payload)
      setConfirmCreate(false)
      setShowCreate(false)
      setForm(emptyForm)
      setConfirmPassword('')
      setKeywordInput(form.sdt)
      setKeyword(form.sdt)
      setPage(1)
      setReload((value) => value + 1)
      setNotice(`Đã tạo tài khoản cho ${form.ten}.`)
    } catch (error) {
      setConfirmCreate(false)
      setFormError(error.status === 400 || error.status === 0 ? error.message : 'Chưa tạo được khách hàng. Vui lòng thử lại.')
    } finally {
      setBusy(false)
    }
  }

  return <div className="reception-customers">
    <div className="page-heading reception-customers-heading"><div><h1>Khách hàng</h1><p className="page-description">Tra cứu thông tin liên hệ và đặt lịch cho khách tại quầy.</p></div><button type="button" className="primary-button" onClick={() => setShowCreate(true)}>Tạo khách hàng</button></div>
    {showCreate && <section className="customer-create-panel"><div className="customer-panel-heading"><h2>Tạo tài khoản Khách hàng</h2><button type="button" className="secondary-button" onClick={() => { setShowCreate(false); setFormError('') }}>Đóng</button></div>
      <p>Khách hàng cần biết và giữ mật khẩu đăng nhập của mình.</p>
      <form onSubmit={reviewCreate} className="customer-create-form">
        <label>Họ và tên<input required maxLength={150} value={form.ten} onChange={(event) => setForm({ ...form, ten: event.target.value })} /></label>
        <label>Số điện thoại<input required inputMode="tel" placeholder="10 chữ số, bắt đầu bằng 0" value={form.sdt} onChange={(event) => setForm({ ...form, sdt: event.target.value })} /></label>
        <label>Mật khẩu<input required type="password" minLength={8} autoComplete="new-password" value={form.matKhau} onChange={(event) => setForm({ ...form, matKhau: event.target.value })} /></label>
        <label>Nhập lại mật khẩu<input required type="password" autoComplete="new-password" value={confirmPassword} onChange={(event) => setConfirmPassword(event.target.value)} /></label>
        <label>Giới tính<select value={form.gioiTinh} onChange={(event) => setForm({ ...form, gioiTinh: event.target.value })}><option value="">Chưa chọn</option><option value="Nam">Nam</option><option value="Nữ">Nữ</option><option value="Khác">Khác</option></select></label>
        <label>Ngày sinh<input type="date" value={form.ngaySinh} onChange={(event) => setForm({ ...form, ngaySinh: event.target.value })} /></label>
        <label className="customer-wide-field">Địa chỉ<input maxLength={255} value={form.diaChi} onChange={(event) => setForm({ ...form, diaChi: event.target.value })} /></label>
        {formError && <p className="customer-form-error" role="alert">{formError}</p>}
        <button type="submit" className="primary-button">Tạo tài khoản</button>
      </form>
    </section>}
    <form className="customer-search" onSubmit={(event) => { event.preventDefault(); setKeyword(keywordInput.trim()); setPage(1); setReload((value) => value + 1) }}>
      <label>Tên, số điện thoại hoặc mã khách hàng<input type="search" value={keywordInput} onChange={(event) => setKeywordInput(event.target.value)} placeholder="Tìm khách hàng" /></label>
      <button type="submit" className="secondary-button">Tìm</button>
      {keyword && <button type="button" className="customer-clear" onClick={() => { setKeywordInput(''); setKeyword(''); setPage(1) }}>Xóa bộ lọc</button>}
    </form>
    {list.status === 'loading' && <div className="customer-feedback" role="status">Đang tải khách hàng…</div>}
    {list.status === 'error' && <div className="customer-feedback" role="alert"><p>{list.error}</p><button type="button" className="secondary-button" onClick={() => setReload((value) => value + 1)}>Thử lại</button></div>}
    {list.status === 'success' && <><p className="customer-count">{list.totalItems} khách hàng</p>
      {list.items.length === 0 && <div className="customer-feedback">Không có khách hàng phù hợp.</div>}
      <div className="customer-list">{list.items.map((customer) => <article key={customer.id}>
        <div><span>{customer.id}</span><h2>{customer.ten}</h2><p>{customer.sdt} · {customer.trangThai}</p></div>
        <div className="customer-actions"><button type="button" className="secondary-button" onClick={() => setDetailId(customer.id)}>Chi tiết</button>
          {customer.trangThai === 'Hoạt động' && <button type="button" className="primary-button" onClick={() => onNavigate(`/le-tan/dat-lich?maKhachHang=${encodeURIComponent(customer.id)}`)}>Đặt lịch</button>}</div>
      </article>)}</div>
      <ListPagination page={page} pageSize={10} totalItems={list.totalItems} onPageChange={setPage} />
    </>}
    {detailId && <div className="dialog-backdrop" onMouseDown={(event) => { if (event.target === event.currentTarget) setDetailId('') }}><section className="confirm-dialog customer-detail-dialog" role="dialog" aria-modal="true" aria-label="Chi tiết khách hàng">
      <div className="customer-panel-heading"><h2>Khách hàng {detailId}</h2><button type="button" className="secondary-button" onClick={() => setDetailId('')}>Đóng</button></div>
      {detail.status === 'loading' && <p>Đang tải…</p>}
      {detail.status === 'error' && <p role="alert">{detail.error}</p>}
      {detail.status === 'success' && detail.data && <dl><dt>Họ và tên</dt><dd>{detail.data.ten}</dd><dt>Số điện thoại</dt><dd>{detail.data.sdt}</dd><dt>Ngày sinh</dt><dd>{detail.data.ngaySinh || 'Chưa cập nhật'}</dd><dt>Giới tính</dt><dd>{detail.data.gioiTinh || 'Chưa cập nhật'}</dd><dt>Địa chỉ</dt><dd>{detail.data.diaChi || 'Chưa cập nhật'}</dd><dt>Trạng thái</dt><dd>{detail.data.trangThai}</dd></dl>}
    </section></div>}
    {confirmCreate && <ConfirmDialog title="Tạo tài khoản Khách hàng?" message={`Tạo tài khoản cho ${form.ten} với số ${form.sdt}?`}
      confirmLabel={busy ? 'Đang tạo…' : 'Tạo tài khoản'} busy={busy} onConfirm={createCustomer} onCancel={() => setConfirmCreate(false)} />}
    <ToastNotice message={notice} onDismiss={() => setNotice('')} />
  </div>
}
