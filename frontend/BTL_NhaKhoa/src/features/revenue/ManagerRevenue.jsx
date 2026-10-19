import { useEffect, useState } from 'react'
import { revenueApi } from './revenueApi'
import './ManagerRevenue.css'

const money = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND', maximumFractionDigits: 0 })
const number = new Intl.NumberFormat('vi-VN')

function selectedYear(query) {
  const value = new URLSearchParams(query).get('nam')
  const year = Number(value)
  return value && Number.isInteger(year) && year >= 1900 && year <= 9998
    ? year : new Date().getFullYear()
}

function validReport(report, year) {
  return report?.nam === year && Number.isFinite(report.tongDoanhThu) &&
    Number.isInteger(report.tongKhoanThu) && Array.isArray(report.cacThang) &&
    report.cacThang.length === 12 && report.cacThang.every((item, index) =>
      item?.thang === index + 1 && Number.isFinite(item.soTien) && Number.isInteger(item.soKhoanThu))
}

export default function ManagerRevenue({ token, query, onNavigate }) {
  const year = selectedYear(query)
  const [reload, setReload] = useState(0)
  const [state, setState] = useState({ status: 'loading', data: null, error: '' })

  useEffect(() => {
    const controller = new AbortController()
    revenueApi.byYear(token, year, controller.signal).then((report) => {
      if (controller.signal.aborted) return
      setState(validReport(report, year)
        ? { status: 'success', data: report, error: '' }
        : { status: 'error', data: null, error: 'Máy chủ chưa trả báo cáo doanh thu hợp lệ.' })
    }).catch((error) => {
      if (!controller.signal.aborted) setState({ status: 'error', data: null, error: error.message })
    })
    return () => controller.abort()
  }, [token, year, reload])

  function changeYear(nextYear) {
    if (nextYear < 1900 || nextYear > 9998) return
    setState({ status: 'loading', data: null, error: '' })
    onNavigate(`/quan-ly/doanh-thu?nam=${nextYear}`)
  }

  const report = state.status === 'success' && state.data?.nam === year ? state.data : null
  const loading = state.status === 'loading' || (state.status === 'success' && !report)
  const highestMonth = report?.cacThang.reduce((highest, item) =>
    item.soTien > highest.soTien ? item : highest, report.cacThang[0])
  const maximum = report ? Math.max(...report.cacThang.map((item) => item.soTien)) : 0

  return <div className="manager-revenue">
    <div className="page-heading manager-revenue-heading">
      <div><h1>Doanh thu</h1><p className="page-description">Khoản thu đã xác nhận theo thời điểm nhận tiền.</p></div>
      <div className="manager-revenue-year" aria-label="Chọn năm báo cáo">
        <button type="button" className="secondary-button" disabled={year <= 1900} onClick={() => changeYear(year - 1)}>Năm trước</button>
        <strong>Năm {year}</strong>
        <button type="button" className="secondary-button" disabled={year >= 9998} onClick={() => changeYear(year + 1)}>Năm sau</button>
      </div>
    </div>

    {loading && <div className="manager-revenue-feedback" role="status">Đang tải báo cáo doanh thu…</div>}
    {state.status === 'error' && <div className="manager-revenue-feedback manager-revenue-error" role="alert">
      <strong>Không tải được báo cáo doanh thu.</strong><p>{state.error}</p>
      <button type="button" className="primary-button" onClick={() => setReload((value) => value + 1)}>Thử lại</button>
    </div>}

    {report && <>
      <div className="manager-revenue-summary">
        <div><span>Tổng thu năm {year}</span><strong>{money.format(report.tongDoanhThu)}</strong></div>
        <div><span>Số khoản thu</span><strong>{number.format(report.tongKhoanThu)}</strong></div>
        <div><span>Tháng thu cao nhất</span><strong>{highestMonth.soTien > 0 ? `Tháng ${highestMonth.thang}` : 'Chưa có'}</strong></div>
      </div>

      <section className="manager-revenue-panel" aria-labelledby="manager-revenue-months-title">
        <div className="manager-revenue-panel-heading"><div><h2 id="manager-revenue-months-title">Doanh thu theo tháng</h2><p>Không tính khoản thu đã hủy hoặc thuộc hóa đơn đã hủy.</p></div><span>{year}</span></div>
        <div className="manager-revenue-months" role="list">
          {report.cacThang.map((item) => <div className="manager-revenue-month" role="listitem" key={item.thang}>
            <span className="manager-revenue-month-label">Tháng {String(item.thang).padStart(2, '0')}</span>
            <div className="manager-revenue-track" aria-hidden="true"><span style={{ width: `${maximum > 0 ? item.soTien / maximum * 100 : 0}%` }} /></div>
            <div className="manager-revenue-month-value"><strong>{money.format(item.soTien)}</strong><small>{number.format(item.soKhoanThu)} khoản thu</small></div>
          </div>)}
        </div>
      </section>
    </>}
  </div>
}
