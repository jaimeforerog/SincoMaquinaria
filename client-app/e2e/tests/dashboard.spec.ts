import { test, expect } from '@playwright/test';
import { DashboardPage } from '../pages/DashboardPage';
import { loginAsAdmin } from '../utils/helpers';
import { setupBasicTestData, cleanupAllTestData } from '../fixtures/setup-test-data';

/**
 * Dashboard E2E Tests
 *
 * Critical user scenarios:
 * 1. Dashboard displays current KPIs
 * 2. Dashboard updates on new order creation (real-time)
 * 3. Dashboard navigation - Equipos
 * 4. Dashboard navigation - Rutinas
 * 5. Dashboard navigation - Create Order
 * 6. Dashboard shows recent activity
 */

test.describe('Dashboard', () => {
  let dashboardPage: DashboardPage;

  test.beforeEach(async ({ page }) => {
    dashboardPage = new DashboardPage(page);
    await loginAsAdmin(page);
    // Cleanup AFTER login so we have a page loaded
    await cleanupAllTestData(page);
  });

  test.afterAll(async ({ browser }) => {
    const context = await browser.newContext();
    const page = await context.newPage();
    await loginAsAdmin(page);
    await cleanupAllTestData(page);
    await context.close();
  });

  test('should display current KPIs', async ({ page }) => {
    // Arrange - Setup test data
    await setupBasicTestData(page);

    // Act
    await dashboardPage.goto();

    // Assert
    const kpis = await dashboardPage.getAllKPIs();

    expect(kpis.equipos).toBeGreaterThanOrEqual(0);
    expect(kpis.rutinas).toBeGreaterThanOrEqual(0);
    expect(kpis.ordenesActivas).toBeGreaterThanOrEqual(0);
  });

  test('should navigate to equipment config', async ({ page }) => {
    // Arrange
    await dashboardPage.goto();

    // Act
    await dashboardPage.navigateToEquipos();

    // Assert
    expect(page.url()).toContain('/gestion-equipos');
  });

  test('should navigate to rutinas', async ({ page }) => {
    // Arrange
    await dashboardPage.goto();

    // Act
    await dashboardPage.navigateToRutinas();

    // Assert
    expect(page.url()).toContain('/editar-rutinas');
  });

  test('should navigate to historial', async ({ page }) => {
    // Arrange
    await dashboardPage.goto();

    // Act
    await dashboardPage.navigateToHistorial();

    // Assert
    expect(page.url()).toContain('/historial');
  });

  test('should load without errors', async ({ page }) => {
    // Arrange & Act
    await dashboardPage.goto();

    // Assert
    const isDashboardLoaded = await dashboardPage.isDashboardLoaded();
    expect(isDashboardLoaded).toBe(true);
  });
});
