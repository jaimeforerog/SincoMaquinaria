/// <reference types="vitest" />
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],

  build: {
    // Optimizaciones de build
    target: 'es2020',
    minify: 'terser',
    terserOptions: {
      compress: {
        drop_console: true,  // Eliminar console.logs en producción
        drop_debugger: true,
      },
    },

    // Code splitting manual para optimizar chunks
    rollupOptions: {
      output: {
        manualChunks: {
          // Vendor chunks - React ecosystem
          'react-vendor': ['react', 'react-dom', 'react-router-dom'],

          // Material-UI core (el más pesado)
          'mui-core': ['@mui/material', '@emotion/react', '@emotion/styled'],

          // Material-UI icons (separado para lazy load)
          'mui-icons': ['@mui/icons-material'],

          // PDF generation (solo se carga cuando se necesita)
          'pdf-vendor': ['jspdf', 'jspdf-autotable'],

          // SignalR para real-time
          'signalr': ['@microsoft/signalr'],
        },
      },
    },

    // Advertir si un chunk es muy grande
    chunkSizeWarningLimit: 500,
  },

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
      },
      '/test': {
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
    include: [
      'src/**/*.{test,spec}.{js,jsx,ts,tsx}' // Only include unit tests from src folder
    ],
    exclude: [
      '**/node_modules/**',
      '**/dist/**',
      '**/e2e/**', // Exclude Playwright E2E tests
      '**/.{idea,git,cache,output,temp}/**',
      '**/{karma,rollup,webpack,vite,vitest,jest,ava,babel,nyc,cypress,tsup,build}.config.*'
    ]
  }
})
