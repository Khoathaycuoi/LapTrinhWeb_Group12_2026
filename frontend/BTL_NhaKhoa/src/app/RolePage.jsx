import CustomerDashboard from '../features/dashboard/CustomerDashboard'
import MyAppointments from '../features/appointments/MyAppointments'
import BookingPage from '../features/appointments/BookingPage'
import ReceptionAppointments from '../features/appointments/ReceptionAppointments'
import PlaceholderPage from '../pages/PlaceholderPage'

export default function RolePage({ session, routePath, query, navigationState, onNavigate }) {
  if (session.role === 'KHACH_HANG' && routePath === '/khach-hang/overview') {
    return <CustomerDashboard token={session.token} onNavigate={onNavigate} />
  }

  if (session.role === 'KHACH_HANG' && routePath === '/khach-hang/lich-hen') {
    return <MyAppointments token={session.token} groupParam={new URLSearchParams(query).get('nhom')}
      statusParam={new URLSearchParams(query).get('trangThai')}
      arrivalNotice={navigationState?.notice}
      onGroupChange={(group) => onNavigate(group === 'all' ? routePath : `${routePath}?nhom=${group}`)} />
  }

  if (session.role === 'KHACH_HANG' && routePath === '/khach-hang/dat-lich') {
    return <BookingPage token={session.token} onNavigate={onNavigate} />
  }

  if (session.role === 'LE_TAN' && routePath === '/le-tan/lich-hen') {
    return <ReceptionAppointments token={session.token} />
  }

  return <PlaceholderPage role={session.role} path={routePath} />
}
