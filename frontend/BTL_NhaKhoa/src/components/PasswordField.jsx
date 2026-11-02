import { useId, useState } from 'react'
import './PasswordField.css'

export default function PasswordField({ label, hint, ...inputProps }) {
  const id = useId()
  const [visible, setVisible] = useState(false)

  return <div className="password-field">
    <label htmlFor={id}>{label}</label>
    <span className="password-input">
      <input {...inputProps} id={id} type={visible ? 'text' : 'password'} spellCheck={false} autoCapitalize="off" />
      <button type="button" className="password-input-toggle" aria-label={`${visible ? 'Ẩn' : 'Hiện'} ${label.toLocaleLowerCase('vi-VN')}`}
        aria-pressed={visible} onClick={() => setVisible((current) => !current)}>
        {visible ? 'Ẩn' : 'Hiện'}
      </button>
    </span>
    {hint && <small>{hint}</small>}
  </div>
}
