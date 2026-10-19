import CustomerDashboard from '../features/dashboard/CustomerDashboard'
import ManagerDashboard from '../features/dashboard/ManagerDashboard'
import ManagerAppointments from '../features/appointments/ManagerAppointments'
import MyAppointments from '../features/appointments/MyAppointments'
import BookingPage from '../features/appointments/BookingPage'
import ReceptionAppointments from '../features/appointments/ReceptionAppointments'
import ReceptionIntakePage from '../features/appointments/ReceptionIntakePage'
import ReceptionDashboard from '../features/dashboard/ReceptionDashboard'
import ReceptionInvoices from '../features/invoices/ReceptionInvoices'
import ManagerInvoices from '../features/invoices/ManagerInvoices'
import CustomerInvoices from '../features/invoices/CustomerInvoices'
import ReceptionCustomers from '../features/customers/ReceptionCustomers'
import ReceptionBookingPage from '../features/appointments/ReceptionBookingPage'
import ReceptionAttendance from '../features/attendance/ReceptionAttendance'
import ManagerAttendance from '../features/attendance/ManagerAttendance'
import ManagerStaff from '../features/staff/ManagerStaff'
import ManagerServices from '../features/services/ManagerServices'
import ManagerRooms from '../features/rooms/ManagerRooms'
import DoctorDashboard from '../features/dashboard/DoctorDashboard'
import DoctorAppointments from '../features/appointments/DoctorAppointments'
import DoctorRecords from '../features/records/DoctorRecords'
import CustomerRecords from '../features/records/CustomerRecords'
import PlaceholderPage from '../pages/PlaceholderPage'
import AccountPage from '../features/account/AccountPage'
import ManagerRevenue from '../features/revenue/ManagerRevenue'
import { roles } from './roles'

export default function RolePage({ session, routePath, query, navigationState, onNavigate, onProfileNameChange, onAvatarChange, onPhoneChanged, onLogout }) {
  if (routePath === `${roles[session.role]?.base}/tai-khoan`) {
    return <AccountPage token={session.token} role={session.role} onProfileNameChange={onProfileNameChange} onAvatarChange={onAvatarChange} onPhoneChanged={onPhoneChanged} onLogout={onLogout} />
  }
  if (session.role === 'QUAN_LY' && routePath === '/quan-ly/overview') {
    return <ManagerDashboard token={session.token} onNavigate={onNavigate} />
  }

  if (session.role === 'QUAN_LY' && routePath === '/quan-ly/lich-hen') {
    return <ManagerAppointments token={session.token} query={query} />
  }

  if (session.role === 'QUAN_LY' && routePath === '/quan-ly/hoa-don') {
    return <ManagerInvoices token={session.token} query={query} onNavigate={onNavigate} />
  }

  if (session.role === 'QUAN_LY' && routePath === '/quan-ly/doanh-thu') {
    return <ManagerRevenue token={session.token} query={query} onNavigate={onNavigate} />
  }

  if (session.role === 'QUAN_LY' && routePath === '/quan-ly/khach-hang') {
    return <ReceptionCustomers token={session.token} onNavigate={onNavigate} managerMode />
  }

  if (session.role === 'QUAN_LY' && routePath === '/quan-ly/dat-lich') {
    return <ReceptionBookingPage token={session.token} query={query} onNavigate={onNavigate} staffBasePath="/quan-ly" />
  }

  if (session.role === 'QUAN_LY' && routePath === '/quan-ly/nhan-su') {
    return <ManagerStaff token={session.token} currentPhone={session.phone} currentAvatarUrl={session.avatarUrl} />
  }

  if (session.role === 'QUAN_LY' && routePath === '/quan-ly/dich-vu') {
    return <ManagerServices token={session.token} />
  }

  if (session.role === 'QUAN_LY' && routePath === '/quan-ly/phong') {
    return <ManagerRooms token={session.token} />
  }

  if (session.role === 'QUAN_LY' && routePath === '/quan-ly/cham-cong') {
    return <ManagerAttendance token={session.token} />
  }

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

  if (session.role === 'KHACH_HANG' && routePath === '/khach-hang/hoa-don') {
    return <CustomerInvoices token={session.token} query={query} onNavigate={onNavigate} />
  }

  if (session.role === 'KHACH_HANG' && routePath === '/khach-hang/lich-su-kham') {
    return <CustomerRecords token={session.token} query={query} onNavigate={onNavigate} />
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
