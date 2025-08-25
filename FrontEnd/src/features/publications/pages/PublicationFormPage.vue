<script setup lang="ts">
import { onMounted, reactive, ref, computed } from 'vue'
import { useRoute, useRouter, onBeforeRouteLeave } from 'vue-router'
import { usePublicationsStore } from '../store/publications.store'
import type { Publication } from '../types/publication'

// Opciones
import { usePublicationOptions } from '../composables/usePublicationOptions'
const { propertyTypes, operationTypes } = usePublicationOptions()

const route = useRoute()
const router = useRouter()
const store = usePublicationsStore()

const isEdit = computed(() => !!route.params.id)
const pageTitle = computed(() => (isEdit.value ? 'Editar publicación' : 'Nueva publicación'))

// ---- Form ----
const form = reactive<Publication>({
    id: undefined,
    tipoPropiedad: '',
    tipoOperacion: '',
    descripcion: '',
    ambientes: 0,
    m2: 0,
    antiguedad: 0,
    lat: null,
    lng: null,
    imagenes: []
})

const submitting = ref(false)
const submitted = ref(false)
const newImageUrl = ref('')

// ---- Errores de UI ----
const errors = reactive<Record<string, string>>({})

// snapshot para detectar cambios
const snapshot = ref<string>('')

onMounted(async () => {
    if (isEdit.value) {
        const id = Number(route.params.id)
        const data = await store.fetchOne(id)
        Object.assign(form, {
            ...data,
            lat: data.lat ?? null,
            lng: data.lng ?? null
        })
    }
    snapshot.value = JSON.stringify(form)
})

// Guardar confirm si hay cambios sin guardar
const isDirty = computed(() => JSON.stringify(form) !== snapshot.value)

onBeforeRouteLeave((to, from, next) => {
    if (!isDirty.value) return next()
    if (confirm('Hay cambios sin guardar. ¿Salir de todas formas?')) next()
    else next(false)
})

// ---- Toasts ----
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

// ---- Imágenes ----
function addImage() {
    const url = (newImageUrl.value || '').trim()
    if (!url) return

    const isUrl = /^https?:\/\/.+/i.test(url)
    if (!isUrl) {
        addToast('warning', 'URL inválida', 'Ingresá una URL que comience con http(s)://', 2200)
        return
    }

    const dup = form.imagenes.some(i => i.url.toLowerCase() === url.toLowerCase())
    if (dup) {
        addToast('info', 'Ya existe', 'Esa imagen ya está agregada.', 1800)
        return
    }

    form.imagenes.push({ url })
    newImageUrl.value = ''
}

function removeImage(ix: number) {
    form.imagenes.splice(ix, 1)
}

// ---- Validación ----
function validate(): boolean {
    // reset
    errors.tipoPropiedad = ''
    errors.tipoOperacion = ''
    errors.descripcion = ''
    errors.ambientes = ''
    errors.m2 = ''
    errors.antiguedad = ''
    errors.numericos = ''
    errors.lat = ''
    errors.lng = ''

    if (!form.tipoPropiedad) errors.tipoPropiedad = 'Campo requerido.'
    if (!form.tipoOperacion) errors.tipoOperacion = 'Campo requerido.'
    if (!form.descripcion?.trim()) errors.descripcion = 'Campo requerido.'

    if (form.ambientes == null) errors.ambientes = 'Campo requerido.'
    if (form.m2 == null) errors.m2 = 'Campo requerido.'
    if (form.antiguedad == null) errors.antiguedad = 'Campo requerido.'

    if (
        (form.ambientes != null && form.ambientes < 0) ||
        (form.m2 != null && form.m2 < 0) ||
        (form.antiguedad != null && form.antiguedad < 0)
    ) {
        errors.numericos = 'Ambientes, M2 y Antigüedad no pueden ser negativos.'
    }

    if (form.lat == null) errors.lat = 'Campo requerido.'
    if (form.lng == null) errors.lng = 'Campo requerido.'
    if (form.lat != null && (form.lat < -90 || form.lat > 90)) {
        errors.lat = 'La latitud debe estar entre -90 y 90.'
    }
    if (form.lng != null && (form.lng < -180 || form.lng > 180)) {
        errors.lng = 'La longitud debe estar entre -180 y 180.'
    }

    return !errors.tipoPropiedad && !errors.tipoOperacion && !errors.descripcion &&
        !errors.ambientes && !errors.m2 && !errors.antiguedad &&
        !errors.numericos && !errors.lat && !errors.lng
}

