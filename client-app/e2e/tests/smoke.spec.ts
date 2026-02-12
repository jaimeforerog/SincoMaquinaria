import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { DashboardPage } from '../pages/DashboardPage';
import { testData } from '../fixtures/test-data';
import { TIMEOUTS } from '../e2e.config';

/**
 * Smoke Tests - Fast critical path tests for CI
 *
 * These are the absolute minimum tests that MUST pass:
 * 1. Can login
 * 2. Can load dashboard
 * 3. Can navigate to main pages
 *
 * Run time: ~30 seconds
 */

test.describe('Smoke Tests - Critical Path', () => {
  test('should login and load dashboard', async ({ page }) => {
    const loginPage = new LoginPage(page);
    const dashboardPage = new DashboardPage(page);

    // Login
    await loginPage.goto();
    await loginPage.login(testData.users.admin.email, testData.users.admin.password);

    // Verify dashboard loads
    await dashboardPage.waitForDashboard();
    expect(page.url()).toMatch(/\/$|\/dashboard$/);
  });

  test('should navigate to main pages', async ({ page, browserName }) => {
    const loginPage = new LoginPage(page);

    // Login
    await loginPage.goto();
    await loginPage.login(testData.users.admin.email, testData.users.admin.password);

    // Helper function for robust navigation (especially for Firefox)
    const navigateRobustly = async (url: string) => {
      try {
        await page.goto(url, { waitUntil: 'commit', timeout: TIMEOUTS.pageLoad });
        await page.waitForLoadState('domcontentloaded');
        // Firefox-specific: wait for network idle to ensure stability
        if (browserName === 'firefox') {
          await page.waitForLoadState('networkidle', { timeout: 5000 }).catch(() => {
            console.log(`[Firefox] Network idle timeout for ${url}, continuing anyway`);
          });
        }
      } catch (error) {
        // If navigation fails, try one more time with more lenient waitUntil
        console.log(`[Navigation] Failed to navigate to ${url}, retrying...`);
        await page.waitForLoadState('load');
        await page.goto(url, { waitUntil: 'domcontentloaded', timeout: TIMEOUTS.pageLoad });
        await page.waitForLoadState('domcontentloaded');
      }
    };

    // Test navigation
    await navigateRobustly('/gestion-equipos');
    expect(page.url()).toContain('/gestion-equipos');

    await navigateRobustly('/historial');
    expect(page.url()).toContain('/historial');
  });
});
