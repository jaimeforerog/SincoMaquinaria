import { Page } from '@playwright/test';
import { BasePage } from './BasePage';

/**
 * Page Object Model for Equipment Configuration page
 *
 * Handles all interactions with equipment management including:
 * - Creating new equipment
 * - Editing existing equipment
 * - Searching equipment
 * - Deleting equipment
 */
export class EquipmentConfigPage extends BasePage {
  // Selectors
  private readonly pageHeading = 'text=Configuración de Equipos';
  private readonly newEquipmentButton = 'button:has-text("Nuevo"), button:has-text("Agregar")';
  private readonly searchInput = 'input[placeholder*="Buscar"], input[type="search"]';
  private readonly equipmentTable = 'table';
  private readonly editButtons = 'button[aria-label*="edit"], button:has(svg)';
  private readonly deleteButtons = 'button[aria-label*="delete"], button:has-text("Eliminar")';
  private readonly dialogTitle = '[role="dialog"] h2';
  private readonly saveButton = 'button:has-text("Guardar")';
  private readonly cancelButton = 'button:has-text("Cancelar")';
  private readonly confirmButton = 'button:has-text("Confirmar")';
  private readonly loadingSpinner = '[role="progressbar"]';

  // Form field selectors
  private readonly placaInput = 'input[name="placa"], #placa';
  private readonly descripcionInput = 'input[name="descripcion"], #descripcion';
  private readonly marcaInput = 'input[name="marca"], #marca';
  private readonly modeloInput = 'input[name="modelo"], #modelo';
  private readonly serieInput = 'input[name="serie"], #serie';
  private readonly codigoInput = 'input[name="codigo"], #codigo';
  private readonly grupoSelect = 'select[name="grupo"], #grupo';
  private readonly rutinaSelect = 'select[name="rutina"], #rutina';

  constructor(page: Page) {
    super(page);
  }

  /**
   * Navigate to equipment config page
   */
  async goto() {
    await super.goto('/gestion-equipos');
    await this.waitForPageLoad();
  }

  /**
   * Wait for page to load
   */
  async waitForPageLoad() {
    await this.page.waitForLoadState('networkidle');
    await this.waitForLoadingComplete();
  }

  /**
   * Click new equipment button
   */
  async clickNewEquipment() {
    const button = this.page.locator(this.newEquipmentButton).first();
    await button.click();
    await this.waitForElement('[role="dialog"]');
  }

  /**
   * Fill equipment form fields
   */
  async fillEquipmentForm(data: {
    placa: string;
    descripcion: string;
    marca?: string;
    modelo?: string;
    serie?: string;
    codigo?: string;
    grupo?: string;
    rutina?: string;
  }) {
    // Fill text inputs
    await this.page.locator(this.placaInput).first().fill(data.placa);
    await this.page.locator(this.descripcionInput).first().fill(data.descripcion);

    if (data.marca) {
      const marcaInput = this.page.locator(this.marcaInput).first();
      if (await marcaInput.isVisible()) {
        await marcaInput.fill(data.marca);
      }
    }

    if (data.modelo) {
      const modeloInput = this.page.locator(this.modeloInput).first();
      if (await modeloInput.isVisible()) {
        await modeloInput.fill(data.modelo);
      }
    }

    if (data.serie) {
      const serieInput = this.page.locator(this.serieInput).first();
      if (await serieInput.isVisible()) {
        await serieInput.fill(data.serie);
      }
    }

    if (data.codigo) {
      const codigoInput = this.page.locator(this.codigoInput).first();
      if (await codigoInput.isVisible()) {
        await codigoInput.fill(data.codigo);
      }
    }

    // Fill select fields
    if (data.grupo) {
      const grupoSelect = this.page.locator(this.grupoSelect).first();
      if (await grupoSelect.isVisible()) {
        await grupoSelect.selectOption({ label: data.grupo });
      }
    }

    if (data.rutina) {
      const rutinaSelect = this.page.locator(this.rutinaSelect).first();
      if (await rutinaSelect.isVisible()) {
        await rutinaSelect.selectOption({ label: data.rutina });
      }
    }
  }

