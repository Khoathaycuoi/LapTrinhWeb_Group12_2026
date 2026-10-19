import { useEffect, useState } from 'react'
import { dashboardApi } from './dashboardApi'
import './DashboardCommon.css'
import './ManagerDashboard.css'

const numberFormatter = new Intl.NumberFormat('vi-VN')
const moneyFormatter = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND', maximumFractionDigits: 0 })
const shortDateFormatter = new Intl.DateTimeFormat('vi-VN', { day: '2-digit', month: '2-digit' })

const appointmentStatuses = {
  CHO_XAC_NHAN: 'Chờ xác nhận',
  DA_XAC_NHAN: 'Đã xác nhận',
  DA_DEN: 'Đã đến',
  DANG_KHAM: 'Đang khám',
  DA_HOAN_THANH: 'Hoàn thành',
  KHONG_DEN: 'Không đến',
  DA_HUY: 'Đã hủy',
  TU_CHOI: 'Từ chối',
  HET_HAN: 'Hết hạn',
}

function formatShortDate(value) {
  if (!/^\d{4}-\d{2}-\d{2}$/.test(value ?? '')) return '—'
  const date = new Date(`${value}T00:00:00`)
  return Number.isNaN(date.getTime()) ? '—' : shortDateFormatter.format(date)
}

function Metric({ label, value, description, money = false, href, onNavigate }) {
  const content = <>
    <span className="dashboard-metric-label">{label}</span>
    <strong>{money ? moneyFormatter.format(value) : numberFormatter.format(value)}</strong>
    <span className="manager-dashboard-caption">{description}</span>
  </>
  if (!href) return <div className="dashboard-metric manager-dashboard-metric">{content}</div>
  return <a href={href} className="dashboard-card-link dashboard-metric manager-dashboard-metric"
    onClick={(event) => {
      if (event.defaultPrevented || event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) return
      event.preventDefault()
      onNavigate(href)
    }}>{content}</a>
}

function RevenueChart({ weeks }) {
  const maximum = Math.max(0, ...weeks.map((week) => week.soTien))
  return <section className="dashboard-panel manager-dashboard-panel" aria-labelledby="manager-revenue-title">
    <div className="dashboard-panel-heading"><div><h2 id="manager-revenue-title">Doanh thu 4 tuần</h2><p>Khoản thu đã xác nhận, tính theo tuần từ thứ Hai.</p></div></div>
    {weeks.length ? <div className="manager-dashboard-bars">
      {weeks.map((week) => <div className="manager-dashboard-bar-row" key={week.tuanBatDau}>
        <span className="manager-dashboard-bar-label">{formatShortDate(week.tuanBatDau)} - {formatShortDate(week.tuanKetThuc)}</span>
        <div className="manager-dashboard-bar-track" aria-hidden="true"><span style={{ width: `${maximum > 0 ? week.soTien / maximum * 100 : 0}%` }} /></div>
        <strong>{moneyFormatter.format(week.soTien)}</strong>
      </div>)}
    </div> : <p className="dashboard-empty">Chưa có dữ liệu doanh thu.</p>}
  </section>
}

function AppointmentChart({ statuses }) {
  const visible = statuses.filter((item) => item.soLuong > 0)
  const maximum = Math.max(0, ...visible.map((item) => item.soLuong))
  return <section className="dashboard-panel manager-dashboard-panel" aria-labelledby="manager-appointment-title">
    <div className="dashboard-panel-heading"><div><h2 id="manager-appointment-title">Trạng thái lịch hẹn 7 ngày</h2><p>Lịch có ngày hẹn trong 7 ngày gần đây.</p></div></div>
    {visible.length ? <div className="manager-dashboard-bars">
      {visible.map((item) => <div className="manager-dashboard-bar-row" key={item.trangThai}>
        <span className="manager-dashboard-bar-label">{appointmentStatuses[item.trangThai] ?? item.trangThai}</span>
        <div className="manager-dashboard-bar-track manager-dashboard-appointment-track" aria-hidden="true"><span style={{ width: `${item.soLuong / maximum * 100}%` }} /></div>
        <strong>{numberFormatter.format(item.soLuong)}</strong>
      </div>)}
    </div> : <p className="dashboard-empty">Chưa có lịch hẹn trong 7 ngày gần đây.</p>}
  </section>
}

