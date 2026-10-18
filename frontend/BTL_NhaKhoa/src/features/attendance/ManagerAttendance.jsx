import { useEffect, useState } from 'react'
import ListPagination from '../../components/ListPagination'
import ToastNotice from '../../components/ToastNotice'
import { attendanceApi } from './attendanceApi'
import '../../components/ConfirmDialog.css'
import './ManagerAttendance.css'

const reviewLabels = {
  CHUA_CO_DU_LIEU: 'Chưa có dữ liệu', CHUA_HOAN_TAT: 'Chưa hoàn tất',
  CHO_DUYET: 'Chờ duyệt', DA_DUYET: 'Đã duyệt', TU_CHOI: 'Bị từ chối',
}
const periodLabels = {
  CHUA_XAC_DINH: 'Chưa xác định', LAM_VIEC: 'Làm việc',
  NGHI_PHEP: 'Nghỉ phép', VANG_KHONG_PHEP: 'Vắng không phép',
}
const reviewTones = { CHUA_CO_DU_LIEU: 'neutral', CHUA_HOAN_TAT: 'pending', CHO_DUYET: 'processing', DA_DUYET: 'success', TU_CHOI: 'error' }
const periodTones = { CHUA_XAC_DINH: 'neutral', LAM_VIEC: 'success', NGHI_PHEP: 'pending', VANG_KHONG_PHEP: 'error' }
function ReviewState({ value }) { return <span className={`manager-attendance-state manager-attendance-state-${reviewTones[value] || 'neutral'}`}>{reviewLabels[value] || value}</span> }
const tabs = [
  { id: 'pending', label: 'Chờ duyệt', status: 'CHO_DUYET' },
  { id: 'incomplete', label: 'Chưa hoàn tất', status: 'CHUA_HOAN_TAT' },
  { id: 'all', label: 'Tất cả', status: '' },
  { id: 'day', label: 'Theo ngày' },
]
const dateTimeFormatter = new Intl.DateTimeFormat('vi-VN', { dateStyle: 'short', timeStyle: 'short' })

