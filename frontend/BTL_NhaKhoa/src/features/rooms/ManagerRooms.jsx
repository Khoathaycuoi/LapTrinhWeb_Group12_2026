import { useEffect, useState } from 'react'
import ConfirmDialog from '../../components/ConfirmDialog'
import ListPagination from '../../components/ListPagination'
import ToastNotice from '../../components/ToastNotice'
import { roomsApi } from './roomsApi'
import './ManagerRooms.css'

const statuses = {
  HOAT_DONG: 'Hoạt động',
  TAM_NGUNG: 'Tạm ngừng',
  BAO_TRI: 'Bảo trì',
}
const appointmentStatuses = {
  CHO_XAC_NHAN: 'Chờ xác nhận', DA_XAC_NHAN: 'Đã xác nhận', DA_DEN: 'Đã đến',
  DANG_KHAM: 'Đang khám', DA_HOAN_THANH: 'Đã hoàn thành', DA_HUY: 'Đã hủy',
  KHONG_DEN: 'Không đến', TU_CHOI: 'Từ chối', HET_HAN: 'Hết hạn',
}
const dateTimeFormatter = new Intl.DateTimeFormat('vi-VN', { dateStyle: 'short', timeStyle: 'short' })

function formatTime(value) {
  if (!value) return '—'
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? '—' : dateTimeFormatter.format(date)
}

function RoomHistory({ token, room, onClose }) {
  const [fromInput, setFromInput] = useState('')
  const [toInput, setToInput] = useState('')
  const [dates, setDates] = useState({ from: '', to: '' })
  const [filterError, setFilterError] = useState('')
  const [page, setPage] = useState(1)
  const [reload, setReload] = useState(0)
  const [list, setList] = useState({ status: 'loading', items: [], totalItems: 0, error: '' })

  useEffect(() => {
    const controller = new AbortController()
    setList({ status: 'loading', items: [], totalItems: 0, error: '' })
    roomsApi.history(token, room.maPhong, dates, page, controller.signal).then((result) => {
      if (controller.signal.aborted) return
      if (!Array.isArray(result?.items) || !Number.isInteger(result.totalItems)) {
        setList({ status: 'error', items: [], totalItems: 0, error: 'Máy chủ chưa trả lịch sử phòng.' })
        return
      }
      const lastPage = Math.max(1, Math.ceil(result.totalItems / 10))
      if (page > lastPage) { setPage(lastPage); return }
      setList({ status: 'success', items: result.items, totalItems: result.totalItems, error: '' })
    }).catch((error) => {
      if (!controller.signal.aborted) setList({ status: 'error', items: [], totalItems: 0,
        error: [0, 400, 404].includes(error.status) ? error.message : 'Chưa tải được lịch sử phòng.' })
    })
    return () => controller.abort()
  }, [token, room.maPhong, dates, page, reload])

  function applyDates(event) {
    event.preventDefault()
    if (fromInput && toInput && fromInput > toInput) {
      setFilterError('Từ ngày không được sau đến ngày.')
      return
    }
    setFilterError('')
    setDates({ from: fromInput, to: toInput })
    setPage(1)
  }

  return <section className="manager-room-history" aria-labelledby="manager-room-history-title">
    <div className="manager-rooms-row"><div><h2 id="manager-room-history-title">Lịch sử {room.tenPhong}</h2><p>Lịch từng được phân vào phòng này, kể cả lịch đã hủy.</p></div>
      <button type="button" className="secondary-button" onClick={onClose}>Đóng</button></div>
    <form className="manager-room-filters" onSubmit={applyDates}>
      <label>Từ ngày<input type="date" value={fromInput} onChange={(event) => setFromInput(event.target.value)} /></label>
      <label>Đến ngày<input type="date" value={toInput} onChange={(event) => setToInput(event.target.value)} /></label>
      <button type="submit" className="secondary-button">Lọc</button>
      {(dates.from || dates.to) && <button type="button" className="manager-room-clear" onClick={() => { setFromInput(''); setToInput(''); setDates({ from: '', to: '' }); setFilterError(''); setPage(1) }}>Xóa bộ lọc</button>}
    </form>
    {filterError && <p className="manager-room-error" role="alert">{filterError}</p>}
    {list.status === 'loading' && <p role="status">Đang tải lịch sử…</p>}
    {list.status === 'error' && <div role="alert"><p>{list.error}</p><button type="button" className="secondary-button" onClick={() => setReload((value) => value + 1)}>Thử lại</button></div>}
    {list.status === 'success' && <><p>{list.totalItems} lịch từng được phân phòng</p>
      {list.items.length === 0 && <p>Không có lịch trong khoảng ngày đã chọn.</p>}
      <div className="manager-room-history-list">{list.items.map((item) => <article key={item.maLichHen}>
        <div><strong>{item.maLichHen} · {formatTime(item.thoiGianBatDau)}</strong><span>{item.tenKhachHang || item.maKhachHang} · Bác sĩ {item.tenBacSi || item.maBacSi || 'chưa phân công'}</span></div>
        <span>{appointmentStatuses[item.trangThai] || item.trangThai}</span>
        {item.lyDoHuy && <small>Lý do hủy: {item.lyDoHuy}</small>}
      </article>)}</div>
      <ListPagination page={page} pageSize={10} totalItems={list.totalItems} onPageChange={setPage} />
    </>}
  </section>
}

