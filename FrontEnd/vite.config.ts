// vite.config.ts
import { fileURLToPath, URL } from 'node:url'
import { defineConfig, loadEnv } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')

  // 1) lee primero de variables de entorno (útil en Docker)
  // 2) luego de archivos .env
  // 3) fallback docker (service name "backend")
  // 4) fallback local https típico
  const target =
    process.env.VITE_API_TARGET ||
    env.VITE_API_TARGET ||
    'https://localhost:7257'

  return {
    plugins: [vue()],
    resolve: {
      alias: { '@': fileURLToPath(new URL('./src', import.meta.url)) },
    },
    server: {
      host: true,
      port: 5173,
      proxy: {
        '/api': {
          target,
          changeOrigin: true,
          secure: false,
          // opcional: logs para ver a dónde está proxyeando
          configure(proxy) {
            proxy.on('proxyReq', (_req, req) => {
              console.log('[proxy:req]', req.method, req.url, '->', target)
            })
            proxy.on('proxyRes', (res, req) => {
              console.log('[proxy:res]', req.method, req.url, res.statusCode)
            })
            proxy.on('error', (err, req) => {
              console.error('[proxy:error]', req.method, req.url, '->', target, err.message)
            })
          },
        },
      },
    },
  }
})
