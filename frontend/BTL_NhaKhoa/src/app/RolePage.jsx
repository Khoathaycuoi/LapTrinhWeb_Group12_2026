import CustomerDashboard from '../features/dashboard/CustomerDashboard'
import MyAppointments from '../features/appointments/MyAppointments'
import BookingPage from '../features/appointments/BookingPage'
import ReceptionAppointments from '../features/appointments/ReceptionAppointments'
import ReceptionIntakePage from '../features/appointments/ReceptionIntakePage'
import ReceptionDashboard from '../features/dashboard/ReceptionDashboard'
import ReceptionInvoices from '../features/invoices/ReceptionInvoices'
import ReceptionCustomers from '../features/customers/ReceptionCustomers'
import ReceptionBookingPage from '../features/appointments/ReceptionBookingPage'
import ReceptionAttendance from '../features/attendance/ReceptionAttendance'
import DoctorDashboard from '../features/dashboard/DoctorDashboard'
import DoctorAppointments from '../features/appointments/DoctorAppointments'
import DoctorRecords from '../features/records/DoctorRecords'
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
    return <ReceptionAppointments token={session.token} query={query} arrivalNotice={navigationState?.notice} />
  }

  if (session.role === 'LE_TAN' && routePath === '/le-tan/khach-hang') {
    return <ReceptionCustomers token={session.token} onNavigate={onNavigate} />
  }

  if (session.role === 'LE_TAN' && routePath === '/le-tan/dat-lich') {
    return <ReceptionBookingPage token={session.token} query={query} onNavigate={onNavigate} />
  }

  if (session.role === 'LE_TAN' && routePath === '/le-tan/cham-cong') {
    return <ReceptionAttendance token={session.token} />
  }

  if (session.role === 'LE_TAN' && routePath === '/le-tan/tiep-nhan') {
    return <ReceptionIntakePage token={session.token} query={query} />
  }

  if (session.role === 'LE_TAN' && routePath === '/le-tan/overview') {
    return <ReceptionDashboard token={session.token} onNavigate={onNavigate} />
  }

  if (session.role === 'LE_TAN' && routePath === '/le-tan/hoa-don') {
    return <ReceptionInvoices token={session.token} query={query} onNavigate={onNavigate} />
  }

  if (session.role === 'BAC_SI' && routePath === '/bac-si/overview') {
    return <DoctorDashboard token={session.token} onNavigate={onNavigate} />
  }

  if (session.role === 'BAC_SI' && routePath === '/bac-si/lich-hen') {
    return <DoctorAppointments token={session.token} query={query} navigationState={navigationState} onNavigate={onNavigate} />
  }

  if (session.role === 'BAC_SI' && routePath === '/bac-si/ho-so-kham') {
    return <DoctorRecords token={session.token} query={query} navigationState={navigationState} onNavigate={onNavigate} />
  }

  if (session.role === 'BAC_SI' && routePath === '/bac-si/cham-cong') {
    return <ReceptionAttendance token={session.token} />
  }

  return <PlaceholderPage role={session.role} path={routePath} />
}
