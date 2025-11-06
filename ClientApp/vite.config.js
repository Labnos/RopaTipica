import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { fileURLToPath, URL } from 'node:url'

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    },
    extensions: ['.js', '.json', '.jsx', '.mjs', '.ts', '.tsx', '.vue']
  },
  server: {
    port: 5173,
    strictPort: false,
    proxy: {
      '/api': {
        target: 'http://localhost:5229',
        changeOrigin: true,
        secure: false,
        rewrite: (path) => path,
        logLevel: 'debug'
      }
    },
    cors: {
      origin: '*',
      credentials: true
    }
  },
  build: {
    outDir: '../wwwroot',
    emptyOutDir: true,
    assetsDir: 'assets'
  }
})