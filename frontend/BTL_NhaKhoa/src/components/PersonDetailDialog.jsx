import { apiAssetUrl } from '../lib/api'
import './PersonDetailDialog.css'

function displayDate(value) {
  if (!value) return 'Chưa cập nhật'
  const [year, month, day] = value.slice(0, 10).split('-')
  return year && month && day ? `${day}/${month}/${year}` : value
}

function DetailField({ label, value, wide = false }) {
  return <div className={`person-detail-field${wide ? ' person-detail-field-wide' : ''}`}>
    <dt>{label}</dt><dd>{value || value === 0 ? value : 'Chưa cập nhật'}</dd>
  </div>
}

export default function PersonDetailDialog({ type, id, detail, avatarUrl, onClose }) {
  const isStaff = type === 'staff'
  const title = isStaff ? 'Chi tiết nhân sự' : 'Chi tiết khách hàng'
  const person = detail.data
  const photoUrl = avatarUrl || person?.anhDaiDienUrl

  return <div className="dialog-backdrop" onMouseDown={(event) => { if (event.target === event.currentTarget) onClose() }}>
    <section className="person-detail-dialog" role="dialog" aria-modal="true" aria-labelledby="person-detail-title">
      <div className="person-detail-heading">
        <div className="person-detail-identity">
          {person && <span className="person-detail-avatar">{photoUrl
            ? <img src={apiAssetUrl(photoUrl)} alt={`Ảnh đại diện của ${person.ten}`} />
            : (person.ten?.trim().split(/\s+/).at(-1)?.[0]?.toLocaleUpperCase('vi-VN') || '?')}</span>}
          <div><span className="person-detail-kicker">{isStaff ? 'Nhân sự' : 'Khách hàng'} · {id}</span><h2 id="person-detail-title">{person?.ten || title}</h2></div>
        </div>
        <button type="button" className="secondary-button" onClick={onClose}>Đóng</button>
      </div>
      {detail.status === 'loading' && <p className="person-detail-feedback" role="status">Đang tải thông tin…</p>}
      {detail.status === 'error' && <p className="person-detail-feedback" role="alert">{detail.error}</p>}
      {detail.status === 'success' && person && <dl className="person-detail-grid">
        <DetailField label="Số điện thoại" value={person.sdt} />
        <DetailField label="Trạng thái" value={person.trangThai} />
        <DetailField label="Ngày sinh" value={displayDate(person.ngaySinh)} />
        <DetailField label="Giới tính" value={person.gioiTinh} />
        <DetailField label="Địa chỉ" value={person.diaChi} wide />
        {isStaff && person.role === 'BAC_SI' && <>
          <DetailField label="Bằng cấp" value={person.bangCap} />
          <DetailField label="Chứng chỉ" value={person.chungChi} />
          <DetailField label="Số năm kinh nghiệm" value={person.soNamKinhNghiem} />
        </>}
      </dl>}
    </section>
  </div>
}
