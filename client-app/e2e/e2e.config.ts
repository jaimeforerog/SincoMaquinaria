/**
 * E2E Testing Configuration
 *
 * Centralized configuration for all E2E tests.
 * Update these values to adjust timeouts, URLs, and other settings globally.
 */

/**
 * Timeout values (in milliseconds)
 */
export const TIMEOUTS = {
  /** Standard navigation timeout */
  navigation: 15000,

  /** API response timeout */
  apiResponse: 30000,

  /** Page load timeout */
  pageLoad: 10000,

  /** Short wait for UI updates */
  short: 2000,

  /** Medium wait for animations/transitions */
  medium: 5000,

  /** Long wait for heavy operations */
  long: 30000,

  /** Firefox-specific stabilization delay */
  firefoxStabilization: 1000,
} as const;

/**
 * Base URLs for different environments
 */
export const URLS = {
  /** Frontend base URL */
  frontend: process.env.BASE_URL || 'http://localhost:5173',

  /** Backend API base URL */
  backend: process.env.API_URL || 'http://localhost:5000',
} as const;

/**
 * Test user credentials
 */
export const TEST_USER = {
  email: 'e2e-test@sinco.com',
  password: 'TestPassword123',
  nombre: 'E2E Test Admin',
} as const;

/**
 * Test data prefixes for easy identification and cleanup
 */
export const TEST_PREFIXES = {
  equipo: 'E2E-',
  rutina: 'Test-Rutina-',
  order: 'E2E-Order-',
  empleado: 'E2E-Empleado-',
} as const;

/**
 * Retry configuration for flaky operations
 */
export const RETRY = {
  /** Number of retries for navigation */
  navigationAttempts: 2,

  /** Delay between retry attempts */
  retryDelay: 1000,
} as const;

/**
 * Feature flags for conditional test execution
 */
export const FEATURES = {
  /** Enable real-time dashboard tests (requires WebSocket) */
  realtimeDashboard: false,

  /** Enable Excel import tests */
  excelImport: true,

  /** Enable PDF export tests */
  pdfExport: false,
} as const;

/**
 * Selector strategies
 */
export const SELECTORS = {
  /** Prefer data-testid attributes */
  preferTestId: true,

  /** Fallback to role-based selectors */
  useRoleSelectors: true,
} as const;
