import { useEffect, useRef, useState } from 'react'
import ListPagination from '../../components/ListPagination'
import ConfirmDialog from '../../components/ConfirmDialog'
import ToastNotice from '../../components/ToastNotice'
import { invoicesApi } from './invoicesApi'
import InvoicePaymentForm from './InvoicePaymentForm'
import InvoiceCreation from './InvoiceCreation'
import '../../components/ConfirmDialog.css'
import './ReceptionInvoices.css'

const pageSize = 10
const moneyFormatter = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND', maximumFractionDigits: 0 })
const dateFormatter = new Intl.DateTimeFormat('vi-VN', { dateStyle: 'medium', timeStyle: 'short' })
const statusLabels = {
  CHUA_THANH_TOAN: 'Chưa thanh toán',
  THANH_TOAN_MOT_PHAN: 'Thanh toán một phần',
  DA_THANH_TOAN: 'Đã thanh toán',
  DA_HUY: 'Đã hủy',
}
const methodLabels = { TIEN_MAT: 'Tiền mặt', CHUYEN_KHOAN: 'Chuyển khoản', KHAC: 'Khác' }

function formatDate(value) {
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? '—' : dateFormatter.format(date)
}

function paymentFilterFromQuery(query) {
  const params = new URLSearchParams(query)
  if (params.get('conNo') === 'true') return 'unpaid'
  if (params.get('trangThai') === 'DA_THANH_TOAN') return 'paid'
  return 'all'
}

