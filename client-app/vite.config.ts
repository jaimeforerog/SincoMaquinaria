/// <reference types="vitest" />
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
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
      }
    }
  },
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: './src/setupTests.ts',
  }
})
