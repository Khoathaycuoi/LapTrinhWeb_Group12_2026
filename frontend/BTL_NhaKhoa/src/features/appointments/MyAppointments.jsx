import { useEffect, useState } from 'react'
import { appointmentsApi } from './appointmentsApi'
import ListPagination from '../../components/ListPagination'
import ToastNotice from '../../components/ToastNotice'
import '../../components/ConfirmDialog.css'
import './MyAppointments.css'

const groups = [
  ['Tất cả', 'all'],
  ['Sắp tới', 'upcoming'],
  ['Đang diễn ra', 'active'],
  ['Đã kết thúc', 'finished'],
]

const upcomingStatuses = ['Chờ xác nhận', 'Đã xác nhận']
const activeStatuses = ['Đã đến', 'Đang khám']
const finishedStatuses = ['Đã hoàn thành', 'Đã hủy', 'Không đến', 'Từ chối', 'Hết hạn']
const groupStatuses = {
  upcoming: upcomingStatuses,
  active: activeStatuses,
  finished: finishedStatuses,
}
const pageSize = 10

const statusTones = {
  'Chờ xác nhận': 'pending',
  'Đã xác nhận': 'confirmed',
  'Đã đến': 'processing',
  'Đang khám': 'processing',
  'Đã hoàn thành': 'complete',
  'Đã hủy': 'muted',
  'Không đến': 'error',
  'Từ chối': 'error',
  'Hết hạn': 'muted',
}

const dateFormatter = new Intl.DateTimeFormat('vi-VN', { dateStyle: 'full' })
const moneyFormatter = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND', maximumFractionDigits: 0 })

function formatAppointmentDate(date, time) {
  if (!date) return 'Chưa rõ ngày hẹn'
  const parsed = new Date(`${date}T${time || '00:00:00'}`)
  if (Number.isNaN(parsed.getTime())) return `${date} ${time || ''}`.trim()
  return `${dateFormatter.format(parsed)}, ${time?.slice(0, 5) || '—'}`
}

function canCancel(appointment) {
  if (!upcomingStatuses.includes(appointment.trangThai)) return false
  const start = new Date(`${appointment.ngayHen}T${appointment.gioHen}`)
  if (Number.isNaN(start.getTime())) return false
  const timeLeft = start.getTime() - Date.now()
  return appointment.trangThai === 'Chờ xác nhận'
    ? timeLeft > 0
    : timeLeft >= 12 * 60 * 60 * 1000
}

function CancelDialog({ appointment, reason, error, busy, onReasonChange, onCancel, onConfirm }) {
  useEffect(() => {
    const onKeyDown = (event) => {
      if (event.key === 'Escape' && !busy) onCancel()
    }
    window.addEventListener('keydown', onKeyDown)
    return () => window.removeEventListener('keydown', onKeyDown)
  }, [busy, onCancel])

  return <div className="dialog-backdrop" onMouseDown={(event) => { if (event.target === event.currentTarget && !busy) onCancel() }}>
    <form className="confirm-dialog appointment-cancel-dialog" role="dialog" aria-modal="true" aria-labelledby="cancel-appointment-title" onSubmit={onConfirm}>
      <h2 id="cancel-appointment-title">Hủy lịch hẹn {appointment.maLichHen}?</h2>
      <p>{appointment.trangThai === 'Chờ xác nhận'
        ? 'Lịch chờ xác nhận có thể hủy trước giờ hẹn.'
        : 'Lịch đã xác nhận chỉ có thể tự hủy trước giờ hẹn ít nhất 12 tiếng.'}</p>
      <label htmlFor="cancel-reason">Lý do hủy <span aria-hidden="true">*</span></label>
      <textarea id="cancel-reason" autoFocus required maxLength={2000} rows={3} value={reason} onChange={(event) => onReasonChange(event.target.value)} placeholder="Nhập lý do hủy lịch hẹn" />
      {error && <div className="alert alert-error" role="alert">{error}</div>}
      <div className="confirm-actions">
        <button type="button" className="secondary-button" disabled={busy} onClick={onCancel}>Giữ lịch</button>
        <button type="submit" className="primary-button" disabled={busy || !reason.trim()}>{busy ? 'Đang hủy…' : 'Xác nhận hủy'}</button>
      </div>
    </form>
  </div>
}

