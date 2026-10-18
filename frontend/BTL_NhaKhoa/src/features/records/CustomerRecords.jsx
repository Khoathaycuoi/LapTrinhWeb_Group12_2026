import { useEffect, useState } from 'react'
import ListPagination from '../../components/ListPagination'
import { customerRecordsApi } from './customerRecordsApi'
import './CustomerRecords.css'

const pageSize = 10
const dateTime = new Intl.DateTimeFormat('vi-VN', { dateStyle: 'medium', timeStyle: 'short' })
const dateOnly = new Intl.DateTimeFormat('vi-VN', { dateStyle: 'medium' })

function formatDate(value, withTime = true) {
  if (!value) return '—'
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? '—' : (withTime ? dateTime : dateOnly).format(date)
}

function RecordDetail({ token, recordId, onBack }) {
  const [reload, setReload] = useState(0)
  const [state, setState] = useState({ status: 'loading', record: null, error: '' })

  useEffect(() => {
    const controller = new AbortController()
    setState({ status: 'loading', record: null, error: '' })
    customerRecordsApi.detail(token, recordId, controller.signal).then((record) => {
      if (controller.signal.aborted) return
      setState(record?.maHoSoKham
        ? { status: 'success', record, error: '' }
        : { status: 'error', record: null, error: 'Chưa tải được hồ sơ khám.' })
    }).catch((error) => {
      if (!controller.signal.aborted) setState({ status: 'error', record: null,
        error: error.status === 0 ? error.message : 'Hiện chưa thể xem hồ sơ khám.' })
    })
    return () => controller.abort()
  }, [token, recordId, reload])

  const record = state.record
  return <div className="customer-records customer-record-detail">
    <button type="button" className="customer-record-back" onClick={onBack}>← Lịch sử khám</button>
    <div className="page-heading"><div><h1>Chi tiết lần khám</h1><p className="page-description">Hồ sơ {recordId}</p></div></div>
    {state.status === 'loading' && <div className="customer-record-feedback" role="status">Đang tải hồ sơ khám…</div>}
    {state.status === 'error' && <div className="customer-record-feedback" role="alert"><p>{state.error}</p><button type="button" className="secondary-button" onClick={() => setReload((value) => value + 1)}>Thử lại</button></div>}
    {record && <>
      <div className="customer-record-summary"><div><span>Ngày khám</span><strong>{formatDate(record.ngayKham)}</strong></div><div><span>Bác sĩ</span><strong>{record.tenBacSi || 'Chưa có thông tin'}</strong></div>{record.ngayTaiKham && <div><span>Hẹn tái khám</span><strong>{formatDate(`${record.ngayTaiKham}T00:00:00`, false)}</strong></div>}</div>
      <section className="customer-record-section"><h2>Kết quả khám</h2>
        {record.trieuChung && <div className="customer-record-field"><span>Triệu chứng</span><p>{record.trieuChung}</p></div>}
        <div className="customer-record-field"><span>Chẩn đoán</span><p>{record.chanDoan || 'Chưa có thông tin.'}</p></div>
        <div className="customer-record-field"><span>Tóm tắt cho bạn</span><p>{record.tomTatChoKhach || 'Chưa có thông tin.'}</p></div>
      </section>
      <section className="customer-record-section"><h2>Dịch vụ đã thực hiện</h2>
        {record.dichVuThucHiens?.length ? <ul className="customer-record-list">{record.dichVuThucHiens.map((service) => <li key={service.maDichVuThucHien}><strong>{service.tenDichVu}</strong><span>Số lượng: {service.soLuong}</span>{service.ghiChu && <small>{service.ghiChu}</small>}</li>)}</ul> : <p>Không có dịch vụ được ghi nhận trong lần khám này.</p>}
      </section>
      <section className="customer-record-section"><h2>Đơn thuốc</h2>
        {record.donThuocs?.length ? <div className="customer-record-prescriptions">{record.donThuocs.map((prescription) => <div key={prescription.maDonThuoc}><strong>Đơn {prescription.maDonThuoc} · {formatDate(prescription.ngayKe)}</strong>{prescription.ghiChu && <p>{prescription.ghiChu}</p>}<ul className="customer-record-list">{prescription.chiTietDonThuocs?.map((medicine, index) => <li key={`${medicine.tenThuoc}-${index}`}><strong>{medicine.tenThuoc}{medicine.hamLuong ? ` · ${medicine.hamLuong}` : ''}</strong><span>Liều dùng: {medicine.lieuDung} · Số lượng: {medicine.soLuong}</span>{(medicine.soLanMoiNgay || medicine.soNgayDung) && <span>{medicine.soLanMoiNgay ? `${medicine.soLanMoiNgay} lần/ngày` : ''}{medicine.soLanMoiNgay && medicine.soNgayDung ? ' · ' : ''}{medicine.soNgayDung ? `${medicine.soNgayDung} ngày` : ''}</span>}{medicine.huongDan && <small>{medicine.huongDan}</small>}</li>)}</ul></div>)}</div> : <p>Không có đơn thuốc trong lần khám này.</p>}
      </section>
    </>}
  </div>
}

