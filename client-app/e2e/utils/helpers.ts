import { Page, BrowserContext } from '@playwright/test';
import { testData } from '../fixtures/test-data';

/**
 * Helper utilities for E2E tests
 *
 * Common functions used across multiple test suites:
 * - Authentication helpers
 * - Test data cleanup
 * - Test data creation
 * - Navigation helpers
 */

/**
 * Ensure test admin user exists
 */
export async function ensureTestAdminExists(page: Page) {
  try {
    // Try to setup admin user (only works if no users exist)
    const response = await page.request.post('/api/auth/setup', {
      headers: { 'Content-Type': 'application/json' },
      data: {
        email: testData.users.admin.email,
        nombre: testData.users.admin.nombre,
        password: testData.users.admin.password,
      },
    });

    if (response.ok()) {
      console.log('✅ Test admin user created');
    } else if (response.status() === 400) {
      // User already exists, that's fine
      console.log('✅ Test admin user already exists');
    }
  } catch (error) {
    console.log('⚠️ Could not setup admin user, assuming it exists:', error);
  }
}

/**
 * Login as admin user
 */
export async function loginAsAdmin(page: Page) {
  // Ensure test admin exists first
  await ensureTestAdminExists(page);

  await page.goto('/login');

  // Fill credentials
  await page.locator('input[autocomplete="username"]').first().fill(testData.users.admin.email);
  await page.locator('input[autocomplete="current-password"]').first().fill(testData.users.admin.password);

  // Submit form
  await page.locator('button[type="submit"]').first().click();

  // Wait for redirect to dashboard
  await page.waitForURL(/^.*\/$|^.*\/dashboard$/);
}

/**
 * Login with custom credentials
 */
export async function login(page: Page, email: string, password: string) {
  await page.goto('/login');

  await page.locator('input[autocomplete="username"]').first().fill(email);
  await page.locator('input[autocomplete="current-password"]').first().fill(password);

  await page.locator('button[type="submit"]').first().click();

  // Wait for redirect
  await page.waitForURL(/^.*\/$|^.*\/dashboard$/);
}

/**
 * Logout current user
 */
export async function logout(page: Page) {
  // Look for logout button in the navigation/header
  const logoutButton = page.getByRole('button', { name: /logout|cerrar sesión|salir/i });

  if (await logoutButton.isVisible()) {
    await logoutButton.click();
    await page.waitForURL('/login');
  }
}

/**
 * Get authentication token from localStorage
 */
export async function getAuthToken(page: Page): Promise<string | null> {
  return await page.evaluate(() => localStorage.getItem('token'));
}

/**
 * Get refresh token from localStorage
 */
export async function getRefreshToken(page: Page): Promise<string | null> {
  return await page.evaluate(() => localStorage.getItem('refreshToken'));
}

/**
 * Set authentication token in localStorage
 */
export async function setAuthToken(page: Page, token: string) {
  await page.evaluate((t) => localStorage.setItem('token', t), token);
}

/**
 * Clear authentication tokens
 */
export async function clearAuthTokens(page: Page) {
  await page.evaluate(() => {
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
  });
}

/**
 * Check if user is authenticated
 */
export async function isAuthenticated(page: Page): Promise<boolean> {
  const token = await getAuthToken(page);
  return token !== null && token !== '';
}

/**
 * Create test equipment via API
 */
export async function createTestEquipo(page: Page, equipoData: any): Promise<string> {
  const token = await getAuthToken(page);

  const response = await page.request.post('/api/equipos', {
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`,
    },
    data: equipoData,
  });

  if (!response.ok()) {
    throw new Error(`Failed to create test equipo: ${response.status()}`);
  }

  const data = await response.json();
  return data.id || data.Id;
}

/**
 * Create test rutina via API
 */
export async function createTestRutina(page: Page, rutinaData: any): Promise<string> {
  const token = await getAuthToken(page);

  const response = await page.request.post('/api/rutinas', {
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`,
    },
    data: rutinaData,
  });

  if (!response.ok()) {
    throw new Error(`Failed to create test rutina: ${response.status()}`);
  }

  const data = await response.json();
  return data.id || data.Id;
}

/**
 * Create test order via API
 */
