import './AuthPages.css'

export default function AuthLayout({ children, variant }) {
  return <main className="auth-page">
    <section className="auth-art" aria-label="Phòng khám nha khoa">
      <div className="brand-mark"><img src="/tooth-favicon.png" alt="" /></div>
      <p className="eyebrow">NHA KHOA G12</p>
      <h1>Chăm sóc nụ cười,<br />bắt đầu từ sự an tâm.</h1>
      <p className="art-copy">Một không gian chăm sóc sức khỏe răng miệng tận tâm cho cả gia đình.</p>
      <div className="art-decoration" aria-hidden="true"><span>✧</span><span>＋</span><span>◌</span></div>
      <div className="art-footer">Hệ thống quản lý phòng khám</div>
    </section>
    <section className={`auth-panel${variant === 'register' ? ' auth-panel--register' : ''}`}>
      <div className="auth-form-wrap">
        <div className="mobile-brand"><span><img src="/tooth-favicon.png" alt="" /></span> Nha Khoa G12</div>
        {children}
      </div>
    </section>
  </main>
}
