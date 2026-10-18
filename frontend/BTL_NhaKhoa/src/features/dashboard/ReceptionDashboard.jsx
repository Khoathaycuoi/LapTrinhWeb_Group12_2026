import { useEffect, useState } from 'react'
import { dashboardApi } from './dashboardApi'
import './DashboardCommon.css'
import './ReceptionDashboard.css'

const timeFormatter = new Intl.DateTimeFormat('vi-VN', { hour: '2-digit', minute: '2-digit' })

function appointmentTime(value) {
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? '—' : timeFormatter.format(date)
}

function NavigateCard({ href, label, onNavigate, className, children }) {
  function handleClick(event) {
    if (event.defaultPrevented || event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) return
    event.preventDefault()
    onNavigate(href)
  }

  return <a href={href} className={`dashboard-card-link ${className}`} aria-label={label} onClick={handleClick}>{children}</a>
}

function AppointmentPanel({ title, appointments, emptyText, href, onNavigate }) {
  return <NavigateCard href={href} className="dashboard-panel reception-dashboard-panel" label={`Mở ${title.toLocaleLowerCase('vi-VN')}`} onNavigate={onNavigate}>
    <div className="dashboard-panel-heading"><h2>{title}</h2><span className="dashboard-card-arrow" aria-hidden="true">↗</span></div>
    {appointments.length ? <ul className="reception-dashboard-list">{appointments.map((appointment) => <li key={appointment.maLichHen}>
      <span className="reception-dashboard-time">{appointmentTime(appointment.thoiGianBatDau)}</span>
      <span><strong>{appointment.tenKhachHang || appointment.maKhachHang}</strong><small>{appointment.maLichHen} · {appointment.tenBacSi || 'Chưa phân công bác sĩ'}</small></span>
    </li>)}</ul> : <p className="dashboard-empty">{emptyText}</p>}
  </NavigateCard>
}

export default function ReceptionDashboard({ token, onNavigate }) {
  const [reload, setReload] = useState(0)
  const [state, setState] = useState({ status: 'loading', data: null, error: '' })

  useEffect(() => {
    const controller = new AbortController()
    dashboardApi.reception(token, controller.signal)
      .then((data) => {
        if (controller.signal.aborted) return
        if (!data || !Array.isArray(data.lichChoXacNhanGanNhat) || !Array.isArray(data.lichSapDienRa)) {
          setState({ status: 'error', data: null, error: 'Máy chủ chưa trả dữ liệu tổng quan.' })
          return
        }
        setState({ status: 'success', data, error: '' })
      })
      .catch((error) => {
        if (!controller.signal.aborted) setState({ status: 'error', data: null, error: error.message })
      })
    return () => controller.abort()
  }, [token, reload])

  const data = state.data
  const todayPath = data?.ngay ? `/le-tan/lich-hen?${new URLSearchParams({ ngayHen: data.ngay, nhom: 'tracked' })}` : '/le-tan/lich-hen'

  return <div className="reception-dashboard">
    <div className="page-heading"><div><h1>Tổng quan</h1><p className="page-description">Lịch hẹn và khách cần tiếp nhận tại Nha Khoa G12.</p></div></div>

    {state.status === 'loading' && <div className="dashboard-feedback" role="status">Đang tải thông tin tổng quan…</div>}
    {state.status === 'error' && <div className="dashboard-feedback dashboard-feedback-error" role="alert">
      <strong>Không tải được thông tin tổng quan.</strong><p>{state.error}</p>
      <button type="button" className="primary-button" onClick={() => { setState({ status: 'loading', data: null, error: '' }); setReload((value) => value + 1) }}>Thử lại</button>
    </div>}

    {state.status === 'success' && data && <>
      <div className="reception-dashboard-metrics">
        <NavigateCard href="/le-tan/lich-hen" className="dashboard-metric" label="Xem lịch chờ xác nhận" onNavigate={onNavigate}>
          <span className="dashboard-metric-label">Chờ xác nhận</span><strong>{data.soLichChoXacNhan}</strong><span className="dashboard-card-arrow" aria-hidden="true">↗</span>
        </NavigateCard>
        <NavigateCard href={todayPath} className="dashboard-metric" label="Xem lịch hẹn hôm nay" onNavigate={onNavigate}>
          <span className="dashboard-metric-label">Lịch hôm nay</span><strong>{data.soLichHomNay}</strong><span className="dashboard-card-arrow" aria-hidden="true">↗</span>
        </NavigateCard>
        <NavigateCard href="/le-tan/tiep-nhan?nhom=arrived" className="dashboard-metric" label="Xem khách đã đến chờ khám" onNavigate={onNavigate}>
          <span className="dashboard-metric-label">Khách đã đến chờ khám</span><strong>{data.soKhachChoKham}</strong><span className="dashboard-card-arrow" aria-hidden="true">↗</span>
        </NavigateCard>
        <NavigateCard href="/le-tan/hoa-don?conNo=true" className="dashboard-metric" label="Xem hóa đơn còn nợ" onNavigate={onNavigate}>
          <span className="dashboard-metric-label">Hóa đơn còn nợ</span><strong>{data.soHoaDonConNo}</strong><span className="dashboard-card-arrow" aria-hidden="true">↗</span>
        </NavigateCard>
      </div>

      <div className="dashboard-panels">
        <AppointmentPanel title="Lịch chờ xác nhận gần nhất" appointments={data.lichChoXacNhanGanNhat} emptyText="Không có lịch đang chờ xác nhận." href="/le-tan/lich-hen" onNavigate={onNavigate} />
        <AppointmentPanel title="Lịch sắp diễn ra hôm nay" appointments={data.lichSapDienRa} emptyText="Không có lịch đã xác nhận sắp diễn ra hôm nay." href="/le-tan/tiep-nhan?nhom=upcoming" onNavigate={onNavigate} />
      </div>
    </>}
  </div>
}
