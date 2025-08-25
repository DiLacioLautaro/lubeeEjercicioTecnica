<script setup lang="ts">
import { ref, watch } from 'vue'
import type { Publication } from '../types/publication'

const props = defineProps<{
    publication: Publication
    onEdit?: (id: number) => void
    onDelete?: (id: number) => void
    selectable?: boolean
    selected?: boolean
    onToggleSelect?: (id: number, checked: boolean) => void
}>()

const checked = ref(!!props.selected)
watch(() => props.selected, v => { checked.value = !!v })

function toggleSelection(val: boolean) {
    checked.value = val
    if (props.publication.id && props.onToggleSelect) {
        props.onToggleSelect(props.publication.id, val)
    }
}

function cover() {
    return props.publication.imagenes?.[0]?.url || ''
}

function fmtCoord(n: unknown) {
    return typeof n === 'number' ? n.toFixed(6) : (n ?? '—') as string
}

function opBadgeClass(op?: string) {
    const txt = (op || '').toLowerCase()
    if (txt.includes('nueva')) return 'text-bg-warning'
    if (txt.includes('venta')) return 'text-bg-primary'
    if (txt.includes('alquiler tempor')) return 'text-bg-info'
    if (txt.includes('alquiler')) return 'text-bg-success'
    return 'text-bg-secondary'
}
</script>

<template>
    <article class="publication-card card shadow-soft  overflow-hidden position-relative">
        <!-- checkbox flotante -->
        <div v-if="selectable" class="select-box position-absolute">
                <input class="form-check-input" type="checkbox" :checked="checked" :id="`sel-${publication.id ?? ''}`"
                    :aria-label="`Seleccionar publicación ${publication.id ?? ''}`"
                    @change="toggleSelection(($event.target as HTMLInputElement).checked)" />
        </div>

        <!-- cover -->
        <div class="cover">
            <img v-if="cover()" :src="cover()" alt="imagen principal" />
            <div v-else class="cover--placeholder text-muted">
                <i class="bi bi-image"></i>
            </div>

            <div class="op-chip">
                <span class="badge fw-semibold" :class="opBadgeClass(publication.tipoOperacion)">
                    {{ publication.tipoOperacion || '—' }}
                </span>
            </div>
            <div class="cover-overlay"></div>
        </div>

        <!-- contenido -->
        <div class="content p-3 p-md-4">
            <header class="content__header d-flex align-items-baseline gap-2">
                <h3 class="content__title m-0 d-flex align-items-center gap-2">
                    <span class="badge text-bg-light text-uppercase fw-semibold text-secondary-emphasis">
                        {{ publication.tipoPropiedad || '—' }}
                    </span>
                </h3>
            </header>

            <p v-if="publication.descripcion" class="desc mb-2 mb-md-3">
                {{ publication.descripcion }}
            </p>
            <p v-else class="desc desc--reserve mb-2 mb-md-3" aria-hidden="true"></p>

            <ul class="facts">
                <li><i class="bi bi-house-door me-1"></i>{{ publication.ambientes ?? '—' }} amb</li>
                <li><i class="bi bi-bounding-box me-1"></i>{{ publication.m2 ?? '—' }} m²</li>
                <li><i class="bi bi-clock me-1"></i>{{ publication.antiguedad ?? '—' }} años</li>
            </ul>

            <div v-if="publication.lat != null && publication.lng != null"
                class="coords d-flex align-items-center gap-2">
                <i class="bi bi-geo-alt text-secondary"></i>
                <span class="nums">
                    <b>Lat:</b> {{ fmtCoord(publication.lat) }} ·
                    <b>Lon:</b> {{ fmtCoord(publication.lng) }}
                </span>
            </div>
            <div v-else class="coords coords--reserve" aria-hidden="true"></div>

            <!-- galería (reserva alto cuando está vacía) -->
            <div class="gallery mt-3" :class="{ 'is-empty': !(publication.imagenes?.length > 1) }">
                <template v-if="publication.imagenes?.length > 1">
                    <div v-for="(img, ix) in publication.imagenes.slice(1, 5)" :key="ix" class="thumb" :title="img.url">
                        <img :src="img.url" alt="img" />
                    </div>
                    <div v-if="publication.imagenes.length > 5" class="thumb more"
                        :title="`+${publication.imagenes.length - 5} imágenes`">
                        +{{ publication.imagenes.length - 5 }}
                    </div>
                </template>
            </div>
        </div>

        <!-- acciones -->
        <footer class="actions d-flex flex-wrap justify-content-end gap-2 gap-md-3 px-3 px-md-4 pb-3 pb-md-4 pt-2">
            <button type="button" class="btn btn-sm btn-pill btn-soft-secondary"
                @click="publication.id && onEdit?.(publication.id)">
                <i class="bi bi-pencil me-1"></i> Editar
            </button>

            <button type="button" class="btn btn-sm btn-pill btn-soft-danger"
                @click="publication.id && onDelete?.(publication.id)">
                <i class="bi bi-trash me-1"></i> Eliminar
            </button>
        </footer>
    </article>
</template>

<style scoped>
/* =========================
   Variables locales (scopeadas)
   ========================= */
.publication-card {
    --card-radius: 16px;
    --thumb-h: 58px;
    --pill-radius: 999px;
    --card-border: #e5e7eb;
    --card-border-hover: #cfd8e3;
}

