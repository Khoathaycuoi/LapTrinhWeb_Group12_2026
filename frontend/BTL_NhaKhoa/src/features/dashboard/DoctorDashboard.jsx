import { useEffect, useState } from 'react'
import { dashboardApi } from './dashboardApi'
import './DashboardCommon.css'
import './DoctorDashboard.css'

const timeFormatter = new Intl.DateTimeFormat('vi-VN', { hour: '2-digit', minute: '2-digit' })
const statuses = { CHO_XAC_NHAN: 'Chờ xác nhận', DA_XAC_NHAN: 'Đã xác nhận', DA_DEN: 'Đã đến', DANG_KHAM: 'Đang khám', DA_HOAN_THANH: 'Đã hoàn thành', TU_CHOI: 'Từ chối', DA_HUY: 'Đã hủy', KHONG_DEN: 'Không đến', HET_HAN: 'Hết hạn' }

function LinkCard({ href, onNavigate, children, className = '' }) {
  return <a href={href} className={`dashboard-card-link ${className}`} onClick={(event) => {
    if (event.defaultPrevented || event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) return
    event.preventDefault()
    onNavigate(href)
  }}>{children}</a>
}

export default function DoctorDashboard({ token, onNavigate }) {
  const [reload, setReload] = useState(0)
  const [state, setState] = useState({ status: 'loading', data: null, error: '' })

  useEffect(() => {
    const controller = new AbortController()
    dashboardApi.doctor(token, controller.signal).then((data) => {
      if (controller.signal.aborted) return
      if (!data || !Array.isArray(data.lichHomNay)) throw new Error('Chưa tải được thông tin tổng quan.')
      setState({ status: 'success', data, error: '' })
    }).catch((error) => { if (!controller.signal.aborted) setState({ status: 'error', data: null, error: error.message }) })
    return () => controller.abort()
  }, [token, reload])

  const data = state.data
  const cards = data ? [
    ['Lịch hôm nay', data.soLichHomNay, '/bac-si/lich-hen'],
    ['Chờ xác nhận', data.soLichChoXacNhan, '/bac-si/lich-hen?nhom=pending'],
    ['Khách đã đến', data.soKhachChoKham, '/bac-si/lich-hen?nhom=arrived'],
    ['Đang khám', data.soCaDangKham, '/bac-si/lich-hen?nhom=examining'],
    ['Hồ sơ chưa chốt', data.soHoSoChuaChot, '/bac-si/ho-so-kham?chuaChot=true'],
  ] : []

  return <div className="doctor-dashboard">
    <div className="page-heading"><div><h1>Tổng quan</h1><p className="page-description">Lịch hẹn và lượt khám bạn phụ trách hôm nay.</p></div></div>
    {state.status === 'loading' && <div className="dashboard-feedback" role="status">Đang tải tổng quan…</div>}
    {state.status === 'error' && <div className="dashboard-feedback dashboard-feedback-error" role="alert"><p>{state.error}</p><button type="button" className="primary-button" onClick={() => { setState({ status: 'loading', data: null, error: '' }); setReload((value) => value + 1) }}>Thử lại</button></div>}
    {data && <><div className="doctor-dashboard-metrics">{cards.map(([label, value, href]) => <LinkCard key={label} href={href} onNavigate={onNavigate} className="dashboard-metric"><span className="dashboard-metric-label">{label}</span><strong>{value}</strong><span className="dashboard-card-arrow" aria-hidden="true">↗</span></LinkCard>)}</div>
      <section className="dashboard-panel doctor-dashboard-schedule"><div className="dashboard-panel-heading"><h2>Lịch hôm nay</h2></div>
        {data.lichHomNay.length === 0 ? <p className="dashboard-empty">Hôm nay chưa có lịch được phân công.</p> : <ul>{data.lichHomNay.map((item) => <li key={item.maLichHen}><LinkCard href="/bac-si/lich-hen" onNavigate={onNavigate} className="doctor-dashboard-appointment"><span className="doctor-dashboard-time">{timeFormatter.format(new Date(item.thoiGianBatDau))}</span><span><strong>{item.tenKhachHang || item.maKhachHang}</strong><small>{item.maLichHen} · {statuses[item.trangThai] || item.trangThai}</small></span><span className="dashboard-card-arrow" aria-hidden="true">↗</span></LinkCard></li>)}</ul>}
      </section></>}
  </div>
}
