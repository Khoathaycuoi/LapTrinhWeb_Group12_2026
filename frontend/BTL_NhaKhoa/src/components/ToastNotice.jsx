import { useEffect, useRef } from 'react'
import './ToastNotice.css'

export default function ToastNotice({ message, onDismiss, tone = 'success' }) {
  const onDismissRef = useRef(onDismiss)

  useEffect(() => { onDismissRef.current = onDismiss }, [onDismiss])

  useEffect(() => {
    if (!message || !onDismissRef.current) return undefined
    const timer = window.setTimeout(() => onDismissRef.current?.(), tone === 'error' ? 6000 : 4000)
    return () => window.clearTimeout(timer)
  }, [message, tone])

  if (!message) return null

  return <div className={`toast-notice toast-notice-${tone}`} role={tone === 'error' ? 'alert' : 'status'}>
    <span className="toast-notice-icon" aria-hidden="true">{tone === 'success' ? '✓' : '!'}</span>
    <span className="toast-notice-message">{message}</span>
    {onDismiss && <button type="button" aria-label="Đóng thông báo" onClick={onDismiss}>×</button>}
  </div>
}
