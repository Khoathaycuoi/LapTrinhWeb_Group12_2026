import { useState } from 'react'

const money = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND', maximumFractionDigits: 0 })

export default function DoctorServiceSection({ record, editable, onReview }) {
  const [quantities, setQuantities] = useState({})
  const planned = record.dichVusDuKien || []
  const performed = record.dichVuThucHiens || []

  return <section className="doctor-record-section"><h2>Dịch vụ trong lịch</h2>
    {planned.length === 0 && <p>Không có dịch vụ dự kiến trong lịch hẹn.</p>}
    <div className="doctor-service-list">{planned.map((service) => {
      const done = performed.find((item) => item.maDichVu === service.maDichVu)
      return <div key={service.maDichVu} className="doctor-service-row"><div><strong>{service.tenDichVu}</strong><span>{money.format(service.donGiaTaiThoiDiemDat)} · {service.thoiLuongTaiThoiDiemDatPhut} phút</span>{done && <small>Đã thực hiện · Số lượng {done.soLuong}</small>}</div>
        {editable && (done ? <button type="button" className="doctor-text-danger" onClick={() => onReview({ type: 'removeService', service })}>Bỏ đánh dấu</button>
          : <div className="doctor-service-action"><label>Số lượng<input type="number" min="1" max="65535" value={quantities[service.maDichVu] || '1'} onChange={(event) => setQuantities({ ...quantities, [service.maDichVu]: event.target.value })} /></label><button type="button" className="secondary-button" onClick={() => {
            const quantity = Number(quantities[service.maDichVu] || '1')
            if (Number.isInteger(quantity) && quantity >= 1 && quantity <= 65535) onReview({ type: 'perform', service, quantity })
          }}>Đã thực hiện</button></div>)}
      </div>
    })}</div>
  </section>
}
