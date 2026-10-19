import { useCallback, useEffect, useRef, useState } from 'react'
import ConfirmDialog from '../../components/ConfirmDialog'
import { bookingApi } from './bookingApi'
import './BookingPage.css'

const moneyFormatter = new Intl.NumberFormat('vi-VN', {
  style: 'currency', currency: 'VND', maximumFractionDigits: 0,
})
const dateFormatter = new Intl.DateTimeFormat('vi-VN', { dateStyle: 'full' })
const steps = ['Chọn dịch vụ', 'Chọn bác sĩ và ngày hẹn', 'Chọn giờ hẹn']

function normalizeSearch(value) {
  return String(value ?? '').normalize('NFD').replace(/[\u0300-\u036f]/g, '').replace(/đ/g, 'd').replace(/Đ/g, 'D').toLocaleLowerCase('vi-VN').trim()
}

function todayLocal() {
  const now = new Date()
  const month = String(now.getMonth() + 1).padStart(2, '0')
  const day = String(now.getDate()).padStart(2, '0')
  return `${now.getFullYear()}-${month}-${day}`
}

function timeLabel(time) {
  return time.slice(0, 5)
}

function dateLabel(date) {
  const parsed = new Date(`${date}T00:00:00`)
  return Number.isNaN(parsed.getTime()) ? date : dateFormatter.format(parsed)
}