export async function createTestOrder(page: Page, orderData: any): Promise<string> {
  const token = await getAuthToken(page);

  const response = await page.request.post('/api/ordenes', {
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`,
    },
    data: orderData,
  });

  if (!response.ok()) {
    throw new Error(`Failed to create test order: ${response.status()}`);
  }

  const data = await response.json();
  return data.id || data.Id;
}

/**
 * Delete test equipo via API
 */
export async function deleteTestEquipo(page: Page, equipoId: string) {
  const token = await getAuthToken(page);

  await page.request.delete(`/api/equipos/${equipoId}`, {
    headers: {
      'Authorization': `Bearer ${token}`,
    },
  });
}

/**
 * Delete test rutina via API
 */
export async function deleteTestRutina(page: Page, rutinaId: string) {
  const token = await getAuthToken(page);

  await page.request.delete(`/api/rutinas/${rutinaId}`, {
    headers: {
      'Authorization': `Bearer ${token}`,
    },
  });
}

/**
 * Delete test order via API
 */
export async function deleteTestOrder(page: Page, ordenId: string) {
  const token = await getAuthToken(page);

  await page.request.delete(`/api/ordenes/${ordenId}`, {
    headers: {
      'Authorization': `Bearer ${token}`,
    },
  });
}

/**
 * Clean up all test data (equipos with E2E- or TEST- prefix)
 */
export async function cleanupTestData(page: Page) {
  const token = await getAuthToken(page);

  if (!token) {
    console.warn('No auth token found, skipping cleanup');
    return;
  }

  try {
    // Get all equipos
    const equiposResponse = await page.request.get('/api/equipos', {
      headers: { 'Authorization': `Bearer ${token}` },
    });

    if (equiposResponse.ok()) {
      const equipos = await equiposResponse.json();
      const data = equipos.data || equipos;

      // Delete test equipos
      for (const equipo of data) {
        if (equipo.placa.startsWith('E2E-') || equipo.placa.startsWith('TEST-')) {
          await deleteTestEquipo(page, equipo.id);
        }
      }
    }

    // Get all rutinas
    const rutinasResponse = await page.request.get('/api/rutinas', {
      headers: { 'Authorization': `Bearer ${token}` },
    });

    if (rutinasResponse.ok()) {
      const rutinas = await rutinasResponse.json();
      const data = rutinas.data || rutinas;

      // Delete test rutinas
      for (const rutina of data) {
        if (rutina.nombre.includes('E2E') || rutina.nombre.includes('Test')) {
          await deleteTestRutina(page, rutina.id);
        }
      }
    }
  } catch (error) {
    console.warn('Error during cleanup:', error);
  }
}

/**
 * Wait for API response
 */
export async function waitForApiResponse(page: Page, urlPattern: string | RegExp, timeout = 10000) {
  return await page.waitForResponse(
    (response) => {
      const url = response.url();
      if (typeof urlPattern === 'string') {
        return url.includes(urlPattern);
      }
      return urlPattern.test(url);
    },
    { timeout }
  );
}

/**
 * Take screenshot with timestamp
 */
export async function takeScreenshot(page: Page, name: string) {
  const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
  const path = `test-results/screenshots/${name}-${timestamp}.png`;
  await page.screenshot({ path, fullPage: true });
  return path;
}

/**
 * Wait for element to be visible with custom timeout
 */
export async function waitForElement(page: Page, selector: string, timeout = 10000) {
  await page.waitForSelector(selector, { state: 'visible', timeout });
}

/**
 * Simulate slow network for testing loading states
 */
export async function enableSlowNetwork(context: BrowserContext) {
  await context.route('**/*', async (route) => {
    await new Promise((resolve) => setTimeout(resolve, 1000));
    await route.continue();
  });
}

/**
 * Disable slow network
 */
export async function disableSlowNetwork(context: BrowserContext) {
  await context.unroute('**/*');
}

/**
 * Get all validation errors from the page
 */
export async function getValidationErrors(page: Page): Promise<string[]> {
  const errors = page.locator('[role="alert"], .error-message, .MuiFormHelperText-root.Mui-error');
  const count = await errors.count();
  const errorTexts: string[] = [];

  for (let i = 0; i < count; i++) {
    const text = await errors.nth(i).textContent();
    if (text) {
      errorTexts.push(text.trim());
    }
  }

  return errorTexts;
}

/**
 * Generate unique timestamp-based identifier
 */
export function generateUniqueId(): string {
  return `${Date.now()}-${Math.random().toString(36).substring(2, 9)}`;
}

/**
 * Wait for specific amount of time (use sparingly)
 */
export async function wait(milliseconds: number) {
  return new Promise((resolve) => setTimeout(resolve, milliseconds));
}
