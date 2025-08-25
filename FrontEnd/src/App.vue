<template>
  <!-- Header full width -->
  <header class="app-header sticky-top shadow-sm">
    <div class="container-fluid">
      <div class="app-header__inner">
        <div class="brand d-flex align-items-center gap-2">
          <span class="brand-pill" aria-hidden="true">
            <i class="bi bi-lightning-charge-fill"></i>
          </span>
          <h1 class="app-title mb-0">Prueba Técnica 2025</h1>
        </div>

        <!-- (Opcional) acciones a la derecha
        <div class="d-none d-sm-flex align-items-center gap-2">
          <button class="btn btn-sm btn-outline-secondary">Acción</button>
        </div>
        -->
      </div>
    </div>
  </header>

  <!-- Main full width -->
  <main class="app-main container-fluid px-3 px-md-4 py-3">
    <RouterView />
  </main>
</template>

<script setup lang="ts">
// nada extra
</script>

<style scoped>
/* ===== Header ===== */
.app-header {
  width: 100%;
  background: rgba(255, 255, 255, 0.86);
  border-bottom: 1px solid #e5e7eb;
  backdrop-filter: saturate(160%) blur(10px);
}

.app-header__inner {
  min-height: 56px;
  display: grid;
  grid-template-columns: 1fr auto;
  align-items: center;
  gap: .75rem;
  padding: .5rem .25rem;
}

@media (max-width: 576px) {
  .app-header__inner {
    grid-template-columns: 1fr;
    gap: .5rem;
    padding: .45rem .15rem;
  }
}

/* Marca / título */
.brand-pill {
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

.app-title {
  letter-spacing: -0.01em;
  color: #0f172a;
  font-weight: 800;
  font-size: clamp(1.05rem, 1rem + .6vw, 1.35rem);
}

/* ===== Main ===== */
.app-main {
  width: 100%;
}

/* ===== Pequeños ajustes de estética ===== */
.shadow-sm {
  box-shadow: 0 2px 6px rgba(0, 0, 0, .06) !important;
}

/* ===== Responsive fino ===== */
@media (max-width: 640px) {
  .brand-pill {
    width: 38px;
    height: 38px;
    border-radius: 10px;
  }

  .app-title {
    font-size: 1.1rem;
  }
}
</style>
