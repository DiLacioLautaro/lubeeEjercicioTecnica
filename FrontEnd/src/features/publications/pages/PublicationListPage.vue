<script setup lang="ts">
import { onMounted, ref, computed, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { usePublicationsStore } from '../store/publications.store'
import type { Publication } from '../types/publication'
import { truncate } from '@/utils/formatters'

import PublicationCard from '../components/PublicationCard.vue'
import PublicationListFilters from '../components/PublicationListFilters.vue'

import { usePublicationOptions } from '../composables/usePublicationOptions'
import { usePublicationsFilters } from '../composables/usePublicationsFilters'

const store = usePublicationsStore()
const router = useRouter()

type ViewMode = 'table' | 'grid'
const view = ref<ViewMode>('table')
function onChangeView(v: ViewMode) { view.value = v }

// selección (compartida entre vistas)
const selected = ref<Publication[]>([])
const deletingMany = ref(false)
const selectedCount = computed(() => selected.value.length)

function isSelected(id?: number | null) {
  if (!id) return false
  return selected.value.some(x => x.id === id)
}
function toggleSelect(id: number, on: boolean) {
  if (!id) return
  if (on) {
    if (!isSelected(id)) {
      const found = store.items.find(x => x.id === id)
      selected.value.push(found ?? ({ id } as Publication))
    }
  } else {
    const ix = selected.value.findIndex(x => x.id === id)
    if (ix !== -1) selected.value.splice(ix, 1)
  }
}

// combos
const { propertyTypes, operationTypes } = usePublicationOptions()
const propertyTypesWithAll = computed(() => [{ label: 'Todos', value: null }, ...propertyTypes.value])
const operationTypesWithAll = computed(() => [{ label: 'Todos', value: null }, ...operationTypes.value])

// filtros (centralizados)
const { search, filterTipo, filterOperacion, uiFiltered } =
  usePublicationsFilters(computed(() => store.items))

onMounted(() => store.fetchAll())

function editRow(id: number) { router.push({ name: 'publication-edit', params: { id } }) }
function newRow() { router.push({ name: 'publication-new' }) }
function handleToggleSelect(id: number, on: boolean) { toggleSelect(id, on) }
function previewUrl(p: any): string | null { return p?.imagenes?.[0]?.url || null }

/* -------- Orden + páginas -------- */
type SortKey =
  | 'id'
  | 'tipoPropiedad'
  | 'tipoOperacion'
  | 'descripcion'
  | 'ambientes'
  | 'm2'
  | 'antiguedad'

const sortBy = ref<SortKey>('id')
const sortDir = ref<'asc' | 'desc'>('asc')

function setSort(k: SortKey) {
  if (sortBy.value === k) sortDir.value = sortDir.value === 'asc' ? 'desc' : 'asc'
  else { sortBy.value = k; sortDir.value = 'asc' }
}

const page = ref(1)
const pageSize = ref(8)
const pageSizeOptions = [8, 16, 32]

const ordered = computed(() => {
  const arr = [...uiFiltered.value]
  const k = sortBy.value
  const dir = sortDir.value
  arr.sort((a: any, b: any) => {
    const va = (a?.[k] ?? '') as string | number
    const vb = (b?.[k] ?? '') as string | number
    const A = typeof va === 'string' ? va.toLowerCase() : va
    const B = typeof vb === 'string' ? vb.toLowerCase() : vb
    const cmp = A > B ? 1 : A < B ? -1 : 0
    return dir === 'asc' ? cmp : -cmp
  })
  return arr
})

const totalPages = computed(() => Math.max(1, Math.ceil(ordered.value.length / pageSize.value)))
const pageItems = computed(() => {
  const p = Math.min(page.value, totalPages.value)
  const start = (p - 1) * pageSize.value
  const end = start + pageSize.value
  return ordered.value.slice(start, end)
})

function goPrev() { if (page.value > 1) page.value-- }
function goNext() { if (page.value < totalPages.value) page.value++ }
function goFirst() { page.value = 1 }
function goLast() { page.value = totalPages.value }
function changePageSize(val: number) { pageSize.value = val; page.value = 1 }

// seleccionar todo en página
function toggleSelectAllOnPage(on: boolean) {
  for (const p of pageItems.value) if (p.id) toggleSelect(p.id, on)
}
const allOnPageSelected = computed(() =>
  pageItems.value.length > 0 && pageItems.value.every(p => isSelected(p.id))
)

/* -------- Toasts / Confirm -------- */
type ToastVariant = 'success' | 'danger' | 'warning' | 'info'
type ToastItem = { id: number, variant: ToastVariant, title?: string, detail?: string }
const toasts = ref<ToastItem[]>([])
let toastId = 0
function addToast(variant: ToastVariant, title?: string, detail?: string, life = 3000) {
  const id = ++toastId
  toasts.value.push({ id, variant, title, detail })
  window.setTimeout(() => {
    const ix = toasts.value.findIndex(t => t.id === id)
    if (ix !== -1) toasts.value.splice(ix, 1)
  }, life)
}

const confirmState = reactive<{ show: boolean; title: string; message: string; accepting: boolean; onAccept?: () => Promise<void> | void; }>(
  { show: false, title: '', message: '', accepting: false, onAccept: undefined }
)
function openConfirm(opts: { title: string, message: string, onAccept: () => Promise<void> | void }) {
  Object.assign(confirmState, { ...opts, accepting: false, show: true })
}
async function acceptConfirm() {
  if (!confirmState.onAccept) return
  confirmState.accepting = true
  try { await confirmState.onAccept(); confirmState.show = false } finally { confirmState.accepting = false }
}

async function removeRow(id: number) {
  openConfirm({
    title: 'Eliminar publicación',
    message: '¿Desea eliminar esta publicación? Esta acción no se puede deshacer.',
    onAccept: async () => {
      try {
        await store.remove(id)
        const ix = selected.value.findIndex(x => x.id === id)
        if (ix !== -1) selected.value.splice(ix, 1)
        addToast('success', 'Eliminado', `Se eliminó la publicación (ID ${id}).`, 2200)
      } catch (err: any) {
        const status = err?.response?.status ?? err?.status
        let detail = 'Ocurrió un error al eliminar.'
        if (status === 404) detail = 'La publicación no existe o ya fue eliminada.'
        else if (status === 409) detail = 'No se puede eliminar porque tiene dependencias.'
        else if (status === 500) detail = 'Error interno del servidor.'
        addToast('danger', 'No se pudo eliminar', detail, 3200)
      }
    }
  })
}

async function removeSelected() {
  if (!selected.value.length) return
  const ids = [...new Set(selected.value.map(p => p.id!).filter(Boolean))]
  const count = ids.length

  openConfirm({
    title: 'Eliminar publicaciones seleccionadas',
    message: `¿Seguro querés eliminar ${count} publicación${count > 1 ? 'es' : ''}? Esta acción no se puede deshacer.`,
    onAccept: async () => {
      deletingMany.value = true
      try {
        const res: any = await store.removeMany(ids)
        selected.value = selected.value.filter(p => !ids.includes(p.id!))
        const detail = (res && typeof res.deletedCount === 'number')
          ? `Eliminadas: ${res.deletedCount} • No encontradas: ${res.notFoundCount}`
          : `Eliminadas: ${count}`
        addToast(res?.notFoundCount ? 'warning' : 'success', 'Eliminación en lote', detail, 3000)
      } catch (e: any) {
        addToast('danger', 'No se pudo eliminar', e?.response?.data?.detail || e?.message || 'Error al eliminar publicaciones.', 3200)
      } finally {
        deletingMany.value = false
      }
    }
  })
}
</script>

<template>
  <section class="container-fluid px-3 page">
    <!-- Toasts -->
    <div class="position-fixed top-0 end-0 p-3" style="z-index: 1060">
      <div style="min-width: 280px;">
        <div
          v-for="t in toasts"
          :key="t.id"
          class="alert alert-dismissible fade show mb-2"
          :class="{
            'alert-success': t.variant === 'success',
            'alert-danger': t.variant === 'danger',
            'alert-warning': t.variant === 'warning',
            'alert-info': t.variant === 'info'
          }"
          role="alert"
        >
          <strong v-if="t.title">{{ t.title }}</strong>
          <span v-if="t.detail"> {{ t.detail }}</span>
          <button type="button" class="btn-close" @click="toasts = toasts.filter(x => x.id !== t.id)"></button>
        </div>
      </div>
    </div>

    <!-- Confirm -->
    <div v-if="confirmState.show">
      <div class="modal fade show d-block" tabindex="-1" role="dialog" aria-modal="true">
        <div class="modal-dialog">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title">{{ confirmState.title }}</h5>
              <button type="button" class="btn-close" aria-label="Close" @click="confirmState.show = false"></button>
            </div>
            <div class="modal-body">
              <p class="mb-0">{{ confirmState.message }}</p>
            </div>
            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" :disabled="confirmState.accepting" @click="confirmState.show = false">
                Cancelar
              </button>
              <button type="button" class="btn btn-danger" :disabled="confirmState.accepting" @click="acceptConfirm()">
                <span v-if="confirmState.accepting" class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>
                Eliminar
              </button>
            </div>
          </div>
        </div>
      </div>
      <div class="modal-backdrop fade show"></div>
    </div>

    <!-- Card -->
    <div class="card shadow-sm rounded-2xl w-100">
      <div class="card-body">
        <div class="card-title">
          <div class="title-wrap">
            <div class="icon-pill" aria-hidden="true">🏷️</div>
            <div>
              <h2 class="title mb-0">Publicaciones</h2>
              <p class="subtitle">
                <span v-if="store.loading">Cargando…</span>
                <span v-else>{{ store.items.length }} en total • {{ uiFiltered.length }} en vista</span>
              </p>
            </div>
          </div>

          <PublicationListFilters
            :view="view"
            @change-view="onChangeView"
            v-model:search="search"
            v-model:filterTipo="filterTipo"
            v-model:filterOperacion="filterOperacion"
            :propertyTypes="propertyTypesWithAll"
            :operationTypes="operationTypesWithAll"
            :bulkDisabled="selectedCount === 0"
            :bulkLoading="deletingMany"
            :bulkCount="selectedCount"
            @new="newRow"
            @bulkDelete="removeSelected"
          />
        </div>

        <!-- ===== TABLA ===== -->
        <div v-if="view === 'table'">
          <div class="table-wrapper">
            <!-- El scroll vive acá adentro -->
            <div class="table-scroll">
              <!-- Header -->
              <div
                class="row align-items-center py-2 border-bottom small text-secondary fw-semibold table-header text-center"
              >
                <!-- Selección -->
                <div class="col-12 col-md-1 col-lg-1 d-flex justify-content-center align-items-center">
                  <input
                    class="form-check-input select-all"
                    type="checkbox"
                    @click.stop
                    :checked="allOnPageSelected"
                    @change="toggleSelectAllOnPage(($event.target as HTMLInputElement).checked)"
                  />
                </div>

                <!-- Imagen (encabezado vacío) -->
                <div class="col-12 col-md-1 col-lg-1 text-center">
                  <span class="visually-hidden">Imagen</span>
                </div>

                <!-- Tipo Propiedad -->
                <div
                  class="col-6 col-md-2 col-lg-1 d-flex justify-content-center align-items-center gap-1 th-click"
                  @click="setSort('tipoPropiedad')"
                >
                  <span>Tipo Propiedad</span>
                  <i
                    class="bi ms-1"
                    :class="sortBy === 'tipoPropiedad' ? (sortDir === 'asc' ? 'bi-caret-up-fill' : 'bi-caret-down-fill') : 'bi-arrow-down-up'"
                  ></i>
                </div>

                <!-- Tipo Operación -->
                <div
                  class="col-6 col-md-2 col-lg-1 d-flex justify-content-center align-items-center gap-1 th-click"
                  @click="setSort('tipoOperacion')"
                >
                  <span>Tipo Operación</span>
                  <i
                    class="bi ms-1"
                    :class="sortBy === 'tipoOperacion' ? (sortDir === 'asc' ? 'bi-caret-up-fill' : 'bi-caret-down-fill') : 'bi-arrow-down-up'"
                  ></i>
                </div>

                <!-- Ambientes -->
                <div
                  class="col-6 col-md-1 col-lg-1 d-flex justify-content-center align-items-center gap-1 th-click"
                  @click="setSort('ambientes')"
                >
                  <span>Amb.</span>
                  <i
                    class="bi ms-1"
                    :class="sortBy === 'ambientes' ? (sortDir === 'asc' ? 'bi-caret-up-fill' : 'bi-caret-down-fill') : 'bi-arrow-down-up'"
                  ></i>
                </div>

                <!-- M² -->
                <div
                  class="col-6 col-md-1 col-lg-1 d-flex justify-content-center align-items-center gap-1 th-click"
                  @click="setSort('m2')"
                >
                  <span>M²</span>
                  <i
                    class="bi ms-1"
                    :class="sortBy === 'm2' ? (sortDir === 'asc' ? 'bi-caret-up-fill' : 'bi-caret-down-fill') : 'bi-arrow-down-up'"
                  ></i>
                </div>

                <!-- Antigüedad -->
                <div
                  class="col-6 col-md-1 col-lg-1 d-flex justify-content-center align-items-center gap-1 th-click"
                  @click="setSort('antiguedad')"
                >
                  <span>Ant.</span>
                  <i
                    class="bi ms-1"
                    :class="sortBy === 'antiguedad' ? (sortDir === 'asc' ? 'bi-caret-up-fill' : 'bi-caret-down-fill') : 'bi-arrow-down-up'"
                  ></i>
                </div>

                <!-- Descripción -->
                <div
                  class="col-12 col-md-2 col-lg-2 d-flex justify-content-center align-items-center gap-1 th-click"
                  @click="setSort('descripcion')"
                >
                  <span>Descripción</span>
                  <i
                    class="bi ms-1"
                    :class="sortBy === 'descripcion' ? (sortDir === 'asc' ? 'bi-caret-up-fill' : 'bi-caret-down-fill') : 'bi-arrow-down-up'"
                  ></i>
                </div>

                <!-- Lat / Lon (lg+) -->
                <div class="col-12 col-lg-2 d-none d-lg-flex justify-content-center">
                  Lat / Lon
                </div>

                <!-- Acciones (vacío) -->
                <div class="col-12 col-md-1 col-lg-1 text-center">
                  <span class="visually-hidden">Acciones</span>
                </div>
              </div>

              <!-- Filas -->
              <div v-if="store.loading" class="empty">Cargando…</div>
              <div v-else-if="!pageItems.length" class="empty">
                <p class="mb-0">No hay publicaciones.</p>
              </div>

              <div
                v-else
                v-for="p in pageItems"
                :key="p.id"
                class="row align-items-center py-2 border-bottom table-row text-center"
              >
                <!-- Selección -->
                <div class="col-12 col-md-1 col-lg-1 d-flex justify-content-center">
                  <input
                    class="form-check-input"
                    type="checkbox"
                    :checked="isSelected(p.id)"
                    @change="toggleSelect(p.id!, ($event.target as HTMLInputElement).checked)"
                  />
                </div>

                <!-- Imagen -->
                <div class="col-12 col-md-1 col-lg-1 d-flex justify-content-center">
                  <div class="thumb thumb-md" :aria-label="previewUrl(p) ? 'Imagen' : 'Sin imagen'">
                    <img v-if="previewUrl(p)" :src="previewUrl(p)!" alt="img" />
                    <div v-else class="text-muted small d-flex align-items-center justify-content-center w-100 h-100">
                      <i class="bi bi-image"></i>
                    </div>
                  </div>
                </div>

                <!-- Tipo Propiedad -->
                <div class="col-6 col-md-2 col-lg-1">
                  <div class="cell-value">{{ p.tipoPropiedad }}</div>
                </div>

                <!-- Tipo Operación -->
                <div class="col-6 col-md-2 col-lg-1">
                  <div class="cell-value">{{ p.tipoOperacion }}</div>
                </div>

                <!-- Ambientes -->
                <div class="col-6 col-md-1 col-lg-1">
                  <div class="cell-value">{{ p.ambientes ?? '—' }}</div>
                </div>

                <!-- M² -->
                <div class="col-6 col-md-1 col-lg-1">
                  <div class="cell-value">{{ p.m2 ?? '—' }}</div>
                </div>

                <!-- Ant. -->
                <div class="col-6 col-md-1 col-lg-1">
                  <div class="cell-value">{{ p.antiguedad ?? '—' }}</div>
                </div>

                <!-- Descripción -->
                <div class="col-12 col-md-2 col-lg-2">
                  <div class="cell-value text-truncate" :title="p.descripcion">
                    {{ truncate(p.descripcion, 100) }}
                  </div>
                </div>

                <!-- Lat/Lon (lg+) -->
                <div class="col-12 col-lg-2 d-none d-lg-block">
                  <span class="coords"><b>Lat:</b> {{ p.lat ?? '-' }} · <b>Lon:</b> {{ p.lng ?? '-' }}</span>
                </div>

                <!-- Acciones -->
                <div class="col-12 col-md-1 col-lg-1 d-flex justify-content-center justify-content-md-end gap-1 mt-2 mt-md-0">
                  <button class="btn btn-sm btn-outline-secondary" @click="editRow(p.id!)" aria-label="Editar">
                    Editar
                  </button>
                  <button class="btn btn-sm btn-outline-danger" @click="removeRow(p.id!)" aria-label="Eliminar">
                    Eliminar
                  </button>
                </div>
              </div>
            </div>
            <!-- /table-scroll -->
          </div>
          <!-- /table-wrapper -->

          <!-- Paginación (fuera del scroll) -->
          <div class="d-flex align-items-center gap-2 mt-2">
            <div class="ms-auto d-flex align-items-center gap-2">
              <label class="form-label mb-0 small text-muted">Filas:</label>
              <select
                class="form-select form-select-sm w-auto"
                :value="pageSize"
                @change="changePageSize(parseInt(($event.target as HTMLSelectElement).value))"
              >
                <option v-for="opt in pageSizeOptions" :key="opt" :value="opt">{{ opt }}</option>
              </select>
            </div>
            <nav v-if="totalPages > 1" aria-label="Paginación" class="ms-auto">
              <ul class="pagination pagination-sm mb-0">
                <li class="page-item" :class="{ disabled: page === 1 }">
                  <button class="page-link" @click="goFirst">«</button>
                </li>
                <li class="page-item" :class="{ disabled: page === 1 }">
                  <button class="page-link" @click="goPrev">Anterior</button>
                </li>
                <li class="page-item disabled">
                  <span class="page-link">{{ page }} / {{ totalPages }}</span>
                </li>
                <li class="page-item" :class="{ disabled: page === totalPages }">
                  <button class="page-link" @click="goNext">Siguiente</button>
                </li>
                <li class="page-item" :class="{ disabled: page === totalPages }">
                  <button class="page-link" @click="goLast">»</button>
                </li>
              </ul>
            </nav>
          </div>
        </div>

        <!-- ===== GRILLA ===== -->
        <div v-else>
          <div v-if="store.loading" class="empty">Cargando…</div>
          <div v-else-if="!uiFiltered.length" class="empty">
            <p class="mb-0">No hay publicaciones que coincidan.</p>
          </div>
          <div v-else class="row g-3">
            <div class="col-12 col-sm-6 col-lg-4 col-xxl-3" v-for="p in uiFiltered" :key="p.id">
              <PublicationCard
                :publication="p"
                :onEdit="editRow"
                :onDelete="removeRow"
                :selectable="true"
                :selected="isSelected(p.id)"
                :onToggleSelect="(id, on) => handleToggleSelect(id, on)"
              />
            </div>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<style scoped>
