import { defineStore } from 'pinia'
import {
  listPublications,
  getPublication,
  createPublication,
  updatePublication,
  deletePublication,
  bulkDeletePublications
} from '../services/publications.api'
import type { Publication } from '../types/publication'
import { BulkDeleteResult } from '../services/types/bulk-delete.types'

export const usePublicationsStore = defineStore('publications', {
  state: () => ({
    items: [] as Publication[],
    loading: false,
    error: null as string | null,
    selectedIds: new Set<number>()
  }),

  getters: {
    selectedCount: (s) => s.selectedIds.size,
    isSelected: (s) => (id: number) => s.selectedIds.has(id)
  },

  actions: {
    // ===== selección =====
    clearSelection() {
      this.selectedIds.clear()
    },
    toggle(id: number) {
      this.selectedIds.has(id) ? this.selectedIds.delete(id) : this.selectedIds.add(id)
    },
    select(id: number, on: boolean) {
      on ? this.selectedIds.add(id) : this.selectedIds.delete(id)
    },

    // ===== CRUD =====
    async fetchAll() {
      this.loading = true
      this.error = null
      try {
        const { data } = await listPublications()
        this.items = data
      } catch (e: any) {
        this.error = e?.response?.data?.detail || e?.message || 'Error al listar publicaciones.'
        throw e
      } finally {
        this.loading = false
      }
    },

    async fetchOne(id: number) {
      this.error = null
      try {
        const { data } = await getPublication(id)
        return data as Publication
      } catch (e: any) {
        this.error = e?.response?.data?.detail || e?.message || 'Error al obtener la publicación.'
        throw e
      }
    },

    async create(p: Publication) {
      this.error = null
      try {
        const { data } = await createPublication(p)
        this.items = [data as Publication, ...this.items]
        return data as Publication
      } catch (e: any) {
        this.error = e?.response?.data?.detail || e?.message || 'Error al crear la publicación.'
        throw e
      }
    },

    async update(id: number, p: Publication) {
      this.error = null
      try {
        await updatePublication(id, p)
        const ix = this.items.findIndex(x => x.id === id)
        if (ix !== -1) this.items[ix] = { ...this.items[ix], ...p, id }
      } catch (e: any) {
        this.error = e?.response?.data?.detail || e?.message || 'Error al actualizar la publicación.'
        throw e
      }
    },

    async remove(id: number) {
      this.error = null
      try {
        await deletePublication(id)
        this.items = this.items.filter(x => x.id !== id)
        this.selectedIds.delete(id)
      } catch (e: any) {
        this.error = e?.response?.data?.detail || e?.message || 'Error al eliminar la publicación.'
        throw e
      }
    },

    async removeMany(ids?: number[]): Promise<BulkDeleteResult> {
      this.error = null
      const toDelete = ids ?? Array.from(this.selectedIds)
      if (!toDelete.length) {
        return { deletedIds: [], notFoundIds: [], deletedCount: 0, notFoundCount: 0 }
      }

      try {
        const { data } = await bulkDeletePublications(toDelete)

        this.items = this.items.filter(x => !data.deletedIds.includes(x.id ?? -1))

        for (const id of [...data.deletedIds, ...data.notFoundIds]) {
          this.selectedIds.delete(id)
        }

        return data
      } catch (e: any) {
        this.error = e?.response?.data?.detail || e?.message || 'Error al eliminar publicaciones.'
        throw e
      }
    }
  }
})
