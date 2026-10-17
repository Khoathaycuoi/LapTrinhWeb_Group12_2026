import { useState } from 'react'
import ConfirmDialog from '../components/ConfirmDialog'
import './UtilityPages.css'

export default function UnsupportedRolePage({ onLogout }) {
  const [confirmLogout, setConfirmLogout] = useState(false)

  return <div className="auth-error">
    <h1>Vai trò chưa được hỗ trợ</h1>
    <p>Tài khoản hiện tại không có vai trò thuộc hệ thống.</p>
    <button className="primary-button" onClick={() => setConfirmLogout(true)}>Đăng xuất</button>
    {confirmLogout && <ConfirmDialog title="Xác nhận đăng xuất" message="Bạn có chắc muốn kết thúc phiên đăng nhập hiện tại?" confirmLabel="Đăng xuất" onCancel={() => setConfirmLogout(false)} onConfirm={onLogout} />}
  </div>
}
