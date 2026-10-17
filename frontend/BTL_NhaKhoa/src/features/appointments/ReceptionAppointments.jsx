import { useEffect, useState } from 'react'
import { receptionAppointmentsApi } from './receptionAppointmentsApi'
import ReceptionCancelDialog from './ReceptionCancelDialog'
import ReceptionActionDialog from './ReceptionActionDialog'
import ListPagination from '../../components/ListPagination'
import ToastNotice from '../../components/ToastNotice'
import '../../components/ConfirmDialog.css'
import './ReceptionAppointments.css'

const dateFormatter = new Intl.DateTimeFormat('vi-VN', { dateStyle: 'full' })
const pageSize = 10
const statusTones = {
  'Chờ xác nhận': 'pending', 'Đã xác nhận': 'confirmed', 'Đã đến': 'processing',
  'Đang khám': 'processing', 'Đã hoàn thành': 'confirmed', 'Đã hủy': 'muted',
  'Không đến': 'muted', 'Từ chối': 'muted', 'Hết hạn': 'muted',
}

function appointmentTime(appointment) {
  const date = new Date(`${appointment.ngayHen}T${appointment.gioHen || '00:00:00'}`)
  return Number.isNaN(date.getTime())
    ? `${appointment.ngayHen} ${appointment.gioHen || ''}`
    : `${dateFormatter.format(date)}, ${appointment.gioHen.slice(0, 5)}`
}

function isFuture(appointment) {
  const date = new Date(`${appointment.ngayHen}T${appointment.gioHen || '00:00:00'}`)
  return !Number.isNaN(date.getTime()) && date.getTime() > Date.now()
}

