import { useEffect, useState } from 'react'
import ListPagination from '../../components/ListPagination'
import { doctorRecordsApi } from './doctorRecordsApi'

const dateTime = new Intl.DateTimeFormat('vi-VN', { dateStyle: 'medium', timeStyle: 'short' })
const dateOnly = new Intl.DateTimeFormat('vi-VN', { dateStyle: 'medium' })

function formatDate(value, onlyDate = false) {
  if (!value) return '—'
  const date = new Date(onlyDate ? `${value}T00:00:00` : value)
  return Number.isNaN(date.getTime()) ? '—' : (onlyDate ? dateOnly : dateTime).format(date)
}

function HistoryDetail({ token, recordId, onBack }) {
  const [reload, setReload] = useState(0)
  const [state, setState] = useState({ status: 'loading', record: null, error: '' })

  useEffect(() => {
    const controller = new AbortController()
    queueMicrotask(() => { if (!controller.signal.aborted) setState({ status: 'loading', record: null, error: '' }) })
    doctorRecordsApi.detail(token, recordId, controller.signal).then((record) => {
      if (!controller.signal.aborted) setState(record?.maHoSoKham
        ? { status: 'success', record, error: '' }
        : { status: 'error', record: null, error: 'Chưa tải được hồ sơ khám.' })
    }).catch((error) => {
      if (!controller.signal.aborted) setState({ status: 'error', record: null, error: error.message })
    })
    return () => controller.abort()
  }, [token, recordId, reload])

  const record = state.record
  return <div className="doctor-records doctor-patient-history">
    <button type="button" className="doctor-back-link" onClick={onBack}><span aria-hidden="true">←</span>Lịch sử khám</button>
    <div className="page-heading"><div><h1>Chi tiết lần khám</h1><p className="page-description">Hồ sơ {recordId} · Chỉ xem</p></div></div>
    {state.status === 'loading' && <div className="doctor-record-feedback" role="status">Đang tải hồ sơ…</div>}
    {state.status === 'error' && <div className="doctor-record-feedback" role="alert"><p>{state.error}</p><button type="button" className="secondary-button" onClick={() => setReload((value) => value + 1)}>Thử lại</button></div>}
    {record && <>
      <div className="doctor-record-summary"><div><span>Ngày khám</span><strong>{formatDate(record.ngayKham)}</strong></div><div><span>Bác sĩ</span><strong>{record.tenBacSi || record.maBacSi}</strong></div><div><span>Ngày tái khám</span><strong>{formatDate(record.ngayTaiKham, true)}</strong></div></div>
      <section className="doctor-record-section"><h2>Kết quả khám</h2><div className="doctor-history-fields">
        <div><span>Triệu chứng</span><p>{record.trieuChung || 'Chưa có thông tin.'}</p></div>
        <div><span>Chẩn đoán</span><p>{record.chanDoan || 'Chưa có thông tin.'}</p></div>
        <div><span>Tóm tắt cho khách</span><p>{record.tomTatChoKhach || 'Chưa có thông tin.'}</p></div>
        <div><span>Ghi chú nội bộ</span><p>{record.ghiChuNoiBo || 'Không có ghi chú.'}</p></div>
      </div></section>
      <section className="doctor-record-section"><h2>Dịch vụ đã thực hiện</h2>
        {record.dichVuThucHiens?.length ? <div className="doctor-history-list">{record.dichVuThucHiens.map((service) => <div key={service.maDichVuThucHien}><strong>{service.tenDichVu}</strong><span>Số lượng: {service.soLuong}</span>{service.ghiChu && <p>{service.ghiChu}</p>}</div>)}</div> : <p>Không có dịch vụ được ghi nhận.</p>}
      </section>
      <section className="doctor-record-section"><h2>Đơn thuốc</h2>
        {record.donThuocs?.length ? <div className="doctor-history-list">{record.donThuocs.map((prescription) => <div key={prescription.maDonThuoc}><strong>Đơn {prescription.maDonThuoc} · {formatDate(prescription.ngayKe)}</strong>{prescription.ghiChu && <p>{prescription.ghiChu}</p>}<ul>{prescription.chiTietDonThuocs?.map((medicine, index) => <li key={`${medicine.tenThuoc}-${index}`}><strong>{medicine.tenThuoc}{medicine.hamLuong ? ` · ${medicine.hamLuong}` : ''}</strong><span>Liều dùng: {medicine.lieuDung} · Số lượng: {medicine.soLuong}</span><span>{medicine.soLanMoiNgay ? `${medicine.soLanMoiNgay} lần/ngày` : ''}{medicine.soNgayDung ? ` · ${medicine.soNgayDung} ngày` : ''}</span>{medicine.huongDan && <p>{medicine.huongDan}</p>}</li>)}</ul></div>)}</div> : <p>Không có đơn thuốc.</p>}
      </section>
    </>}
  </div>
}