function normalize() {
    form.tipoPropiedad = (form.tipoPropiedad || '').trim()
    form.tipoOperacion = (form.tipoOperacion || '').trim()
    form.descripcion = (form.descripcion || '').trim()

    if ((form.lat as unknown) === '') form.lat = null
    if ((form.lng as unknown) === '') form.lng = null
}

// ---- Guardar ----
async function save() {
    submitted.value = true
    if (!validate()) {
        addToast('warning', 'Revisá los campos obligatorios')
        return
    }

    normalize()
    submitting.value = true
    try {
        if (isEdit.value && form.id) {
            await store.update(form.id, form)
            addToast('success', 'Actualizado')
        } else {
            await store.create(form)
            addToast('success', 'Creado')
        }
        snapshot.value = JSON.stringify(form)
        router.push({ name: 'publications-list' })
    } catch (e: any) {
        const status = e?.response?.status ?? e?.status
        const detail = e?.response?.data?.detail || e?.message || 'Error al guardar'
        let title = 'Error'
        if (status === 400) title = 'Validación'
        if (status === 500) title = 'Error interno'
        addToast('danger', title, detail, 3200)
    } finally {
        submitting.value = false
    }
}

function cancel() {
    if (!isDirty.value) {
        router.push({ name: 'publications-list' })
        return
    }
    if (confirm('Hay cambios sin guardar. ¿Descartar y volver?')) {
        router.push({ name: 'publications-list' })
    }
}
</script>

