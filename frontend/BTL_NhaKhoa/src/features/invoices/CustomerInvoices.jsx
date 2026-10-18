import { useEffect, useState } from 'react'
import ListPagination from '../../components/ListPagination'
import { invoicesApi } from './invoicesApi'
import '../../components/ConfirmDialog.css'
import './CustomerInvoices.css'

const pageSize = 10
const money = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND', maximumFractionDigits: 0 })
const dateTime = new Intl.DateTimeFormat('vi-VN', { dateStyle: 'medium', timeStyle: 'short' })
const statusLabels = {
  CHUA_THANH_TOAN: 'Chưa thanh toán',
  THANH_TOAN_MOT_PHAN: 'Thanh toán một phần',
  DA_THANH_TOAN: 'Đã thanh toán',
  DA_HUY: 'Đã hủy',
}
const methodLabels = { TIEN_MAT: 'Tiền mặt', CHUYEN_KHOAN: 'Chuyển khoản', KHAC: 'Khác' }
const filters = [['all', 'Tất cả'], ['unpaid', 'Còn nợ'], ['paid', 'Đã thanh toán']]

function formatDate(value) {
  if (!value) return '—'
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? '—' : dateTime.format(date)
}

function filterFromQuery(query) {
  const params = new URLSearchParams(query)
  if (params.get('conNo') === 'true') return 'unpaid'
  if (params.get('trangThai') === 'DA_THANH_TOAN') return 'paid'
  return 'all'
}

function InvoiceDetail({ token, invoiceId, onClose }) {
  const [reload, setReload] = useState(0)
  const [state, setState] = useState({ status: 'loading', invoice: null, error: '' })

  useEffect(() => {
    const controller = new AbortController()
    setState({ status: 'loading', invoice: null, error: '' })
    invoicesApi.detail(token, invoiceId, controller.signal).then((invoice) => {
      if (controller.signal.aborted) return
      setState(invoice?.maHoaDon
        ? { status: 'success', invoice, error: '' }
        : { status: 'error', invoice: null, error: 'Chưa tải được chi tiết hóa đơn.' })
    }).catch((error) => {
      if (!controller.signal.aborted) setState({ status: 'error', invoice: null,
        error: error.status === 0 ? error.message : 'Hiện chưa thể xem chi tiết hóa đơn.' })
    })
    return () => controller.abort()
  }, [token, invoiceId, reload])

  useEffect(() => {
    function onKeyDown(event) { if (event.key === 'Escape') onClose() }
    window.addEventListener('keydown', onKeyDown)
    return () => window.removeEventListener('keydown', onKeyDown)
  }, [onClose])

  const invoice = state.invoice
  return <div className="dialog-backdrop" onMouseDown={(event) => { if (event.target === event.currentTarget) onClose() }}>
    <section className="confirm-dialog customer-invoice-dialog" role="dialog" aria-modal="true" aria-labelledby="customer-invoice-title">
      <div className="customer-invoice-dialog-heading"><h2 id="customer-invoice-title">Hóa đơn {invoiceId}</h2><button type="button" aria-label="Đóng chi tiết hóa đơn" onClick={onClose}>×</button></div>
      {state.status === 'loading' && <p role="status">Đang tải chi tiết hóa đơn…</p>}
      {state.status === 'error' && <div role="alert"><p>{state.error}</p><button type="button" className="secondary-button" onClick={() => setReload((value) => value + 1)}>Thử lại</button></div>}
      {invoice && <>
        <p className="customer-invoice-date">Lập ngày {formatDate(invoice.ngayLap)} · Hồ sơ {invoice.maHoSoKham}</p>
        <span className={`customer-invoice-status customer-invoice-status-${invoice.trangThai}`}>{statusLabels[invoice.trangThai] || invoice.trangThai}</span>
        <h3>Dịch vụ đã thực hiện</h3>
        <ul className="customer-invoice-lines">{invoice.chiTietHoaDons?.map((line) => <li key={line.maDichVuThucHien}><span>{line.tenDichVu} × {line.soLuong}<small>{money.format(line.donGia)}/lần</small></span><strong>{money.format(line.thanhTien)}</strong></li>)}</ul>
        <div className="customer-invoice-totals"><div><span>Tổng tiền</span><strong>{money.format(invoice.tongTien)}</strong></div><div><span>Đã thanh toán</span><strong>{money.format(invoice.daThanhToan)}</strong></div><div><span>Còn nợ</span><strong>{money.format(invoice.conNo)}</strong></div></div>
        {invoice.conNo > 0 && invoice.trangThai !== 'DA_HUY' && <div className="customer-invoice-transfer"><img src="/payment-qr.jpg" alt="Mã QR chuyển khoản của phòng khám" /><div><strong>Chuyển khoản</strong><span>Mã hóa đơn: {invoice.maHoaDon}</span><span>Số tiền còn nợ: {money.format(invoice.conNo)}</span><small>Nhập mã hóa đơn vào nội dung chuyển khoản. Lễ tân sẽ ghi nhận sau khi kiểm tra tiền vào.</small></div></div>}
        <h3>Các khoản đã ghi nhận</h3>
        {invoice.thanhToans?.length ? <ul className="customer-invoice-payments">{invoice.thanhToans.map((payment) => <li key={payment.maThanhToan}><div><strong>{money.format(payment.soTien)}</strong><span>{methodLabels[payment.phuongThuc] || payment.phuongThuc} · {formatDate(payment.thoiDiemThanhToan)}</span>{payment.trangThai === 'DA_HUY' && payment.lyDoHuy && <small>Lý do hủy: {payment.lyDoHuy}</small>}</div><span className={payment.trangThai === 'DA_HUY' ? 'cancelled' : 'confirmed'}>{payment.trangThai === 'DA_HUY' ? 'Đã hủy' : 'Đã xác nhận'}</span></li>)}</ul> : <p>Chưa có khoản thanh toán nào được ghi nhận.</p>}
      </>}
    </section>
  </div>
}

