import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react'

export default defineConfig({
    plugins: [react()],
    test: {
        environment: 'jsdom',
        globals: true,
        setupFiles: ['./src/setupTests.ts'],
        fileParallelism: false,
        include: ['src/**/*.{test,spec}.{js,jsx,ts,tsx}'], // Only include unit tests from src
        exclude: [
            '**/node_modules/**',
            '**/dist/**',
            '**/e2e/**', // Exclude Playwright E2E tests
            '**/.{idea,git,cache,output,temp}/**'
        ],
        coverage: {
            provider: 'v8',
            reporter: ['text', 'text-summary'],
            exclude: [
                'e2e/**',
                'src/main.tsx',
                'src/setupTests.ts',
                'playwright.config.ts',
            ]
        }
    },
})