export default function ManagerDashboard({ token, onNavigate }) {
  const [reload, setReload] = useState(0)
  const [state, setState] = useState({ status: 'loading', data: null, error: '' })

  useEffect(() => {
    const controller = new AbortController()
    dashboardApi.manager(token, controller.signal).then((data) => {
      if (controller.signal.aborted) return
      const numbers = [data?.soLichHomNay, data?.soLichChoXacNhan, data?.doanhThuThang,
        data?.tongCongNo, data?.soPhongHoatDong, data?.soPhongTamNgung, data?.soPhongBaoTri]
      if (!data || !Array.isArray(data.doanhThuBonTuan) || !Array.isArray(data.lichHenTheoTrangThaiBayNgay) ||
        numbers.some((value) => typeof value !== 'number' || !Number.isFinite(value)) ||
        data.doanhThuBonTuan.some((week) => !week || !week.tuanBatDau || !week.tuanKetThuc ||
          typeof week.soTien !== 'number' || !Number.isFinite(week.soTien)) ||
        data.lichHenTheoTrangThaiBayNgay.some((item) => !item || !item.trangThai ||
          typeof item.soLuong !== 'number' || !Number.isFinite(item.soLuong))) {
        setState({ status: 'error', data: null, error: 'Chưa tải được dữ liệu tổng quan.' })
        return
      }
      setState({ status: 'success', data, error: '' })
    }).catch((error) => {
      if (!controller.signal.aborted) setState({ status: 'error', data: null, error: error.message })
    })
    return () => controller.abort()
  }, [token, reload])

  const data = state.data
  const totalRooms = data ? data.soPhongHoatDong + data.soPhongTamNgung + data.soPhongBaoTri : 0

  return <div className="manager-dashboard">
    <div className="page-heading"><div><h1>Tổng quan</h1><p className="page-description">Lịch hẹn, doanh thu và tình trạng phòng khám.</p></div></div>
    {state.status === 'loading' && <div className="dashboard-feedback" role="status">Đang tải thông tin tổng quan…</div>}
    {state.status === 'error' && <div className="dashboard-feedback dashboard-feedback-error" role="alert">
      <strong>Không tải được thông tin tổng quan.</strong><p>{state.error}</p>
      <button type="button" className="primary-button" onClick={() => setReload((value) => value + 1)}>Thử lại</button>
    </div>}
    {data && <>
      <div className="manager-dashboard-metrics">
        <Metric label="Lịch hôm nay" value={data.soLichHomNay} description="Lịch đang được theo dõi"
          href={data.ngay ? `/quan-ly/lich-hen?${new URLSearchParams({ ngayHen: data.ngay, nhom: 'tracked' })}` : '/quan-ly/lich-hen'} onNavigate={onNavigate} />
        <Metric label="Chờ xác nhận" value={data.soLichChoXacNhan} description="Lịch chưa đến giờ hẹn"
          href="/quan-ly/lich-hen?tab=pending" onNavigate={onNavigate} />
        <Metric label="Đã thu trong tháng" value={data.doanhThuThang} description="Khoản thu đã xác nhận" money
          href={data.ngay ? `/quan-ly/doanh-thu?nam=${data.ngay.slice(0, 4)}` : '/quan-ly/doanh-thu'} onNavigate={onNavigate} />
        <Metric label="Tổng còn nợ" value={data.tongCongNo} description="Số tiền chưa thanh toán" money
          href="/quan-ly/hoa-don?conNo=true" onNavigate={onNavigate} />
      </div>
      <div className="manager-dashboard-panels">
        <RevenueChart weeks={data.doanhThuBonTuan} />
        <AppointmentChart statuses={data.lichHenTheoTrangThaiBayNgay} />
      </div>
      <section className="dashboard-panel manager-dashboard-rooms" aria-labelledby="manager-rooms-title">
        <div className="dashboard-panel-heading"><h2 id="manager-rooms-title">Tình trạng phòng khám</h2><span>{numberFormatter.format(totalRooms)} phòng</span></div>
        <div className="manager-dashboard-room-list">
          <div><span className="manager-dashboard-room-dot is-active" />Hoạt động<strong>{numberFormatter.format(data.soPhongHoatDong)}</strong></div>
          <div><span className="manager-dashboard-room-dot is-paused" />Tạm ngừng<strong>{numberFormatter.format(data.soPhongTamNgung)}</strong></div>
          <div><span className="manager-dashboard-room-dot is-maintenance" />Bảo trì<strong>{numberFormatter.format(data.soPhongBaoTri)}</strong></div>
        </div>
      </section>
    </>}
  </div>
}
