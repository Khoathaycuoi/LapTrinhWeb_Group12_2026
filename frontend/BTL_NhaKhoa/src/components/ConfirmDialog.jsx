import { useEffect } from 'react'
import './ConfirmDialog.css'

export default function ConfirmDialog({ title, message, confirmLabel = 'Xác nhận', onConfirm, onCancel }) {
  useEffect(() => {
    const onKeyDown = (event) => {
      if (event.key === 'Escape') onCancel()
    }
    window.addEventListener('keydown', onKeyDown)
    return () => window.removeEventListener('keydown', onKeyDown)
  }, [onCancel])

  return <div className="dialog-backdrop" onMouseDown={(event) => { if (event.target === event.currentTarget) onCancel() }}>
    <section className="confirm-dialog" role="alertdialog" aria-modal="true" aria-labelledby="confirm-title" aria-describedby="confirm-message">
      <div className="confirm-icon">?</div>
      <h2 id="confirm-title">{title}</h2>
      <p id="confirm-message">{message}</p>
      <div className="confirm-actions">
        <button className="secondary-button" autoFocus onClick={onCancel}>Ở lại</button>
        <button className="primary-button" onClick={onConfirm}>{confirmLabel}</button>
      </div>
    </section>
  </div>
}
