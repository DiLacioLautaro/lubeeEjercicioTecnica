
export function formatNumber(n?: number): string {
  return typeof n === 'number' ? n.toLocaleString('es-AR') : '-'
}

export function formatCurrency(n?: number, currency: string = 'ARS'): string {
  return typeof n === 'number'
    ? n.toLocaleString('es-AR', { style: 'currency', currency })
    : '-'
}

export function formatDate(date?: string | Date): string {
  if (!date) return '-'
  const d = typeof date === 'string' ? new Date(date) : date
  return d.toLocaleDateString('es-AR', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  })
}

export const truncate = (s?: string, max = 80) =>
  (s ?? '').length > max ? (s ?? '').slice(0, max) + '…' : (s ?? '')
