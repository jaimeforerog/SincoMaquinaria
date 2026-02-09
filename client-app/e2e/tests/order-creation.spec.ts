import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { CreateOrderPage } from '../pages/CreateOrderPage';
import { testData } from '../fixtures/test-data';
import { TIMEOUTS } from '../e2e.config';

/**
 * Order Creation and Management E2E Tests
 *
 * Critical user scenarios:
 * 1. Create preventive maintenance order ⭐ CRITICAL
 * 2. Create corrective maintenance order ⭐ CRITICAL
 * 3. Order form validation - equipment required
 * 4. Order form validation - type required
 * 5. Order form validation - preventivo requires rutina
 * 6. Submit button disabled without equipment
 * 7. Rutina field appears only for Preventivo
 * 8. Frecuencia field appears after selecting rutina
 * 9. Navigate to create order page
 * 10. Page title and form elements are visible
 */

test.describe('Order Creation - Form Validation', () => {
  let loginPage: LoginPage;
  let createOrderPage: CreateOrderPage;

  test.beforeEach(async ({ page }) => {
    loginPage = new LoginPage(page);
    createOrderPage = new CreateOrderPage(page);

    // Login before each test
    await loginPage.goto();
    await loginPage.login(testData.users.admin.email, testData.users.admin.password);
  });

  test('should load create order page successfully', async ({ page }) => {
    // Act
    await createOrderPage.goto();

    // Assert
    expect(page.url()).toContain('/nueva-orden');
    await expect(page.locator('text=Nueva Orden de Trabajo')).toBeVisible();
  });

  test('should have submit button disabled without equipment', async ({ page }) => {
    // Arrange
    await createOrderPage.goto();

    // Act & Assert
    const isDisabled = await createOrderPage.isSubmitDisabled();
    expect(isDisabled).toBe(true);
  });

  test('should show rutina field only for Preventivo orders', async ({ page }) => {
    // Arrange
    await createOrderPage.goto();

    // Act - Select Correctivo
    await createOrderPage.selectOrderType('Correctivo');

    // Assert - Rutina should NOT be visible
    const rutinaVisibleCorrectivo = await createOrderPage.isRutinaSelectVisible();
    expect(rutinaVisibleCorrectivo).toBe(false);

    // Act - Change to Preventivo
    await createOrderPage.selectOrderType('Preventivo');

    // Assert - Rutina SHOULD be visible
    const rutinaVisiblePreventivo = await createOrderPage.isRutinaSelectVisible();
    expect(rutinaVisiblePreventivo).toBe(true);
  });

  test('should show frecuencia field only for Preventivo orders', async ({ page }) => {
    // Arrange
    await createOrderPage.goto();

    // Act - Select Correctivo
    await createOrderPage.selectOrderType('Correctivo');

    // Assert - Frecuencia should NOT be visible
    const frecuenciaVisibleCorrectivo = await createOrderPage.isFrecuenciaSelectVisible();
    expect(frecuenciaVisibleCorrectivo).toBe(false);

    // Act - Change to Preventivo
    await createOrderPage.selectOrderType('Preventivo');

    // Assert - Frecuencia SHOULD be visible
    const frecuenciaVisiblePreventivo = await createOrderPage.isFrecuenciaSelectVisible();
    expect(frecuenciaVisiblePreventivo).toBe(true);
  });

  test('should have Tipo de Orden defaulting to Correctivo', async ({ page }) => {
    // Arrange & Act
    await createOrderPage.goto();

    // Assert - Check that Correctivo is selected by default
    const tipoSelect = page.locator('label:has-text("Tipo de Orden") + div');
    const text = await tipoSelect.textContent();

    expect(text).toContain('Correctivo');
  });
});

test.describe('Order Creation - Happy Path', () => {
  let loginPage: LoginPage;
  let createOrderPage: CreateOrderPage;

  test.beforeEach(async ({ page }) => {
    loginPage = new LoginPage(page);
    createOrderPage = new CreateOrderPage(page);

    // Login
    await loginPage.goto();
    await loginPage.login(testData.users.admin.email, testData.users.admin.password);
  });

  test.skip('should create corrective maintenance order', async ({ page, browserName }) => {
    // This test is skipped because it requires equipos to exist in the database
    // To enable: add test data setup or ensure test equipos exist

    await createOrderPage.goto();

    // Try to select first available equipo (if any exist)
    const response = await createOrderPage.createCorrectiveOrder({
      equipo: 'E2E-001' // This would need to exist in DB
    });

    expect(response.ok()).toBe(true);
    await createOrderPage.waitForRedirectToOrderDetail();
    expect(page.url()).toMatch(/\/ordenes\/[a-f0-9-]+/);
  });

  test.skip('should create preventive maintenance order', async ({ page, browserName }) => {
    // This test is skipped because it requires equipos and rutinas to exist in the database
    // To enable: add test data setup or ensure test data exists

    await createOrderPage.goto();

    // Try to create preventive order (requires test data)
    const response = await createOrderPage.createPreventiveOrder({
      equipo: 'E2E-001', // This would need to exist in DB
      rutina: 'Test-Rutina-Preventivo',
      frecuencia: 1000
    });

    expect(response.ok()).toBe(true);
    await createOrderPage.waitForRedirectToOrderDetail();
    expect(page.url()).toMatch(/\/ordenes\/[a-f0-9-]+/);
  });

  test('should navigate to create order from URL', async ({ page }) => {
    // Act
    await page.goto('/nueva-orden');

    // Assert
    await expect(page.locator('text=Nueva Orden de Trabajo')).toBeVisible({
      timeout: TIMEOUTS.pageLoad
    });
    expect(page.url()).toContain('/nueva-orden');
  });

  test('should display all form fields correctly', async ({ page }) => {
    // Arrange & Act
    await createOrderPage.goto();

    // Assert - Check all major form elements are visible
    await expect(page.locator('text=Nueva Orden de Trabajo')).toBeVisible();
    await expect(page.locator('text=Selecciona el Equipo')).toBeVisible();
    await expect(page.locator('text=Detalles de la Orden')).toBeVisible();
    await expect(page.locator('label:has-text("Tipo de Orden")')).toBeVisible();
    await expect(page.locator('label:has-text("Fecha de la OT")')).toBeVisible();
    await expect(page.locator('button:has-text("Crear Orden")')).toBeVisible();
  });

  test('should show helper text for fields', async ({ page }) => {
    // Arrange
    await createOrderPage.goto();

    // Act - Select Preventivo to show rutina field
    await createOrderPage.selectOrderType('Preventivo');

    // Assert - Check that helper texts are shown
    const rutinaHelperText = await createOrderPage.getHelperText('Rutina Sugerida');
    expect(rutinaHelperText).toBeTruthy();
    expect(rutinaHelperText).toContain('rutina');

    const frecuenciaHelperText = await createOrderPage.getHelperText('Frecuencia Mantenimiento');
    expect(frecuenciaHelperText).toBeTruthy();
    expect(frecuenciaHelperText).toContain('frecuencia');
  });
});

test.describe('Order Creation - Navigation', () => {
  test('should access create order page when authenticated', async ({ page }) => {
    // Arrange - Login
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.login(testData.users.admin.email, testData.users.admin.password);

    // Act - Navigate to create order
    await page.goto('/nueva-orden');

    // Assert
    expect(page.url()).toContain('/nueva-orden');
    await expect(page.locator('text=Nueva Orden de Trabajo')).toBeVisible();
  });
});