  /**
   * Submit equipment form (create or edit)
   */
  async submitEquipment() {
    const saveBtn = this.page.locator(this.saveButton).first();
    await saveBtn.click();
    await this.waitForLoadingComplete();
  }

  /**
   * Search for equipment by placa
   */
  async searchEquipment(placa: string) {
    const searchBox = this.page.locator(this.searchInput).first();
    if (await searchBox.isVisible()) {
      await searchBox.fill(placa);
      await this.wait(500); // Wait for filter to apply
    }
  }

  /**
   * Click edit button for specific equipment by placa
   */
  async editEquipment(placa: string) {
    // Find the row containing the placa
    const row = this.page.locator(`tr:has-text("${placa}")`).first();

    // Find and click the edit button in that row
    const editBtn = row.locator('button').filter({ hasText: /edit/i }).or(
      row.locator('button').first()
    );

    await editBtn.click();
    await this.waitForElement('[role="dialog"]');
  }

  /**
   * Delete equipment by placa
   */
  async deleteEquipment(placa: string) {
    // Find the row containing the placa
    const row = this.page.locator(`tr:has-text("${placa}")`).first();

    // Find and click the delete button
    const deleteBtn = row.locator('button').filter({ hasText: /delete|eliminar/i }).or(
      row.locator('button').nth(1)
    );

    await deleteBtn.click();

    // Wait for confirmation dialog
    await this.waitForElement('[role="dialog"]');

    // Confirm deletion
    const confirmBtn = this.page.locator(this.confirmButton).first();
    await confirmBtn.click();

    await this.waitForLoadingComplete();
  }

  /**
   * Get list of equipment placas visible in the table
   */
  async getVisibleEquipmentPlacas(): Promise<string[]> {
    const rows = this.page.locator('tbody tr');
    const count = await rows.count();
    const placas: string[] = [];

    for (let i = 0; i < count; i++) {
      const cells = rows.nth(i).locator('td');
      const firstCell = await cells.first().textContent();
      if (firstCell) {
        placas.push(firstCell.trim());
      }
    }

    return placas;
  }

  /**
   * Check if equipment exists in table by placa
   */
  async isEquipmentVisible(placa: string): Promise<boolean> {
    const row = this.page.locator(`tr:has-text("${placa}")`);
    return await row.isVisible();
  }

  /**
   * Get validation error messages
   */
  async getValidationErrors(): Promise<string[]> {
    const errors = this.page.locator('[role="alert"], .error-message, .MuiFormHelperText-root.Mui-error');
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
   * Close dialog
   */
  async closeDialog() {
    const cancelBtn = this.page.locator(this.cancelButton).first();
    await cancelBtn.click();
  }

  /**
   * Wait for loading to complete
   */
  async waitForLoadingComplete() {
    const spinner = this.page.locator(this.loadingSpinner);
    try {
      await spinner.waitFor({ state: 'visible', timeout: 1000 });
      await spinner.waitFor({ state: 'hidden', timeout: 10000 });
    } catch {
      // Spinner might not appear if data loads quickly
    }
  }

  /**
   * Get total equipment count from table
   */
  async getEquipmentCount(): Promise<number> {
    const rows = this.page.locator('tbody tr');
    return await rows.count();
  }

  /**
   * Create new equipment (complete flow)
   */
  async createEquipment(data: {
    placa: string;
    descripcion: string;
    marca?: string;
    modelo?: string;
    serie?: string;
    codigo?: string;
    grupo?: string;
    rutina?: string;
  }) {
    await this.clickNewEquipment();
    await this.fillEquipmentForm(data);
    await this.submitEquipment();
  }

  /**
   * Check if dialog is open
   */
  async isDialogOpen(): Promise<boolean> {
    const dialog = this.page.locator('[role="dialog"]');
    return await dialog.isVisible();
  }

  /**
   * Get dialog title text
   */
  async getDialogTitle(): Promise<string | null> {
    const title = this.page.locator(this.dialogTitle).first();
    if (await title.isVisible()) {
      return await title.textContent();
    }
    return null;
  }
}