export default function BookingPage({ token, onNavigate, mode = 'customer', customerId, customerName, staffBasePath = '/le-tan' }) {
  const atReception = mode === 'reception'
  const [catalogReload, setCatalogReload] = useState(0)
  const [catalog, setCatalog] = useState({ status: 'loading', services: [], doctors: [], error: '' })
  const [selectedServiceIds, setSelectedServiceIds] = useState([])
  const [step, setStep] = useState(1)
  const [serviceSearch, setServiceSearch] = useState('')
  const [doctorSearch, setDoctorSearch] = useState('')
  const [doctorId, setDoctorId] = useState('')
  const [date, setDate] = useState('')
  const [selectedTime, setSelectedTime] = useState('')
  const [slots, setSlots] = useState({ key: '', status: 'idle', times: [], error: '', warning: '' })
  const [booking, setBooking] = useState(false)
  const [confirmBooking, setConfirmBooking] = useState(false)
  const [bookingError, setBookingError] = useState('')
  const [draftReady, setDraftReady] = useState(atReception)
  const [draftError, setDraftError] = useState('')
  const draftSaveQueue = useRef(Promise.resolve())

  useEffect(() => {
    const controller = new AbortController()
    let active = true
    Promise.all([
      bookingApi.services(token, controller.signal),
      bookingApi.doctors(token, controller.signal),
      atReception ? Promise.resolve(null) : bookingApi.draft(token, controller.signal).catch(() => null),
    ]).then(([services, doctors, draft]) => {
      if (!active) return
      if (!Array.isArray(services) || !Array.isArray(doctors)) {
        setCatalog({ status: 'error', services: [], doctors: [], error: 'Máy chủ chưa trả thông tin đặt lịch hợp lệ.' })
        return
      }
      if (!atReception) {
        if (!draft) setDraftError('Chưa tải được bản nháp. Bạn vẫn có thể đặt lịch, nhưng lựa chọn có thể mất khi tải lại trang.')
        const serviceIds = Array.isArray(draft?.danhSachMaDichVu)
          ? draft.danhSachMaDichVu.filter((id) => services.some((service) => service.maDichVu === id)) : []
        const restoredDoctor = doctors.some((doctor) => doctor.id === draft?.maBacSi) ? draft.maBacSi : ''
        const restoredDate = typeof draft?.ngayHen === 'string' && draft.ngayHen >= todayLocal() ? draft.ngayHen : ''
        const restoredStep = Math.min(3, Math.max(1, Number(draft?.buoc) || 1))
        setSelectedServiceIds(serviceIds)
        setDoctorId(restoredDoctor)
        setDate(restoredDate)
        setSelectedTime(restoredDate && typeof draft?.gioHen === 'string' ? draft.gioHen : '')
        setStep(serviceIds.length ? restoredDate ? restoredStep : Math.min(restoredStep, 2) : 1)
        setDraftReady(true)
      }
      setCatalog({ status: 'success', services, doctors, error: '' })
    }).catch((error) => {
      if (active) setCatalog({ status: 'error', services: [], doctors: [], error: error.message })
    })
    return () => { active = false; controller.abort() }
  }, [token, catalogReload, atReception])

  const queueDraftSave = useCallback((payload) => {
    const save = draftSaveQueue.current.catch(() => {}).then(() => bookingApi.saveDraft(token, payload))
    draftSaveQueue.current = save
    save.then(() => setDraftError('')).catch(() => setDraftError('Chưa lưu được bản nháp. Vui lòng kiểm tra kết nối.'))
    return save
  }, [token])

  useEffect(() => {
    if (atReception || !draftReady || catalog.status !== 'success' || booking) return
    queueDraftSave({ danhSachMaDichVu: selectedServiceIds, maBacSi: doctorId || null,
      ngayHen: date || null, gioHen: selectedTime || null, buoc: step })
  }, [atReception, draftReady, catalog.status, booking, selectedServiceIds, doctorId, date, selectedTime, step, queueDraftSave])

  const selectedServices = catalog.services.filter((service) => selectedServiceIds.includes(service.maDichVu))
  const totalMinutes = selectedServices.reduce((sum, service) => sum + Number(service.thoiLuongDuKienPhut), 0)
  const estimatedPrice = selectedServices.reduce((sum, service) => sum + Number(service.donGia), 0)
  const slotKey = catalog.status === 'success' && step === 3 && date && totalMinutes > 0 && (!atReception || doctorId)
    ? `${date}|${doctorId || 'all'}|${totalMinutes}` : ''

  useEffect(() => {
    if (!slotKey) return undefined
    const controller = new AbortController()
    let active = true
    const ids = doctorId ? [doctorId] : catalog.doctors.map((doctor) => doctor.id)

    Promise.allSettled(ids.map((id) => bookingApi.availableSlots(token, id, date, totalMinutes, controller.signal,
      atReception ? customerId : undefined)))
      .then((results) => {
        if (!active) return
        const fulfilled = results.filter((result) => result.status === 'fulfilled' && Array.isArray(result.value))
        const rejected = results.filter((result) => result.status === 'rejected')
        const invalidCount = results.length - fulfilled.length - rejected.length
        if (fulfilled.length === 0 && results.length > 0) {
          setSlots({ key: slotKey, status: 'error', times: [], error: rejected[0]?.reason.message ?? 'Máy chủ chưa trả danh sách giờ trống hợp lệ.', warning: '' })
          return
        }
        const times = [...new Set(fulfilled.flatMap((result) => result.value))].sort()
        setSlots({ key: slotKey, status: 'success', times, error: '',
          warning: rejected.length || invalidCount ? 'Một số bác sĩ chưa trả được giờ trống. Danh sách dưới đây chỉ gồm các giờ đã tra cứu thành công.' : '' })
        setSelectedTime((current) => current && !times.includes(current) ? '' : current)
      })

    return () => { active = false; controller.abort() }
  }, [token, catalog.doctors, doctorId, date, totalMinutes, slotKey, atReception, customerId])

  function toggleService(id) {
    setSelectedServiceIds((current) => current.includes(id) ? current.filter((value) => value !== id) : [...current, id])
    setSelectedTime('')
    setBookingError('')
  }

  function nextStep() {
    setBookingError('')
    if (step === 1 && (selectedServices.length === 0 || totalMinutes <= 0)) {
      setBookingError('Vui lòng chọn ít nhất một dịch vụ để tiếp tục.')
      return
    }
    if (step === 2 && (!date || date < todayLocal() || (atReception && !doctorId))) {
      setBookingError(atReception && !doctorId ? 'Vui lòng chọn bác sĩ để đặt lịch tại quầy.' : 'Vui lòng chọn ngày hẹn từ hôm nay trở đi.')
      return
    }
    setStep((current) => Math.min(current + 1, 3))
  }

  function previousStep() {
    setBookingError('')
    setStep((current) => Math.max(current - 1, 1))
  }

  function submit(event) {
    event.preventDefault()
    setBookingError('')
    if (step !== 3 || !selectedServiceIds.length || !date || date < todayLocal() ||
      (atReception && (!customerId || !doctorId)) ||
      !selectedTime || slots.key !== slotKey || !slots.times.includes(selectedTime)) {
      setBookingError('Vui lòng chọn dịch vụ, ngày và giờ còn trống trước khi đặt lịch.')
      return
    }
    setConfirmBooking(true)
  }

  async function sendBooking() {
    if (booking) return
    setBooking(true)
    try {
      if (!atReception) {
        await queueDraftSave({ danhSachMaDichVu: selectedServiceIds, maBacSi: doctorId || null,
          ngayHen: date, gioHen: selectedTime, buoc: step }).catch(() => {})
      }
      const response = await bookingApi.book(token, {
        ...(atReception ? { maKhachHang: customerId } : {}),
        ...(doctorId ? { maBacSi: doctorId } : {}),
        ngayHen: date,
        gioHen: selectedTime,
        danhSachMaDichVu: selectedServiceIds,
      })
      onNavigate(atReception ? `${staffBasePath}/lich-hen?ngayHen=${date}` : '/khach-hang/lich-hen?nhom=upcoming', false, {
        notice: typeof response === 'string' ? response : 'Đã gửi yêu cầu đặt lịch.',
      })
    } catch (error) {
      setConfirmBooking(false)
      setBookingError(error.message)
    } finally {
      setBooking(false)
    }
  }

  const visibleSlots = slots.key === slotKey ? slots : { status: 'loading', times: [], error: '', warning: '' }
  const morningSlots = visibleSlots.times.filter((time) => timeLabel(time) < '12:00')
  const afternoonSlots = visibleSlots.times.filter((time) => timeLabel(time) >= '12:00')
  const selectedDoctor = catalog.doctors.find((doctor) => doctor.id === doctorId)
  const serviceQuery = normalizeSearch(serviceSearch)
  const doctorQuery = normalizeSearch(doctorSearch)
  const visibleServices = catalog.services.filter((service) => normalizeSearch(service.tenDichVu).includes(serviceQuery))
  const visibleDoctors = catalog.doctors.filter((doctor) => normalizeSearch(doctor.ten).includes(doctorQuery))

  return <div className="booking-page">
    <div className="page-heading"><div><h1>{atReception ? `Đặt lịch cho ${customerName}` : 'Đặt lịch khám'}</h1><p className="page-description">{atReception ? 'Chọn dịch vụ, bác sĩ và giờ hẹn cho khách tại quầy.' : 'Chọn dịch vụ, thời gian và bác sĩ phù hợp. Phòng khám sẽ xác nhận yêu cầu của bạn.'}</p></div></div>

    {catalog.status === 'loading' && <div className="booking-feedback" role="status">Đang tải dịch vụ và bác sĩ…</div>}
    {catalog.status === 'error' && <div className="booking-feedback booking-feedback-error" role="alert">
      <strong>Không tải được thông tin đặt lịch.</strong><p>{catalog.error}</p>
      <button type="button" className="primary-button" onClick={() => { setCatalog({ status: 'loading', services: [], doctors: [], error: '' }); setCatalogReload((value) => value + 1) }}>Thử lại</button>
    </div>}

    {catalog.status === 'success' && <form onSubmit={(event) => { if (step === 3) submit(event); else { event.preventDefault(); nextStep() } }} className="booking-form">
      {draftError && !atReception && <div className="alert alert-error" role="status">{draftError}</div>}
      <ol className="booking-stepper" aria-label="Các bước đặt lịch">
        {steps.map((label, index) => <li key={label} className={step === index + 1 ? 'active' : step > index + 1 ? 'completed' : ''} aria-current={step === index + 1 ? 'step' : undefined}>
          <span>{String(index + 1).padStart(2, '0')}</span><strong>{label}</strong>
        </li>)}
      </ol>

      {step === 1 && <section className="booking-panel">
        <div className="booking-section-heading"><span>01</span><div><h2>Chọn dịch vụ</h2><p>Có thể chọn nhiều dịch vụ trong cùng một lịch hẹn.</p></div></div>
        {catalog.services.length > 0 && <label className="booking-search-label" htmlFor="booking-service-search">Tìm dịch vụ
          <input id="booking-service-search" type="search" value={serviceSearch} onChange={(event) => setServiceSearch(event.target.value)} placeholder="Nhập tên dịch vụ" autoComplete="off" />
        </label>}
        {visibleServices.length ? <div className="booking-services">{visibleServices.map((service) => <label key={service.maDichVu} className={selectedServiceIds.includes(service.maDichVu) ? 'booking-service selected' : 'booking-service'}>
          <input type="checkbox" checked={selectedServiceIds.includes(service.maDichVu)} onChange={() => toggleService(service.maDichVu)} />
          <span className="booking-service-content"><strong>{service.tenDichVu}</strong><span>{service.thoiLuongDuKienPhut} phút · {moneyFormatter.format(service.donGia)}</span>{service.moTa && <small>{service.moTa}</small>}</span>
        </label>)}</div> : <p className="booking-empty">{catalog.services.length ? 'Không tìm thấy dịch vụ phù hợp.' : 'Hiện chưa có dịch vụ đang hoạt động để đặt lịch.'}</p>}
        {selectedServices.length > 0 && <div className="booking-selection-summary"><span>Đã chọn {selectedServices.length} dịch vụ · {totalMinutes} phút</span><strong>Giá tham khảo: {moneyFormatter.format(estimatedPrice)}</strong></div>}
      </section>}

      {step === 2 && <section className="booking-panel">
        <div className="booking-section-heading"><span>02</span><div><h2>Chọn bác sĩ và ngày hẹn</h2><p>{atReception ? 'Đặt tại quầy cần chọn bác sĩ để xác nhận lịch ngay khi còn chỗ.' : 'Bạn có thể để phòng khám sắp xếp bác sĩ.'}</p></div></div>
        <div className="booking-fields booking-step-two">
          <label className="booking-search-label" htmlFor="booking-doctor-search">Tìm bác sĩ
            <input id="booking-doctor-search" type="search" value={doctorSearch} onChange={(event) => setDoctorSearch(event.target.value)} placeholder="Nhập tên bác sĩ" autoComplete="off" />
          </label>
          <label className="booking-date-field">Ngày hẹn<input type="date" min={todayLocal()} value={date} onChange={(event) => { setDate(event.target.value); setSelectedTime(''); setBookingError('') }} required /></label>
          <div className="booking-doctor-field">
            <span className="booking-doctor-list-title">Bác sĩ</span>
            <div className="booking-doctor-options" role="radiogroup" aria-label="Chọn bác sĩ">
              {!atReception && <label className={doctorId === '' ? 'booking-doctor-option selected' : 'booking-doctor-option'}>
                <input type="radio" name="booking-doctor" checked={doctorId === ''} onChange={() => { setDoctorId(''); setSelectedTime('') }} />Để phòng khám sắp xếp
              </label>}
              {visibleDoctors.map((doctor) => <label key={doctor.id} className={doctorId === doctor.id ? 'booking-doctor-option selected' : 'booking-doctor-option'}>
                <input type="radio" name="booking-doctor" checked={doctorId === doctor.id} onChange={() => { setDoctorId(doctor.id); setSelectedTime('') }} />{doctor.ten}
              </label>)}
              {doctorQuery && visibleDoctors.length === 0 && <p className="booking-empty">Không tìm thấy bác sĩ phù hợp.</p>}
            </div>
            {selectedDoctor && doctorQuery && !visibleDoctors.some((doctor) => doctor.id === doctorId) && <p className="booking-selected-doctor">Đang chọn: {selectedDoctor.ten}</p>}
          </div>
        </div>
      </section>}

      {step === 3 && <section className="booking-panel">
        <div className="booking-section-heading"><span>03</span><div><h2>Chọn giờ hẹn</h2><p>Giờ trống được tra cứu theo tổng thời lượng dịch vụ đã chọn.</p></div></div>
        <div className="booking-review"><span>{selectedServices.length} dịch vụ · {totalMinutes} phút</span><span>{selectedDoctor ? `Bác sĩ ${selectedDoctor.ten}` : 'Phòng khám sắp xếp bác sĩ'}</span><span>{dateLabel(date)}</span></div>
        {!date || !selectedServiceIds.length ? <p className="booking-empty">Chọn ít nhất một dịch vụ và ngày hẹn để xem giờ trống.</p>
          : visibleSlots.status === 'loading' ? <p className="booking-empty" role="status">Đang tìm giờ trống…</p>
            : visibleSlots.status === 'error' ? <div className="alert alert-error" role="alert">{visibleSlots.error}</div>
              : <>
                {visibleSlots.warning && <div className="alert alert-error" role="alert">{visibleSlots.warning}</div>}
                {visibleSlots.times.length === 0 ? <p className="booking-empty">{catalog.doctors.length === 0 ? 'Hiện chưa có bác sĩ hoạt động để tra cứu giờ trống.' : 'Không có giờ trống phù hợp trong ngày này. Vui lòng chọn ngày hoặc bác sĩ khác.'}</p>
                  : <div className="booking-slot-groups">
                    {morningSlots.length > 0 && <div><h3>Buổi sáng</h3><div className="booking-slots">{morningSlots.map((time) => <button key={time} type="button" className={selectedTime === time ? 'booking-slot selected' : 'booking-slot'} aria-pressed={selectedTime === time} onClick={() => { setSelectedTime(time); setBookingError('') }}>{timeLabel(time)}</button>)}</div></div>}
                    {afternoonSlots.length > 0 && <div><h3>Buổi chiều</h3><div className="booking-slots">{afternoonSlots.map((time) => <button key={time} type="button" className={selectedTime === time ? 'booking-slot selected' : 'booking-slot'} aria-pressed={selectedTime === time} onClick={() => { setSelectedTime(time); setBookingError('') }}>{timeLabel(time)}</button>)}</div></div>}
                  </div>}
              </>}
      </section>}

      <div className="booking-submit-row">
        {step === 3 && <p>{atReception ? 'Lịch sẽ được xác nhận ngay nếu bác sĩ và phòng còn trống.' : <>Lịch của Khách hàng sẽ ở trạng thái <strong>Chờ xác nhận</strong>. Bạn có thể theo dõi trong “Lịch hẹn của tôi”.</>}</p>}
        {bookingError && <div className="alert alert-error" role="alert">{bookingError}</div>}
        <div className="booking-navigation">
          {step > 1 && <button type="button" className="secondary-button" disabled={booking} onClick={previousStep}>Quay lại</button>}
          {step < 3 ? <button type="button" className="primary-button" onClick={nextStep}>Tiếp tục</button>
            : <button type="submit" className="primary-button" disabled={booking || !selectedTime || slots.key !== slotKey || !slots.times.includes(selectedTime)}>{atReception ? 'Đặt lịch tại quầy' : 'Gửi yêu cầu đặt lịch'}</button>}
        </div>
      </div>
    </form>}
    {confirmBooking && <ConfirmDialog title="Xác nhận đặt lịch" message={`${atReception ? `Đặt lịch cho ${customerName}` : `Gửi yêu cầu cho ${selectedServices.length} dịch vụ`} vào ${dateLabel(date)}, lúc ${timeLabel(selectedTime)}${selectedDoctor ? ` với bác sĩ ${selectedDoctor.ten}` : ', để phòng khám sắp xếp bác sĩ'}?${atReception ? ' Hệ thống sẽ kiểm tra lại và xác nhận nếu còn chỗ.' : ' Lịch sẽ ở trạng thái Chờ xác nhận.'}`}
      confirmLabel={booking ? 'Đang gửi…' : 'Xác nhận đặt lịch'} busy={booking} onCancel={() => setConfirmBooking(false)} onConfirm={sendBooking} />}
  </div>
}
