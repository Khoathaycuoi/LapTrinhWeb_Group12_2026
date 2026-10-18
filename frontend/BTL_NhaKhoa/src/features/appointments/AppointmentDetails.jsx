import './AppointmentDetails.css'

const dateTimeFormatter = new Intl.DateTimeFormat('vi-VN', { dateStyle: 'medium', timeStyle: 'short' })

function formatDateTime(value) {
  if (!value) return null
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? null : dateTimeFormatter.format(date)
}

function Detail({ label, value }) {
  if (!value) return null
  return <div><dt>{label}</dt><dd>{value}</dd></div>
}

export default function AppointmentDetails({ appointment, id }) {
  const services = appointment.dichVusDuKien ?? []
  return <div id={id} className="appointment-extra-details">
    <dl>
      <Detail label="Mã lịch hẹn" value={appointment.maLichHen} />
      <Detail label="Khách hàng" value={appointment.tenKH || appointment.maKH} />
      <Detail label="Bác sĩ" value={appointment.tenBacSi || 'Chưa phân công'} />
      <Detail label="Phòng khám" value={appointment.maPhong || 'Chưa phân phòng'} />
      <Detail label="Người hủy" value={appointment.nguoiHuy || (appointment.trangThai === 'Đã hủy' ? 'Không có thông tin' : null)} />
      <Detail label="Lý do hủy" value={appointment.lyDoHuy || (appointment.trangThai === 'Đã hủy' ? 'Không có lý do được ghi nhận.' : null)} />
      <Detail label="Đặt lúc" value={formatDateTime(appointment.createdAt)} />
      <Detail label="Hủy lúc" value={formatDateTime(appointment.huyLuc) || (appointment.trangThai === 'Đã hủy' ? 'Không có thông tin' : null)} />
      <Detail label="Đã đến lúc" value={formatDateTime(appointment.denLuc)} />
      <Detail label="Bắt đầu khám lúc" value={formatDateTime(appointment.batDauKhamLuc)} />
      <Detail label="Hoàn thành lúc" value={formatDateTime(appointment.hoanThanhLuc)} />
      <Detail label="Lý do từ chối" value={appointment.lyDoTuChoi} />
      <Detail label="Từ chối lúc" value={formatDateTime(appointment.tuChoiLuc)} />
      <Detail label="Ghi chú không đến" value={appointment.ghiChuKhongDen} />
      <Detail label="Người đánh dấu không đến" value={appointment.nguoiDanhDauKhongDen} />
      <Detail label="Đánh dấu lúc" value={formatDateTime(appointment.khongDenLuc)} />
    </dl>
    {services.length > 0 && <div className="appointment-extra-services">
      <h3>Dịch vụ dự kiến</h3>
      <ul>{services.map((service) => <li key={service.maDichVu}>
        <span>{service.tenDichVu}</span>
        <span>{service.thoiLuongTaiThoiDiemDatPhut} phút</span>
      </li>)}</ul>
    </div>}
  </div>
}
