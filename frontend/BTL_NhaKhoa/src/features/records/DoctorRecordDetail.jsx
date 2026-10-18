import { useEffect, useState } from 'react'
import ConfirmDialog from '../../components/ConfirmDialog'
import ToastNotice from '../../components/ToastNotice'
import DoctorServiceSection from './DoctorServiceSection'
import DoctorPrescriptionSection from './DoctorPrescriptionSection'
import { doctorRecordsApi } from './doctorRecordsApi'

const fields = ['trieuChung', 'chanDoan', 'tomTatChoKhach', 'ghiChuNoiBo', 'ngayTaiKham']
const formFromRecord = (record) => Object.fromEntries(fields.map((field) => [field, record[field] || '']))
const actionCopy = {
  perform: ['Ghi nhận dịch vụ?', 'Dịch vụ đã thực hiện sẽ được dùng khi lập hóa đơn.', 'Ghi nhận'],
  removeService: ['Bỏ dịch vụ đã thực hiện?', 'Dịch vụ này sẽ được bỏ khỏi hồ sơ khám.', 'Bỏ dịch vụ'],
  prescribe: ['Lưu đơn thuốc?', 'Kiểm tra tên thuốc, liều dùng và số lượng trước khi lưu.', 'Lưu đơn'],
  removePrescription: ['Bỏ đơn thuốc?', 'Đơn thuốc này sẽ được bỏ khỏi hồ sơ khám.', 'Bỏ đơn'],
  finalize: ['Chốt hồ sơ khám?', 'Sau khi chốt, bác sĩ không thể sửa hồ sơ hoặc đơn thuốc.', 'Chốt hồ sơ'],
  complete: ['Hoàn thành lịch khám?', 'Lịch hẹn sẽ chuyển sang Đã hoàn thành.', 'Hoàn thành'],
}

