import { useEffect, useState } from 'react'
import ListPagination from '../../components/ListPagination'
import { receptionCustomersApi } from '../customers/receptionCustomersApi'
import BookingPage from './BookingPage'
import './ReceptionBookingPage.css'

export default function ReceptionBookingPage({ token, query = '', onNavigate }) {
  const [customerId, setCustomerId] = useState(() => new URLSearchParams(query).get('maKhachHang') || '')
  const [customer, setCustomer] = useState(null)
  const [customerError, setCustomerError] = useState('')
  const [searchInput, setSearchInput] = useState('')
  const [keyword, setKeyword] = useState('')
  const [page, setPage] = useState(1)
  const [reload, setReload] = useState(0)
  const [list, setList] = useState({ status: 'idle', items: [], totalItems: 0, error: '' })

  useEffect(() => { setCustomerId(new URLSearchParams(query).get('maKhachHang') || '') }, [query])

  useEffect(() => {
    if (!customerId) { setCustomer(null); return undefined }
    const controller = new AbortController()
    setCustomer(null)
    setCustomerError('')
    receptionCustomersApi.detail(token, customerId, controller.signal).then((result) => {
      if (controller.signal.aborted) return
      if (result?.role !== 'KHACH_HANG' || result.trangThai !== 'Hoạt động') {
        setCustomerError('Chỉ có thể đặt lịch cho Khách hàng đang hoạt động.')
        return
      }
      setCustomer(result)
    }).catch((error) => {
      if (!controller.signal.aborted) setCustomerError(error.status === 0 ? error.message : 'Không tìm thấy khách hàng này.')
    })
    return () => controller.abort()
  }, [token, customerId])

  useEffect(() => {
    if (customerId) return undefined
    const controller = new AbortController()
    setList({ status: 'loading', items: [], totalItems: 0, error: '' })
    receptionCustomersApi.list(token, keyword, page, controller.signal).then((result) => {
      if (controller.signal.aborted) return
      if (!Array.isArray(result?.items) || !Number.isInteger(result.totalItems)) {
        setList({ status: 'error', items: [], totalItems: 0, error: 'Chưa tải được danh sách khách hàng.' })
        return
      }
      const lastPage = Math.max(1, Math.ceil(result.totalItems / 10))
      if (page > lastPage) { setPage(lastPage); return }
      setList({ status: 'success', items: result.items, totalItems: result.totalItems, error: '' })
    }).catch((error) => {
      if (!controller.signal.aborted) setList({ status: 'error', items: [], totalItems: 0,
        error: error.status === 0 ? error.message : 'Chưa tìm được khách hàng.' })
    })
    return () => controller.abort()
  }, [token, customerId, keyword, page, reload])

  function chooseCustomer(id) { onNavigate(`/le-tan/dat-lich?maKhachHang=${encodeURIComponent(id)}`) }

  return <div className="reception-booking">
    {customerId && !customer && <div className="booking-feedback" role={customerError ? 'alert' : 'status'}>{customerError || 'Đang tải khách hàng…'}<button type="button" className="secondary-button" onClick={() => onNavigate('/le-tan/dat-lich')}>Chọn khách khác</button></div>}
    {customer && <><div className="reception-booking-customer"><strong>Khách hàng: {customer.ten}</strong><span>{customer.id} · {customer.sdt}</span><button type="button" className="secondary-button" onClick={() => onNavigate('/le-tan/dat-lich')}>Chọn khách khác</button></div>
      <BookingPage key={customer.id} token={token} onNavigate={onNavigate} mode="reception" customerId={customer.id} customerName={customer.ten} />
    </>}
    {!customerId && <><div className="page-heading"><div><h1>Đặt lịch tại quầy</h1><p className="page-description">Chọn khách hàng trước khi chọn dịch vụ và giờ khám.</p></div></div>
      <form className="reception-booking-search" onSubmit={(event) => { event.preventDefault(); setKeyword(searchInput.trim()); setPage(1) }}>
        <label>Tìm tên, số điện thoại hoặc mã khách<input type="search" value={searchInput} onChange={(event) => setSearchInput(event.target.value)} /></label>
        <button type="submit" className="secondary-button">Tìm</button>
      </form>
      {list.status === 'loading' && <div className="booking-feedback" role="status">Đang tải khách hàng…</div>}
      {list.status === 'error' && <div className="booking-feedback" role="alert"><p>{list.error}</p><button type="button" className="secondary-button" onClick={() => setReload((value) => value + 1)}>Thử lại</button></div>}
      {list.status === 'success' && <>{list.items.length === 0 && <div className="booking-feedback">Không có khách hàng phù hợp.</div>}
        <div className="reception-booking-list">{list.items.map((item) => <article key={item.id}><div><strong>{item.ten}</strong><span>{item.id} · {item.sdt} · {item.trangThai}</span></div>
          <button type="button" className="primary-button" disabled={item.trangThai !== 'Hoạt động'} onClick={() => chooseCustomer(item.id)}>Chọn</button></article>)}</div>
        <ListPagination page={page} pageSize={10} totalItems={list.totalItems} onPageChange={setPage} />
      </>}
    </>}
  </div>
}
