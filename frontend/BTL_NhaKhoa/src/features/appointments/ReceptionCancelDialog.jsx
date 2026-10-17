import { useEffect, useState } from 'react'
import { receptionAppointmentsApi } from './receptionAppointmentsApi'

export default function ReceptionCancelDialog({ appointment, token, onClose, onCancelled }) {
  const [reason, setReason] = useState('')
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  useEffect(() => {
    function onKeyDown(event) { if (event.key === 'Escape' && !busy) onClose() }
    window.addEventListener('keydown', onKeyDown)
    return () => window.removeEventListener('keydown', onKeyDown)
  }, [busy, onClose])

  async function submit(event) {
    event.preventDefault()
    if (busy || !reason.trim()) return
    setBusy(true)
    setError('')
    try {
      await receptionAppointmentsApi.cancel(token, appointment.maLichHen, reason.trim())
      onCancelled(appointment.maLichHen)
    } catch (requestError) {
      setError(requestError.message)
    } finally {
      setBusy(false)
    }
  }

  return <div className="dialog-backdrop" onMouseDown={(event) => { if (event.target === event.currentTarget && !busy) onClose() }}>
    <form className="confirm-dialog reception-dialog reception-cancel-dialog" role="dialog" aria-modal="true" aria-labelledby="reception-cancel-title" onSubmit={submit}>
      <h2 id="reception-cancel-title">Hủy lịch hẹn {appointment.maLichHen}?</h2>
      <p>Khách hàng: {appointment.tenKH || appointment.maKH}. Lịch sau khi hủy sẽ không còn giữ chỗ của bác sĩ và phòng.</p>
      <label htmlFor="reception-cancel-reason">Lý do hủy <span aria-hidden="true">*</span></label>
      <textarea id="reception-cancel-reason" autoFocus required maxLength={2000} rows={3} value={reason}
        onChange={(event) => setReason(event.target.value)} placeholder="Nhập lý do để khách hàng biết vì sao lịch bị hủy" />
      {error && <div className="alert alert-error" role="alert">{error}</div>}
      <div className="confirm-actions">
        <button type="button" className="secondary-button" disabled={busy} onClick={onClose}>Giữ lịch</button>
        <button type="submit" className="primary-button" disabled={busy || !reason.trim()}>{busy ? 'Đang hủy…' : 'Xác nhận hủy'}</button>
      </div>
    </form>
  </div>
}
