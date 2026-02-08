import { test, expect } from '@playwright/test';
import { ImportPage } from '../pages/ImportPage';
import { loginAsAdmin } from '../utils/helpers';

/**
 * Excel Import E2E Tests
 *
 * Critical user scenarios:
 * 1. Import rutinas from valid Excel
 * 2. Import equipos from valid Excel
 * 3. Import validation - invalid Excel format
 * 4. Import validation - duplicate rutina names
 * 5. Import validation - missing required fields
 * 6. Switch between import tabs
 *
 * Note: Excel test files need to be created manually:
 * - e2e/fixtures/test-rutinas.xlsx
 * - e2e/fixtures/test-equipos.xlsx
 * - e2e/fixtures/invalid-rutinas.xlsx
 */

test.describe('Excel Import', () => {
  let importPage: ImportPage;

  test.beforeEach(async ({ page }) => {
    importPage = new ImportPage(page);
    await loginAsAdmin(page);
    await importPage.goto();
  });

  test('should switch between import tabs', async ({ page }) => {
    // Act
    await importPage.selectTab('Rutinas');
    let isActive = await importPage.isTabActive('Rutinas');
    expect(isActive).toBe(true);

    await importPage.selectTab('Equipos');
    isActive = await importPage.isTabActive('Equipos');
    expect(isActive).toBe(true);

    await importPage.selectTab('Empleados');
    isActive = await importPage.isTabActive('Empleados');
    expect(isActive).toBe(true);
  });

  test.skip('should import rutinas from valid Excel', async ({ page }) => {
    // NOTE: This test requires actual Excel files to be created
    // Skip for now - implement after creating test Excel files

    const filePath = 'e2e/fixtures/test-rutinas.xlsx';

    await importPage.selectTab('Rutinas');
    await importPage.uploadFile(filePath);
    await importPage.clickImport();

    const successMessage = await importPage.getSuccessMessage();
    expect(successMessage).toBeTruthy();
  });

  test.skip('should import equipos from valid Excel', async ({ page }) => {
    // NOTE: This test requires actual Excel files to be created
    const filePath = 'e2e/fixtures/test-equipos.xlsx';

    await importPage.selectTab('Equipos');
    await importPage.uploadFile(filePath);
    await importPage.clickImport();

    const successMessage = await importPage.getSuccessMessage();
    expect(successMessage).toBeTruthy();
  });

  test('should have import button disabled when no file selected', async ({ page }) => {
    await importPage.selectTab('Rutinas');

    const isDisabled = await importPage.isImportButtonDisabled();
    expect(isDisabled).toBe(true);
  });
});
