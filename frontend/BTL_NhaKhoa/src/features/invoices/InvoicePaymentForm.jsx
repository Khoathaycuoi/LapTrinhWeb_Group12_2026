import { useState } from 'react'

const money = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND', maximumFractionDigits: 0 })
const methods = [
  { value: 'TIEN_MAT', label: 'Tiền mặt' },
  { value: 'CHUYEN_KHOAN', label: 'Chuyển khoản' },
  { value: 'KHAC', label: 'Khác', description: 'Ghi rõ cách thanh toán trong ghi chú.' },
]

export default function InvoicePaymentForm({ invoice, error, onErrorDismiss, onReview, onCancel }) {
  const [step, setStep] = useState('method')
  const [amount, setAmount] = useState(String(invoice.conNo))
  const [method, setMethod] = useState('')
  const [note, setNote] = useState('')
  const [transferVerified, setTransferVerified] = useState(false)
  const [qrStatus, setQrStatus] = useState('loading')
  const [validationError, setValidationError] = useState('')

  function clearError() {
    setValidationError('')
    onErrorDismiss()
  }

  function nextStep() {
    if (!method) { setValidationError('Vui lòng chọn phương thức thanh toán.'); return }
    clearError()
    setStep('details')
  }

  function review(event) {
    event.preventDefault()
    clearError()
    const rawAmount = amount.trim()
    const value = Number(rawAmount)
    if (!/^\d+(?:\.\d{1,2})?$/.test(rawAmount) || !Number.isFinite(value) || value <= 0) {
      setValidationError('Vui lòng nhập số tiền lớn hơn 0, tối đa hai chữ số thập phân.')
      return
    }
    if (value > invoice.conNo) {
      setValidationError(`Số tiền đã nhận không được vượt quá số còn nợ ${money.format(invoice.conNo)}.`)
      return
    }
    if (method === 'CHUYEN_KHOAN' && qrStatus !== 'ready') {
      setValidationError('Chưa tải được mã QR chuyển khoản. Vui lòng thử lại sau.')
      return
    }
    if (method === 'CHUYEN_KHOAN' && !transferVerified) {
      setValidationError('Vui lòng kiểm tra tiền đã vào tài khoản phòng khám trước khi ghi nhận.')
      return
    }
    if (method === 'KHAC' && !note.trim()) {
      setValidationError('Vui lòng ghi rõ phương thức thanh toán trong ghi chú.')
      return
    }
    onReview({ soTien: value, phuongThuc: method, ghiChu: method === 'KHAC' ? note.trim() : null })
  }

  return <div className="invoice-payment-form">
    <h3>Thanh toán</h3>
    {step === 'method' ? <>
      <p>Chọn cách khách thanh toán hóa đơn {invoice.maHoaDon}.</p>
      <div className="invoice-payment-methods" role="radiogroup" aria-label="Phương thức thanh toán">
        {methods.map((option) => <label key={option.value} className={`invoice-payment-method ${method === option.value ? 'selected' : ''}`}>
          <input type="radio" name="invoice-payment-method" value={option.value} checked={method === option.value}
            onChange={() => { setMethod(option.value); clearError() }} />
          <span><strong>{option.label}</strong>{option.description && <small>{option.description}</small>}</span>
        </label>)}
      </div>
      {(validationError || error) && <p className="invoice-payment-error" role="alert">{validationError || error}</p>}
      <div className="invoice-payment-actions"><button type="button" className="secondary-button" onClick={onCancel}>Đóng</button><button type="button" className="primary-button" onClick={nextStep}>Tiếp tục</button></div>
    </> : <form noValidate onSubmit={review}>
      <div className="invoice-payment-step-heading"><strong>{methods.find((option) => option.value === method)?.label}</strong><button type="button" onClick={() => { setStep('method'); clearError() }}>Đổi phương thức</button></div>
      {method === 'CHUYEN_KHOAN' && <div className="invoice-transfer-panel">
        {qrStatus !== 'error' && <img src="/payment-qr.jpg" alt="Mã QR chuyển khoản tới tài khoản của phòng khám" onLoad={() => setQrStatus('ready')} onError={() => setQrStatus('error')} />}
        {qrStatus === 'error' && <p className="invoice-payment-error" role="alert">Không tải được mã QR chuyển khoản. Chưa thể ghi nhận bằng phương thức này.</p>}
        <div><strong>Quét mã để chuyển khoản</strong><dl><div><dt>Mã hóa đơn</dt><dd>{invoice.maHoaDon}</dd></div><div><dt>Còn nợ</dt><dd>{money.format(invoice.conNo)}</dd></div></dl></div>
      </div>}
      <div className="invoice-payment-fields">
      <label>Số tiền đã nhận (đ)
        <input type="text" inputMode="decimal" autoComplete="off" value={amount} aria-invalid={Boolean(validationError)}
          onChange={(event) => { setAmount(event.target.value); clearError() }} />
      </label>
      </div>
      {method === 'KHAC' && <label>Ghi chú (bắt buộc)
        <textarea value={note} maxLength={255} rows={2} onChange={(event) => { setNote(event.target.value); clearError() }} />
      </label>}
      {method === 'CHUYEN_KHOAN' && qrStatus === 'ready' && <label className="invoice-transfer-verified"><input type="checkbox" checked={transferVerified} onChange={(event) => { setTransferVerified(event.target.checked); clearError() }} /><span>Tôi đã kiểm tra tiền vào tài khoản phòng khám.</span></label>}
      {(validationError || error) && <p className="invoice-payment-error" role="alert">{validationError || error}</p>}
      <div className="invoice-payment-actions"><button type="button" className="secondary-button" onClick={onCancel}>Đóng</button><button type="submit" className="primary-button" disabled={method === 'CHUYEN_KHOAN' && qrStatus !== 'ready'}>Ghi nhận thanh toán</button></div>
    </form>}
  </div>
}
