import { useEffect, useState } from 'react'
import { receptionAppointmentsApi } from './receptionAppointmentsApi'
import '../../components/ConfirmDialog.css'
import './ReceptionActionDialog.css'

export default function ReceptionActionDialog({ appointment, action, token, onClose, onDone }) {
  const [note, setNote] = useState('')
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)
  const noShow = action === 'no-show'

  useEffect(() => {
    function onKeyDown(event) { if (event.key === 'Escape' && !busy) onClose() }
    window.addEventListener('keydown', onKeyDown)
    return () => window.removeEventListener('keydown', onKeyDown)
  }, [busy, onClose])

  async function submit(event) {
    event.preventDefault()
    if (busy) return
    setBusy(true)
    setError('')
    try {
      if (noShow) await receptionAppointmentsApi.markNoShow(token, appointment.maLichHen, note.trim())
      else await receptionAppointmentsApi.receive(token, appointment.maLichHen)
      onDone(appointment.maLichHen)
    } catch (requestError) {
      setError(requestError.message)
    } finally {
      setBusy(false)
    }
  }

  return <div className="dialog-backdrop" onMouseDown={(event) => { if (event.target === event.currentTarget && !busy) onClose() }}>
    <form className="confirm-dialog reception-action-dialog" role="dialog" aria-modal="true" aria-labelledby="reception-action-title" onSubmit={submit}>
      <h2 id="reception-action-title">{noShow ? 'Đánh dấu không đến?' : 'Tiếp nhận khách?'}</h2>
      <p><strong>{appointment.tenKH || appointment.maKH}</strong> · {appointment.maLichHen} · {appointment.ngayHen?.split('-').reverse().join('/')} {appointment.gioHen?.slice(0, 5)}</p>
      <p>{noShow ? 'Lịch sẽ chuyển sang Không đến và không thể tiếp nhận lại.' : 'Lịch sẽ chuyển sang Đã đến.'}</p>
      {noShow && <label htmlFor="reception-no-show-note">Ghi chú (không bắt buộc)
        <textarea id="reception-no-show-note" autoFocus maxLength={2000} rows={3} value={note}
          onChange={(event) => setNote(event.target.value)} placeholder="Thông tin cần lưu về việc khách không đến" />
      </label>}
      {error && <div className="alert alert-error" role="alert">{error}</div>}
      <div className="confirm-actions">
        <button type="button" className="secondary-button" disabled={busy} onClick={onClose}>Đóng</button>
        <button type="submit" className="primary-button" disabled={busy}>{busy ? 'Đang xử lý…' : noShow ? 'Đánh dấu không đến' : 'Tiếp nhận'}</button>
      </div>
    </form>
  </div>
}
