import CustomerDashboard from '../features/dashboard/CustomerDashboard'
import MyAppointments from '../features/appointments/MyAppointments'
import PlaceholderPage from '../pages/PlaceholderPage'

export default function RolePage({ session, routePath, query, onNavigate }) {
  if (session.role === 'KHACH_HANG' && routePath === '/khach-hang/overview') {
    return <CustomerDashboard token={session.token} onNavigate={onNavigate} />
  }

  if (session.role === 'KHACH_HANG' && routePath === '/khach-hang/lich-hen') {
    return <MyAppointments token={session.token} groupParam={new URLSearchParams(query).get('nhom')}
      onGroupChange={(group) => onNavigate(group === 'all' ? routePath : `${routePath}?nhom=${group}`)} />
  }

  return <PlaceholderPage role={session.role} path={routePath} />
}
