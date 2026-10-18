import ReceptionAppointments from './ReceptionAppointments'

export default function ManagerAppointments({ token, query }) {
  return <ReceptionAppointments token={token} query={query} defaultTab="all" managerMode
    description="Theo dõi và xử lý lịch hẹn của phòng khám." />
}
