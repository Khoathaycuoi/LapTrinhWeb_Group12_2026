import { useEffect, useState } from 'react'
import ListPagination from '../../components/ListPagination'
import { doctorRecordsApi } from './doctorRecordsApi'
import DoctorRecordDetail from './DoctorRecordDetail'
import './DoctorRecords.css'

const dateFormatter = new Intl.DateTimeFormat('vi-VN', { dateStyle: 'medium', timeStyle: 'short' })

export default function DoctorRecords({ token, query = '', navigationState, onNavigate }) {
  const params = new URLSearchParams(query)
  const recordId = params.get('maHoSoKham')
  const appointmentId = params.get('maLichHen')
  const unfinalized = params.get('chuaChot') === 'true'
  const [page, setPage] = useState(1)
  const [reload, setReload] = useState(0)
  const [list, setList] = useState({ status: 'loading', items: [], totalItems: 0, error: '' })

  useEffect(() => {
    if (recordId || appointmentId) return undefined
    const controller = new AbortController()
    queueMicrotask(() => { if (!controller.signal.aborted) setList({ status: 'loading', items: [], totalItems: 0, error: '' }) })
    doctorRecordsApi.list(token, page, unfinalized, controller.signal).then((result) => {
      if (controller.signal.aborted) return
      if (!Array.isArray(result?.items) || !Number.isInteger(result.totalItems)) {
        setList({ status: 'error', items: [], totalItems: 0, error: 'Chưa tải được hồ sơ khám.' })
        return
      }
      const lastPage = Math.max(1, Math.ceil(result.totalItems / 10))
      if (page > lastPage) { setPage(lastPage); return }
      setList({ status: 'success', items: result.items, totalItems: result.totalItems, error: '' })
    }).catch((error) => {
      if (!controller.signal.aborted) setList({ status: 'error', items: [], totalItems: 0,
        error: error.status === 0 ? error.message : 'Chưa tải được hồ sơ khám.' })
    })
    return () => controller.abort()
  }, [token, page, reload, recordId, appointmentId, unfinalized])

  if (recordId || appointmentId) return <DoctorRecordDetail key={recordId || appointmentId} token={token}
    recordId={recordId} appointmentId={appointmentId} initialNotice={navigationState?.notice}
    onBack={() => { setReload((value) => value + 1); onNavigate(unfinalized ? '/bac-si/ho-so-kham?chuaChot=true' : '/bac-si/ho-so-kham') }} />

  return <div className="doctor-records">
    <div className="page-heading"><div><h1>Hồ sơ khám</h1><p className="page-description">Các lượt khám do bạn phụ trách.</p></div></div>
    <div className="doctor-record-tabs" role="group" aria-label="Lọc hồ sơ khám"><button type="button" className={!unfinalized ? 'active' : ''} aria-pressed={!unfinalized} onClick={() => { setPage(1); onNavigate('/bac-si/ho-so-kham') }}>Tất cả</button><button type="button" className={unfinalized ? 'active' : ''} aria-pressed={unfinalized} onClick={() => { setPage(1); onNavigate('/bac-si/ho-so-kham?chuaChot=true') }}>Chưa chốt</button></div>
    {list.status === 'loading' && <div className="doctor-record-feedback" role="status">Đang tải hồ sơ…</div>}
    {list.status === 'error' && <div className="doctor-record-feedback" role="alert"><p>{list.error}</p><button type="button" className="secondary-button" onClick={() => setReload((value) => value + 1)}>Thử lại</button></div>}
    {list.status === 'success' && <><p className="doctor-record-count">{list.totalItems} hồ sơ</p>
      {list.items.length === 0 && <div className="doctor-record-feedback">Chưa có hồ sơ khám.</div>}
      <div className="doctor-record-list">{list.items.map((item) => <article key={item.maHoSoKham}>
        <div><small>{item.maHoSoKham}</small><h2>{dateFormatter.format(new Date(item.ngayKham))}</h2><p>{item.tomTat || 'Chưa có tóm tắt cho khách'}</p></div>
        <div className="doctor-record-list-actions"><span className={item.chotLuc ? 'doctor-record-state finalized' : 'doctor-record-state'}>{item.chotLuc ? 'Đã chốt' : 'Chưa chốt'}</span><button type="button" className="secondary-button" onClick={() => onNavigate(`/bac-si/ho-so-kham?maHoSoKham=${encodeURIComponent(item.maHoSoKham)}${unfinalized ? '&chuaChot=true' : ''}`)}>Mở hồ sơ</button></div>
      </article>)}</div><ListPagination page={page} pageSize={10} totalItems={list.totalItems} onPageChange={setPage} /></>}
  </div>
}
