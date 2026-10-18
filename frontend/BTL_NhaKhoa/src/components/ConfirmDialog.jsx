import { useEffect } from 'react'
import './ConfirmDialog.css'

export default function ConfirmDialog({ title, message, confirmLabel = 'Xác nhận', busy = false, onConfirm, onCancel }) {
  useEffect(() => {
    const onKeyDown = (event) => {
      if (event.key === 'Escape' && !busy) onCancel()
    }
    window.addEventListener('keydown', onKeyDown)
    return () => window.removeEventListener('keydown', onKeyDown)
  }, [busy, onCancel])

  return <div className="dialog-backdrop" onMouseDown={(event) => { if (event.target === event.currentTarget && !busy) onCancel() }}>
    <section className="confirm-dialog" role="alertdialog" aria-modal="true" aria-labelledby="confirm-title" aria-describedby="confirm-message">
      <div className="confirm-icon">?</div>
      <h2 id="confirm-title">{title}</h2>
      <p id="confirm-message">{message}</p>
      <div className="confirm-actions">
        <button className="secondary-button" autoFocus disabled={busy} onClick={onCancel}>Ở lại</button>
        <button className="primary-button" disabled={busy} onClick={onConfirm}>{confirmLabel}</button>
      </div>
    </section>
  </div>
}
