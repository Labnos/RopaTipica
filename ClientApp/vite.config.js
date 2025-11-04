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
    strictPort: true,
    proxy: {
      '/api': {
        target: 'http://localhost:5229', // Backend .NET
        changeOrigin: true,
        secure: false
      }
    }
  },
  build: {
    outDir: '../wwwroot', // Esto hace que el build de producción se copie a wwwroot
    emptyOutDir: true,
    assetsDir: 'assets'
  }
})
