import { defineConfig, devices } from '@playwright/test';

/**
 * Playwright configuration for E2E testing
 *
 * This configuration sets up:
 * - Test directory: ./e2e
 * - Parallel execution
 * - Retries in CI
 * - Multiple reporters (HTML, JUnit, JSON)
 * - Multi-browser testing (Chromium, Firefox)
 * - Auto-start web server
 */
export default defineConfig({
  testDir: './e2e',

  // Run tests sequentially to avoid database conflicts
  fullyParallel: false,

  // Fail the build on CI if you accidentally left test.only in the source code
  forbidOnly: !!process.env.CI,

  // Retry on CI only
  retries: process.env.CI ? 2 : 0,

  // Always use 1 worker for E2E tests to avoid race conditions
  workers: 1,

  // Reporter configuration
  reporter: [
    ['html', { outputFolder: 'playwright-report' }],
    ['junit', { outputFile: 'test-results/junit.xml' }],
    ['json', { outputFile: 'test-results/results.json' }],
    ['list'], // Console output
  ],

  // Shared settings for all the projects below
  use: {
    // Base URL for navigation
    baseURL: 'http://localhost:5173',

    // Collect trace when retrying the failed test
    trace: 'on-first-retry',

    // Screenshot on failure
    screenshot: 'only-on-failure',

    // Video on failure
    video: 'retain-on-failure',

    // Maximum time each action such as `click()` can take
    actionTimeout: 10000,

    // Maximum time each navigation can take
    navigationTimeout: 30000,

    // Route API calls directly to backend (bypass Vite proxy for E2E tests)
    extraHTTPHeaders: {
      'X-Playwright-Test': 'true',
    },
  },

  // Configure projects for major browsers
  projects: [
    {
      name: 'chromium',
      use: {
        ...devices['Desktop Chrome'],
        viewport: { width: 1280, height: 720 },
      },
    },

    // Firefox disabled temporarily due to navigation flakiness
    // TODO: Re-enable and fix NS_BINDING_ABORTED error
    // {
    //   name: 'firefox',
    //   use: {
    //     ...devices['Desktop Firefox'],
    //     viewport: { width: 1280, height: 720 },
    //   },
    // },

    // Uncomment for WebKit testing (Safari)
    // {
    //   name: 'webkit',
    //   use: {
    //     ...devices['Desktop Safari'],
    //     viewport: { width: 1280, height: 720 },
    //   },
    // },
  ],

  // Run your local dev server before starting the tests
  webServer: {
    command: 'npm run dev',
    url: 'http://localhost:5173',
    reuseExistingServer: !process.env.CI,
    timeout: 120000,
    stdout: 'ignore',
    stderr: 'pipe',
  },

  // Test output directories
  outputDir: 'test-results',

  // Global timeout for each test
  timeout: process.env.CI ? 120000 : 60000, // 2 min in CI, 1 min local

  // Expect timeout
  expect: {
    timeout: process.env.CI ? 15000 : 10000, // Longer in CI
  },
});