<template>
    <section class="container page">
        <!-- Toasts -->
        <div class="position-fixed top-0 end-0 p-3" style="z-index:1060">
            <div style="min-width:280px;">
                <div v-for="t in toasts" :key="t.id" class="alert alert-dismissible fade show mb-2" :class="{
                    'alert-success': t.variant === 'success',
                    'alert-danger': t.variant === 'danger',
                    'alert-warning': t.variant === 'warning',
                    'alert-info': t.variant === 'info'
                }" role="alert">
                    <strong v-if="t.title">{{ t.title }}</strong>
                    <span v-if="t.detail"> {{ t.detail }}</span>
                    <button type="button" class="btn-close"
                        @click="toasts = toasts.filter(x => x.id !== t.id)"></button>
                </div>
            </div>
        </div>

        <!-- Card -->
        <div class="card shadow-sm rounded-2">
            <div class="card-body">
                <!-- Header -->
                <header class="d-flex align-items-center justify-content-between gap-2 pb-2">
                    <div class="d-flex align-items-center gap-2">
                        <div class="icon-pill" aria-hidden="true">
                            <i class="bi" :class="isEdit ? 'bi-pencil' : 'bi-plus-lg'"></i>
                        </div>
                        <div>
                            <h2 class="mb-0 title">{{ pageTitle }}</h2>
                            <p class="subtitle mb-0">
                                Completá los campos y guardá para {{ isEdit ? 'actualizar' : 'publicar' }}.
                                <span class="badge rounded-pill" :class="isEdit ? 'text-bg-info' : 'text-bg-primary'">
                                    <i class="bi me-1" :class="isEdit ? 'bi-pencil' : 'bi-stars'"></i>
                                    {{ isEdit ? 'Editando' : 'Nuevo' }}
                                </span>
                            </p>
                        </div>
                    </div>

                    <div>
                        <!-- Botón volver con estilo soft/pill -->
                        <button type="button" class="btn btn-pill btn-soft-secondary" @click="cancel">
                            <i class="bi bi-arrow-left me-1"></i> Volver
                        </button>
                    </div>
                </header>

                <hr class="my-3 soft-hr" />

                <!-- Form -->
                <form @submit.prevent="save" class="d-grid gap-4">
                    <!-- Propiedad -->
                    <section class="d-grid gap-3">
                        <div class="d-flex align-items-center gap-2 text-secondary">
                            <i class="bi bi-house-door-fill"></i>
                            <h3 class="h6 m-0 fw-bold text-dark">Propiedad</h3>
                        </div>

                        <div class="row g-3">
                            <div class="col-12 col-md-6">
                                <label for="tipoProp" class="form-label">Tipo Propiedad <b
                                        class="text-primary">*</b></label>
                                <select id="tipoProp" v-model="form.tipoPropiedad" class="form-select"
                                    :class="{ 'is-invalid': submitted && !!errors.tipoPropiedad }">
                                    <option value="" disabled>Seleccioná Tipo de Propiedad</option>
                                    <option v-for="opt in propertyTypes" :key="opt.value" :value="opt.value">
                                        {{ opt.label }}
                                    </option>
                                </select>
                                <div v-if="submitted && errors.tipoPropiedad" class="invalid-feedback">
                                    {{ errors.tipoPropiedad }}
                                </div>
                            </div>

                            <div class="col-12 col-md-6">
                                <label for="tipoOp" class="form-label">Tipo Operación <b
                                        class="text-primary">*</b></label>
                                <select id="tipoOp" v-model="form.tipoOperacion" class="form-select"
                                    :class="{ 'is-invalid': submitted && !!errors.tipoOperacion }">
                                    <option value="" disabled>Seleccioná Tipo de Operación</option>
                                    <option v-for="opt in operationTypes" :key="opt.value" :value="opt.value">
                                        {{ opt.label }}
                                    </option>
                                </select>
                                <div v-if="submitted && errors.tipoOperacion" class="invalid-feedback">
                                    {{ errors.tipoOperacion }}
                                </div>
                            </div>
                        </div>

                        <div>
                            <label for="desc" class="form-label">Descripción <b class="text-primary">*</b></label>
                            <textarea id="desc" v-model="form.descripcion" rows="4"
                                placeholder="Detalles de la propiedad…" class="form-control"
                                :class="{ 'is-invalid': submitted && !!errors.descripcion }"></textarea>
                            <div v-if="submitted && errors.descripcion" class="invalid-feedback">
                                {{ errors.descripcion }}
                            </div>
                        </div>
                    </section>

                    <hr class="my-2 soft-hr" />

                    <!-- Detalles -->
                    <section class="d-grid gap-3">
                        <div class="d-flex align-items-center gap-2 text-secondary">
                            <i class="bi bi-sliders"></i>
                            <h3 class="h6 m-0 fw-bold text-dark">Detalles</h3>
                        </div>

                        <div class="row g-3 align-items-end">
                            <div class="col-12 col-md-4">
                                <label for="amb" class="form-label">Ambientes <b class="text-primary">*</b></label>
                                <input id="amb" type="number" min="0" step="1" v-model.number="form.ambientes"
                                    class="form-control" :class="{ 'is-invalid': submitted && !!errors.ambientes }" />
                                <div v-if="submitted && errors.ambientes" class="invalid-feedback">
                                    {{ errors.ambientes }}
                                </div>
                            </div>

                            <div class="col-12 col-md-4">
                                <label for="m2" class="form-label">M2 <b class="text-primary">*</b></label>
                                <input id="m2" type="number" min="0" step="1" v-model.number="form.m2"
                                    class="form-control" :class="{ 'is-invalid': submitted && !!errors.m2 }" />
                                <div v-if="submitted && errors.m2" class="invalid-feedback">
                                    {{ errors.m2 }}
                                </div>
                            </div>

                            <div class="col-12 col-md-4">
                                <label for="ant" class="form-label">Antigüedad (años) <b
                                        class="text-primary">*</b></label>
                                <input id="ant" type="number" min="0" step="1" v-model.number="form.antiguedad"
                                    class="form-control" :class="{ 'is-invalid': submitted && !!errors.antiguedad }" />
                                <div v-if="submitted && errors.antiguedad" class="invalid-feedback">
                                    {{ errors.antiguedad }}
                                </div>
                            </div>
                        </div>

                        <div v-if="submitted && errors.numericos" class="text-danger small">
                            {{ errors.numericos }}
                        </div>
                    </section>

                    <hr class="my-2 soft-hr" />

                    <!-- Ubicación -->
                    <section class="d-grid gap-3">
                        <div class="d-flex align-items-center gap-2 text-secondary">
                            <i class="bi bi-geo-alt-fill"></i>
                            <h3 class="h6 m-0 fw-bold text-dark">Ubicación</h3>
                        </div>

                        <div class="row g-3">
                            <div class="col-12 col-md-6">
                                <label for="lat" class="form-label">Latitud</label>
                                <input id="lat" type="number" step="0.000001" min="-90" max="90"
                                    v-model.number="form.lat" class="form-control"
                                    :class="{ 'is-invalid': submitted && !!errors.lat }" placeholder="Ej: -34.6037" />
                                <div v-if="submitted && errors.lat" class="invalid-feedback">
                                    {{ errors.lat }}
                                </div>
                            </div>

                            <div class="col-12 col-md-6">
                                <label for="lng" class="form-label">Longitud</label>
                                <input id="lng" type="number" step="0.000001" min="-180" max="180"
                                    v-model.number="form.lng" class="form-control"
                                    :class="{ 'is-invalid': submitted && !!errors.lng }" placeholder="Ej: -58.3816" />
                                <div v-if="submitted && errors.lng" class="invalid-feedback">
                                    {{ errors.lng }}
                                </div>
                            </div>
                        </div>
                    </section>

                    <hr class="my-2 soft-hr" />

                    <!-- Imágenes -->
                    <section class="d-grid gap-3">
                        <div class="d-flex align-items-center gap-2 text-secondary">
                            <i class="bi bi-images"></i>
                            <h3 class="h6 m-0 fw-bold text-dark">Imágenes</h3>
                        </div>

                        <div class="row g-2 align-items-center">
                            <div class="col">
                                <input v-model="newImageUrl" type="url" class="form-control" placeholder="https://..."
                                    inputmode="url" aria-label="URL de imagen" @keyup.enter="addImage" />
                            </div>
                            <div class="col-auto">
                                <!-- Botón agregar estilo soft/pill -->
                                <button type="button" class="btn btn-pill btn-soft-primary" @click="addImage">
                                    <i class="bi bi-plus-lg me-1"></i> Agregar
                                </button>
                            </div>
                        </div>

                        <div v-if="form.imagenes.length" class="row g-3 mt-1">
                            <div v-for="(img, ix) in form.imagenes" :key="ix" class="col-6 col-sm-4 col-md-3">
                                <div class="position-relative border rounded-3 overflow-hidden bg-light"
                                    style="height:120px;">
                                    <img :src="img.url" alt="img" class="w-100 h-100" style="object-fit:cover;" />
                                    <button type="button"
                                        class="btn btn-icon btn-dark position-absolute top-0 end-0 m-2"
                                        @click="removeImage(ix)" aria-label="Quitar imagen">
                                        <i class="bi bi-x-lg"></i>
                                    </button>
                                </div>
                            </div>
                        </div>
                        <p v-else class="text-muted mb-0">No agregaste imágenes aún.</p>
                    </section>

                    <!-- Acciones -->
                    <div class="d-flex flex-wrap justify-content-end gap-2 pt-2">
                        <button type="button" class="btn btn-pill btn-soft-secondary" @click="cancel">
                            <i class="bi bi-arrow-left me-1"></i> Cancelar
                        </button>

                        <button type="submit" class="btn btn-primary btn-pill" :disabled="submitting">
                            <span v-if="submitting" class="spinner-border spinner-border-sm me-1" role="status"
                                aria-hidden="true"></span>
                            <i v-else class="bi me-1" :class="isEdit ? 'bi-save' : 'bi-send'"></i>
                            {{ isEdit ? 'Guardar cambios' : 'Crear publicación' }}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    </section>
