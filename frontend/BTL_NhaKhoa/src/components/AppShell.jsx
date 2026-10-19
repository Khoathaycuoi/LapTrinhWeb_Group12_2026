import { roleNavigation, roles } from '../app/roles'

import './AppShell.css'

const icons = ['◫', '▦', '♙', '✚', '▤', '◈', '◷', '▧']

export default function AppShell({ role, name, path, onNavigate, children }) {
  const info = roles[role]
  const items = roleNavigation[role] ?? []
  const activeItem = items.find(([, key]) => path.endsWith(`/${key}`)) ?? items[0]
  const displayName = name?.trim() || 'Tài khoản của tôi'
  const lastName = name?.trim().split(/\s+/).at(-1)
  const avatarLetter = lastName ? Array.from(lastName)[0].toLocaleUpperCase('vi-VN') : '?'
  return <div className="app-shell">
    <aside className="sidebar">
      <a className="brand" href={`${info.base}/overview`} onClick={(event) => { event.preventDefault(); onNavigate(`${info.base}/overview`) }}>
        <span className="brand-icon"><img src="/tooth-favicon.png" alt="" /></span><span className="brand-name">Nha Khoa G12</span>
      </a>
      <div className="nav-caption">KHÔNG GIAN {info.label.toLocaleUpperCase('vi-VN')}</div>
      <nav className="side-nav" aria-label="Điều hướng chính">
        {items.map(([label, key], index) => {
          const href = `${info.base}/${key}`
          const selected = activeItem?.[1] === key
          return <a key={key} href={href} className={selected ? 'nav-link active' : 'nav-link'} aria-current={selected ? 'page' : undefined}
            onClick={(event) => { event.preventDefault(); onNavigate(href) }}><span className="nav-icon">{key === 'tai-khoan' ? '◉' : icons[index % icons.length]}</span>{label}</a>
        })}
      </nav>
      <div className="sidebar-bottom"><span className="online-dot" />Hệ thống phòng khám</div>
    </aside>
    <div className="main-column">
      <header className="topbar">
        <a className="topbar-user" href={`${info.base}/tai-khoan`} onClick={(event) => { event.preventDefault(); onNavigate(`${info.base}/tai-khoan`) }} aria-label="Mở tài khoản của tôi">
          <span className="avatar">{avatarLetter}</span><span className="user-role">{displayName}</span>
        </a>
      </header>
      <div className="page-content">{children}</div>
    </div>
  </div>
}
