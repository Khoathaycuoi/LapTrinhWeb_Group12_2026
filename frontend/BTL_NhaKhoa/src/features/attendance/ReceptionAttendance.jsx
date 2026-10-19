import { useEffect, useState } from 'react'
import ConfirmDialog from '../../components/ConfirmDialog'
import ListPagination from '../../components/ListPagination'
import ToastNotice from '../../components/ToastNotice'
import { attendanceApi } from './attendanceApi'
import './ReceptionAttendance.css'

const timeFormatter = new Intl.DateTimeFormat('vi-VN', { hour: '2-digit', minute: '2-digit' })
const reviewLabels = {
  CHUA_HOAN_TAT: 'Chưa hoàn tất', CHO_DUYET: 'Chờ duyệt',
  DA_DUYET: 'Đã duyệt', TU_CHOI: 'Bị từ chối',
}
const periodLabels = {
  CHUA_XAC_DINH: 'Chưa xác định', LAM_VIEC: 'Làm việc',
  NGHI_PHEP: 'Nghỉ phép', VANG_KHONG_PHEP: 'Vắng không phép',
}
const reviewLabel = (value) => reviewLabels[value] || 'Chưa xác định'
const periodLabel = (value) => periodLabels[value] || 'Chưa xác định'
const reviewTone = { CHUA_HOAN_TAT: 'pending', CHO_DUYET: 'processing', DA_DUYET: 'success', TU_CHOI: 'error' }
const periodTone = { CHUA_XAC_DINH: 'neutral', LAM_VIEC: 'success', NGHI_PHEP: 'pending', VANG_KHONG_PHEP: 'error' }
const dateTimeFormatter = new Intl.DateTimeFormat('vi-VN', { dateStyle: 'short', timeStyle: 'short' })

function time(value) {
  if (!value) return '—'
  const parsed = new Date(value)
  return Number.isNaN(parsed.getTime()) ? '—' : timeFormatter.format(parsed)
}

function dateTime(value) {
  if (!value) return '—'
  const parsed = new Date(value)
  return Number.isNaN(parsed.getTime()) ? '—' : dateTimeFormatter.format(parsed)
}

function HistoryPeriod({ title, state, start, end, late, early, reason }) {
  return <div className="attendance-history-period">
    <div className="attendance-history-period-heading"><h3>{title}</h3><span className={`attendance-status attendance-status-${periodTone[state] || 'neutral'}`}>{periodLabel(state)}</span></div>
    <p>Giờ vào: {time(start)} · Giờ ra: {time(end)}</p>
    {late > 0 && <p className="attendance-history-warning">Đi trễ: {late} phút</p>}
    {early > 0 && <p className="attendance-history-warning">Về sớm: {early} phút</p>}
    {reason && <p>Lý do: {reason}</p>}
  </div>
}

function HistoryItem({ item }) {
  const [expanded, setExpanded] = useState(false)
  const detailId = `attendance-detail-${item.maChamCong}`
  return <article>
    <div className="attendance-history-day"><strong>{item.ngayChamCong}</strong><span className={`attendance-status attendance-status-${reviewTone[item.trangThaiDuyet] || 'neutral'}`}>{reviewLabel(item.trangThaiDuyet)}</span>
      <button type="button" className="attendance-history-toggle" aria-expanded={expanded} aria-controls={detailId} onClick={() => setExpanded((value) => !value)}>{expanded ? 'Thu gọn' : 'Chi tiết'}</button>
    </div>
    <div>Sáng: {time(item.gioVaoSang)} – {time(item.gioRaSang)}</div>
    <div>Chiều: {time(item.gioVaoChieu)} – {time(item.gioRaChieu)}</div>
    <div className="attendance-history-duration">{Math.floor(item.tongPhutLamViec / 60)} giờ {item.tongPhutLamViec % 60} phút</div>
    {item.lyDoTuChoi && !expanded && <small className="attendance-history-rejected">Lý do từ chối: {item.lyDoTuChoi}</small>}
    {expanded && <div id={detailId} className="attendance-history-detail">
      <div className="attendance-history-periods">
        <HistoryPeriod title="Buổi sáng" state={item.tinhTrangSang} start={item.gioVaoSang} end={item.gioRaSang} late={item.soPhutTreSang} early={item.soPhutVeSomSang} reason={item.lyDoSang} />
        <HistoryPeriod title="Buổi chiều" state={item.tinhTrangChieu} start={item.gioVaoChieu} end={item.gioRaChieu} late={item.soPhutTreChieu} early={item.soPhutVeSomChieu} reason={item.lyDoChieu} />
      </div>
      {item.nguoiDuyet && <p className="attendance-history-approved">Đã duyệt bởi {item.nguoiDuyet} lúc {dateTime(item.duyetLuc)}.</p>}
      {item.lyDoTuChoi && <p className="attendance-history-rejected">Bị từ chối bởi {item.nguoiTuChoi || 'Quản lý'} lúc {dateTime(item.tuChoiLuc)}. Lý do: {item.lyDoTuChoi}</p>}
      {item.lichSuChinhSua?.length > 0 && <div className="attendance-history-changes"><h3>Lịch sử chỉnh sửa</h3><ul>{item.lichSuChinhSua.map((change, index) => <li key={`${change.thoiDiemSua}-${index}`}>
        <strong>{dateTime(change.thoiDiemSua)} · {change.nguoiSua}</strong>
        <span>Buổi {change.buoi === 'SANG' ? 'sáng' : 'chiều'}: {periodLabel(change.tinhTrangCu)} → {periodLabel(change.tinhTrangMoi)}; {time(change.gioVaoCu)}–{time(change.gioRaCu)} → {time(change.gioVaoMoi)}–{time(change.gioRaMoi)}</span>
        <span>Lý do: {change.lyDo}</span>
      </li>)}</ul></div>}
    </div>}
  </article>
}

