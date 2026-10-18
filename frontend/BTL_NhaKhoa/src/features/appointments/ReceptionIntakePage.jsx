import { useEffect, useState } from 'react'
import ListPagination from '../../components/ListPagination'
import ToastNotice from '../../components/ToastNotice'
import ReceptionActionDialog from './ReceptionActionDialog'
import { receptionAppointmentsApi } from './receptionAppointmentsApi'
import './ReceptionIntakePage.css'

const pageSize = 10
const dateFormatter = new Intl.DateTimeFormat('vi-VN', { dateStyle: 'full' })
const tabs = [
  ['waiting', 'Chờ khách đến'],
  ['active', 'Đã đến / Đang khám'],
  ['no-show', 'Không đến'],
]

function localDate(date) {
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

function canMarkNoShow(appointment, now) {
  const start = new Date(`${appointment.ngayHen}T${appointment.gioHen || '00:00:00'}`)
  return !Number.isNaN(start.getTime()) && now >= start.getTime() + 30 * 60 * 1000
}

function IntakeCard({ appointment, tab, now, onAction }) {
  const services = appointment.dichVusDuKien ?? []
  return <article className="intake-card">
    <div className="intake-card-heading">
      <div><span className="intake-code">{appointment.maLichHen}</span><h2>{appointment.gioHen?.slice(0, 5)} · {appointment.tenKH || appointment.maKH}</h2></div>
      <span className="intake-status">{appointment.trangThai}</span>
    </div>
    <div className="intake-card-info">
      <div><span>Mã khách hàng</span><strong>{appointment.maKH}</strong></div>
      <div><span>Dịch vụ đã đặt</span><strong>{services.length ? services.map((service) => service.tenDichVu).join(', ') : 'Chưa có thông tin'}</strong></div>
      <div><span>Bác sĩ</span><strong>{appointment.tenBacSi || 'Chưa phân công'}</strong></div>
      <div><span>Phòng</span><strong>{appointment.maPhong || 'Chưa phân phòng'}</strong></div>
      <div><span>Thời lượng dự kiến</span><strong>{appointment.thoiLuongDuKienPhut} phút</strong></div>
    </div>
    {appointment.ghi_chu && <p className="intake-card-note">Ghi chú: {appointment.ghi_chu}</p>}
    {tab === 'no-show' && appointment.ghiChuKhongDen && <p className="intake-card-note">Ghi chú không đến: {appointment.ghiChuKhongDen}</p>}
    {tab === 'active' && appointment.denLuc && <p className="intake-card-note">Đã tiếp nhận lúc {new Date(appointment.denLuc).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}</p>}
    {tab === 'waiting' && <div className="intake-card-actions">
      <button type="button" className="primary-button" onClick={() => onAction('receive', appointment)}>Tiếp nhận</button>
      {canMarkNoShow(appointment, now)
        ? <button type="button" className="intake-no-show-button" onClick={() => onAction('no-show', appointment)}>Không đến</button>
        : <span>Chỉ đánh dấu Không đến sau giờ hẹn ít nhất 30 phút.</span>}
    </div>}
  </article>
}

export default function ReceptionIntakePage({ token, query = '' }) {
  const [now, setNow] = useState(Date.now())
  const today = localDate(new Date(now))
  const [tab, setTab] = useState(() => ['active', 'arrived'].includes(new URLSearchParams(query).get('nhom')) ? 'active' : 'waiting')
  const [arrivedOnly, setArrivedOnly] = useState(() => new URLSearchParams(query).get('nhom') === 'arrived')
  const [upcomingOnly, setUpcomingOnly] = useState(() => new URLSearchParams(query).get('nhom') === 'upcoming')
  const [keywordInput, setKeywordInput] = useState('')
  const [keyword, setKeyword] = useState('')
  const [page, setPage] = useState(1)
  const [reload, setReload] = useState(0)
  const [list, setList] = useState({ status: 'loading', items: [], totalItems: 0, error: '' })
  const [action, setAction] = useState(null)
  const [notice, setNotice] = useState('')

  useEffect(() => {
    const timer = window.setInterval(() => setNow(Date.now()), 60_000)
    return () => window.clearInterval(timer)
  }, [])

  useEffect(() => { setPage(1) }, [today])

  useEffect(() => {
    const group = new URLSearchParams(query).get('nhom')
    setTab(['active', 'arrived'].includes(group) ? 'active' : 'waiting')
    setArrivedOnly(group === 'arrived')
    setUpcomingOnly(group === 'upcoming')
    setPage(1)
  }, [query])

  useEffect(() => {
    const controller = new AbortController()
    setList({ status: 'loading', items: [], totalItems: 0, error: '' })
    receptionAppointmentsApi.intakeList(token, { date: today, tab, arrivedOnly, upcomingOnly, keyword, page, pageSize }, controller.signal)
      .then((result) => {
        if (controller.signal.aborted) return
        if (!Array.isArray(result?.items) || !Number.isInteger(result.totalItems)) {
          setList({ status: 'error', items: [], totalItems: 0, error: 'Hiện chưa thể hiển thị danh sách tiếp nhận. Vui lòng thử lại sau.' })
          return
        }
        const lastPage = Math.max(1, Math.ceil(result.totalItems / pageSize))
        if (page > lastPage) { setPage(lastPage); return }
        setList({ status: 'success', items: result.items, totalItems: result.totalItems, error: '' })
      })
      .catch((error) => {
        if (!controller.signal.aborted) setList({
          status: 'error', items: [], totalItems: 0,
          error: error.status === 0 ? error.message : 'Hiện chưa thể hiển thị danh sách tiếp nhận. Vui lòng thử lại sau.',
        })
      })
    return () => controller.abort()
  }, [token, today, tab, arrivedOnly, upcomingOnly, keyword, page, reload])

  function refresh() { setReload((value) => value + 1) }

  return <div className="reception-intake">
    <div className="page-heading"><div><h1>Tiếp nhận</h1><p className="page-description">Lịch khám {dateFormatter.format(new Date(`${today}T00:00:00`))}. Kiểm tra dịch vụ đã đặt trước khi tiếp nhận khách.</p></div></div>
    <ToastNotice message={notice} onDismiss={() => setNotice('')} />
    <div className="intake-filters">
      <div className="intake-tabs" role="group" aria-label="Trạng thái tiếp nhận">
        {tabs.map(([value, label]) => <button key={value} type="button" className={tab === value ? 'active' : ''} aria-pressed={tab === value}
          onClick={() => { if (value !== tab || arrivedOnly || upcomingOnly) { setPage(1); setArrivedOnly(false); setUpcomingOnly(false); setTab(value) } }}>{label}</button>)}
      </div>
      <form onSubmit={(event) => { event.preventDefault(); setPage(1); setKeyword(keywordInput.trim()); refresh() }}>
        <label htmlFor="intake-search">Tên hoặc số điện thoại khách</label>
        <div><input id="intake-search" type="search" value={keywordInput} onChange={(event) => setKeywordInput(event.target.value)} placeholder="Tìm khách trong ngày" />
          <button type="submit" className="secondary-button">Tìm</button></div>
      </form>
      <button type="button" className="intake-refresh" onClick={refresh}>Làm mới</button>
      {arrivedOnly && <button type="button" className="intake-refresh" onClick={() => { setPage(1); setArrivedOnly(false) }}>Xem cả đang khám</button>}
      {upcomingOnly && <button type="button" className="intake-refresh" onClick={() => { setPage(1); setUpcomingOnly(false) }}>Xem cả hôm nay</button>}
    </div>
    {list.status === 'loading' && <div className="intake-feedback" role="status">Đang tải danh sách tiếp nhận…</div>}
    {list.status === 'error' && <div className="intake-feedback" role="alert"><strong>Không tải được danh sách tiếp nhận.</strong><p>{list.error}</p><button type="button" className="primary-button" onClick={refresh}>Thử lại</button></div>}
    {list.status === 'success' && <>
      <div className="intake-count">{list.totalItems} lịch hẹn phù hợp</div>
      {list.items.length ? <div className="intake-list">{list.items.map((appointment) => <IntakeCard key={appointment.maLichHen} appointment={appointment} tab={tab} now={now} onAction={(type, item) => setAction({ type, appointment: item })} />)}</div>
        : <div className="intake-feedback">Không có lịch hẹn nào trong nhóm này hôm nay.</div>}
      <ListPagination page={page} pageSize={pageSize} totalItems={list.totalItems} onPageChange={setPage} />
    </>}
    {action && <ReceptionActionDialog appointment={action.appointment} action={action.type} token={token}
      onClose={() => setAction(null)} onDone={(id) => {
        setAction(null)
        setNotice(action.type === 'receive' ? `Đã tiếp nhận lịch ${id}.` : `Đã đánh dấu Không đến cho lịch ${id}.`)
        refresh()
      }} />}
  </div>
}