</template>

<style scoped>
:root {
    --radius-md: .6rem;
    --ring: 0 0 0 .2rem rgba(13, 110, 253, .25);
}

/* ===== Card ===== */
.page {
    padding-top: .9rem;
}

.title {
    font-size: clamp(1.25rem, 1.05rem + .7vw, 1.7rem);
    font-weight: 800;
}

.subtitle {
    color: #64748b;
    font-size: .96rem;
    display: inline-flex;
    gap: .5rem;
    flex-wrap: wrap;
}

.icon-pill {
    width: 46px;
    height: 46px;
    border-radius: 12px;
    display: grid;
    place-items: center;
    background: #eef2ff;
    color: #4338ca;
    border: 1px solid #c7d2fe;
}

.soft-hr {
    opacity: .35;
}

.form-control,
.form-select,
textarea {
    border-radius: var(--radius-md);
}

.form-control:focus,
.form-select:focus,
textarea:focus {
    box-shadow: var(--ring);
    border-color: #86b7fe;
    outline: none;
}

.btn-pill {
    border-radius: 999px;
}

.btn-soft-primary,
.btn-soft-secondary,
.btn-soft-danger,
.btn-primary.btn-pill {
    border-width: 1.5px;
    font-weight: 600;
    transition: background-color .15s ease, border-color .15s ease, color .15s ease, box-shadow .15s ease, transform .08s ease;
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

.btn-soft-primary:focus-visible {
    box-shadow: var(--ring);
}

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

.btn-soft-secondary:focus-visible {
    box-shadow: var(--ring);
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

.btn-soft-danger:focus-visible {
    box-shadow: var(--ring);
}

.btn-primary.btn-pill {
    padding-inline: 1rem;
}

.btn-primary.btn-pill:active,
.btn-soft-primary:active,
.btn-soft-secondary:active,
.btn-soft-danger:active {
    transform: translateY(1px);
}

.btn-icon {
    width: 30px;
    height: 30px;
    padding: 0;
    border-radius: 999px;
    display: grid;
    place-items: center;
    opacity: .9;
}

.btn-icon:hover {
    opacity: 1;
}

@media (max-width: 768px) {
    header {
        flex-direction: column;
        align-items: flex-start;
        gap: .5rem;
    }
}
</style>
