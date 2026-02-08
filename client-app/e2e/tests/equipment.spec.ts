import { test, expect } from '@playwright/test';
import { EquipmentConfigPage } from '../pages/EquipmentConfigPage';
import { loginAsAdmin } from '../utils/helpers';
import { generateUniqueEquipo, testData } from '../fixtures/test-data';
import { cleanupAllTestData } from '../fixtures/setup-test-data';

/**
 * Equipment Management E2E Tests
 *
 * Critical user scenarios:
 * 1. Create new equipment
 * 2. Equipment form validation - placa required
 * 3. Equipment form validation - descripcion required
 * 4. Edit existing equipment
 * 5. Search equipment by placa
 * 6. Filter equipment by grupo
 * 7. Delete equipment
 * 8. Prevent duplicate placa
 */

test.describe('Equipment Management', () => {
  let equipmentPage: EquipmentConfigPage;

  test.beforeEach(async ({ page }) => {
    equipmentPage = new EquipmentConfigPage(page);
    await loginAsAdmin(page);
    await equipmentPage.goto();
  });

  test.afterAll(async ({ browser }) => {
    const context = await browser.newContext();
    const page = await context.newPage();
    await loginAsAdmin(page);
    await cleanupAllTestData(page);
    await context.close();
  });

  test('should create new equipment', async ({ page }) => {
    // Arrange
    const uniqueEquipo = generateUniqueEquipo(testData.equipos[0]);

    // Act
    await equipmentPage.clickNewEquipment();
    await equipmentPage.fillEquipmentForm({
      placa: uniqueEquipo.placa,
      descripcion: uniqueEquipo.descripcion,
      marca: uniqueEquipo.marca,
      modelo: uniqueEquipo.modelo,
      serie: uniqueEquipo.serie,
    });
    await equipmentPage.submitEquipment();

    // Assert
    await equipmentPage.waitForLoadingComplete();
    const isVisible = await equipmentPage.isEquipmentVisible(uniqueEquipo.placa);
    expect(isVisible).toBe(true);
  });

  test('should validate placa is required', async ({ page }) => {
    // Act
    await equipmentPage.clickNewEquipment();
    await equipmentPage.fillEquipmentForm({
      placa: '',
      descripcion: 'Test description',
    });
    await equipmentPage.submitEquipment();

    // Assert - Should show validation error
    const errors = await equipmentPage.getValidationErrors();
    expect(errors.length).toBeGreaterThan(0);
  });

  test('should validate descripcion is required', async ({ page }) => {
    // Act
    await equipmentPage.clickNewEquipment();
    await equipmentPage.fillEquipmentForm({
      placa: 'TEST-PLACA',
      descripcion: '',
    });
    await equipmentPage.submitEquipment();

    // Assert
    const errors = await equipmentPage.getValidationErrors();
    expect(errors.length).toBeGreaterThan(0);
  });

  test('should search equipment by placa', async ({ page }) => {
    // Arrange - Create test equipment
    const uniqueEquipo = generateUniqueEquipo(testData.equipos[0]);
    await equipmentPage.createEquipment({
      placa: uniqueEquipo.placa,
      descripcion: uniqueEquipo.descripcion,
    });

    // Act
    await equipmentPage.searchEquipment(uniqueEquipo.placa);

    // Assert
    const isVisible = await equipmentPage.isEquipmentVisible(uniqueEquipo.placa);
    expect(isVisible).toBe(true);
  });

  test('should edit existing equipment', async ({ page }) => {
    // Arrange - Create equipment first
    const uniqueEquipo = generateUniqueEquipo(testData.equipos[0]);
    await equipmentPage.createEquipment({
      placa: uniqueEquipo.placa,
      descripcion: uniqueEquipo.descripcion,
    });

    // Act - Edit the equipment
    await equipmentPage.editEquipment(uniqueEquipo.placa);
    await equipmentPage.fillEquipmentForm({
      placa: uniqueEquipo.placa,
      descripcion: 'Updated Description',
    });
    await equipmentPage.submitEquipment();

    // Assert
    await equipmentPage.waitForLoadingComplete();
    const isVisible = await equipmentPage.isEquipmentVisible(uniqueEquipo.placa);
    expect(isVisible).toBe(true);
  });
});