export default function CustomerRecords({ token, query = '', onNavigate }) {
  const recordId = new URLSearchParams(query).get('maHoSoKham')
  const [page, setPage] = useState(1)
  const [reload, setReload] = useState(0)
  const [state, setState] = useState({ status: 'loading', items: [], totalItems: 0, error: '' })

  useEffect(() => {
    if (recordId) return undefined
    const controller = new AbortController()
    setState({ status: 'loading', items: [], totalItems: 0, error: '' })
    customerRecordsApi.list(token, page, controller.signal).then((result) => {
      if (controller.signal.aborted) return
      if (!Array.isArray(result?.items) || !Number.isInteger(result.totalItems)) {
        setState({ status: 'error', items: [], totalItems: 0, error: 'Chưa tải được lịch sử khám.' })
        return
      }
      const lastPage = Math.max(1, Math.ceil(result.totalItems / pageSize))
      if (page > lastPage) { setPage(lastPage); return }
      setState({ status: 'success', items: result.items, totalItems: result.totalItems, error: '' })
    }).catch((error) => {
      if (!controller.signal.aborted) setState({ status: 'error', items: [], totalItems: 0,
        error: error.status === 0 ? error.message : 'Hiện chưa thể hiển thị lịch sử khám.' })
    })
    return () => controller.abort()
  }, [token, page, recordId, reload])

  if (recordId) return <RecordDetail token={token} recordId={recordId} onBack={() => {
    onNavigate('/khach-hang/lich-su-kham')
    setReload((value) => value + 1)
  }} />

  return <div className="customer-records">
    <div className="page-heading"><div><h1>Lịch sử khám</h1><p className="page-description">Các lần khám đã chốt và hướng dẫn dành cho bạn.</p></div></div>
    {state.status === 'loading' && <div className="customer-record-feedback" role="status">Đang tải lịch sử khám…</div>}
    {state.status === 'error' && <div className="customer-record-feedback" role="alert"><p>{state.error}</p><button type="button" className="secondary-button" onClick={() => setReload((value) => value + 1)}>Thử lại</button></div>}
    {state.status === 'success' && <><p className="customer-record-count">{state.totalItems} lần khám</p>
      {state.items.length ? <div className="customer-record-cards">{state.items.map((record) => <article key={record.maHoSoKham}><div><small>{record.maHoSoKham}</small><h2>{formatDate(record.ngayKham)}</h2><p>Bác sĩ: {record.tenBacSi || 'Chưa có thông tin'}</p><p>{record.tomTat || 'Chưa có tóm tắt lần khám.'}</p></div><button type="button" className="secondary-button" onClick={() => onNavigate(`/khach-hang/lich-su-kham?maHoSoKham=${encodeURIComponent(record.maHoSoKham)}`)}>Xem chi tiết</button></article>)}</div> : <div className="customer-record-feedback">Bạn chưa có lần khám đã chốt.</div>}
      <ListPagination page={page} pageSize={pageSize} totalItems={state.totalItems} onPageChange={setPage} />
    </>}
  </div>
}