/* ========== Página ========== */
.page {
  padding-top: 1rem;
}

/* ========== Card Header (título) ========== */
.card-title {
  display: grid;
  grid-template-columns: 1fr auto;
  align-items: center;
  gap: .75rem;
  margin-bottom: .5rem;
}

.title-wrap {
  display: flex;
  align-items: center;
  gap: .75rem;
}

.icon-pill {
  width: 42px;
  height: 42px;
  border-radius: 12px;
  display: grid;
  place-items: center;
  background: #eef2ff;
  color: #4338ca;
  border: 1px solid #c7d2fe;
  font-size: 1rem;
}

.title {
  margin: 0;
  font-size: clamp(1.15rem, 1.05rem + .4vw, 1.5rem);
  font-weight: 800;
  color: #0f172a;
  letter-spacing: -.01em;
}

.subtitle {
  margin: .15rem 0 0;
  color: #64748b;
  font-size: .95rem;
}

@media (max-width: 768px) {
  .card-title {
    grid-template-columns: 1fr;
    align-items: start;
  }
}

/* ========== Tabla ========== */
/* Wrapper fijo; el scroll vive en .table-scroll */
.table-wrapper {
  position: relative;
  max-width: 100%;
  overflow: hidden;
}

/* Scroll horizontal solo si hace falta */
.table-scroll {
  overflow-x: auto;
  overflow-y: visible;
  -webkit-overflow-scrolling: touch;
  scrollbar-gutter: stable both-edges; /* evita que la barra tape contenido */
  padding-bottom: .25rem;               /* colchón por si aparece la barra */
  width: 100%;
}

