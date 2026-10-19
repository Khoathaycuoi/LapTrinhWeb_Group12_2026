import { useEffect, useState } from 'react'
import ListPagination from '../../components/ListPagination'
import ToastNotice from '../../components/ToastNotice'
import { servicesApi } from './servicesApi'
import '../../components/ConfirmDialog.css'
import './ManagerServices.css'

const emptyForm = { tenDichVu: '', moTa: '', donGia: '', thoiLuongDuKienPhut: '30' }
const money = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND', maximumFractionDigits: 0 })

export default function ManagerServices({ token }) {
  const [nameInput, setNameInput] = useState('')
  const [statusInput, setStatusInput] = useState('')
  const [filters, setFilters] = useState({ name: '', status: '' })
  const [page, setPage] = useState(1)
  const [reload, setReload] = useState(0)
  const [list, setList] = useState({ status: 'loading', items: [], totalItems: 0, error: '' })
  const [editing, setEditing] = useState(null)
  const [form, setForm] = useState(emptyForm)
  const [formError, setFormError] = useState('')
  const [formBusy, setFormBusy] = useState(false)
  const [statusTarget, setStatusTarget] = useState(null)
  const [statusError, setStatusError] = useState('')
  const [statusBusy, setStatusBusy] = useState(false)
  const [notice, setNotice] = useState('')

  useEffect(() => {
    const controller = new AbortController()
    queueMicrotask(() => { if (!controller.signal.aborted) setList({ status: 'loading', items: [], totalItems: 0, error: '' }) })
    servicesApi.list(token, filters, page, controller.signal).then((result) => {
      if (controller.signal.aborted) return
      if (!Array.isArray(result?.items) || !Number.isInteger(result.totalItems)) {
        setList({ status: 'error', items: [], totalItems: 0, error: 'Máy chủ chưa trả danh sách dịch vụ.' })
        return
      }
      const lastPage = Math.max(1, Math.ceil(result.totalItems / 10))
      if (page > lastPage) { setPage(lastPage); return }
      setList({ status: 'success', items: result.items, totalItems: result.totalItems, error: '' })
    }).catch((error) => {
      if (!controller.signal.aborted) setList({ status: 'error', items: [], totalItems: 0,
        error: error.status === 0 ? error.message : 'Chưa tải được danh sách dịch vụ.' })
    })
    return () => controller.abort()
  }, [token, filters, page, reload])

  function openForm(service = null) {
    setEditing(service || 'new')
    setForm(service ? {
      tenDichVu: service.tenDichVu,
      moTa: service.moTa || '',
      donGia: String(service.donGia),
      thoiLuongDuKienPhut: String(service.thoiLuongDuKienPhut),
    } : emptyForm)
    setFormError('')
  }

  async function save(event) {
    event.preventDefault()
    if (formBusy) return
    const name = form.tenDichVu.trim()
    const price = Number(form.donGia)
    const minutes = Number(form.thoiLuongDuKienPhut)
    if (!name || !Number.isFinite(price) || price < 0 || !Number.isInteger(minutes) || minutes < 1 || minutes > 65535) {
      setFormError('Vui lòng nhập tên, đơn giá không âm và thời lượng từ 1 đến 65.535 phút.')
      return
    }
    setFormBusy(true)
    setFormError('')
    try {
      const payload = { tenDichVu: name, moTa: form.moTa.trim(), donGia: price, thoiLuongDuKienPhut: minutes }
      if (editing === 'new') await servicesApi.create(token, payload)
      else await servicesApi.update(token, payload)
      setNotice(editing === 'new' ? `Đã thêm dịch vụ ${name}.` : `Đã cập nhật dịch vụ ${name}.`)
      setEditing(null)
      setReload((value) => value + 1)
    } catch (error) {
      setFormError([0, 400, 404, 409].includes(error.status) ? error.message : 'Chưa lưu được dịch vụ. Vui lòng thử lại.')
    } finally {
      setFormBusy(false)
    }
  }

  async function changeStatus() {
    if (!statusTarget || statusBusy) return
    setStatusBusy(true)
    setStatusError('')
    try {
      await servicesApi.setActive(token, statusTarget.maDichVu, statusTarget.trangThai !== 'Hoạt động')
      setNotice(statusTarget.trangThai === 'Hoạt động' ? `Đã ngừng dịch vụ ${statusTarget.tenDichVu}.` : `Đã khôi phục dịch vụ ${statusTarget.tenDichVu}.`)
      setStatusTarget(null)
      setReload((value) => value + 1)
    } catch (error) {
      setStatusError([0, 400, 404].includes(error.status) ? error.message : 'Chưa đổi được trạng thái dịch vụ. Vui lòng thử lại.')
    } finally {
      setStatusBusy(false)
    }
  }

  return <div className="manager-services">
    <div className="page-heading manager-services-heading"><div><h1>Dịch vụ</h1><p className="page-description">Quản lý giá và thời lượng dự kiến của dịch vụ.</p></div>
      <button type="button" className="primary-button" onClick={() => openForm()}>Thêm dịch vụ</button></div>
    {editing && <section className="manager-services-form-card"><div className="manager-services-form-heading"><h2>{editing === 'new' ? 'Thêm dịch vụ' : `Sửa ${editing.tenDichVu}`}</h2>
      <button type="button" className="secondary-button" disabled={formBusy} onClick={() => setEditing(null)}>Đóng</button></div>
      <form className="manager-services-form" onSubmit={save}>
        <label>Tên dịch vụ<input required maxLength={150} value={form.tenDichVu} readOnly={editing !== 'new'} onChange={(event) => setForm({ ...form, tenDichVu: event.target.value })} /></label>
        <label>Đơn giá (đ)<input required type="number" min="0" step="0.01" value={form.donGia} onChange={(event) => setForm({ ...form, donGia: event.target.value })} /></label>
        <label>Thời lượng dự kiến (phút)<input required type="number" min="1" max="65535" value={form.thoiLuongDuKienPhut} onChange={(event) => setForm({ ...form, thoiLuongDuKienPhut: event.target.value })} /></label>
        <label className="manager-services-description">Mô tả<textarea rows="3" value={form.moTa} onChange={(event) => setForm({ ...form, moTa: event.target.value })} /></label>
        {editing !== 'new' && <p className="manager-services-hint">API hiện tại chỉ cho sửa mô tả, giá và thời lượng; tên dịch vụ được giữ nguyên.</p>}
        {formError && <p className="manager-services-error" role="alert">{formError}</p>}
        <button type="submit" className="primary-button" disabled={formBusy}>{formBusy ? 'Đang lưu…' : 'Lưu dịch vụ'}</button>
      </form>
    </section>}
    <form className="manager-services-filters" onSubmit={(event) => { event.preventDefault(); setFilters({ name: nameInput.trim(), status: statusInput }); setPage(1) }}>
      <label>Tên dịch vụ<input type="search" value={nameInput} onChange={(event) => setNameInput(event.target.value)} placeholder="Tìm dịch vụ" /></label>
      <label>Trạng thái<select value={statusInput} onChange={(event) => setStatusInput(event.target.value)}><option value="">Tất cả</option><option value="DANG_HOAT_DONG">Hoạt động</option><option value="NGUNG_HOAT_DONG">Ngừng hoạt động</option></select></label>
      <button type="submit" className="secondary-button">Lọc</button>
      {(filters.name || filters.status) && <button type="button" className="manager-services-clear" onClick={() => { setNameInput(''); setStatusInput(''); setFilters({ name: '', status: '' }); setPage(1) }}>Xóa bộ lọc</button>}
    </form>
    {list.status === 'loading' && <div className="manager-services-feedback" role="status">Đang tải dịch vụ…</div>}
    {list.status === 'error' && <div className="manager-services-feedback" role="alert"><p>{list.error}</p><button type="button" className="secondary-button" onClick={() => setReload((value) => value + 1)}>Thử lại</button></div>}
    {list.status === 'success' && <><p className="manager-services-count">{list.totalItems} dịch vụ</p>
      {list.items.length === 0 && <div className="manager-services-feedback">Không có dịch vụ phù hợp.</div>}
      <div className="manager-services-list">{list.items.map((service) => <article key={service.maDichVu}>
        <div><span>{service.maDichVu}</span><h2>{service.tenDichVu}</h2><p>{service.moTa || 'Chưa có mô tả'}</p><small>{money.format(service.donGia)} · {service.thoiLuongDuKienPhut} phút · <span className={`manager-services-state ${service.trangThai === 'Hoạt động' ? 'manager-services-state-active' : ''}`}>{service.trangThai}</span></small></div>
        <div className="manager-services-actions"><button type="button" className="secondary-button" onClick={() => openForm(service)}>Sửa</button>
          <button type="button" className="secondary-button" onClick={() => { setStatusTarget(service); setStatusError('') }}>{service.trangThai === 'Hoạt động' ? 'Ngừng hoạt động' : 'Khôi phục'}</button></div>
      </article>)}</div>
      <ListPagination page={page} pageSize={10} totalItems={list.totalItems} onPageChange={setPage} />
    </>}
    {statusTarget && <div className="dialog-backdrop" onMouseDown={(event) => { if (event.target === event.currentTarget && !statusBusy) setStatusTarget(null) }}><section className="confirm-dialog manager-services-status" role="alertdialog" aria-modal="true" aria-labelledby="manager-services-status-title">
      <h2 id="manager-services-status-title">{statusTarget.trangThai === 'Hoạt động' ? 'Ngừng' : 'Khôi phục'} dịch vụ?</h2><p>{statusTarget.tenDichVu}</p>
      {statusError && <p className="manager-services-error" role="alert">{statusError}</p>}
      <div className="confirm-actions"><button type="button" className="secondary-button" disabled={statusBusy} onClick={() => setStatusTarget(null)}>Quay lại</button><button type="button" className="primary-button" disabled={statusBusy} onClick={changeStatus}>{statusBusy ? 'Đang cập nhật…' : 'Xác nhận'}</button></div>
    </section></div>}
    <ToastNotice message={notice} onDismiss={() => setNotice('')} />
  </div>
}
