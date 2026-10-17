import { roleNavigation } from '../app/roles'
import './PlaceholderPage.css'

export default function PlaceholderPage({ role, path }) {
  const key = path.split('/').filter(Boolean).at(-1)
  const navItem = roleNavigation[role].find(([, route]) => route === key)
  const known = Boolean(navItem)
  const label = navItem?.[0] ?? key.replaceAll('-', ' ')

  return <>
    <div className="page-heading"><div><h1>{key === 'overview' ? 'Tổng quan' : label}</h1><p className="page-description">Chào mừng bạn đến với Nha Khoa G12.</p></div></div>
    {key === 'overview' ? <section className="welcome-card">
      <div className="welcome-icon">✧</div><div><h2>Chào mừng đến với Nha Khoa G12</h2><p>Các thông tin tổng quan sẽ được cập nhật tại đây.</p></div>
    </section> : <section className="placeholder-card"><div className="placeholder-mark">◌</div><h2>{known ? 'Tính năng đang được hoàn thiện' : 'Không tìm thấy trang'}</h2><p>{known ? 'Mục này chưa thể sử dụng. Vui lòng quay lại sau.' : 'Đường dẫn bạn mở không thuộc khu vực này.'}</p></section>}
  </>
}
