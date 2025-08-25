// src/services/api.ts
import axios from 'axios'

const baseURL =
  (import.meta.env.VITE_API_BASE?.replace(/\/$/, '')) ?? '/api'

export default axios.create({
  baseURL, // -> https://localhost:7257/api
  headers: { 'Content-Type': 'application/json' },
})
