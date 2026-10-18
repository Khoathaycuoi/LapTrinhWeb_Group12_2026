import ReceptionInvoices from './ReceptionInvoices'

export default function ManagerInvoices({ token, query, onNavigate }) {
  return <ReceptionInvoices token={token} query={query} onNavigate={onNavigate} managerMode />
}
