// src/features/publications/services/publications.api.ts
import api from '@/services/api'
import type { Publication } from '../types/publication'
import { toDto } from '../mappers/toDto'
import { BulkDeleteRequest, BulkDeleteResult } from './types/bulk-delete.types'

// ---- ya existentes ----
export const listPublications  = () => api.get('/publications')
export const getPublication    = (id: number) => api.get(`/publications/${id}`)
export const createPublication = (p: Publication) => api.post('/publications', toDto(p))
export const updatePublication = (id: number, p: Publication) => api.put(`/publications/${id}`, toDto(p))
export const deletePublication = (id: number) => api.delete(`/publications/${id}`)
export const bulkDeletePublications = (ids: number[]) =>
  api.post<BulkDeleteResult>('/publications/bulk-delete', { ids } as BulkDeleteRequest)
