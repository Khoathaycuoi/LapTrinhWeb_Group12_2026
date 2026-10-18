import { useState } from 'react'

export default function InvoicePaymentForm({ invoice, error, onErrorDismiss, onReview, onCancel }) {
  const [amount, setAmount] = useState(String(invoice.conNo))
  const [method, setMethod] = useState('TIEN_MAT')
  const [note, setNote] = useState('')
  const [validationError, setValidationError] = useState('')

  function updateValue(setter, value) {
    setter(value)
    setValidationError('')
    onErrorDismiss()
  }

  function review(event) {
    event.preventDefault()
    const value = Number(amount)
    if (!/^\d+(?:\.\d{1,2})?$/.test(amount) || !Number.isFinite(value) || value <= 0) {
      setValidationError('Nhập số tiền hợp lệ lớn hơn 0, tối đa hai chữ số thập phân.')
      return
    }
    if (value > invoice.conNo) {
      setValidationError('Số tiền thu không được vượt quá số còn nợ.')
      return
    }
    onReview({ soTien: value, phuongThuc: method, ghiChu: note.trim() || null })
  }

  return <form className="invoice-payment-form" onSubmit={review}>
    <h3>Ghi nhận thanh toán</h3>
    <p>Chỉ ghi khoản thu sau khi đã nhận hoặc xác minh tiền vào tài khoản phòng khám.</p>
    <div className="invoice-payment-fields">
      <label>Số tiền đã nhận (đ)
        <input type="number" min="0.01" max={invoice.conNo} step="0.01" required value={amount}
          onChange={(event) => updateValue(setAmount, event.target.value)} />
      </label>
      <label>Phương thức
        <select value={method} onChange={(event) => updateValue(setMethod, event.target.value)}>
          <option value="TIEN_MAT">Tiền mặt</option>
          <option value="CHUYEN_KHOAN">Chuyển khoản</option>
          <option value="KHAC">Khác</option>
        </select>
      </label>
    </div>
    <label>Ghi chú (tùy chọn)
      <textarea value={note} maxLength={255} rows={2} onChange={(event) => updateValue(setNote, event.target.value)} />
    </label>
    {(validationError || error) && <p className="invoice-payment-error" role="alert">{validationError || error}</p>}
    <div className="invoice-payment-actions">
      <button type="button" className="secondary-button" onClick={onCancel}>Hủy</button>
      <button type="submit" className="primary-button">Tiếp tục</button>
    </div>
  </form>
}
