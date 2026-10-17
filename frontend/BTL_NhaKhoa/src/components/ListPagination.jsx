import './ListPagination.css'

function visiblePages(page, totalPages) {
  if (totalPages <= 5) return Array.from({ length: totalPages }, (_, index) => index + 1)

  const start = Math.max(2, Math.min(page - 1, totalPages - 3))
  const pageNumbers = [1, start, start + 1, start + 2, totalPages]
  const uniquePages = [...new Set(pageNumbers)].sort((left, right) => left - right)
  const pages = []

  uniquePages.forEach((number, index) => {
    const previous = uniquePages[index - 1]
    if (previous && number - previous === 2) pages.push(previous + 1)
    else if (previous && number - previous > 2) pages.push('…')
    pages.push(number)
  })
  return pages
}

export default function ListPagination({ page, pageSize, totalItems, onPageChange }) {
  const totalPages = Math.max(1, Math.ceil(totalItems / pageSize))
  if (totalPages <= 1) return null

  return <nav className="list-pagination" aria-label="Phân trang danh sách">
    <span>Trang {page} / {totalPages}</span>
    <div className="list-pagination-controls">
      <button type="button" disabled={page <= 1} onClick={() => onPageChange(page - 1)}>Trước</button>
      {visiblePages(page, totalPages).map((item, index) => typeof item === 'number'
        ? <button key={item} type="button" className={item === page ? 'list-pagination-current' : ''}
            aria-label={`Trang ${item}`} aria-current={item === page ? 'page' : undefined}
            onClick={() => { if (item !== page) onPageChange(item) }}>{item}</button>
        : <span key={`ellipsis-${index}`} className="list-pagination-ellipsis" aria-hidden="true">…</span>)}
      <button type="button" disabled={page >= totalPages} onClick={() => onPageChange(page + 1)}>Tiếp</button>
    </div>
  </nav>
}