export default function DoctorRecordDetail({ token, recordId, appointmentId, initialNotice, onBack }) {
  const [reload, setReload] = useState(0)
  const [state, setState] = useState({ status: 'loading', record: null, error: '' })
  const [history, setHistory] = useState({ status: 'loading', text: '', error: '' })
  const [form, setForm] = useState(null)
  const [dirty, setDirty] = useState(false)
  const [pending, setPending] = useState(null)
  const [busy, setBusy] = useState(false)
  const [actionError, setActionError] = useState('')
  const [notice, setNotice] = useState(initialNotice || '')

  useEffect(() => {
    const controller = new AbortController()
    queueMicrotask(() => { if (!controller.signal.aborted) setState({ status: 'loading', record: null, error: '' }) })
    const load = recordId ? doctorRecordsApi.detail(token, recordId, controller.signal)
      : doctorRecordsApi.fromAppointment(token, appointmentId, controller.signal)
    load.then((record) => {
      if (controller.signal.aborted) return
      if (!record?.maHoSoKham) throw new Error('Không tìm thấy hồ sơ khám.')
      setState({ status: 'success', record, error: '' })
      setForm(formFromRecord(record))
      setDirty(false)
    }).catch((error) => {
      if (!controller.signal.aborted) setState({ status: 'error', record: null, error: error.message })
    })
    return () => controller.abort()
  }, [token, recordId, appointmentId, reload])

  const record = state.record
  useEffect(() => {
    const controller = new AbortController()
    if (!record?.maKhachHang || !['DA_XAC_NHAN', 'DA_DEN', 'DANG_KHAM'].includes(record.trangThaiLichHen)) {
      queueMicrotask(() => { if (!controller.signal.aborted) setHistory({ status: 'unavailable', text: '', error: '' }) })
      return () => controller.abort()
    }
    queueMicrotask(() => { if (!controller.signal.aborted) setHistory({ status: 'loading', text: '', error: '' }) })
    doctorRecordsApi.medicalHistory(token, record.maKhachHang, controller.signal)
      .then((result) => { if (!controller.signal.aborted) setHistory({ status: 'success', text: result?.tienSuBenh || '', error: '' }) })
      .catch((error) => { if (!controller.signal.aborted) setHistory({ status: 'error', text: '', error: error.message }) })
    return () => controller.abort()
  }, [token, record?.maKhachHang, record?.trangThaiLichHen])

  const editable = record?.trangThaiLichHen === 'DANG_KHAM' && !record.chotLuc
  function updateForm(field, value) {
    setForm((current) => ({ ...current, [field]: value }))
    setDirty(true)
    setActionError('')
  }

  async function save() {
    if (!record || !form || busy) return
    setBusy(true)
    setActionError('')
    try {
      await doctorRecordsApi.save(token, record.maHoSoKham, {
        trieuChung: form.trieuChung.trim() || null,
        chanDoan: form.chanDoan.trim() || null,
        tomTatChoKhach: form.tomTatChoKhach.trim() || null,
        ghiChuNoiBo: form.ghiChuNoiBo.trim() || null,
        ngayTaiKham: form.ngayTaiKham || null,
      })
      setNotice('Đã lưu hồ sơ khám.')
      setReload((value) => value + 1)
    } catch (error) { setActionError(error.message) }
    finally { setBusy(false) }
  }

  function review(action) {
    setActionError('')
    if (dirty) { setActionError('Vui lòng lưu thay đổi hồ sơ trước khi tiếp tục.'); return }
    if (action.type === 'finalize') {
      if (!record.chanDoan?.trim() || !record.tomTatChoKhach?.trim()) {
        setActionError('Cần có chẩn đoán và tóm tắt cho khách trước khi chốt.'); return
      }
    }
    setPending(action)
  }

  async function confirmAction() {
    if (!pending || !record || busy) return
    setBusy(true)
    setActionError('')
    try {
      const id = record.maHoSoKham
      if (pending.type === 'perform') await doctorRecordsApi.performService(token, id, { maDichVu: pending.service.maDichVu, soLuong: pending.quantity })
      if (pending.type === 'removeService') await doctorRecordsApi.removeService(token, id, pending.service.maDichVu)
      if (pending.type === 'prescribe') await doctorRecordsApi.prescribe(token, id, pending.payload)
      if (pending.type === 'removePrescription') await doctorRecordsApi.removePrescription(token, id, pending.prescription.maDonThuoc)
      if (pending.type === 'finalize') await doctorRecordsApi.finalize(token, id)
      if (pending.type === 'complete') await doctorRecordsApi.completeAppointment(token, record.maLichHen)
      setNotice(pending.type === 'complete' ? 'Đã hoàn thành lịch khám.' : pending.type === 'finalize' ? 'Đã chốt hồ sơ khám.' : 'Đã cập nhật hồ sơ khám.')
      setPending(null)
      setReload((value) => value + 1)
    } catch (error) {
      setPending(null)
      setActionError(error.message)
    } finally { setBusy(false) }
  }

  return <div className="doctor-records doctor-record-detail">
    <button type="button" className="doctor-back-link" onClick={onBack}>← Danh sách hồ sơ</button>
    <div className="page-heading"><div><h1>Hồ sơ khám</h1><p className="page-description">{record?.maHoSoKham || recordId || appointmentId} {record?.maLichHen && `· Lịch ${record.maLichHen}`}</p></div></div>
    {state.status === 'loading' && <div className="doctor-record-feedback" role="status">Đang tải hồ sơ…</div>}
    {state.status === 'error' && <div className="doctor-record-feedback" role="alert"><p>{state.error}</p><button type="button" className="secondary-button" onClick={() => setReload((value) => value + 1)}>Thử lại</button></div>}
    {record && form && <>
      <div className="doctor-record-summary"><div><span>Khách hàng</span><strong>{record.maKhachHang}</strong></div><div><span>Ngày khám</span><strong>{new Intl.DateTimeFormat('vi-VN', { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(record.ngayKham))}</strong></div><div><span>Trạng thái</span><strong>{record.chotLuc ? 'Đã chốt' : 'Chưa chốt'}</strong></div></div>
      <section className="doctor-record-section"><h2>Tiền sử bệnh</h2>
        {history.status === 'loading' && <p>Đang tải tiền sử bệnh…</p>}
        {history.status === 'success' && <p className="doctor-record-history">{history.text || 'Khách chưa cung cấp tiền sử bệnh.'}</p>}
        {history.status === 'error' && <p>{history.error}</p>}
        {history.status === 'unavailable' && <p>Không thể xem tiền sử bệnh ở trạng thái lịch hiện tại.</p>}
      </section>
      <section className="doctor-record-section"><div className="doctor-record-section-heading"><h2>Thông tin khám</h2>{record.chotLuc && <span>Đã chốt</span>}</div>
        <div className="doctor-record-form">
          <label>Triệu chứng<textarea rows={3} value={form.trieuChung} disabled={!editable || busy} onChange={(event) => updateForm('trieuChung', event.target.value)} /></label>
          <label>Chẩn đoán<textarea rows={3} value={form.chanDoan} disabled={!editable || busy} onChange={(event) => updateForm('chanDoan', event.target.value)} /></label>
          <label>Tóm tắt cho khách<textarea rows={3} value={form.tomTatChoKhach} disabled={!editable || busy} onChange={(event) => updateForm('tomTatChoKhach', event.target.value)} /></label>
          <label>Ghi chú nội bộ<textarea rows={3} value={form.ghiChuNoiBo} disabled={!editable || busy} onChange={(event) => updateForm('ghiChuNoiBo', event.target.value)} /></label>
          <label>Ngày tái khám<input type="date" value={form.ngayTaiKham} disabled={!editable || busy} onChange={(event) => updateForm('ngayTaiKham', event.target.value)} /></label>
        </div>{editable && <div className="doctor-record-actions"><button type="button" className="primary-button" disabled={!dirty || busy} onClick={save}>{busy ? 'Đang lưu…' : 'Lưu hồ sơ'}</button></div>}
      </section>
      <DoctorServiceSection record={record} editable={editable} onReview={review} />
      <DoctorPrescriptionSection key={`${record.maHoSoKham}-${reload}`} record={record} editable={editable} onReview={review} />
      {actionError && <p className="doctor-form-error" role="alert">{actionError}</p>}
      {editable && <div className="doctor-record-footer"><span>Chẩn đoán và tóm tắt cho khách là bắt buộc khi chốt.</span><button type="button" className="primary-button" onClick={() => review({ type: 'finalize' })}>Chốt hồ sơ</button></div>}
      {record.chotLuc && record.trangThaiLichHen === 'DANG_KHAM' && <div className="doctor-record-footer"><span>Hồ sơ đã chốt. Hoàn thành lịch để kết thúc lượt khám.</span><button type="button" className="primary-button" onClick={() => review({ type: 'complete' })}>Hoàn thành lịch</button></div>}
    </>}
    {pending && <ConfirmDialog title={actionCopy[pending.type][0]} message={actionCopy[pending.type][1]} confirmLabel={actionCopy[pending.type][2]} busy={busy} onConfirm={confirmAction} onCancel={() => setPending(null)} />}
    <ToastNotice message={notice} onDismiss={() => setNotice('')} />
  </div>
}