export default function ManagerRooms({ token }) {
  const [page, setPage] = useState(1)
  const [reload, setReload] = useState(0)
  const [list, setList] = useState({ status: 'loading', items: [], totalItems: 0, error: '' })
  const [historyRoom, setHistoryRoom] = useState(null)
  const [showCreate, setShowCreate] = useState(false)
  const [newName, setNewName] = useState('')
  const [createError, setCreateError] = useState('')
  const [createConfirm, setCreateConfirm] = useState(false)
  const [createBusy, setCreateBusy] = useState(false)
  const [statusTarget, setStatusTarget] = useState(null)
  const [statusError, setStatusError] = useState('')
  const [statusBusy, setStatusBusy] = useState(false)
  const [notice, setNotice] = useState('')

  useEffect(() => {
    const controller = new AbortController()
    setList({ status: 'loading', items: [], totalItems: 0, error: '' })
    roomsApi.list(token, page, controller.signal).then((result) => {
      if (controller.signal.aborted) return
      if (!Array.isArray(result?.items) || !Number.isInteger(result.totalItems)) {
        setList({ status: 'error', items: [], totalItems: 0, error: 'Máy chủ chưa trả danh sách phòng.' })
        return
      }
      const lastPage = Math.max(1, Math.ceil(result.totalItems / 10))
      if (page > lastPage) { setPage(lastPage); return }
      setList({ status: 'success', items: result.items, totalItems: result.totalItems, error: '' })
    }).catch((error) => {
      if (!controller.signal.aborted) setList({ status: 'error', items: [], totalItems: 0,
        error: error.status === 0 ? error.message : 'Chưa tải được danh sách phòng.' })
    })
    return () => controller.abort()
  }, [token, page, reload])

  async function createRoom() {
    if (createBusy) return
    setCreateBusy(true)
    setCreateError('')
    try {
      await roomsApi.create(token, newName.trim())
      setNotice(`Đã thêm phòng ${newName.trim()}.`)
      setCreateConfirm(false)
      setShowCreate(false)
      setNewName('')
      setReload((value) => value + 1)
    } catch (error) {
      setCreateConfirm(false)
      setCreateError([0, 400].includes(error.status) ? error.message : 'Chưa thêm được phòng. Vui lòng thử lại.')
    } finally {
      setCreateBusy(false)
    }
  }

  async function changeStatus() {
    if (!statusTarget || statusBusy) return
    setStatusBusy(true)
    setStatusError('')
    try {
      await roomsApi.setStatus(token, statusTarget.room.maPhong, statusTarget.status)
      setNotice(`Đã chuyển ${statusTarget.room.tenPhong} sang ${statuses[statusTarget.status].toLocaleLowerCase('vi-VN')}.`)
      setStatusTarget(null)
      setReload((value) => value + 1)
    } catch (error) {
      setStatusError([0, 400].includes(error.status) ? error.message : 'Chưa cập nhật được trạng thái phòng. Vui lòng thử lại.')
    } finally {
      setStatusBusy(false)
    }
  }

  return <div className="manager-rooms">
    <div className="page-heading manager-rooms-row"><div><h1>Phòng khám</h1><p className="page-description">Quản lý trạng thái vận hành và xem lịch đã phân phòng.</p></div>
      <button type="button" className="primary-button" onClick={() => { setShowCreate(true); setCreateError('') }}>Thêm phòng</button></div>
    {showCreate && <section className="manager-room-create"><div className="manager-rooms-row"><h2>Thêm phòng</h2><button type="button" className="secondary-button" disabled={createBusy} onClick={() => { setShowCreate(false); setCreateError('') }}>Đóng</button></div>
      <form onSubmit={(event) => { event.preventDefault(); setCreateError(''); if (newName.trim()) setCreateConfirm(true); else setCreateError('Vui lòng nhập tên phòng.') }}>
        <label>Tên phòng<input required maxLength={100} value={newName} onChange={(event) => setNewName(event.target.value)} /></label>
        {createError && <p className="manager-room-error" role="alert">{createError}</p>}
        <button type="submit" className="primary-button">Thêm phòng</button>
      </form>
    </section>}
    {list.status === 'loading' && <div className="manager-room-feedback" role="status">Đang tải danh sách phòng…</div>}
    {list.status === 'error' && <div className="manager-room-feedback" role="alert"><p>{list.error}</p><button type="button" className="secondary-button" onClick={() => setReload((value) => value + 1)}>Thử lại</button></div>}
    {list.status === 'success' && <><p className="manager-room-count">{list.totalItems} phòng</p>
      {list.items.length === 0 && <div className="manager-room-feedback">Chưa có phòng nào.</div>}
      <div className="manager-room-list">{list.items.map((room) => <article key={room.maPhong}>
        <div><span>{room.maPhong}</span><h2>{room.tenPhong}</h2><p>Trạng thái vận hành: {statuses[room.trangThai] || room.trangThai}</p></div>
        <div className="manager-room-actions"><button type="button" className="secondary-button" onClick={() => setHistoryRoom(room)}>Lịch sử</button>
          <label>Đổi trạng thái<select value={room.trangThai} onChange={(event) => { setStatusTarget({ room, status: event.target.value }); setStatusError('') }}>
            {Object.entries(statuses).map(([value, label]) => <option key={value} value={value}>{label}</option>)}</select></label></div>
      </article>)}</div>
      <ListPagination page={page} pageSize={10} totalItems={list.totalItems} onPageChange={setPage} />
    </>}
    {historyRoom && <RoomHistory key={historyRoom.maPhong} token={token} room={historyRoom} onClose={() => setHistoryRoom(null)} />}
    {createConfirm && <ConfirmDialog title="Thêm phòng?" message={`Tạo phòng ${newName.trim()}?`} confirmLabel={createBusy ? 'Đang thêm…' : 'Thêm phòng'} busy={createBusy} onConfirm={createRoom} onCancel={() => setCreateConfirm(false)} />}
    {statusTarget && <div className="dialog-backdrop" onMouseDown={(event) => { if (event.target === event.currentTarget && !statusBusy) setStatusTarget(null) }}><section className="confirm-dialog manager-room-status-dialog" role="alertdialog" aria-modal="true" aria-labelledby="manager-room-status-title">
      <h2 id="manager-room-status-title">Đổi trạng thái phòng?</h2>
      <p>{statusTarget.room.tenPhong} → {statuses[statusTarget.status]}</p>
      {statusTarget.status !== 'HOAT_DONG' && <p>Các lịch đã xác nhận cần được xử lý trước khi ngừng phòng.</p>}
      {statusError && <p className="manager-room-error" role="alert">{statusError}</p>}
      <div className="confirm-actions"><button type="button" className="secondary-button" disabled={statusBusy} onClick={() => setStatusTarget(null)}>Quay lại</button><button type="button" className="primary-button" disabled={statusBusy} onClick={changeStatus}>{statusBusy ? 'Đang cập nhật…' : 'Xác nhận'}</button></div>
    </section></div>}
    <ToastNotice message={notice} onDismiss={() => setNotice('')} />
  </div>
}
