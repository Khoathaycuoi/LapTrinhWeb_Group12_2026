import { useEffect, useState } from 'react'
import ConfirmDialog from '../../components/ConfirmDialog'
import ListPagination from '../../components/ListPagination'
import ToastNotice from '../../components/ToastNotice'
import AppointmentDetails from './AppointmentDetails'
import { doctorAppointmentsApi } from './doctorAppointmentsApi'
import './DoctorAppointments.css'

const tabs = [
  ['today', 'Hôm nay'], ['pending', 'Chờ xác nhận'], ['arrived', 'Khách đã đến'],
  ['examining', 'Đang khám'], ['all', 'Tất cả'],
]
const statusTones = {
  'Chờ xác nhận': 'pending', 'Đã xác nhận': 'confirmed', 'Đã đến': 'processing',
  'Đang khám': 'processing', 'Đã hoàn thành': 'confirmed',
}
const dateFormatter = new Intl.DateTimeFormat('vi-VN', { dateStyle: 'full' })

function todayLocal() {
  const now = new Date()
  return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`
}

function appointmentDate(item) {
  const date = new Date(`${item.ngayHen}T00:00:00`)
  return `${Number.isNaN(date.getTime()) ? item.ngayHen : dateFormatter.format(date)}, ${item.gioHen?.slice(0, 5) || ''}`
}

function isFuture(item) {
  return new Date(`${item.ngayHen}T${item.gioHen || '00:00:00'}`).getTime() > Date.now()
}

function DoctorDecisionDialog({ token, action, onClose, onDone }) {
  const [rooms, setRooms] = useState({ status: 'loading', items: [], error: '' })
  const [roomId, setRoomId] = useState('')
  const [reason, setReason] = useState('')
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')
  const item = action.item

  useEffect(() => {
    if (action.type !== 'confirm') return undefined
    const controller = new AbortController()
    doctorAppointmentsApi.rooms(token, item.maLichHen, controller.signal)
      .then((items) => { if (!controller.signal.aborted) setRooms(Array.isArray(items)
        ? { status: 'success', items, error: '' }
        : { status: 'error', items: [], error: 'Chưa tải được phòng trống.' }) })
      .catch((requestError) => { if (!controller.signal.aborted) setRooms({ status: 'error', items: [], error: requestError.message }) })
    return () => controller.abort()
  }, [action.type, item.maLichHen, token])

  useEffect(() => {
    function closeOnEscape(event) { if (event.key === 'Escape' && !busy) onClose() }
    window.addEventListener('keydown', closeOnEscape)
    return () => window.removeEventListener('keydown', closeOnEscape)
  }, [busy, onClose])

  async function submit(event) {
    event.preventDefault()
    if (busy) return
    if (action.type !== 'confirm' && !reason.trim()) { setError('Vui lòng nhập lý do.'); return }
    setBusy(true)
    setError('')
    try {
      if (action.type === 'confirm') await doctorAppointmentsApi.confirm(token, item.maLichHen, roomId)
      else if (action.type === 'reject') await doctorAppointmentsApi.reject(token, item.maLichHen, reason.trim())
      else await doctorAppointmentsApi.cancel(token, item.maLichHen, reason.trim())
      onDone(action.type, item.maLichHen)
    } catch (requestError) {
      setError(requestError.message)
    } finally {
      setBusy(false)
    }
  }

  return <div className="dialog-backdrop" onMouseDown={(event) => { if (event.target === event.currentTarget && !busy) onClose() }}>
    <form className="confirm-dialog doctor-decision-dialog" role="dialog" aria-modal="true" aria-label={action.type === 'confirm' ? 'Xác nhận lịch hẹn' : action.type === 'reject' ? 'Từ chối lịch hẹn' : 'Hủy lịch hẹn'} onSubmit={submit}>
      <h2>{action.type === 'confirm' ? 'Xác nhận lịch hẹn?' : action.type === 'reject' ? 'Từ chối lịch hẹn?' : 'Hủy lịch hẹn?'}</h2>
      <p>{item.tenKH || item.maKH} · {appointmentDate(item)}</p>
      {action.type === 'confirm' ? <>
        <label>Phòng khám<select value={roomId} onChange={(event) => setRoomId(event.target.value)} disabled={rooms.status !== 'success'}>
          <option value="">Để hệ thống chọn phòng</option>
          {rooms.items.map((room) => <option key={room.maPhong} value={room.maPhong}>{room.tenPhong} ({room.maPhong})</option>)}
        </select></label>
        {rooms.status === 'loading' && <p role="status">Đang tìm phòng trống…</p>}
        {rooms.status === 'success' && rooms.items.length === 0 && <p role="status">Không có phòng trống cho lịch này.</p>}
        {rooms.status === 'error' && <p role="alert">{rooms.error}</p>}
      </> : <label>{action.type === 'reject' ? 'Lý do từ chối' : 'Lý do hủy'}<textarea required maxLength={500} rows={3} value={reason} onChange={(event) => { setReason(event.target.value); setError('') }} placeholder="Nhập lý do để khách hàng biết" /></label>}
      {error && <p className="doctor-form-error" role="alert">{error}</p>}
      <div className="confirm-actions"><button type="button" className="secondary-button" disabled={busy} onClick={onClose}>Quay lại</button>
        <button type="submit" className="primary-button" disabled={busy || (action.type === 'confirm' && (rooms.status !== 'success' || rooms.items.length === 0))}>{busy ? 'Đang xử lý…' : action.type === 'confirm' ? 'Xác nhận' : action.type === 'reject' ? 'Từ chối lịch' : 'Hủy lịch'}</button></div>
    </form>
  </div>
}

export default function DoctorAppointments({ token, query = '', navigationState, onNavigate }) {
  const initialTab = new URLSearchParams(query).get('nhom')
  const [tab, setTab] = useState(tabs.some(([key]) => key === initialTab) ? initialTab : 'today')
  const [date, setDate] = useState(todayLocal)
  const [searchInput, setSearchInput] = useState('')
  const [keyword, setKeyword] = useState('')
  const [page, setPage] = useState(1)
  const [reload, setReload] = useState(0)
  const [list, setList] = useState({ status: 'loading', items: [], totalItems: 0, error: '' })
  const [decision, setDecision] = useState(null)
  const [startTarget, setStartTarget] = useState(null)
  const [busy, setBusy] = useState(false)
  const [actionError, setActionError] = useState('')
  const [notice, setNotice] = useState(navigationState?.notice || '')
  const [expandedId, setExpandedId] = useState(null)

  useEffect(() => {
    const nextTab = new URLSearchParams(query).get('nhom')
    setTab(tabs.some(([key]) => key === nextTab) ? nextTab : 'today')
    setPage(1)
  }, [query])

  useEffect(() => {
    const controller = new AbortController()
    setList({ status: 'loading', items: [], totalItems: 0, error: '' })
    doctorAppointmentsApi.list(token, { tab, date, keyword, page }, controller.signal).then((result) => {
      if (controller.signal.aborted) return
      if (!Array.isArray(result?.items) || !Number.isInteger(result.totalItems)) {
        setList({ status: 'error', items: [], totalItems: 0, error: 'Chưa tải được lịch hẹn.' })
        return
      }
      const lastPage = Math.max(1, Math.ceil(result.totalItems / 10))
      if (page > lastPage) { setPage(lastPage); return }
      setList({ status: 'success', items: result.items, totalItems: result.totalItems, error: '' })
    }).catch((error) => {
      if (!controller.signal.aborted) setList({ status: 'error', items: [], totalItems: 0,
        error: error.status === 0 ? error.message : 'Chưa tải được lịch hẹn.' })
    })
    return () => controller.abort()
  }, [token, tab, date, keyword, page, reload])

  function selectTab(nextTab) {
    setTab(nextTab)
    setPage(1)
    onNavigate(nextTab === 'today' ? '/bac-si/lich-hen' : `/bac-si/lich-hen?nhom=${nextTab}`, true)
  }

  async function startExam() {
    if (!startTarget || busy) return
    setBusy(true)
    setActionError('')
    try {
      const result = await doctorAppointmentsApi.start(token, startTarget.maLichHen)
      setStartTarget(null)
      if (result?.maHoSoKham) onNavigate(`/bac-si/ho-so-kham?maHoSoKham=${encodeURIComponent(result.maHoSoKham)}`, false, { notice: 'Đã bắt đầu khám.' })
      else { setNotice('Đã bắt đầu khám.'); setReload((value) => value + 1) }
    } catch (error) {
      setStartTarget(null)
      setActionError(error.message)
      setReload((value) => value + 1)
    } finally { setBusy(false) }
  }

  return <div className="doctor-appointments">
    <div className="page-heading"><div><h1>Lịch của tôi</h1><p className="page-description">Lịch được phân công, khách đã đến và ca đang khám.</p></div></div>
    {actionError && <p className="doctor-form-error" role="alert">{actionError}</p>}
    <div className="doctor-appointment-filters">
      <div className="doctor-appointment-tabs" role="group" aria-label="Nhóm lịch hẹn">{tabs.map(([key, label]) => <button key={key} type="button" className={tab === key ? 'active' : ''} aria-pressed={tab === key} onClick={() => selectTab(key)}>{label}</button>)}</div>
      {tab === 'today' && <label>Ngày hẹn<input type="date" value={date} onChange={(event) => { setDate(event.target.value); setPage(1) }} /></label>}
      <form onSubmit={(event) => { event.preventDefault(); setKeyword(searchInput.trim()); setPage(1) }}><label>Tên hoặc số điện thoại khách<input type="search" value={searchInput} onChange={(event) => setSearchInput(event.target.value)} /></label><button type="submit" className="secondary-button">Tìm</button></form>
      {keyword && <button type="button" className="doctor-clear-search" onClick={() => { setSearchInput(''); setKeyword(''); setPage(1) }}>Xóa tìm kiếm</button>}
    </div>
    {list.status === 'loading' && <div className="doctor-list-feedback" role="status">Đang tải lịch hẹn…</div>}
    {list.status === 'error' && <div className="doctor-list-feedback" role="alert"><p>{list.error}</p><button type="button" className="secondary-button" onClick={() => setReload((value) => value + 1)}>Thử lại</button></div>}
    {list.status === 'success' && <><p className="doctor-list-count">{list.totalItems} lịch hẹn</p>{list.items.length === 0 && <div className="doctor-list-feedback">Không có lịch hẹn phù hợp.</div>}
      <div className="doctor-appointment-list">{list.items.map((item) => <article key={item.maLichHen} className="doctor-appointment-card">
        <div className="doctor-appointment-heading"><div><small>{item.maLichHen}</small><h2>{appointmentDate(item)}</h2></div><span className={`doctor-status doctor-status-${statusTones[item.trangThai] || 'muted'}`}>{item.trangThai}</span></div>
        <div className="doctor-appointment-meta"><div><span>Khách hàng</span><strong>{item.tenKH || item.maKH}</strong><small>{item.maKH}</small></div><div><span>Dịch vụ dự kiến</span><strong>{item.dichVusDuKien?.map((service) => service.tenDichVu).join(', ') || 'Chưa có thông tin'}</strong></div><div><span>Phòng</span><strong>{item.maPhong || 'Chưa phân phòng'}</strong></div><div><span>Thời lượng</span><strong>{item.thoiLuongDuKienPhut} phút</strong></div></div>
        <div className="doctor-appointment-actions">
          <button type="button" className="doctor-details-button" aria-expanded={expandedId === item.maLichHen}
            onClick={() => setExpandedId(expandedId === item.maLichHen ? null : item.maLichHen)}>
            {expandedId === item.maLichHen ? 'Thu gọn' : 'Xem chi tiết'}
          </button>
          {((item.trangThai === 'Chờ xác nhận' && isFuture(item)) || (item.trangThai === 'Đã xác nhận' && isFuture(item)) || item.trangThai === 'Đã đến' || item.trangThai === 'Đang khám') && <>
          {item.trangThai === 'Chờ xác nhận' && <><button type="button" className="doctor-reject-button" onClick={() => setDecision({ type: 'reject', item })}>Từ chối</button><button type="button" className="primary-button" onClick={() => setDecision({ type: 'confirm', item })}>Xác nhận</button></>}
          {item.trangThai === 'Đã xác nhận' && <button type="button" className="doctor-reject-button" onClick={() => setDecision({ type: 'cancel', item })}>Hủy lịch</button>}
          {item.trangThai === 'Đã đến' && <button type="button" className="primary-button" onClick={() => setStartTarget(item)}>Bắt đầu khám</button>}
          {item.trangThai === 'Đang khám' && <button type="button" className="primary-button" onClick={() => onNavigate(`/bac-si/ho-so-kham?maLichHen=${encodeURIComponent(item.maLichHen)}`)}>Mở hồ sơ khám</button>}
          </>}
        </div>
        {expandedId === item.maLichHen && <AppointmentDetails appointment={item} id={`doctor-details-${item.maLichHen}`} />}
      </article>)}</div><ListPagination page={page} pageSize={10} totalItems={list.totalItems} onPageChange={setPage} /></>}
    {decision && <DoctorDecisionDialog token={token} action={decision} onClose={() => setDecision(null)} onDone={(type, id) => { setDecision(null); setNotice(type === 'confirm' ? `Đã xác nhận lịch ${id}.` : type === 'reject' ? `Đã từ chối lịch ${id}.` : `Đã hủy lịch ${id}.`); setReload((value) => value + 1) }} />}
    {startTarget && <ConfirmDialog title="Bắt đầu khám?" message={`Bắt đầu khám cho ${startTarget.tenKH || startTarget.maKH}, lịch ${startTarget.maLichHen}?`} confirmLabel={busy ? 'Đang bắt đầu…' : 'Bắt đầu khám'} busy={busy} onConfirm={startExam} onCancel={() => setStartTarget(null)} />}
    <ToastNotice message={notice} onDismiss={() => setNotice('')} />
  </div>
}