function InvoiceDetails({ token, invoiceId, onClose, onPaymentRecorded, onPaymentCancelled, managerMode }) {
  const [reload, setReload] = useState(0)
  const [state, setState] = useState({ status: 'loading', data: null, error: '' })
  const [paymentFormOpen, setPaymentFormOpen] = useState(false)
  const [paymentDraft, setPaymentDraft] = useState(null)
  const [paymentBusy, setPaymentBusy] = useState(false)
  const [paymentError, setPaymentError] = useState('')
  const [cancelTarget, setCancelTarget] = useState(null)
  const [cancelReason, setCancelReason] = useState('')
  const [cancelError, setCancelError] = useState('')
  const [cancelBusy, setCancelBusy] = useState(false)
  const paymentPendingRef = useRef(false)

  useEffect(() => {
    const controller = new AbortController()
    invoicesApi.detail(token, invoiceId, controller.signal)
      .then((data) => {
        if (controller.signal.aborted) return
        setState(data?.maHoaDon
          ? { status: 'success', data, error: '' }
          : { status: 'error', data: null, error: 'Máy chủ chưa trả chi tiết hóa đơn.' })
      })
      .catch((error) => {
        if (!controller.signal.aborted) setState({ status: 'error', data: null, error: error.status === 0 ? error.message : 'Hiện chưa thể xem chi tiết hóa đơn. Vui lòng thử lại.' })
      })
    return () => controller.abort()
  }, [token, invoiceId, reload])

  useEffect(() => {
    function onKeyDown(event) { if (event.key === 'Escape' && !paymentDraft && !paymentBusy && !cancelTarget) onClose() }
    window.addEventListener('keydown', onKeyDown)
    return () => window.removeEventListener('keydown', onKeyDown)
  }, [onClose, paymentDraft, paymentBusy, cancelTarget])

  async function cancelPayment(event) {
    event.preventDefault()
    if (!cancelTarget || !cancelReason.trim() || cancelBusy) return
    setCancelBusy(true)
    setCancelError('')
    try {
      await invoicesApi.cancelPayment(token, invoiceId, cancelTarget.maThanhToan, cancelReason.trim())
      const paymentId = cancelTarget.maThanhToan
      setCancelTarget(null)
      setCancelReason('')
      setState({ status: 'loading', data: null, error: '' })
      setReload((value) => value + 1)
      onPaymentCancelled(paymentId)
    } catch (error) {
      setCancelError(error.status === 400 || error.status === 0
        ? error.message : 'Chưa hủy được khoản thu. Vui lòng thử lại.')
    } finally {
      setCancelBusy(false)
    }
  }

  async function recordPayment() {
    if (!paymentDraft || paymentPendingRef.current) return
    paymentPendingRef.current = true
    setPaymentBusy(true)
    setPaymentError('')
    try {
      await invoicesApi.recordPayment(token, invoiceId, paymentDraft)
      setPaymentDraft(null)
      setPaymentFormOpen(false)
      setState({ status: 'loading', data: null, error: '' })
      setReload((value) => value + 1)
      onPaymentRecorded(invoiceId)
    } catch (error) {
      setPaymentDraft(null)
      setPaymentError(error.status === 400 || error.status === 0
        ? error.message : 'Chưa ghi nhận được khoản thu. Vui lòng thử lại.')
    } finally {
      paymentPendingRef.current = false
      setPaymentBusy(false)
    }
  }

  const invoice = state.data
  return <><div className="dialog-backdrop" onMouseDown={(event) => { if (event.target === event.currentTarget && !paymentBusy && !paymentDraft && !cancelTarget) onClose() }}>
    <section className="confirm-dialog invoice-detail-dialog" role="dialog" aria-modal="true" aria-labelledby="invoice-detail-title">
      <div className="invoice-detail-heading"><div><h2 id="invoice-detail-title">Hóa đơn {invoiceId}</h2><p>Chi tiết dịch vụ và các khoản đã thu</p></div><button type="button" aria-label="Đóng chi tiết hóa đơn" disabled={paymentBusy || Boolean(paymentDraft) || Boolean(cancelTarget)} onClick={onClose}>×</button></div>
      {state.status === 'loading' && <p className="invoice-detail-feedback" role="status">Đang tải chi tiết hóa đơn…</p>}
      {state.status === 'error' && <div className="invoice-detail-feedback" role="alert"><p>{state.error}</p><button type="button" className="secondary-button" onClick={() => { setState({ status: 'loading', data: null, error: '' }); setReload((value) => value + 1) }}>Thử lại</button></div>}
      {state.status === 'success' && invoice && <>
        <div className="invoice-detail-meta"><div><span>Khách hàng</span><strong>{invoice.tenKhachHang || invoice.maKhachHang}</strong><small>{invoice.maKhachHang}</small></div><div><span>Hồ sơ khám</span><strong>{invoice.maHoSoKham}</strong></div><div><span>Ngày lập</span><strong>{formatDate(invoice.ngayLap)}</strong></div></div>
        <h3>Dịch vụ đã thực hiện</h3>
        <ul className="invoice-detail-lines">{invoice.chiTietHoaDons?.map((line) => <li key={line.maDichVuThucHien}><span>{line.tenDichVu} × {line.soLuong}</span><strong>{moneyFormatter.format(line.thanhTien)}</strong></li>)}</ul>
        <div className="invoice-detail-totals"><div><span>Tổng hóa đơn</span><strong>{moneyFormatter.format(invoice.tongTien)}</strong></div><div><span>Đã thanh toán</span><strong>{moneyFormatter.format(invoice.daThanhToan)}</strong></div><div><span>Còn nợ</span><strong>{moneyFormatter.format(invoice.conNo)}</strong></div></div>
        {invoice.conNo > 0 && invoice.trangThai !== 'DA_HUY' && (paymentFormOpen
          ? <InvoicePaymentForm invoice={invoice} error={paymentError} onErrorDismiss={() => setPaymentError('')}
              onReview={setPaymentDraft} onCancel={() => { setPaymentFormOpen(false); setPaymentError('') }} />
          : <button type="button" className="primary-button invoice-payment-open" onClick={() => { setPaymentError(''); setPaymentFormOpen(true) }}>Thanh toán</button>)}
        <h3>Khoản thu</h3>
        {invoice.thanhToans?.length ? <ul className="invoice-detail-payments">{invoice.thanhToans.map((payment) => <li key={payment.maThanhToan}>
          <div><strong>{payment.maThanhToan} · {moneyFormatter.format(payment.soTien)}</strong><span>{methodLabels[payment.phuongThuc] || payment.phuongThuc} · {formatDate(payment.thoiDiemThanhToan)}</span></div>
          <span className={payment.trangThai === 'DA_HUY' ? 'invoice-payment-cancelled' : 'invoice-payment-confirmed'}>{payment.trangThai === 'DA_HUY' ? 'Đã hủy' : 'Đã xác nhận'}</span>
          {payment.trangThai === 'DA_HUY' && payment.lyDoHuy && <small>Lý do hủy: {payment.lyDoHuy}</small>}
          {payment.trangThai === 'DA_HUY' && payment.nguoiHuy && <small>Người hủy: {payment.nguoiHuy}{payment.huyLuc ? ` · ${formatDate(payment.huyLuc)}` : ''}</small>}
          {managerMode && payment.trangThai === 'DA_XAC_NHAN' && <button type="button" className="invoice-cancel-payment" onClick={() => { setCancelTarget(payment); setCancelReason(''); setCancelError('') }}>Hủy khoản thu</button>}
        </li>)}</ul> : <p className="invoice-detail-empty">Chưa ghi nhận khoản thu nào.</p>}
      </>}
    </section>
  </div>
    {paymentDraft && <ConfirmDialog title="Xác nhận khoản thu?"
      message={`Đã nhận ${moneyFormatter.format(paymentDraft.soTien)} bằng ${methodLabels[paymentDraft.phuongThuc]?.toLocaleLowerCase('vi-VN') || paymentDraft.phuongThuc} cho hóa đơn ${invoiceId}?`}
      confirmLabel="Ghi nhận" busy={paymentBusy} onConfirm={recordPayment} onCancel={() => setPaymentDraft(null)} />}
    {cancelTarget && <div className="dialog-backdrop invoice-cancel-backdrop" onMouseDown={(event) => { if (event.target === event.currentTarget && !cancelBusy) setCancelTarget(null) }}>
      <form className="confirm-dialog invoice-cancel-dialog" role="dialog" aria-modal="true" aria-labelledby="invoice-cancel-title" onSubmit={cancelPayment}>
        <h2 id="invoice-cancel-title">Hủy khoản thu {cancelTarget.maThanhToan}?</h2>
        <p>Khoản thu {moneyFormatter.format(cancelTarget.soTien)} sẽ không còn tính vào số tiền đã thanh toán của hóa đơn {invoiceId}.</p>
        <label htmlFor="invoice-cancel-reason">Lý do hủy</label>
        <textarea id="invoice-cancel-reason" autoFocus required maxLength={2000} rows={3} value={cancelReason}
          onChange={(event) => setCancelReason(event.target.value)} placeholder="Ví dụ: ghi nhận nhầm khoản thu" />
        {cancelError && <p className="invoice-filter-error" role="alert">{cancelError}</p>}
        <div className="confirm-actions"><button type="button" className="secondary-button" disabled={cancelBusy} onClick={() => setCancelTarget(null)}>Quay lại</button>
          <button type="submit" className="primary-button" disabled={cancelBusy || !cancelReason.trim()}>{cancelBusy ? 'Đang hủy…' : 'Xác nhận hủy'}</button></div>
      </form>
    </div>}
  </>
}