function AttendancePeriod({ title, start, end, canIn, canOut, onAction }) {
  return <section className="attendance-period"><h2>{title}</h2><div className="attendance-times"><div><span>Giờ vào</span><strong>{time(start)}</strong></div><div><span>Giờ ra</span><strong>{time(end)}</strong></div></div>
    <div className="attendance-actions"><button type="button" className="primary-button" disabled={!canIn} onClick={() => onAction('check-in')}>Check-in</button><button type="button" className="secondary-button" disabled={!canOut} onClick={() => onAction('check-out')}>Check-out</button></div>
  </section>
}

export default function ReceptionAttendance({ token }) {
  const [todayReload, setTodayReload] = useState(0)
  const [historyReload, setHistoryReload] = useState(0)
  const [today, setToday] = useState({ status: 'loading', data: null, error: '' })
  const [history, setHistory] = useState({ status: 'loading', items: [], totalItems: 0, error: '' })
  const [page, setPage] = useState(1)
  const [fromDateInput, setFromDateInput] = useState('')
  const [toDateInput, setToDateInput] = useState('')
  const [dateRange, setDateRange] = useState({ fromDate: '', toDate: '' })
  const [filterError, setFilterError] = useState('')
  const [pending, setPending] = useState(null)
  const [busy, setBusy] = useState(false)
  const [actionError, setActionError] = useState('')
  const [notice, setNotice] = useState('')

  useEffect(() => {
    const controller = new AbortController()
    queueMicrotask(() => { if (!controller.signal.aborted) setToday({ status: 'loading', data: null, error: '' }) })
    attendanceApi.today(token, controller.signal).then((data) => {
      if (!controller.signal.aborted) setToday({ status: 'success', data, error: '' })
    }).catch((error) => {
      if (!controller.signal.aborted) setToday({ status: 'error', data: null,
        error: error.status === 0 ? error.message : 'Chưa tải được chấm công hôm nay.' })
    })
    return () => controller.abort()
  }, [token, todayReload])

  useEffect(() => {
    const controller = new AbortController()
    queueMicrotask(() => { if (!controller.signal.aborted) setHistory({ status: 'loading', items: [], totalItems: 0, error: '' }) })
    attendanceApi.history(token, page, dateRange, controller.signal).then((result) => {
      if (controller.signal.aborted) return
      if (!Array.isArray(result?.items) || !Number.isInteger(result.totalItems)) {
        setHistory({ status: 'error', items: [], totalItems: 0, error: 'Máy chủ chưa trả lịch sử chấm công.' })
        return
      }
      const lastPage = Math.max(1, Math.ceil(result.totalItems / 10))
      if (page > lastPage) { setPage(lastPage); return }
      setHistory({ status: 'success', items: result.items, totalItems: result.totalItems, error: '' })
    }).catch((error) => {
      if (!controller.signal.aborted) setHistory({ status: 'error', items: [], totalItems: 0,
        error: error.status === 0 ? error.message : 'Chưa tải được lịch sử chấm công.' })
    })
    return () => controller.abort()
  }, [token, page, dateRange, historyReload])

  function applyDateFilter(event) {
    event.preventDefault()
    if (fromDateInput && toDateInput && fromDateInput > toDateInput) {
      setFilterError('Từ ngày không được sau đến ngày.')
      return
    }
    setFilterError('')
    setPage(1)
    setDateRange({ fromDate: fromDateInput, toDate: toDateInput })
  }

  function updateFromDate(value) {
    setFromDateInput(value)
    setFilterError(value && toDateInput && value > toDateInput ? 'Từ ngày không được sau đến ngày.' : '')
  }

  function updateToDate(value) {
    setToDateInput(value)
    setFilterError(fromDateInput && value && fromDateInput > value ? 'Đến ngày không được trước từ ngày.' : '')
  }

  function clearDateFilter() {
    setFromDateInput('')
    setToDateInput('')
    setDateRange({ fromDate: '', toDate: '' })
    setFilterError('')
    setPage(1)
  }

  async function runAction() {
    if (!pending || busy) return
    setBusy(true)
    setActionError('')
    try {
      await attendanceApi.action(token, pending.type, pending.period)
      setNotice(`Đã ${pending.type === 'check-in' ? 'check-in' : 'check-out'} buổi ${pending.period === 'SANG' ? 'sáng' : 'chiều'}.`)
      setPending(null)
      setTodayReload((value) => value + 1)
      setHistoryReload((value) => value + 1)
    } catch (error) {
      setPending(null)
      setActionError(error.status === 400 || error.status === 409 || error.status === 0
        ? error.message : 'Chưa ghi được chấm công. Vui lòng thử lại.')
      setTodayReload((value) => value + 1)
    } finally {
      setBusy(false)
    }
  }

  const record = today.data?.chamCong
  return <div className="reception-attendance">
    <div className="page-heading"><div><h1>Chấm công</h1><p className="page-description">Ghi nhận giờ vào và giờ ra cho từng buổi làm việc.</p></div></div>
    {actionError && <p className="attendance-error" role="alert">{actionError}</p>}
    {today.status === 'loading' && <div className="attendance-feedback" role="status">Đang tải chấm công hôm nay…</div>}
    {today.status === 'error' && <div className="attendance-feedback" role="alert"><p>{today.error}</p><button type="button" className="secondary-button" onClick={() => setTodayReload((value) => value + 1)}>Thử lại</button></div>}
    {today.status === 'success' && <><div className="attendance-periods"><AttendancePeriod title="Buổi sáng" start={record?.gioVaoSang} end={record?.gioRaSang} canIn={today.data.coTheCheckInSang} canOut={today.data.coTheCheckOutSang} onAction={(type) => setPending({ type, period: 'SANG' })} />
        <AttendancePeriod title="Buổi chiều" start={record?.gioVaoChieu} end={record?.gioRaChieu} canIn={today.data.coTheCheckInChieu} canOut={today.data.coTheCheckOutChieu} onAction={(type) => setPending({ type, period: 'CHIEU' })} /></div>
      <p className="attendance-review">{record ? <>Trạng thái ngày: <span className={`attendance-status attendance-status-${reviewTone[record.trangThaiDuyet] || 'neutral'}`}>{reviewLabel(record.trangThaiDuyet)}</span></> : 'Hôm nay chưa có bản ghi chấm công.'}{record?.lyDoTuChoi && <span className="attendance-history-rejected"> Lý do từ chối: {record.lyDoTuChoi}</span>}</p>
    </>}
    <section className="attendance-history"><h2>Lịch sử chấm công</h2>
      <form className="attendance-date-filter" onSubmit={applyDateFilter}>
        <label>Từ ngày<input type="date" value={fromDateInput} max={toDateInput || undefined} onChange={(event) => updateFromDate(event.target.value)} /></label>
        <label>Đến ngày<input type="date" value={toDateInput} min={fromDateInput || undefined} onChange={(event) => updateToDate(event.target.value)} /></label>
        <button type="submit" className="secondary-button">Lọc</button>
        {(dateRange.fromDate || dateRange.toDate || fromDateInput || toDateInput) && <button type="button" className="attendance-clear-filter" onClick={clearDateFilter}>Xóa bộ lọc</button>}
      </form>
      {filterError && <p className="attendance-error" role="alert">{filterError}</p>}
      {history.status === 'loading' && <p role="status">Đang tải lịch sử…</p>}
      {history.status === 'error' && <div role="alert"><p>{history.error}</p><button type="button" className="secondary-button" onClick={() => setHistoryReload((value) => value + 1)}>Thử lại</button></div>}
      {history.status === 'success' && <>{history.items.length === 0 && <p>{dateRange.fromDate || dateRange.toDate ? 'Không có bản ghi chấm công trong khoảng ngày đã chọn.' : 'Chưa có bản ghi chấm công.'}</p>}
        <div className="attendance-history-list">{history.items.map((item) => <HistoryItem key={item.maChamCong} item={item} />)}</div>
        <ListPagination page={page} pageSize={10} totalItems={history.totalItems} onPageChange={setPage} />
      </>}
    </section>
    {pending && <ConfirmDialog title="Xác nhận chấm công?" message={`${pending.type === 'check-in' ? 'Check-in' : 'Check-out'} buổi ${pending.period === 'SANG' ? 'sáng' : 'chiều'}?`}
      confirmLabel={busy ? 'Đang ghi…' : 'Xác nhận'} busy={busy} onConfirm={runAction} onCancel={() => setPending(null)} />}
    <ToastNotice message={notice} onDismiss={() => setNotice('')} />
  </div>
}