/* ===== Card base (misma altura) ===== */
.publication-card {
    width: 100%;
    border-radius: var(--card-radius);
    display: flex;
    flex-direction: column;
    height: 100%;
    transition: transform .18s ease, box-shadow .18s ease, border-color .18s ease;
}

.publication-card.card {
    border: 1.5px solid var(--card-border) !important;
    border-radius: var(--card-radius);
}

.publication-card:hover {
    transform: translateY(-2px);
    box-shadow: 0 12px 26px rgba(0, 0, 0, .06);
    border-color: var(--card-border-hover);
}

.shadow-soft {
    box-shadow: 0 6px 18px rgba(0, 0, 0, .06);
}

/* checkbox flotante */
.select-box {
    top: .75rem;
    right: .75rem;
    border-radius: .6rem;
    z-index: 3;
    cursor: pointer;
}

/* ===== Cover ===== */
.cover {
    position: relative;
    width: 100%;
    aspect-ratio: 16/7;
    background: #f3f4f6;
    border-bottom: 1px solid var(--card-border);
    border-top-left-radius: var(--card-radius);
    border-top-right-radius: var(--card-radius);
    overflow: hidden;
}

.cover img {
    position: absolute;
    inset: 0;
    width: 100%;
    height: 100%;
    object-fit: cover;
    display: block;
}

.cover--placeholder {
    width: 100%;
    height: 100%;
    display: grid;
    place-items: center;
    font-size: 2rem;
}

.cover-overlay {
    position: absolute;
    inset: 0;
    background: linear-gradient(180deg, rgba(0, 0, 0, 0) 40%, rgba(0, 0, 0, .18) 100%);
    pointer-events: none;
}

/* badge operación */
.op-chip {
    position: absolute;
    left: .85rem;
    bottom: .85rem;
    z-index: 2;
    display: inline-flex;
    align-items: center;
}

.op-chip .badge {
    backdrop-filter: saturate(120%);
    box-shadow: 0 2px 6px rgba(0, 0, 0, .12);
    border: 1px solid rgba(255, 255, 255, .45);
}

/* ===== Contenido ===== */
.content {
    display: flex;
    flex-direction: column;
    gap: .5rem;
    flex: 1;
}

.content__title {
    font-size: 1.05rem;
    font-weight: 800;
    letter-spacing: -.01em;
}

.sep {
    color: #9ca3af;
}

.op {
    color: #111827;
}

/* descripción: 2 líneas fijas */
.desc {
    color: #4b5563;
    line-height: 1.45;
    display: -webkit-box;
    -webkit-box-orient: vertical;
    overflow: hidden;
    min-height: calc(1em * 1.45 * 2);
}

.desc--reserve {
    visibility: hidden;
}

/* facts */
.facts {
    list-style: none;
    padding: 0;
    margin: 0;
    display: flex;
    flex-wrap: wrap;
    gap: .75rem .95rem;
    color: #374151;
    font-size: .95rem;
}

.facts i {
    color: #6b7280;
}

/* coords (reserva alto) */
.coords {
    margin-top: .35rem;
    color: #6b7280;
    min-height: 1.25rem;
    display: flex;
    align-items: center;
}

.coords .nums {
    font-variant-numeric: tabular-nums;
    color: #374151;
}

.coords b {
    color: #111827;
}

.coords--reserve {
    visibility: hidden;
}

/* ===== Galería (reserva alto cuando está vacía) ===== */
.gallery {
    display: grid;
    grid-template-columns: repeat(4, minmax(0, 1fr));
    gap: .4rem;
}

.gallery.is-empty {
    height: calc(var(--thumb-h) + .4rem);
    visibility: hidden;
}

.thumb {
    border: 1px solid var(--card-border);
    border-radius: .6rem;
    overflow: hidden;
    height: var(--thumb-h);
    background: #fafafa;
}

.thumb img {
    width: 100%;
    height: 100%;
    object-fit: cover;
    display: block;
}

.thumb.more {
    display: grid;
    place-items: center;
    background: #111827;
    color: #fff;
    font-weight: 700;
}

/* ===== Acciones ===== */
.actions {
    border-top: 1px solid var(--card-border) !important;
}

.btn-pill {
    border-radius: var(--pill-radius);
}

.btn-soft-secondary,
.btn-soft-danger {
    border-width: 1.5px;
    padding: .38rem .8rem;
    font-weight: 600;
    border-radius: var(--pill-radius);
    transition: background-color .15s ease, border-color .15s ease, color .15s ease, box-shadow .15s ease, transform .08s ease;
}

/* Gris “soft” */
.btn-soft-secondary {
    background: #f3f4f6;
    color: #334155;
    border-color: #e5e7eb;
}

.btn-soft-secondary:hover {
    background: #e7eaee;
    color: #1f2937;
    border-color: #d8dde6;
}

.btn-soft-secondary:active {
    transform: translateY(1px);
}

/* Peligro “soft” */
.btn-soft-danger {
    background: #fff1f2;
    color: #b91c1c;
    border-color: #fecdd3;
}

.btn-soft-danger:hover {
    background: #ffe4e6;
    color: #991b1b;
    border-color: #fda4af;
}

.btn-soft-danger:active {
    transform: translateY(1px);
}

.btn-soft-secondary:focus-visible,
.btn-soft-danger:focus-visible {
    outline: none;
    box-shadow: 0 0 0 .2rem rgba(13, 110, 253, .25);
}

.btn-soft-secondary:disabled,
.btn-soft-danger:disabled {
    opacity: .6;
    cursor: not-allowed;
    box-shadow: none;
}
</style>
