import { useEffect, useState } from 'react'
import ConfirmDialog from '../../components/ConfirmDialog'
import ListPagination from '../../components/ListPagination'
import { invoicesApi } from './invoicesApi'

const money = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND', maximumFractionDigits: 0 })
const date = new Intl.DateTimeFormat('vi-VN', { dateStyle: 'medium', timeStyle: 'short' })

export default function InvoiceCreation({ token, onCreated, onClose }) {
  const [page, setPage] = useState(1)
  const [reload, setReload] = useState(0)
  const [list, setList] = useState({ status: 'loading', items: [], totalItems: 0, error: '' })
  const [selected, setSelected] = useState(null)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    const controller = new AbortController()
    setList({ status: 'loading', items: [], totalItems: 0, error: '' })
    invoicesApi.eligibleExams(token, page, controller.signal).then((result) => {
      if (controller.signal.aborted) return
      if (!Array.isArray(result?.items) || !Number.isInteger(result.totalItems)) {
        setList({ status: 'error', items: [], totalItems: 0, error: 'Máy chủ chưa trả danh sách lượt khám.' })
        return
      }
      const lastPage = Math.max(1, Math.ceil(result.totalItems / 10))
      if (page > lastPage) { setPage(lastPage); return }
      setList({ status: 'success', items: result.items, totalItems: result.totalItems, error: '' })
    }).catch((fetchError) => {
      if (!controller.signal.aborted) setList({ status: 'error', items: [], totalItems: 0,
        error: fetchError.status === 0 ? fetchError.message : 'Chưa tải được lượt khám. Vui lòng thử lại.' })
    })
    return () => controller.abort()
  }, [token, page, reload])

  async function createInvoice() {
    if (!selected || busy) return
    setBusy(true)
    setError('')
    try {
      const result = await invoicesApi.create(token, selected.maHoSoKham)
      setSelected(null)
      onCreated(result?.maHoaDon)
    } catch (createError) {
      setSelected(null)
      setError(createError.status === 400 || createError.status === 0
        ? createError.message : 'Chưa lập được hóa đơn. Vui lòng thử lại.')
      setReload((value) => value + 1)
    } finally {
      setBusy(false)
    }
  }

  return <section className="invoice-creation">
    <div className="invoice-creation-heading"><div><h2>Lập hóa đơn</h2><p>Lượt khám đã hoàn thành, hồ sơ đã chốt và chưa có hóa đơn.</p></div><button type="button" className="secondary-button" onClick={onClose}>Đóng</button></div>
    {error && <p className="invoice-payment-error" role="alert">{error}</p>}
    {list.status === 'loading' && <p role="status">Đang tải lượt khám…</p>}
    {list.status === 'error' && <div role="alert"><p>{list.error}</p><button type="button" className="secondary-button" onClick={() => setReload((value) => value + 1)}>Thử lại</button></div>}
    {list.status === 'success' && <>
      {list.items.length === 0 && <p>Chưa có lượt khám đủ điều kiện lập hóa đơn.</p>}
      <div className="invoice-creation-list">{list.items.map((exam) => <article key={exam.maHoSoKham}>
        <div className="invoice-creation-row"><div><strong>{exam.tenKhachHang}</strong><span>{exam.maKhachHang} · {exam.maHoSoKham} · Khám {date.format(new Date(exam.ngayKham))}</span></div><strong>{money.format(exam.tongTienDuKien)}</strong></div>
        <ul>{exam.dichVusThucHien.map((service) => <li key={service.maDichVuThucHien}>{service.tenDichVu} × {service.soLuong} · {money.format(service.thanhTien)}</li>)}</ul>
        <button type="button" className="primary-button" onClick={() => { setError(''); setSelected(exam) }}>Lập hóa đơn</button>
      </article>)}</div>
      <ListPagination page={page} pageSize={10} totalItems={list.totalItems} onPageChange={setPage} />
    </>}
    {selected && <ConfirmDialog title="Lập hóa đơn?" message={`Lập hóa đơn ${money.format(selected.tongTienDuKien)} cho ${selected.tenKhachHang}, hồ sơ ${selected.maHoSoKham}?`}
      confirmLabel={busy ? 'Đang lập…' : 'Lập hóa đơn'} busy={busy} onConfirm={createInvoice} onCancel={() => setSelected(null)} />}
  </section>
}