export default function CustomerInvoices({ token, query = '', onNavigate }) {
  const [filter, setFilter] = useState(() => filterFromQuery(query))
  const [page, setPage] = useState(1)
  const [reload, setReload] = useState(0)
  const [state, setState] = useState({ status: 'loading', items: [], totalItems: 0, error: '' })
  const [selectedId, setSelectedId] = useState(null)

  useEffect(() => { setFilter(filterFromQuery(query)); setPage(1) }, [query])

  useEffect(() => {
    const controller = new AbortController()
    setState({ status: 'loading', items: [], totalItems: 0, error: '' })
    invoicesApi.list(token, { page, pageSize, paymentFilter: filter }, controller.signal).then((result) => {
      if (controller.signal.aborted) return
      if (!Array.isArray(result?.items) || !Number.isInteger(result.totalItems)) {
        setState({ status: 'error', items: [], totalItems: 0, error: 'Chưa tải được danh sách hóa đơn.' })
        return
      }
      const lastPage = Math.max(1, Math.ceil(result.totalItems / pageSize))
      if (page > lastPage) { setPage(lastPage); return }
      setState({ status: 'success', items: result.items, totalItems: result.totalItems, error: '' })
    }).catch((error) => {
      if (!controller.signal.aborted) setState({ status: 'error', items: [], totalItems: 0,
        error: error.status === 0 ? error.message : 'Hiện chưa thể hiển thị hóa đơn.' })
    })
    return () => controller.abort()
  }, [token, page, filter, reload])

  function selectFilter(value) {
    setFilter(value)
    setPage(1)
    const path = value === 'unpaid' ? '/khach-hang/hoa-don?conNo=true'
      : value === 'paid' ? '/khach-hang/hoa-don?trangThai=DA_THANH_TOAN'
        : '/khach-hang/hoa-don'
    if (`/khach-hang/hoa-don${query ? `?${query}` : ''}` !== path) onNavigate(path, true)
  }

  return <div className="customer-invoices">
    <div className="page-heading"><div><h1>Hóa đơn</h1><p className="page-description">Dịch vụ đã thực hiện và các khoản thanh toán của bạn.</p></div></div>
    <div className="customer-invoice-tabs" role="group" aria-label="Lọc hóa đơn">{filters.map(([value, label]) => <button key={value} type="button" className={filter === value ? 'active' : ''} aria-pressed={filter === value} onClick={() => selectFilter(value)}>{label}</button>)}</div>
    {state.status === 'loading' && <div className="customer-invoice-feedback" role="status">Đang tải hóa đơn…</div>}
    {state.status === 'error' && <div className="customer-invoice-feedback" role="alert"><p>{state.error}</p><button type="button" className="secondary-button" onClick={() => setReload((value) => value + 1)}>Thử lại</button></div>}
    {state.status === 'success' && <><p className="customer-invoice-count">{state.totalItems} hóa đơn</p>
      {state.items.length ? <div className="customer-invoice-list">{state.items.map((invoice) => <article key={invoice.maHoaDon} className="customer-invoice-card"><div className="customer-invoice-card-heading"><div><small>{invoice.maHoaDon}</small><h2>Lập ngày {formatDate(invoice.ngayLap)}</h2><span>Hồ sơ {invoice.maHoSoKham}</span></div><span className={`customer-invoice-status customer-invoice-status-${invoice.trangThai}`}>{statusLabels[invoice.trangThai] || invoice.trangThai}</span></div><div className="customer-invoice-amounts"><div><span>Tổng tiền</span><strong>{money.format(invoice.tongTien)}</strong></div><div><span>Đã thanh toán</span><strong>{money.format(invoice.daThanhToan)}</strong></div><div><span>Còn nợ</span><strong>{money.format(invoice.conNo)}</strong></div></div><button type="button" className="customer-invoice-open" onClick={() => setSelectedId(invoice.maHoaDon)}>Xem chi tiết</button></article>)}</div> : <div className="customer-invoice-feedback">Bạn chưa có hóa đơn{filter === 'unpaid' ? ' còn nợ' : filter === 'paid' ? ' đã thanh toán' : ''}.</div>}
      <ListPagination page={page} pageSize={pageSize} totalItems={state.totalItems} onPageChange={setPage} />
    </>}
    {selectedId && <InvoiceDetail key={selectedId} token={token} invoiceId={selectedId} onClose={() => setSelectedId(null)} />}
  </div>
}