function AppointmentCard({ appointment, expanded, onToggle, onCancel }) {
  const services = appointment.dichVusDuKien ?? []
  const cancellableStatus = ['Chờ xác nhận', 'Đã xác nhận'].includes(appointment.trangThai)
  const hasDetails = Boolean(appointment.maPhong || appointment.lyDoHuy || appointment.lyDoTuChoi ||
    (appointment.ghi_chu && !appointment.lyDoHuy) || services.length)
  const estimatedTotal = services.reduce((sum, service) => sum + Number(service.donGiaTaiThoiDiemDat || 0), 0)

  return <article className="appointment-card">
    <div className="appointment-card-top">
      <div><span className="appointment-code">{appointment.maLichHen}</span><h2>{formatAppointmentDate(appointment.ngayHen, appointment.gioHen)}</h2></div>
      <span className={`appointment-status appointment-status-${statusTones[appointment.trangThai] || 'muted'}`}>{appointment.trangThai}</span>
    </div>
    <div className="appointment-summary">
      <div><span>Bác sĩ</span><strong>{appointment.tenBacSi || 'Chưa phân công'}</strong></div>
      <div><span>Dịch vụ dự kiến</span><strong>{services.length ? services.map((service) => service.tenDichVu).join(', ') : 'Chưa có thông tin'}</strong></div>
      <div><span>Thời lượng dự kiến</span><strong>{appointment.thoiLuongDuKienPhut} phút</strong></div>
    </div>
    <div className="appointment-card-actions">
      {hasDetails && <button type="button" className="appointment-text-button" aria-expanded={expanded} onClick={onToggle}>{expanded ? 'Thu gọn' : 'Chi tiết dịch vụ và lịch'}</button>}
      {canCancel(appointment) && <button type="button" className="appointment-cancel-button" onClick={onCancel}>Hủy lịch</button>}
      {cancellableStatus && !canCancel(appointment) && <span className="appointment-cancel-note">{appointment.trangThai === 'Chờ xác nhận'
        ? 'Lịch chờ đã qua giờ hẹn và không thể hủy.'
        : 'Lịch đã xác nhận không thể tự hủy trong vòng 12 giờ trước giờ hẹn. Vui lòng liên hệ Lễ tân.'}</span>}
    </div>
    {expanded && hasDetails && <div className="appointment-details">
      {(appointment.maPhong || appointment.lyDoHuy || appointment.lyDoTuChoi || (appointment.ghi_chu && !appointment.lyDoHuy)) && <dl>
        {appointment.maPhong && <div><dt>Phòng khám</dt><dd>{appointment.maPhong}</dd></div>}
        {appointment.ghi_chu && !appointment.lyDoHuy && <div><dt>Ghi chú</dt><dd className="appointment-note">{appointment.ghi_chu}</dd></div>}
        {appointment.lyDoHuy && <div><dt>Lý do hủy</dt><dd className="appointment-note">{appointment.lyDoHuy}</dd></div>}
        {appointment.lyDoTuChoi && <div><dt>Lý do từ chối</dt><dd className="appointment-note">{appointment.lyDoTuChoi}</dd></div>}
      </dl>}
      {services.length > 0 && <div className="appointment-services"><h3>Chi phí dự kiến khi đặt</h3><ul>{services.map((service) => <li key={service.maDichVu}><span>{service.tenDichVu} · {service.thoiLuongTaiThoiDiemDatPhut} phút</span><span>{moneyFormatter.format(service.donGiaTaiThoiDiemDat)}</span></li>)}</ul><div className="appointment-estimated-total"><span>Tổng dự kiến</span><strong>{moneyFormatter.format(estimatedTotal)}</strong></div><p>Hóa đơn thực tế chỉ tính các dịch vụ đã thực hiện.</p></div>}
    </div>}
  </article>
}