/* Fuerza ancho mínimo coherente para columnas; ajustá si agregás/quitas columnas */
.table-scroll .table-header,
.table-scroll .table-row {
  min-width: 1100px;
}

/* Cabeceras cliqueables (orden) */
.table-header .th-click {
  cursor: pointer;
  user-select: none;
}

.table-header .th-click:hover {
  color: #0d6efd;
}

/* Checkbox sin offsets raros */
.table-row .form-check-input,
.table-header .select-all.form-check-input {
  position: static;
  margin: 0;
  transform: none;
  vertical-align: middle;
}

/* Miniaturas */
.thumb {
  width: 64px;
  height: 48px;
  border-radius: 10px;
  overflow: hidden;
  border: 1px solid #e5e7eb;
  background: #f3f4f6;
  display: grid;
  place-items: center;
}

.thumb img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

.thumb.thumb-md {
  width: 72px;
  height: 54px;
}

/* Celdas de valor */
.cell-value {
  font-weight: 600;
}

/* Acciones: al final a la derecha en md+ */
@media (min-width: 768px) {
  .table-row .col-12.col-md-1:last-child {
    justify-content: end !important;
  }
}

/* ========== Estados / utilidades ========== */
.empty {
  text-align: center;
  padding: 2rem 1rem;
  color: var(--text-color-secondary, #6b7280);
  display: grid;
  gap: .75rem;
  place-items: center;
}

.coords {
  font-variant-numeric: tabular-nums;
  color: #374151;
}

.rounded-2xl {
  border-radius: 12px;
}

.shadow-sm {
  box-shadow: 0 2px 12px rgba(0, 0, 0, .06);
}

.modal-backdrop.show {
  opacity: .5;
}

/* A11y helpers */
.visually-hidden {
  position: absolute !important;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border: 0;
}
</style>
