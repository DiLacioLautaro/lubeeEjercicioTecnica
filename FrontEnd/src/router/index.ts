// src/router/index.ts
import { createRouter, createWebHistory } from 'vue-router'

// Imports dinámicos (sin layout)
const PublicationsListPage = () =>
    import('../features/publications/pages/PublicationListPage.vue')
const PublicationFormPage = () =>
    import('../features/publications/pages/PublicationFormPage.vue')

const routes = [
    { path: '/', name: 'publications-list', component: PublicationsListPage },
    { path: '/publications/new', name: 'publication-new', component: PublicationFormPage },
    { path: '/publications/:id', name: 'publication-edit', component: PublicationFormPage, props: true },
]

const router = createRouter({
    history: createWebHistory(import.meta.env.BASE_URL),
    routes,
    scrollBehavior() {
        return { top: 0 }
    },
})

export default router