export default function DoctorPatientHistory({ token, customerId, recordId, sourceRecordId, fromAppointments, onNavigate, onBack }) {
  const [page, setPage] = useState(1)
  const [reload, setReload] = useState(0)
  const [state, setState] = useState({ status: 'loading', items: [], totalItems: 0, error: '' })

  useEffect(() => {
    if (recordId) return undefined
    const controller = new AbortController()
    queueMicrotask(() => { if (!controller.signal.aborted) setState({ status: 'loading', items: [], totalItems: 0, error: '' }) })
    doctorRecordsApi.patientHistory(token, customerId, page, controller.signal).then((result) => {
      if (controller.signal.aborted) return
      if (!Array.isArray(result?.items) || !Number.isInteger(result.totalItems)) {
        setState({ status: 'error', items: [], totalItems: 0, error: 'Chưa tải được lịch sử khám.' })
        return
      }
      const lastPage = Math.max(1, Math.ceil(result.totalItems / 10))
      if (page > lastPage) { setPage(lastPage); return }
      setState({ status: 'success', items: result.items, totalItems: result.totalItems, error: '' })
    }).catch((error) => {
      if (!controller.signal.aborted) setState({ status: 'error', items: [], totalItems: 0, error: error.message })
    })
    return () => controller.abort()
  }, [token, customerId, page, recordId, reload])

  const historyUrl = `/bac-si/ho-so-kham?maKhachHang=${encodeURIComponent(customerId)}${sourceRecordId ? `&tuHoSo=${encodeURIComponent(sourceRecordId)}` : ''}${fromAppointments ? '&tuLich=true' : ''}`
  if (recordId) return <HistoryDetail token={token} recordId={recordId} onBack={() => onNavigate(historyUrl)} />
  return <div className="doctor-records doctor-patient-history">
    <button type="button" className="doctor-back-link" onClick={onBack}><span aria-hidden="true">←</span>Quay lại</button>
    <div className="page-heading"><div><h1>Lịch sử khám của bệnh nhân</h1><p className="page-description">Khách hàng {customerId} · Các hồ sơ đã chốt</p></div></div>
    {state.status === 'loading' && <div className="doctor-record-feedback" role="status">Đang tải lịch sử khám…</div>}
    {state.status === 'error' && <div className="doctor-record-feedback" role="alert"><p>{state.error}</p><button type="button" className="secondary-button" onClick={() => setReload((value) => value + 1)}>Thử lại</button></div>}
    {state.status === 'success' && <><p className="doctor-record-count">{state.totalItems} lần khám đã chốt</p>
      {state.items.length === 0 && <div className="doctor-record-feedback">Bệnh nhân chưa có hồ sơ khám đã chốt.</div>}
      <div className="doctor-record-list">{state.items.map((item) => <article key={item.maHoSoKham}><div><small>{item.maHoSoKham}</small><h2>{formatDate(item.ngayKham)}</h2><p>Bác sĩ: {item.tenBacSi || item.maBacSi}</p><p>{item.tomTat || 'Chưa có tóm tắt lần khám.'}</p></div><button type="button" className="secondary-button" onClick={() => onNavigate(`${historyUrl}&maHoSoKham=${encodeURIComponent(item.maHoSoKham)}`)}>Xem chi tiết</button></article>)}</div>
      <ListPagination page={page} pageSize={10} totalItems={state.totalItems} onPageChange={setPage} />
    </>}
  </div>
}