export default function ReceptionInvoices({ token, query = '', onNavigate, managerMode = false }) {
  const [paymentFilter, setPaymentFilter] = useState(() => paymentFilterFromQuery(query))
  const [customerInput, setCustomerInput] = useState('')
  const [customerId, setCustomerId] = useState('')
  const [fromDateInput, setFromDateInput] = useState('')
  const [toDateInput, setToDateInput] = useState('')
  const [dateRange, setDateRange] = useState({ fromDate: '', toDate: '' })
  const [filterError, setFilterError] = useState('')
  const [page, setPage] = useState(1)
  const [reload, setReload] = useState(0)
  const [state, setState] = useState({ status: 'loading', items: [], totalItems: 0, error: '' })
  const [selectedId, setSelectedId] = useState(null)
  const [showCreation, setShowCreation] = useState(false)
  const [notice, setNotice] = useState('')
  const basePath = managerMode ? '/quan-ly/hoa-don' : '/le-tan/hoa-don'

  useEffect(() => { queueMicrotask(() => { setPaymentFilter(paymentFilterFromQuery(query)); setPage(1) }) }, [query])

  useEffect(() => {
    const controller = new AbortController()
    queueMicrotask(() => { if (!controller.signal.aborted) setState({ status: 'loading', items: [], totalItems: 0, error: '' }) })
    invoicesApi.list(token, { page, pageSize, paymentFilter, customerId, ...dateRange }, controller.signal)
      .then((result) => {
        if (controller.signal.aborted) return
        if (!Array.isArray(result?.items) || !Number.isInteger(result.totalItems)) {
          setState({ status: 'error', items: [], totalItems: 0, error: 'Máy chủ chưa trả danh sách hóa đơn.' })
          return
        }
        const lastPage = Math.max(1, Math.ceil(result.totalItems / pageSize))
        if (page > lastPage) { setPage(lastPage); return }
        setState({ status: 'success', items: result.items, totalItems: result.totalItems, error: '' })
      })
      .catch((error) => {
        if (!controller.signal.aborted) setState({ status: 'error', items: [], totalItems: 0, error: error.status === 0 ? error.message : 'Hiện chưa thể hiển thị hóa đơn. Vui lòng thử lại.' })
      })
    return () => controller.abort()
  }, [token, page, paymentFilter, customerId, dateRange, reload])

  function refresh() { setReload((value) => value + 1) }

  function selectPaymentFilter(value) {
    setPage(1)
    setPaymentFilter(value)
    const path = value === 'unpaid' ? `${basePath}?conNo=true`
      : value === 'paid' ? `${basePath}?trangThai=DA_THANH_TOAN`
        : basePath
    if (`${basePath}${query ? `?${query}` : ''}` !== path) onNavigate(path, true)
  }

  function applyFilters(event) {
    event.preventDefault()
    if (fromDateInput && toDateInput && fromDateInput > toDateInput) {
      setFilterError('Từ ngày không được sau đến ngày.')
      return
    }
    setFilterError('')
    setPage(1)
    setCustomerId(customerInput.trim().toUpperCase())
    setDateRange({ fromDate: fromDateInput, toDate: toDateInput })
  }

  function clearFilters() {
    setPaymentFilter('all')
    setCustomerInput('')
    setCustomerId('')
    setFromDateInput('')
    setToDateInput('')
    setDateRange({ fromDate: '', toDate: '' })
    setFilterError('')
    setPage(1)
    if (query) onNavigate(basePath, true)
  }

  return <div className="reception-invoices">
    <div className="page-heading invoice-page-heading"><div><h1>Hóa đơn</h1><p className="page-description">Theo dõi tổng tiền, các khoản đã thu và số tiền còn nợ.</p></div><button type="button" className="primary-button" onClick={() => setShowCreation(true)}>Lập hóa đơn</button></div>
    {showCreation && <InvoiceCreation token={token} onClose={() => setShowCreation(false)} onCreated={(invoiceId) => {
      setShowCreation(false)
      refresh()
      setNotice(`Đã lập hóa đơn ${invoiceId || ''}.`)
      if (invoiceId) setSelectedId(invoiceId)
    }} />}
    <div className="invoice-filters">
      <div className="invoice-tabs" role="group" aria-label="Lọc hóa đơn">
        <button type="button" className={paymentFilter === 'all' ? 'active' : ''} aria-pressed={paymentFilter === 'all'} onClick={() => selectPaymentFilter('all')}>Tất cả</button>
        <button type="button" className={paymentFilter === 'unpaid' ? 'active' : ''} aria-pressed={paymentFilter === 'unpaid'} onClick={() => selectPaymentFilter('unpaid')}>Còn nợ</button>
        <button type="button" className={paymentFilter === 'paid' ? 'active' : ''} aria-pressed={paymentFilter === 'paid'} onClick={() => selectPaymentFilter('paid')}>Đã thanh toán</button>
      </div>
      <form onSubmit={applyFilters} className="invoice-filter-form">
        <label htmlFor="invoice-customer-id">Mã khách hàng
          <input id="invoice-customer-id" type="search" value={customerInput} onChange={(event) => setCustomerInput(event.target.value)} placeholder="Ví dụ: KH_0001" />
        </label>
        <label htmlFor="invoice-from-date">Từ ngày
          <input id="invoice-from-date" type="date" value={fromDateInput} max={toDateInput || undefined} onChange={(event) => { setFromDateInput(event.target.value); setFilterError('') }} />
        </label>
        <label htmlFor="invoice-to-date">Đến ngày
          <input id="invoice-to-date" type="date" value={toDateInput} min={fromDateInput || undefined} onChange={(event) => { setToDateInput(event.target.value); setFilterError('') }} />
        </label>
        <button type="submit" className="secondary-button">Lọc</button>
        <button type="button" className="invoice-clear-filters" onClick={clearFilters}>Xóa bộ lọc</button>
      </form>
    </div>
    {filterError && <p className="invoice-filter-error" role="alert">{filterError}</p>}
    {state.status === 'loading' && <div className="invoice-feedback" role="status">Đang tải hóa đơn…</div>}
    {state.status === 'error' && <div className="invoice-feedback" role="alert"><strong>Không tải được hóa đơn.</strong><p>{state.error}</p><button type="button" className="primary-button" onClick={refresh}>Thử lại</button></div>}
    {state.status === 'success' && <>
      <div className="invoice-count">{state.totalItems} hóa đơn{paymentFilter === 'unpaid' ? ' còn nợ' : paymentFilter === 'paid' ? ' đã thanh toán' : ''}{customerId ? ` của ${customerId}` : ''}{dateRange.fromDate || dateRange.toDate ? ' trong khoảng ngày đã chọn' : ''}</div>
      {state.items.length ? <div className="invoice-list">{state.items.map((invoice) => <article className="invoice-card" key={invoice.maHoaDon}>
        <div className="invoice-card-heading"><div><span>{invoice.maHoaDon}</span><h2>{invoice.tenKhachHang || invoice.maKhachHang}</h2><small>{invoice.maKhachHang} · Lập {formatDate(invoice.ngayLap)} · Hồ sơ {invoice.maHoSoKham}</small></div><span className={`invoice-status invoice-status-${invoice.trangThai}`}>{statusLabels[invoice.trangThai] || invoice.trangThai}</span></div>
        <div className="invoice-card-amounts"><div><span>Tổng tiền</span><strong>{moneyFormatter.format(invoice.tongTien)}</strong></div><div><span>Đã thanh toán</span><strong>{moneyFormatter.format(invoice.daThanhToan)}</strong></div><div><span>Còn nợ</span><strong>{moneyFormatter.format(invoice.conNo)}</strong></div></div>
        <button type="button" className="invoice-detail-button" onClick={() => setSelectedId(invoice.maHoaDon)}>Xem chi tiết</button>
      </article>)}</div> : <div className="invoice-feedback">Không có hóa đơn nào phù hợp.</div>}
      <ListPagination page={page} pageSize={pageSize} totalItems={state.totalItems} onPageChange={setPage} />
    </>}
    {selectedId && <InvoiceDetails key={selectedId} token={token} invoiceId={selectedId}
      managerMode={managerMode} onClose={() => setSelectedId(null)}
      onPaymentRecorded={(invoiceId) => { refresh(); setNotice(`Đã ghi nhận thanh toán cho hóa đơn ${invoiceId}.`) }}
      onPaymentCancelled={(paymentId) => { refresh(); setNotice(`Đã hủy khoản thu ${paymentId}.`) }} />}
    <ToastNotice message={notice} onDismiss={() => setNotice('')} />
  </div>
}
