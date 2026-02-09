import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { DashboardPage } from '../pages/DashboardPage';
import { testData } from '../fixtures/test-data';

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
        await page.goto(url, { waitUntil: 'commit', timeout: 10000 });
        await page.waitForLoadState('domcontentloaded');
        // Extra wait for Firefox to ensure page is stable
        if (browserName === 'firefox') {
          await page.waitForTimeout(1000);
        }
      } catch (error) {
        // If navigation fails, try one more time
        console.log(`Navigation to ${url} failed, retrying...`);
        await page.waitForTimeout(1000);
        await page.goto(url, { waitUntil: 'domcontentloaded' });
      }
    };

    // Test navigation
    await navigateRobustly('/gestion-equipos');
    expect(page.url()).toContain('/gestion-equipos');

    await navigateRobustly('/historial');
    expect(page.url()).toContain('/historial');
  });
});