export default function MyAppointments({ token, groupParam, statusParam, arrivalNotice, onGroupChange }) {
  const group = groups.some(([, value]) => value === groupParam) ? groupParam : 'all'
  const [specificStatus, setSpecificStatus] = useState(statusParam ?? '')
  const [date, setDate] = useState('')
  const [page, setPage] = useState(1)
  const [reload, setReload] = useState(0)
  const [state, setState] = useState({ status: 'loading', items: [], totalItems: 0, error: '' })
  const [expandedId, setExpandedId] = useState(null)
  const [cancelTarget, setCancelTarget] = useState(null)
  const [cancelReason, setCancelReason] = useState('')
  const [cancelError, setCancelError] = useState('')
  const [cancelling, setCancelling] = useState(false)
  const [notice, setNotice] = useState(arrivalNotice ?? '')
  const selectedStatus = (groupStatuses[group] ?? []).includes(specificStatus) ? specificStatus : ''

  useEffect(() => { setPage(1) }, [group])
  useEffect(() => { setSpecificStatus(statusParam ?? ''); setPage(1) }, [statusParam])

  useEffect(() => {
    const controller = new AbortController()
    let active = true
    setState({ status: 'loading', items: [], totalItems: 0, error: '' })
    appointmentsApi.myAppointments(token, { date, group, status: selectedStatus, page, pageSize }, controller.signal)
      .then((result) => {
        if (!active) return
        if (!Array.isArray(result?.items) || !Number.isInteger(result.totalItems)) {
          setState({ status: 'error', items: [], totalItems: 0, error: 'Hiện chưa thể hiển thị lịch hẹn. Vui lòng thử lại sau.' })
          return
        }
        const lastPage = Math.max(1, Math.ceil(result.totalItems / pageSize))
        if (page > lastPage) { setPage(lastPage); return }
        setState({ status: 'success', items: result.items, totalItems: result.totalItems, error: '' })
      })
      .catch((error) => {
        if (active && !controller.signal.aborted) setState({
          status: 'error', items: [], totalItems: 0,
          error: error.status === 0
            ? error.message
            : 'Hiện chưa thể hiển thị lịch hẹn. Vui lòng thử lại sau.',
        })
      })
    return () => { active = false; controller.abort() }
  }, [token, date, group, selectedStatus, page, reload])

  function refresh() {
    setState({ status: 'loading', items: [], totalItems: 0, error: '' })
    setReload((value) => value + 1)
  }

  function closeCancel() {
    setCancelTarget(null)
    setCancelReason('')
    setCancelError('')
  }

  async function confirmCancel(event) {
    event.preventDefault()
    if (!cancelTarget || !cancelReason.trim() || cancelling) return
    setCancelling(true)
    setCancelError('')
    try {
      await appointmentsApi.cancel(token, cancelTarget.maLichHen, cancelReason.trim())
      closeCancel()
      setNotice('Đã hủy lịch hẹn thành công.')
      refresh()
    } catch (error) {
      setCancelError(error.message)
    } finally {
      setCancelling(false)
    }
  }

  return <div className="my-appointments">
    <div className="page-heading"><div><h1>Lịch hẹn của tôi</h1><p className="page-description">Theo dõi lịch đã đặt và trạng thái xử lý tại phòng khám.</p></div></div>
    <ToastNotice message={notice} onDismiss={() => setNotice('')} />
    <div className="appointment-filters">
      <div className="appointment-groups" role="group" aria-label="Lọc lịch hẹn">
        {groups.map(([label, value]) => <button key={value} type="button" className={group === value ? 'appointment-group active' : 'appointment-group'} aria-pressed={group === value} onClick={() => { if (value !== group) { setPage(1); setSpecificStatus(''); onGroupChange(value) } }}>{label}</button>)}
      </div>
      {group !== 'all' && <label>Trạng thái cụ thể<select value={selectedStatus} onChange={(event) => { setPage(1); setSpecificStatus(event.target.value) }}>
        <option value="">Tất cả trong nhóm</option>
        {groupStatuses[group].map((value) => <option key={value} value={value}>{value}</option>)}
      </select></label>}
      <label>Ngày hẹn<input type="date" value={date} onChange={(event) => { setPage(1); setDate(event.target.value) }} /></label>
      {(group !== 'all' || date) && <button type="button" className="appointment-clear-filters" onClick={() => { setPage(1); if (group !== 'all') onGroupChange('all'); setSpecificStatus(''); setDate('') }}>Xóa bộ lọc</button>}
    </div>
    {state.status === 'loading' && <div className="appointment-feedback" role="status">Đang tải lịch hẹn…</div>}
    {state.status === 'error' && <div className="appointment-feedback appointment-feedback-error" role="alert"><strong>Không tải được lịch hẹn.</strong><p>{state.error}</p><button type="button" className="primary-button" onClick={refresh}>Thử lại</button></div>}
    {state.status === 'success' && <>
      <div className="appointment-count">{state.totalItems} lịch hẹn{group !== 'all' || date ? ' phù hợp bộ lọc' : ''}</div>
      {state.items.length ? <div className="appointment-list">{state.items.map((appointment) => <AppointmentCard key={appointment.maLichHen} appointment={appointment} expanded={expandedId === appointment.maLichHen} onToggle={() => setExpandedId(expandedId === appointment.maLichHen ? null : appointment.maLichHen)} onCancel={() => { setCancelTarget(appointment); setCancelReason(''); setCancelError('') }} />)}</div>
        : <div className="appointment-feedback"><strong>Chưa có lịch hẹn nào{group !== 'all' || date ? ' phù hợp bộ lọc' : ''}.</strong><p>{group !== 'all' || date ? 'Bạn có thể đổi hoặc xóa bộ lọc để xem các lịch khác.' : 'Khi bạn đặt lịch, thông tin sẽ xuất hiện tại đây.'}</p></div>}
      <ListPagination page={page} pageSize={pageSize} totalItems={state.totalItems} onPageChange={(nextPage) => { setExpandedId(null); setPage(nextPage) }} />
    </>}
    {cancelTarget && <CancelDialog appointment={cancelTarget} reason={cancelReason} error={cancelError} busy={cancelling} onReasonChange={setCancelReason} onCancel={closeCancel} onConfirm={confirmCancel} />}
  </div>
}