function todayLocal() {
  const date = new Date()
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`
}

function time(value) { return value?.slice(11, 16) || '—' }
function dateTime(value) {
  if (!value) return '—'
  const parsed = new Date(value)
  return Number.isNaN(parsed.getTime()) ? '—' : dateTimeFormatter.format(parsed)
}
function minutes(value) { return `${Math.floor((value || 0) / 60)} giờ ${(value || 0) % 60} phút` }

function Period({ record, period, onEdit }) {
  const morning = period === 'SANG'
  const state = morning ? record.tinhTrangSang : record.tinhTrangChieu
  const start = morning ? record.gioVaoSang : record.gioVaoChieu
  const end = morning ? record.gioRaSang : record.gioRaChieu
  const reason = morning ? record.lyDoSang : record.lyDoChieu
  return <div className="manager-attendance-period">
    <div><strong>Buổi {morning ? 'sáng' : 'chiều'}</strong><span className={`manager-attendance-state manager-attendance-state-${periodTones[state] || 'neutral'}`}>{periodLabels[state] || state}</span></div>
    <p>Vào {time(start)} · Ra {time(end)}</p>
    {reason && <small>Lý do: {reason}</small>}
    {record.trangThaiDuyet !== 'DA_DUYET' && record.trangThaiDuyet !== 'TU_CHOI' &&
      <button type="button" className="secondary-button" onClick={() => onEdit(record, period)}>Chỉnh sửa</button>}
  </div>
}

function Editor({ target, busy, error, onClose, onSave }) {
  const { record, period, create } = target
  const morning = period === 'SANG'
  const oldState = create ? 'CHUA_XAC_DINH' : morning ? record.tinhTrangSang : record.tinhTrangChieu
  const oldIn = create ? '' : morning ? record.gioVaoSang : record.gioVaoChieu
  const oldOut = create ? '' : morning ? record.gioRaSang : record.gioRaChieu
  const hasHours = Boolean(oldIn || oldOut)
  const wasAbsent = ['NGHI_PHEP', 'VANG_KHONG_PHEP'].includes(oldState)
  const [kind, setKind] = useState(wasAbsent ? oldState : 'LAM_VIEC')
  const [start, setStart] = useState(oldIn?.slice(11, 16) || '')
  const [end, setEnd] = useState(oldOut?.slice(11, 16) || '')
  const [reason, setReason] = useState('')
  const [localError, setLocalError] = useState('')

  function review(event) {
    event.preventDefault()
    if (!reason.trim()) { setLocalError('Vui lòng nhập lý do chỉnh sửa.'); return }
    if (kind === 'LAM_VIEC' && !start) { setLocalError('Vui lòng nhập giờ vào.'); return }
    if (kind === 'LAM_VIEC' && end && end < start) { setLocalError('Giờ ra không được trước giờ vào.'); return }
    setLocalError('')
    onSave({
      buoi: period,
      gioVao: kind === 'LAM_VIEC' ? `${record.ngayChamCong}T${start}:00` : null,
      gioRa: kind === 'LAM_VIEC' && end ? `${record.ngayChamCong}T${end}:00` : null,
      tinhTrang: kind === 'LAM_VIEC' ? null : kind,
      lyDo: reason.trim(),
    })
  }

  return <div className="dialog-backdrop"><section className="confirm-dialog manager-attendance-editor" role="dialog" aria-modal="true" aria-labelledby="manager-attendance-editor-title">
    <h2 id="manager-attendance-editor-title">{create ? 'Bổ sung chấm công' : 'Chỉnh sửa chấm công'}</h2>
    <p>{record.hoTen} · {record.ngayChamCong} · Buổi {morning ? 'sáng' : 'chiều'}</p>
    <form noValidate onSubmit={review}>
      <label>Tình trạng<select value={kind} onChange={(event) => { setKind(event.target.value); setLocalError('') }}>
        {!wasAbsent && <option value="LAM_VIEC">Làm việc</option>}
        {!hasHours && <><option value="NGHI_PHEP">Nghỉ phép</option><option value="VANG_KHONG_PHEP">Vắng không phép</option></>}
      </select></label>
      {kind === 'LAM_VIEC' && <div className="manager-attendance-time-fields">
        <label>Giờ vào<input type="time" value={start} onChange={(event) => { setStart(event.target.value); setLocalError('') }} /></label>
        <label>Giờ ra (nếu có)<input type="time" value={end} onChange={(event) => { setEnd(event.target.value); setLocalError('') }} /></label>
      </div>}
      <label>Lý do<textarea rows="3" maxLength={500} value={reason} onChange={(event) => { setReason(event.target.value); setLocalError('') }} placeholder="Nêu lý do bổ sung hoặc chỉnh sửa" /></label>
      {(localError || error) && <p className="manager-attendance-error" role="alert">{localError || error}</p>}
      <div className="confirm-actions"><button type="button" className="secondary-button" disabled={busy} onClick={onClose}>Đóng</button>
        <button type="submit" className="primary-button" disabled={busy}>{busy ? 'Đang lưu…' : 'Lưu chấm công'}</button></div>
    </form>
  </section></div>
}

export default function ManagerAttendance({ token }) {
  const [tab, setTab] = useState('pending')
  const [page, setPage] = useState(1)
  const [reload, setReload] = useState(0)
  const [list, setList] = useState({ status: 'loading', items: [], totalItems: 0, error: '' })
  const [employeeInput, setEmployeeInput] = useState('')
  const [fromInput, setFromInput] = useState('')
  const [toInput, setToInput] = useState('')
  const [filters, setFilters] = useState({ employee: '', fromDate: '', toDate: '' })
  const [filterError, setFilterError] = useState('')
  const [dayInput, setDayInput] = useState(todayLocal)
  const [day, setDay] = useState(todayLocal)
  const [daily, setDaily] = useState({ status: 'loading', items: [], error: '' })
  const [detailId, setDetailId] = useState('')
  const [detail, setDetail] = useState({ status: 'idle', data: null, error: '' })
  const [editor, setEditor] = useState(null)
  const [editError, setEditError] = useState('')
  const [editBusy, setEditBusy] = useState(false)
  const [action, setAction] = useState(null)
  const [actionError, setActionError] = useState('')
  const [actionBusy, setActionBusy] = useState(false)
  const [notice, setNotice] = useState('')

  useEffect(() => {
    if (tab === 'day') return undefined
    const controller = new AbortController()
    queueMicrotask(() => { if (!controller.signal.aborted) setList({ status: 'loading', items: [], totalItems: 0, error: '' }) })
    const status = tabs.find((item) => item.id === tab)?.status || ''
    attendanceApi.managerList(token, page, { ...filters, status }, controller.signal).then((result) => {
      if (controller.signal.aborted) return
      if (!Array.isArray(result?.items) || !Number.isInteger(result.totalItems)) {
        setList({ status: 'error', items: [], totalItems: 0, error: 'Máy chủ chưa trả danh sách chấm công.' })
        return
      }
      const lastPage = Math.max(1, Math.ceil(result.totalItems / 10))
      if (page > lastPage) { setPage(lastPage); return }
      setList({ status: 'success', items: result.items, totalItems: result.totalItems, error: '' })
    }).catch((error) => {
      if (!controller.signal.aborted) setList({ status: 'error', items: [], totalItems: 0,
        error: error.status === 0 ? error.message : 'Chưa tải được danh sách chấm công.' })
    })
    return () => controller.abort()
  }, [token, tab, page, filters, reload])

  useEffect(() => {
    if (tab !== 'day') return undefined
    const controller = new AbortController()
    queueMicrotask(() => { if (!controller.signal.aborted) setDaily({ status: 'loading', items: [], error: '' }) })
    attendanceApi.managerDay(token, day, controller.signal).then((items) => {
      if (!controller.signal.aborted) setDaily(Array.isArray(items)
        ? { status: 'success', items, error: '' }
        : { status: 'error', items: [], error: 'Máy chủ chưa trả tổng hợp ngày.' })
    }).catch((error) => {
      if (!controller.signal.aborted) setDaily({ status: 'error', items: [],
        error: error.status === 0 ? error.message : 'Chưa tải được tổng hợp ngày.' })
    })
    return () => controller.abort()
  }, [token, tab, day, reload])

  useEffect(() => {
    if (!detailId) return undefined
    const controller = new AbortController()
    queueMicrotask(() => { if (!controller.signal.aborted) setDetail({ status: 'loading', data: null, error: '' }) })
    attendanceApi.managerDetail(token, detailId, controller.signal).then((data) => {
      if (!controller.signal.aborted) setDetail({ status: 'success', data, error: '' })
    }).catch((error) => {
      if (!controller.signal.aborted) setDetail({ status: 'error', data: null,
        error: error.status === 0 ? error.message : 'Chưa tải được chi tiết chấm công.' })
    })
    return () => controller.abort()
  }, [token, detailId, reload])

  function chooseTab(value) {
    setTab(value)
    setPage(1)
    setDetailId('')
  }

  function applyFilters(event) {
    event.preventDefault()
    if (fromInput && toInput && fromInput > toInput) { setFilterError('Từ ngày không được sau đến ngày.'); return }
    setFilterError('')
    setPage(1)
    setFilters({ employee: employeeInput.trim().toUpperCase(), fromDate: fromInput, toDate: toInput })
  }

  function openEditor(record, period, create = false) {
    setEditError('')
    setEditor({ record, period, create })
  }

  async function saveEdit(payload) {
    if (!editor || editBusy) return
    setEditBusy(true)
    setEditError('')
    try {
      const saved = editor.create ? await attendanceApi.managerCreate(token, {
        ...payload, maNhanVien: editor.record.maNhanVien, ngayChamCong: editor.record.ngayChamCong,
      }) : await attendanceApi.managerEdit(token, editor.record.maChamCong, payload)
      setNotice(editor.create ? 'Đã bổ sung bản chấm công.' : 'Đã cập nhật chấm công.')
      if (editor.create && saved?.maChamCong) setDetailId(saved.maChamCong)
      setEditor(null)
      setReload((value) => value + 1)
    } catch (error) {
      setEditError([0, 400, 404, 409].includes(error.status) ? error.message : 'Chưa lưu được chấm công. Vui lòng thử lại.')
    } finally {
      setEditBusy(false)
    }
  }

  async function runAction() {
    if (!action || actionBusy) return
    if (action.type === 'reject' && !action.reason.trim()) { setActionError('Vui lòng nhập lý do từ chối.'); return }
    setActionBusy(true)
    setActionError('')
    try {
      if (action.type === 'approve') await attendanceApi.managerApprove(token, action.record.maChamCong)
      else await attendanceApi.managerReject(token, action.record.maChamCong, action.reason.trim())
      setNotice(action.type === 'approve' ? 'Đã duyệt chấm công.' : 'Đã từ chối chấm công.')
      setAction(null)
      setReload((value) => value + 1)
    } catch (error) {
      setActionError([0, 400, 404, 409].includes(error.status) ? error.message : 'Chưa xử lý được chấm công. Vui lòng thử lại.')
    } finally {
      setActionBusy(false)
    }
  }

  const record = detail.data
  return <div className="manager-attendance">
    <div className="page-heading"><div><h1>Chấm công</h1><p className="page-description">Kiểm tra, bổ sung và duyệt chấm công Lễ tân, Bác sĩ.</p></div></div>
    <div className="manager-attendance-tabs" role="group" aria-label="Nhóm chấm công">{tabs.map((item) => <button key={item.id} type="button" className={tab === item.id ? 'active' : ''} aria-pressed={tab === item.id} onClick={() => chooseTab(item.id)}>{item.label}</button>)}</div>
    {tab === 'day' ? <section className="manager-attendance-day">
      <form className="manager-attendance-filters" onSubmit={(event) => { event.preventDefault(); setDay(dayInput) }}><label>Ngày chấm công<input type="date" value={dayInput} onChange={(event) => setDayInput(event.target.value)} /></label><button type="submit" className="secondary-button" disabled={!dayInput}>Xem ngày</button></form>
      {daily.status === 'loading' && <p role="status">Đang tải tổng hợp ngày…</p>}
      {daily.status === 'error' && <div role="alert"><p>{daily.error}</p><button type="button" className="secondary-button" onClick={() => setReload((value) => value + 1)}>Thử lại</button></div>}
      {daily.status === 'success' && <><p>{daily.items.length} nhân viên đang hoạt động · {day}</p>
        {daily.items.length === 0 && <p>Không có Lễ tân hoặc Bác sĩ đang hoạt động.</p>}
        <div className="manager-attendance-list">{daily.items.map((item) => <article key={item.maNhanVien}>
          <div><span>{item.maNhanVien} · {item.vaiTro === 'BAC_SI' ? 'Bác sĩ' : 'Lễ tân'}</span><h2>{item.hoTen}</h2><ReviewState value={item.trangThai} /></div>
          {item.chamCong ? <button type="button" className="secondary-button" onClick={() => setDetailId(item.chamCong.maChamCong)}>Chi tiết</button>
            : <div className="manager-attendance-missing-actions"><button type="button" className="secondary-button" onClick={() => openEditor(item, 'SANG', true)}>Bổ sung sáng</button><button type="button" className="secondary-button" onClick={() => openEditor(item, 'CHIEU', true)}>Bổ sung chiều</button></div>}
        </article>)}</div>
      </>}
    </section> : <>
      <form className="manager-attendance-filters" onSubmit={applyFilters} noValidate>
        <label>Mã nhân viên<input value={employeeInput} onChange={(event) => setEmployeeInput(event.target.value)} placeholder="NV_0001" /></label>
        <label>Từ ngày<input type="date" value={fromInput} onChange={(event) => setFromInput(event.target.value)} /></label>
        <label>Đến ngày<input type="date" value={toInput} onChange={(event) => setToInput(event.target.value)} /></label>
        <button type="submit" className="secondary-button">Lọc</button>
        {(filters.employee || filters.fromDate || filters.toDate) && <button type="button" className="manager-attendance-clear" onClick={() => { setEmployeeInput(''); setFromInput(''); setToInput(''); setFilters({ employee: '', fromDate: '', toDate: '' }); setFilterError(''); setPage(1) }}>Xóa bộ lọc</button>}
      </form>
      {filterError && <p className="manager-attendance-error" role="alert">{filterError}</p>}
      {list.status === 'loading' && <div className="manager-attendance-feedback" role="status">Đang tải chấm công…</div>}
      {list.status === 'error' && <div className="manager-attendance-feedback" role="alert"><p>{list.error}</p><button type="button" className="secondary-button" onClick={() => setReload((value) => value + 1)}>Thử lại</button></div>}
      {list.status === 'success' && <><p className="manager-attendance-count">{list.totalItems} bản chấm công</p>
        {list.items.length === 0 && <div className="manager-attendance-feedback">Không có bản chấm công phù hợp.</div>}
        <div className="manager-attendance-list">{list.items.map((item) => <article key={item.maChamCong}>
          <div><span>{item.maChamCong} · {item.maNhanVien}</span><h2>{item.hoTen} · {item.ngayChamCong}</h2><p><ReviewState value={item.trangThaiDuyet} /> · {minutes(item.tongPhutLamViec)}</p></div>
          <button type="button" className="secondary-button" onClick={() => setDetailId(item.maChamCong)}>Chi tiết</button>
        </article>)}</div>
        <ListPagination page={page} pageSize={10} totalItems={list.totalItems} onPageChange={setPage} />
      </>}
    </>}
    {detailId && <section className="manager-attendance-detail" aria-labelledby="manager-attendance-detail-title">
      <div className="manager-attendance-detail-heading"><h2 id="manager-attendance-detail-title">Chi tiết {detailId}</h2><button type="button" className="secondary-button" onClick={() => setDetailId('')}>Đóng</button></div>
      {detail.status === 'loading' && <p role="status">Đang tải chi tiết…</p>}
      {detail.status === 'error' && <div role="alert"><p>{detail.error}</p><button type="button" className="secondary-button" onClick={() => setReload((value) => value + 1)}>Thử lại</button></div>}
      {detail.status === 'success' && record && <>
        <p>{record.hoTen} · {record.maNhanVien} · {record.ngayChamCong} · <ReviewState value={record.trangThaiDuyet} /></p>
        <div className="manager-attendance-periods"><Period record={record} period="SANG" onEdit={openEditor} /><Period record={record} period="CHIEU" onEdit={openEditor} /></div>
        <p>Tổng giờ làm: {minutes(record.tongPhutLamViec)}</p>
        {(record.soPhutTreSang > 0 || record.soPhutTreChieu > 0 || record.soPhutVeSomSang > 0 || record.soPhutVeSomChieu > 0) && <p>Đi trễ: sáng {record.soPhutTreSang} phút, chiều {record.soPhutTreChieu} phút · Về sớm: sáng {record.soPhutVeSomSang} phút, chiều {record.soPhutVeSomChieu} phút.</p>}
        {record.lyDoTuChoi && <p>Lý do từ chối: {record.lyDoTuChoi} · {record.nguoiTuChoi || 'Chưa rõ người từ chối'} · {dateTime(record.tuChoiLuc)}</p>}
        {record.nguoiDuyet && <p>Đã duyệt bởi {record.nguoiDuyet} lúc {dateTime(record.duyetLuc)}.</p>}
        {record.trangThaiDuyet !== 'DA_DUYET' && record.trangThaiDuyet !== 'TU_CHOI' && <div className="manager-attendance-detail-actions">
          {record.trangThaiDuyet === 'CHO_DUYET' && <button type="button" className="primary-button" onClick={() => { setAction({ type: 'approve', record }); setActionError('') }}>Duyệt</button>}
          <button type="button" className="secondary-button" onClick={() => { setAction({ type: 'reject', record, reason: '' }); setActionError('') }}>Từ chối</button>
        </div>}
        <section className="manager-attendance-changes"><h3>Lịch sử chỉnh sửa</h3>
          {record.lichSuChinhSua?.length ? <ul>{record.lichSuChinhSua.map((change, index) => <li key={`${change.thoiDiemSua}-${index}`}>
            <strong>{dateTime(change.thoiDiemSua)} · {change.nguoiSua}</strong>
            <span>Buổi {change.buoi === 'SANG' ? 'sáng' : 'chiều'}: {periodLabels[change.tinhTrangCu] || 'Chưa xác định'} → {periodLabels[change.tinhTrangMoi] || 'Chưa xác định'}; {time(change.gioVaoCu)}–{time(change.gioRaCu)} → {time(change.gioVaoMoi)}–{time(change.gioRaMoi)}</span>
            <small>Lý do: {change.lyDo}</small></li>)}</ul> : <p>Chưa có chỉnh sửa thủ công.</p>}
        </section>
      </>}
    </section>}
    {editor && <Editor key={`${editor.record.maChamCong || editor.record.maNhanVien}-${editor.period}`} target={editor} busy={editBusy} error={editError} onClose={() => setEditor(null)} onSave={saveEdit} />}
    {action?.type === 'approve' && <div className="dialog-backdrop"><section className="confirm-dialog manager-attendance-reject" role="alertdialog" aria-modal="true" aria-labelledby="manager-attendance-approve-title">
      <h2 id="manager-attendance-approve-title">Duyệt chấm công?</h2><p>{action.record.hoTen} · {action.record.ngayChamCong}</p>
      {actionError && <p className="manager-attendance-error" role="alert">{actionError}</p>}
      <div className="confirm-actions"><button type="button" className="secondary-button" disabled={actionBusy} onClick={() => setAction(null)}>Đóng</button><button type="button" className="primary-button" disabled={actionBusy} onClick={runAction}>{actionBusy ? 'Đang duyệt…' : 'Duyệt'}</button></div>
    </section></div>}
    {action?.type === 'reject' && <div className="dialog-backdrop"><section className="confirm-dialog manager-attendance-reject" role="dialog" aria-modal="true" aria-labelledby="manager-attendance-reject-title">
      <h2 id="manager-attendance-reject-title">Từ chối chấm công?</h2><p>{action.record.hoTen} · {action.record.ngayChamCong}</p>
      <label>Lý do từ chối<textarea rows="3" maxLength={500} value={action.reason} onChange={(event) => { setAction({ ...action, reason: event.target.value }); setActionError('') }} /></label>
      {actionError && <p className="manager-attendance-error" role="alert">{actionError}</p>}
      <div className="confirm-actions"><button type="button" className="secondary-button" disabled={actionBusy} onClick={() => setAction(null)}>Đóng</button><button type="button" className="primary-button" disabled={actionBusy} onClick={runAction}>{actionBusy ? 'Đang xử lý…' : 'Từ chối'}</button></div>
    </section></div>}
    <ToastNotice message={notice} onDismiss={() => setNotice('')} />
  </div>
}
