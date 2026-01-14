/// <reference types="vitest" />
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/auth': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        secure: false
      },
      '/ordenes': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        secure: false
      },
      '/rutinas': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        secure: false
      },
      '/admin': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        secure: false
      },
      '/equipos': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        secure: false
      },
      '/empleados': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        secure: false
      },
      '/configuracion': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        secure: false
      },
      '/api/auditoria': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        secure: false
      },
      '/hubs': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        secure: false,
        ws: true
      }
    }
  },
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: './src/setupTests.ts',
  }
})
