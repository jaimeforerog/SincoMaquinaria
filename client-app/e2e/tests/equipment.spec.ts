import { test, expect } from '@playwright/test';
import { EquipmentConfigPage } from '../pages/EquipmentConfigPage';
import { loginAsAdmin } from '../utils/helpers';
import { generateUniqueEquipo, testData } from '../fixtures/test-data';
import { cleanupAllTestData, ensurePrerequisiteData } from '../fixtures/setup-test-data';

test.describe('Equipment Management', () => {
  let equipmentPage: EquipmentConfigPage;

  test.beforeEach(async ({ page }) => {
    equipmentPage = new EquipmentConfigPage(page);
    await loginAsAdmin(page);
    // Ensure prerequisite data (grupo + rutina) exists before each test
    await ensurePrerequisiteData(page);
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
    // Select required dropdown fields (Grupo and Rutina are mandatory)
    await equipmentPage.selectFirstAvailableOption('Grupo de Mantenimiento');
    await equipmentPage.selectFirstAvailableOption('Rutina Asignada');
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

    // Component uses alert() for validation - capture the browser dialog
    let alertMessage = '';
    page.once('dialog', async (dialog) => {
      alertMessage = dialog.message();
      await dialog.accept();
    });

    await page.getByRole('dialog').getByRole('button', { name: /Crear/i }).click();

    // Assert - alert should mention required fields
    expect(alertMessage).toContain('obligatorio');
    // Dialog should remain open (form was not submitted)
    expect(await equipmentPage.isDialogOpen()).toBe(true);
  });

  test('should validate descripcion is required', async ({ page }) => {
    // Act
    await equipmentPage.clickNewEquipment();
    await equipmentPage.fillEquipmentForm({
      placa: 'TEST-PLACA',
      descripcion: '',
    });

    // Component uses alert() for validation
    let alertMessage = '';
    page.once('dialog', async (dialog) => {
      alertMessage = dialog.message();
      await dialog.accept();
    });

    await page.getByRole('dialog').getByRole('button', { name: /Crear/i }).click();

    // Assert
    expect(alertMessage).toContain('obligatorio');
    expect(await equipmentPage.isDialogOpen()).toBe(true);
  });

  test('should search equipment by placa', async ({ page }) => {
    // Arrange - Create test equipment with required fields
    const uniqueEquipo = generateUniqueEquipo(testData.equipos[0]);
    await equipmentPage.clickNewEquipment();
    await equipmentPage.fillEquipmentForm({
      placa: uniqueEquipo.placa,
      descripcion: uniqueEquipo.descripcion,
    });
    await equipmentPage.selectFirstAvailableOption('Grupo de Mantenimiento');
    await equipmentPage.selectFirstAvailableOption('Rutina Asignada');
    await equipmentPage.submitEquipment();

    // Assert - Verify equipment appears in the table
    await equipmentPage.waitForLoadingComplete();
    const isVisible = await equipmentPage.isEquipmentVisible(uniqueEquipo.placa);
    expect(isVisible).toBe(true);
  });

  test('should edit existing equipment', async ({ page }) => {
    // Arrange - Create equipment first
    const uniqueEquipo = generateUniqueEquipo(testData.equipos[0]);
    await equipmentPage.clickNewEquipment();
    await equipmentPage.fillEquipmentForm({
      placa: uniqueEquipo.placa,
      descripcion: uniqueEquipo.descripcion,
    });
    await equipmentPage.selectFirstAvailableOption('Grupo de Mantenimiento');
    await equipmentPage.selectFirstAvailableOption('Rutina Asignada');
    await equipmentPage.submitEquipment();
    await equipmentPage.waitForLoadingComplete();

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
