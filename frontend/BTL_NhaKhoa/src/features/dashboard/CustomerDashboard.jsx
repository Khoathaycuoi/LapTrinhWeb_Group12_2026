import { useEffect, useState } from 'react'
import { dashboardApi } from './dashboardApi'
import './CustomerDashboard.css'

const moneyFormatter = new Intl.NumberFormat('vi-VN', {
  style: 'currency',
  currency: 'VND',
  maximumFractionDigits: 0,
})

const dateFormatter = new Intl.DateTimeFormat('vi-VN', { dateStyle: 'medium' })
const dateTimeFormatter = new Intl.DateTimeFormat('vi-VN', { dateStyle: 'medium', timeStyle: 'short' })

function formatMoney(value) {
  return typeof value === 'number' ? moneyFormatter.format(value) : '—'
}

function formatDate(value, withTime = false) {
  if (!value) return '—'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return '—'
  return (withTime ? dateTimeFormatter : dateFormatter).format(date)
}

function appointmentStatus(status) {
  if (status === 'CHO_XAC_NHAN') return ['Chờ xác nhận', 'pending']
  if (status === 'DA_XAC_NHAN') return ['Đã xác nhận', 'confirmed']
  return [status || 'Chưa rõ', 'neutral']
}

function SectionLink({ children, onClick }) {
  return <button type="button" className="dashboard-section-link" onClick={onClick}>{children} <span aria-hidden="true">→</span></button>
}

export default function CustomerDashboard({ token, onNavigate }) {
  const [reload, setReload] = useState(0)
  const [state, setState] = useState({ status: 'loading', data: null, error: '' })

  useEffect(() => {
    const controller = new AbortController()
    let active = true

    dashboardApi.customer(token, controller.signal)
      .then((data) => {
        if (!active) return
        setState(data
          ? { status: 'success', data, error: '' }
          : { status: 'error', data: null, error: 'Máy chủ chưa trả dữ liệu tổng quan.' })
      })
      .catch((error) => {
        if (active) setState({ status: 'error', data: null, error: error.message })
      })

    return () => {
      active = false
      controller.abort()
    }
  }, [token, reload])

  const data = state.data
  const appointment = data?.lichSapToiGanNhat
  const exam = data?.lanKhamGanNhat
  const invoices = data?.hoaDonConNoGanNhat ?? []
  const [statusLabel, statusTone] = appointmentStatus(appointment?.trangThai)

  return <div className="customer-dashboard">
    <div className="page-heading">
      <div><h1>Tổng quan</h1><p className="page-description">Lịch hẹn, hóa đơn và lần khám gần nhất của bạn tại Nha Khoa G12.</p></div>
    </div>

    {state.status === 'loading' && <div className="dashboard-feedback" role="status">Đang tải thông tin tổng quan…</div>}

    {state.status === 'error' && <div className="dashboard-feedback dashboard-feedback-error" role="alert">
      <strong>Không tải được thông tin tổng quan.</strong>
      <p>{state.error}</p>
      <button type="button" className="primary-button" onClick={() => { setState({ status: 'loading', data: null, error: '' }); setReload((value) => value + 1) }}>Thử lại</button>
    </div>}

    {state.status === 'success' && data && <>
      <div className="dashboard-metrics">
        <button type="button" className="dashboard-metric" onClick={() => onNavigate('/khach-hang/lich-hen?nhom=upcoming')}>
          <span className="dashboard-metric-label">Lịch hẹn sắp tới</span>
          <strong>{data.soLichSapToi}</strong>
          <span className="dashboard-metric-caption">Xem lịch hẹn <span aria-hidden="true">→</span></span>
        </button>
        <button type="button" className="dashboard-metric" onClick={() => onNavigate('/khach-hang/hoa-don')}>
          <span className="dashboard-metric-label">Hóa đơn còn nợ</span>
          <strong>{data.soHoaDonConNo}</strong>
          <span className="dashboard-metric-caption">Tổng còn nợ: {formatMoney(data.tongCongNo)}</span>
        </button>
      </div>

      <div className="dashboard-panels">
        <section className="dashboard-panel">
          <div className="dashboard-panel-heading"><h2>Lịch hẹn gần nhất</h2><SectionLink onClick={() => onNavigate('/khach-hang/lich-hen?nhom=upcoming')}>Xem lịch hẹn</SectionLink></div>
          {appointment ? <div className="dashboard-appointment">
            <span className={`dashboard-status dashboard-status-${statusTone}`}>{statusLabel}</span>
            <strong>{formatDate(appointment.thoiGianBatDau, true)}</strong>
            <span>Mã lịch hẹn: {appointment.maLichHen}</span>
            <span>{appointment.tenBacSi ? `Bác sĩ: ${appointment.tenBacSi}` : 'Chưa phân công bác sĩ'}</span>
          </div> : <p className="dashboard-empty">Bạn chưa có lịch hẹn sắp tới.</p>}
        </section>

        <section className="dashboard-panel">
          <div className="dashboard-panel-heading"><h2>Hóa đơn còn nợ</h2><SectionLink onClick={() => onNavigate('/khach-hang/hoa-don')}>Xem hóa đơn</SectionLink></div>
          {invoices.length > 0 ? <ul className="dashboard-invoices">{invoices.map((invoice) => <li key={invoice.maHoaDon}>
            <div><strong>{invoice.maHoaDon}</strong><span>Lập ngày {formatDate(invoice.ngayLap)}</span></div>
            <div><strong>{formatMoney(invoice.conNo)}</strong><span>Còn nợ</span></div>
          </li>)}</ul> : <p className="dashboard-empty">Bạn không có hóa đơn còn nợ.</p>}
        </section>

        <section className="dashboard-panel dashboard-exam-panel">
          <div className="dashboard-panel-heading"><h2>Lần khám gần nhất</h2><SectionLink onClick={() => onNavigate('/khach-hang/lich-su-kham')}>Xem lịch sử khám</SectionLink></div>
          {exam ? <div className="dashboard-exam">
            <div className="dashboard-exam-meta"><strong>{formatDate(exam.ngayKham)}</strong><span>Mã hồ sơ: {exam.maHoSoKham}</span></div>
            <p>{exam.tomTat || 'Chưa có tóm tắt lần khám.'}</p>
          </div> : <p className="dashboard-empty">Bạn chưa có lần khám đã chốt.</p>}
        </section>
      </div>
    </>}
  </div>
}
