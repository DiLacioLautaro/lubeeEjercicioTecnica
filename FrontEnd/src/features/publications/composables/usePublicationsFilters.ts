// usePublicationsFilters.ts
import { ref, computed, type Ref } from 'vue'
import type { Publication } from '../types/publication'

export function usePublicationsFilters(items: Ref<Publication[]>) {
  // estado de filtros
  const search = ref('')
  const filterTipo = ref<string | null>(null)
  const filterOperacion = ref<string | null>(null)

  // 1) por combos
  const baseFiltered = computed(() => {
    return items.value.filter(p => {
      const okTipo = !filterTipo.value || p.tipoPropiedad === filterTipo.value
      const okOp   = !filterOperacion.value || p.tipoOperacion === filterOperacion.value
      return okTipo && okOp
    })
  })

  // 2) texto
  const uiFiltered = computed(() => {
    const t = search.value.toLowerCase().trim()
    if (!t) return baseFiltered.value
    return baseFiltered.value.filter(p =>
      (p.tipoPropiedad || '').toLowerCase().includes(t) ||
      (p.tipoOperacion || '').toLowerCase().includes(t) ||
      (p.descripcion || '').toLowerCase().includes(t)
    )
  })

  return {
    // estado
    search, filterTipo, filterOperacion,
    // resultados
    uiFiltered,
  }
}
