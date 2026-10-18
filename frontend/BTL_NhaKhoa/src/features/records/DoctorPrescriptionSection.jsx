import { useState } from 'react'

const emptyMedicine = () => ({ tenThuoc: '', hamLuong: '', lieuDung: '', soLanMoiNgay: '', soNgayDung: '', soLuong: '1', huongDan: '' })

export default function DoctorPrescriptionSection({ record, editable, onReview }) {
  const [medicines, setMedicines] = useState([emptyMedicine()])
  const [note, setNote] = useState('')

  function updateMedicine(index, field, value) {
    setMedicines((current) => current.map((item, position) => position === index ? { ...item, [field]: value } : item))
  }

  function reviewPrescription(event) {
    event.preventDefault()
    const drugs = medicines.map((item) => ({
      tenThuoc: item.tenThuoc.trim(), hamLuong: item.hamLuong.trim() || null,
      lieuDung: item.lieuDung.trim(), soLanMoiNgay: item.soLanMoiNgay ? Number(item.soLanMoiNgay) : null,
      soNgayDung: item.soNgayDung ? Number(item.soNgayDung) : null,
      soLuong: Number(item.soLuong), huongDan: item.huongDan.trim() || null,
    }))
    if (drugs.some((item) => !item.tenThuoc || !item.lieuDung || !Number.isFinite(item.soLuong) || item.soLuong <= 0)) return
    onReview({ type: 'prescribe', payload: { ghiChu: note.trim() || null, danhSachThuoc: drugs } })
  }

  return <section className="doctor-record-section"><h2>Đơn thuốc</h2>
    {(record.donThuocs || []).length === 0 && <p>Chưa có đơn thuốc.</p>}
    <div className="doctor-prescription-list">{record.donThuocs?.map((prescription) => <article key={prescription.maDonThuoc}>
      <div className="doctor-prescription-heading"><strong>{prescription.maDonThuoc}</strong>{editable && <button type="button" className="doctor-text-danger" onClick={() => onReview({ type: 'removePrescription', prescription })}>Bỏ đơn thuốc</button>}</div>
      {prescription.ghiChu && <p>{prescription.ghiChu}</p>}
      <ul>{prescription.chiTietDonThuocs?.map((drug, index) => <li key={`${drug.tenThuoc}-${index}`}><strong>{drug.tenThuoc}{drug.hamLuong ? ` · ${drug.hamLuong}` : ''}</strong><span>{drug.lieuDung} · Số lượng {drug.soLuong}{drug.soLanMoiNgay ? ` · ${drug.soLanMoiNgay} lần/ngày` : ''}{drug.soNgayDung ? ` · ${drug.soNgayDung} ngày` : ''}</span>{drug.huongDan && <small>{drug.huongDan}</small>}</li>)}</ul>
    </article>)}</div>
    {editable && <form className="doctor-prescription-form" onSubmit={reviewPrescription}><h3>Kê đơn mới</h3>
      {medicines.map((medicine, index) => <div className="doctor-medicine-form" key={index}>
        <div className="doctor-medicine-heading"><strong>Thuốc {index + 1}</strong>{medicines.length > 1 && <button type="button" className="doctor-text-danger" onClick={() => setMedicines((current) => current.filter((_, position) => position !== index))}>Bỏ thuốc này</button>}</div>
        <label>Tên thuốc<input required maxLength={255} value={medicine.tenThuoc} onChange={(event) => updateMedicine(index, 'tenThuoc', event.target.value)} /></label>
        <label>Hàm lượng<input maxLength={100} value={medicine.hamLuong} onChange={(event) => updateMedicine(index, 'hamLuong', event.target.value)} /></label>
        <label>Liều dùng<input required maxLength={255} value={medicine.lieuDung} onChange={(event) => updateMedicine(index, 'lieuDung', event.target.value)} placeholder="Ví dụ: 1 viên sau ăn" /></label>
        <label>Số lượng<input required type="number" min="0.01" step="0.01" value={medicine.soLuong} onChange={(event) => updateMedicine(index, 'soLuong', event.target.value)} /></label>
        <label>Số lần mỗi ngày<input type="number" min="1" max="65535" value={medicine.soLanMoiNgay} onChange={(event) => updateMedicine(index, 'soLanMoiNgay', event.target.value)} /></label>
        <label>Số ngày dùng<input type="number" min="1" max="65535" value={medicine.soNgayDung} onChange={(event) => updateMedicine(index, 'soNgayDung', event.target.value)} /></label>
        <label className="doctor-wide-field">Hướng dẫn<input maxLength={255} value={medicine.huongDan} onChange={(event) => updateMedicine(index, 'huongDan', event.target.value)} /></label>
      </div>)}
      <button type="button" className="doctor-add-medicine" onClick={() => setMedicines((current) => [...current, emptyMedicine()])}>+ Thêm thuốc</button>
      <label>Ghi chú đơn thuốc<textarea rows={2} value={note} onChange={(event) => setNote(event.target.value)} /></label>
      <button type="submit" className="primary-button">Kê đơn</button>
    </form>}
  </section>
}
