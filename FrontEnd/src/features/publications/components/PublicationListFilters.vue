<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
    search: string
    filterTipo: string | null
    filterOperacion: string | null
    view: 'table' | 'grid'
    propertyTypes: Array<{ label: string; value: string | null }>
    operationTypes: Array<{ label: string; value: string | null }>
    bulkDisabled?: boolean
    bulkLoading?: boolean
    bulkCount?: number
}>()

const emit = defineEmits<{
    (e: 'update:search', v: string): void
    (e: 'update:filterTipo', v: string | null): void
    (e: 'update:filterOperacion', v: string | null): void
    (e: 'change-view', v: 'table' | 'grid'): void
    (e: 'new'): void
    (e: 'bulkDelete'): void
}>()

const searchModel = computed({
    get: () => props.search,
    set: (v: string) => emit('update:search', v),
})
const filterTipoModel = computed({
    get: () => props.filterTipo,
    set: (v: string | null) => emit('update:filterTipo', v),
})
const filterOperacionModel = computed({
    get: () => props.filterOperacion,
    set: (v: string | null) => emit('update:filterOperacion', v),
})

const bulkLabel = computed(() =>
    props.bulkCount && props.bulkCount > 0
        ? `Eliminar seleccionados (${props.bulkCount})`
        : 'Eliminar seleccionados'
)
</script>

<template>
    <!-- Barra adaptable -->
    <div class="toolbar d-flex align-items-center flex-wrap gap-1" role="toolbar" aria-label="Acciones de publicaciones">
        <!-- Toggle vista -->
        <div class="btn-group btn-group-sm  toolbar-item" role="group" aria-label="Cambiar vista">
            <button type="button" class="btn btn-outline-secondary btn-pill" :class="{ active: props.view === 'table' }"
                @click="emit('change-view', 'table')" aria-label="Vista tabla">
                <i class="bi bi-list-ul me-1"></i> Tabla
            </button>
            <button type="button" class="btn btn-outline-secondary btn-pill" :class="{ active: props.view === 'grid' }"
                @click="emit('change-view', 'grid')" aria-label="Vista grilla">
                <i class="bi bi-grid-3x3-gap me-1"></i> Grilla
            </button>
        </div>

        <!-- Buscador -->
        <div class="input-group input-group-sm search toolbar-item ">
            <span class="input-group-text pill-left"><i class="bi bi-search"></i></span>
            <input type="search" class="form-control control-pill" v-model="searchModel"
                aria-label="Buscar por tipo, operación o descripción" placeholder="Buscar…" />
        </div>

        <!-- Filtros -->
        <select v-model="filterTipoModel" class="form-select form-select-sm control-pill toolbar-item "
            aria-label="Filtrar por tipo de propiedad">
            <option v-for="opt in props.propertyTypes" :key="String(opt.value)" :value="opt.value">
                {{ opt.label }}
            </option>
        </select>

        <select v-model="filterOperacionModel" class="form-select form-select-sm control-pill toolbar-item "
            aria-label="Filtrar por tipo de operación">
            <option v-for="opt in props.operationTypes" :key="String(opt.value)" :value="opt.value">
                {{ opt.label }}
            </option>
        </select>

        <!-- CTAs (se reacomodan y envuelven) -->
        <div class="toolbar-ctas ms-md-auto toolbar-item">
            <button type="button" class="btn btn-primary btn-sm btn-pill btn-soft-primary" @click="emit('new')">
                <i class="bi bi-plus-lg me-1"></i> Crear publicación
            </button>

            <button type="button" class="btn btn-outline-danger btn-sm btn-pill btn-soft-danger"
                :disabled="props.bulkDisabled || props.bulkLoading" @click="emit('bulkDelete')">
                <span v-if="props.bulkLoading" class="spinner-border spinner-border-sm me-1" role="status"
                    aria-hidden="true"></span>
                <i v-else class="bi bi-trash me-1"></i>
                {{ bulkLabel }}
            </button>
        </div>
    </div>
</template>

<style scoped>
/* ===== Tokens ===== */
:root {
    --h: 38px;
    --gap: .5rem;
    --radius: 999px;
    --ring: 0 0 0 .2rem rgba(13, 110, 253, .25);
}

/* ===== Layout ===== */
.toolbar {
    gap: var(--gap);
    row-gap: .4rem;
}

.toolbar-item {
    flex: 0 0 auto;
}

.search {
    flex: 1 1 320px;
    min-width: 220px;
    max-width: 560px;
}

.form-select.control-pill {
    flex: 1 1 180px;
    min-width: 140px;
    max-width: 240px;
}

.toolbar-ctas {
    display: flex;
    flex-wrap: wrap;
    gap: var(--gap);
    justify-content: flex-end;
    flex: 1 0 240px;
    min-width: 220px;
}

.toolbar-ctas .btn {
    flex: 0 1 auto;
}

.btn-pill,
.control-pill,
.input-group-text {
    height: var(--h);
    border-radius: var(--radius);
}

.input-group-text.pill-left {
    border-top-right-radius: 0;
    border-bottom-right-radius: 0;
}

.search .form-control.control-pill {
    border-top-left-radius: 0;
    border-bottom-left-radius: 0;
}

.btn-soft-primary {
    background: #eef2ff;
    color: #4338ca;
    border-color: #e0e7ff;
}

.btn-soft-primary:hover {
    background: #e0e7ff;
    color: #3730a3;
    border-color: #c7d2fe;
}

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

.btn-group .btn.active {
    background: #e9ecef;
    color: #212529;
    border-color: #ced4da;
}

.search .input-group-text {
    background: #f8f9fa;
    border-right: 0;
    display: inline-flex;
    align-items: center;
}

.search .form-control {
    border-left: 0;
}

.search .form-control:focus,
.form-select.control-pill:focus,
.btn-pill:focus {
    box-shadow: var(--ring);
    border-color: #86b7fe;
    outline: none;
}

/* Íconos */
.btn .bi {
    font-size: .95em;
    line-height: 1;
}

@media (max-width: 768px) {
    .search {
        flex: 1 1 100%;
        max-width: none;
    }

    .form-select.control-pill {
        flex: 1 1 100%;
        max-width: none;
    }

    .toolbar-ctas {
        justify-content: stretch;
        gap: .4rem;
        flex: 1 1 100%;
    }

    .toolbar-ctas .btn {
        flex: 1 1 160px;
    }

}
</style>