function isPastDay(appointment) {
  const now = new Date()
  const today = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`
  return appointment.ngayHen < today
}

function ConfirmationDialog({ appointment, doctors, token, onClose, onRetryDoctors, onConfirmed }) {
  const [doctorId, setDoctorId] = useState('')
  const [roomId, setRoomId] = useState('')
  const [rooms, setRooms] = useState({ status: 'loading', items: [], error: '' })
  const [availableDoctors, setAvailableDoctors] = useState({ status: 'loading', items: [], error: '' })
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    const controller = new AbortController()
    receptionAppointmentsApi.availableRooms(token, appointment.maLichHen, controller.signal)
      .then((items) => setRooms(Array.isArray(items)
        ? { status: 'success', items, error: '' }
        : { status: 'error', items: [], error: 'Máy chủ chưa trả danh sách phòng trống.' }))
      .catch((requestError) => {
        if (!controller.signal.aborted) setRooms({ status: 'error', items: [], error: requestError.message })
      })
    return () => controller.abort()
  }, [appointment.maBacSi, appointment.maLichHen, token])

  useEffect(() => {
    const controller = new AbortController()
    receptionAppointmentsApi.availableDoctors(token, appointment.maLichHen, controller.signal)
      .then((items) => {
        if (controller.signal.aborted) return
        setAvailableDoctors(Array.isArray(items)
          ? { status: 'success', items, error: '' }
          : { status: 'error', items: [], error: 'Máy chủ chưa trả danh sách bác sĩ trống.' })
      })
      .catch((requestError) => {
        if (!controller.signal.aborted) setAvailableDoctors({ status: 'error', items: [], error: requestError.message })
      })
    return () => controller.abort()
  }, [appointment.maLichHen, token])

  useEffect(() => {
    function onKeyDown(event) { if (event.key === 'Escape' && !busy) onClose() }
    window.addEventListener('keydown', onKeyDown)
    return () => window.removeEventListener('keydown', onKeyDown)
  }, [busy, onClose])

  async function submit(event) {
    event.preventDefault()
    if (busy) return
    setBusy(true)
    setError('')
    try {
      if (appointment.maBacSi) await receptionAppointmentsApi.confirm(token, appointment.maLichHen, roomId)
      else await receptionAppointmentsApi.assignAndConfirm(token, appointment.maLichHen, doctorId, roomId)
      onConfirmed(appointment.maLichHen)
    } catch (requestError) {
      setError(requestError.message)
    } finally {
      setBusy(false)
    }
  }

  const doctorsReady = availableDoctors.status === 'success'
  const eligibleDoctors = doctorsReady
    ? doctors.items.filter((doctor) => availableDoctors.items.includes(doctor.id))
    : []
  const selectedDoctorAvailable = doctorsReady && availableDoctors.items.includes(appointment.maBacSi || doctorId)

  return <div className="dialog-backdrop" onMouseDown={(event) => { if (event.target === event.currentTarget && !busy) onClose() }}>
    <form className="confirm-dialog reception-dialog" role="dialog" aria-modal="true" aria-labelledby="reception-dialog-title" onSubmit={submit}>
      <h2 id="reception-dialog-title">{appointment.maBacSi ? 'Xác nhận lịch hẹn' : 'Gán bác sĩ và xác nhận'}</h2>
      <p>{appointment.maLichHen} · {appointmentTime(appointment)}<br />Khách hàng: {appointment.tenKH || appointment.maKH}</p>
      {!appointment.maBacSi ? <>
        <label htmlFor="reception-doctor">Bác sĩ phụ trách</label>
        {doctors.status === 'loading' && <div className="reception-dialog-note" role="status">Đang tải bác sĩ…</div>}
        {doctors.status === 'error' && <div className="alert alert-error" role="alert">{doctors.error} <button type="button" className="reception-retry" onClick={onRetryDoctors}>Thử lại</button></div>}
        {availableDoctors.status === 'loading' && <div className="reception-dialog-note" role="status">Đang kiểm tra lịch bác sĩ…</div>}
        {availableDoctors.status === 'error' && <div className="alert alert-error" role="alert">{availableDoctors.error}</div>}
        {doctors.status === 'success' && doctorsReady && <select id="reception-doctor" autoFocus value={doctorId} onChange={(event) => { setDoctorId(event.target.value); setError('') }} required>
          <option value="">Chọn bác sĩ</option>
          {eligibleDoctors.map((doctor) => <option key={doctor.id} value={doctor.id}>{doctor.ten}</option>)}
        </select>}
        {doctors.status === 'success' && doctorsReady && !eligibleDoctors.length && <div className="reception-dialog-note">Không có bác sĩ trống trong khoảng thời gian này.</div>}
      </> : <>
        <div className="reception-dialog-doctor">Bác sĩ: <strong>{appointment.tenBacSi || appointment.maBacSi}</strong></div>
        {availableDoctors.status === 'loading' && <div className="reception-dialog-note" role="status">Đang kiểm tra lịch bác sĩ…</div>}
        {availableDoctors.status === 'error' && <div className="alert alert-error" role="alert">{availableDoctors.error}</div>}
        {doctorsReady && !selectedDoctorAvailable && <div className="reception-dialog-note">Bác sĩ không còn rảnh trong khoảng thời gian này. Vui lòng xếp lịch khác.</div>}
      </>}
      <label htmlFor="reception-room">Phòng khám</label>
      {rooms.status === 'loading' && <div className="reception-dialog-note" role="status">Đang kiểm tra phòng trống…</div>}
      {rooms.status === 'error' && <div className="alert alert-error" role="alert">{rooms.error}</div>}
      {rooms.status === 'success' && <>
        <select id="reception-room" value={roomId} onChange={(event) => setRoomId(event.target.value)}>
          <option value="">Để hệ thống tự phân phòng</option>
          {rooms.items.map((room) => <option key={room.maPhong} value={room.maPhong}>{room.tenPhong} ({room.maPhong})</option>)}
        </select>
        {!rooms.items.length && <div className="reception-dialog-note">Không có phòng hoạt động còn trống cho toàn bộ thời lượng lịch hẹn.</div>}
      </>}
      {error && <div className="alert alert-error" role="alert">{error}</div>}
      <div className="confirm-actions">
        <button type="button" className="secondary-button" disabled={busy} onClick={onClose}>Đóng</button>
        <button type="submit" className="primary-button" disabled={busy || rooms.status !== 'success' || !rooms.items.length || !selectedDoctorAvailable || (!appointment.maBacSi && doctors.status !== 'success')}>
          {busy ? 'Đang xử lý…' : 'Xác nhận'}
        </button>
      </div>
    </form>
  </div>
}

export default function ReceptionAppointments({ token }) {
  const [tab, setTab] = useState('pending')
  const [date, setDate] = useState('')
  const [customerInput, setCustomerInput] = useState('')
  const [customer, setCustomer] = useState('')
  const [page, setPage] = useState(1)
  const [reload, setReload] = useState(0)
  const [list, setList] = useState({ status: 'loading', items: [], totalItems: 0, error: '' })
  const [doctors, setDoctors] = useState({ status: 'loading', items: [], error: '' })
  const [doctorReload, setDoctorReload] = useState(0)
  const [selected, setSelected] = useState(null)
  const [cancelTarget, setCancelTarget] = useState(null)
  const [noShowTarget, setNoShowTarget] = useState(null)
  const [notice, setNotice] = useState('')

  useEffect(() => {
    const controller = new AbortController()
    receptionAppointmentsApi.doctors(token, controller.signal)
      .then((items) => setDoctors(Array.isArray(items)
        ? { status: 'success', items, error: '' }
        : { status: 'error', items: [], error: 'Máy chủ chưa trả danh sách bác sĩ.' }))
      .catch((error) => { if (!controller.signal.aborted) setDoctors({ status: 'error', items: [], error: error.message }) })
    return () => controller.abort()
  }, [token, doctorReload])

  useEffect(() => {
    const controller = new AbortController()
    setList({ status: 'loading', items: [], totalItems: 0, error: '' })
    receptionAppointmentsApi.list(token, {
      status: tab === 'pending' ? 'CHO_XAC_NHAN' : '', upcoming: tab === 'pending',
      date, customer, page, pageSize,
    }, controller.signal)
      .then((result) => {
        if (controller.signal.aborted) return
        if (!Array.isArray(result?.items) || !Number.isInteger(result.totalItems)) {
          setList({ status: 'error', items: [], totalItems: 0, error: 'Hiện chưa thể hiển thị lịch hẹn. Vui lòng thử lại sau.' })
          return
        }
        const lastPage = Math.max(1, Math.ceil(result.totalItems / pageSize))
        if (page > lastPage) { setPage(lastPage); return }
        setList({ status: 'success', items: result.items, totalItems: result.totalItems, error: '' })
      })
      .catch((error) => {
        if (!controller.signal.aborted) setList({
          status: 'error', items: [], totalItems: 0,
          error: error.status === 0 ? error.message : 'Hiện chưa thể hiển thị lịch hẹn. Vui lòng thử lại sau.',
        })
      })
    return () => controller.abort()
  }, [token, tab, date, customer, page, reload])

  function refresh() {
    setList({ status: 'loading', items: [], totalItems: 0, error: '' })
    setReload((value) => value + 1)
  }

  const items = list.items

  return <div className="reception-appointments">
    <div className="page-heading"><div><h1>Lịch hẹn</h1><p className="page-description">Theo dõi yêu cầu đặt lịch và xác nhận lịch chờ cho khách hàng.</p></div></div>
    <ToastNotice message={notice} onDismiss={() => setNotice('')} />
    <div className="reception-filters">
      <div className="reception-tabs" role="group" aria-label="Nhóm lịch hẹn">
        <button type="button" className={tab === 'pending' ? 'active' : ''} aria-pressed={tab === 'pending'} onClick={() => { if (tab !== 'pending') { setPage(1); setTab('pending') } }}>Chờ xác nhận</button>
        <button type="button" className={tab === 'all' ? 'active' : ''} aria-pressed={tab === 'all'} onClick={() => { if (tab !== 'all') { setPage(1); setTab('all') } }}>Tất cả lịch hẹn</button>
      </div>
      <label>Ngày hẹn<input type="date" value={date} onChange={(event) => { setPage(1); setDate(event.target.value) }} /></label>
      <form onSubmit={(event) => { event.preventDefault(); setPage(1); setCustomer(customerInput.trim()); refresh() }}>
        <label htmlFor="reception-customer-search">Tên khách hàng</label>
        <div><input id="reception-customer-search" type="search" value={customerInput} onChange={(event) => setCustomerInput(event.target.value)} placeholder="Tìm theo tên" /><button type="submit" className="secondary-button">Tìm</button></div>
      </form>
      <button type="button" className="reception-refresh" onClick={refresh}>Làm mới</button>
    </div>
    {list.status === 'loading' && <div className="reception-feedback" role="status">Đang tải lịch hẹn…</div>}
    {list.status === 'error' && <div className="reception-feedback" role="alert"><strong>Không tải được lịch hẹn.</strong><p>{list.error}</p><button type="button" className="primary-button" onClick={refresh}>Thử lại</button></div>}
    {list.status === 'success' && <>
      <div className="reception-count">{list.totalItems} lịch hẹn {tab === 'pending' ? 'đang chờ xác nhận' : 'phù hợp'}</div>
      {items.length ? <div className="reception-list">{items.map((appointment) => <article className="reception-card" key={appointment.maLichHen}>
        <div className="reception-card-heading"><div><span className="reception-code">{appointment.maLichHen}</span><h2>{appointmentTime(appointment)}</h2></div><span className={`reception-status reception-status-${statusTones[appointment.trangThai] || 'muted'}`}>{appointment.trangThai}</span></div>
        <div className="reception-card-info">
          <div><span>Khách hàng</span><strong>{appointment.tenKH || appointment.maKH}</strong></div>
          <div><span>Bác sĩ</span><strong>{appointment.tenBacSi || 'Chưa phân công'}</strong></div>
          <div><span>Dịch vụ dự kiến</span><strong>{appointment.dichVusDuKien?.map((item) => item.tenDichVu).join(', ') || 'Chưa có thông tin'}</strong></div>
          <div><span>Thời lượng</span><strong>{appointment.thoiLuongDuKienPhut} phút</strong></div>
        </div>
        {((appointment.trangThai === 'Chờ xác nhận' && isFuture(appointment)) ||
          (appointment.trangThai === 'Đã xác nhận' && (isFuture(appointment) || isPastDay(appointment)))) && <div className="reception-card-actions">
          {isPastDay(appointment)
            ? <button type="button" className="reception-cancel-button" onClick={() => setNoShowTarget(appointment)}>Đánh dấu không đến</button>
            : <button type="button" className="reception-cancel-button" onClick={() => setCancelTarget(appointment)}>Hủy lịch</button>}
          {appointment.trangThai === 'Chờ xác nhận' && <button type="button" className="primary-button" onClick={() => setSelected(appointment)}>Xác nhận</button>}
        </div>}
      </article>)}</div> : <div className="reception-feedback">Chưa có lịch hẹn nào phù hợp.</div>}
      <ListPagination page={page} pageSize={pageSize} totalItems={list.totalItems} onPageChange={setPage} />
    </>}
    {selected && <ConfirmationDialog key={selected.maLichHen} appointment={selected} doctors={doctors} token={token}
      onRetryDoctors={() => { setDoctors({ status: 'loading', items: [], error: '' }); setDoctorReload((value) => value + 1) }}
      onClose={() => setSelected(null)}
      onConfirmed={(id) => { setSelected(null); setNotice(`Đã xác nhận lịch hẹn ${id}.`); refresh() }} />}
    {cancelTarget && <ReceptionCancelDialog appointment={cancelTarget} token={token} onClose={() => setCancelTarget(null)}
      onCancelled={(id) => { setCancelTarget(null); setNotice(`Đã hủy lịch hẹn ${id}.`); refresh() }} />}
    {noShowTarget && <ReceptionActionDialog appointment={noShowTarget} action="no-show" token={token}
      onClose={() => setNoShowTarget(null)} onDone={(id) => { setNoShowTarget(null); setNotice(`Đã đánh dấu Không đến cho lịch ${id}.`); refresh() }} />}
  </div>
}
