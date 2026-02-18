import { Page, Locator } from '@playwright/test';
import { BasePage } from './BasePage';

/**
 * Page Object Model for Equipment Configuration page
 *
 * Uses Playwright's getByLabel/getByRole for MUI component compatibility.
 * MUI TextFields don't have name/id attributes, so we use label associations.
 * MUI Selects render as div[role="combobox"], not native <select> elements.
 */
export class EquipmentConfigPage extends BasePage {
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
   * Wait for page to load - waits for page content instead of networkidle
   * (networkidle hangs on Firefox due to WebSocket and is slow with Redis timeouts)
   */
  async waitForPageLoad() {
    await this.page.waitForLoadState('domcontentloaded');
    // Wait for the page heading to appear
    await this.page.getByRole('heading', { name: 'Configuración de Equipos' }).waitFor({ state: 'visible', timeout: 15000 });
    await this.waitForLoadingComplete();
  }

  /**
   * Get the currently visible MUI Dialog
   */
  private getDialog(): Locator {
    return this.page.getByRole('dialog');
  }

  /**
   * Click new equipment button
   */
  async clickNewEquipment() {
    await this.page.getByRole('button', { name: /Nuevo Equipo/i }).click();
    await this.getDialog().waitFor({ state: 'visible' });
  }

  /**
   * Fill equipment form fields using label-based selectors (MUI compatible)
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
    const dialog = this.getDialog();

    // Fill Placa (disabled in edit mode)
    const placaInput = dialog.getByLabel('Placa');
    if (await placaInput.isEnabled()) {
      await placaInput.clear();
      if (data.placa) {
        await placaInput.fill(data.placa);
      }
    }

    // Fill Descripción
    const descripcionInput = dialog.getByLabel('Descripción');
    await descripcionInput.clear();
    if (data.descripcion) {
      await descripcionInput.fill(data.descripcion);
    }

    if (data.marca) {
      await dialog.getByLabel('Marca').fill(data.marca);
    }

    if (data.modelo) {
      await dialog.getByLabel('Modelo').fill(data.modelo);
    }

    if (data.serie) {
      await dialog.getByLabel('Serie').fill(data.serie);
    }

    if (data.codigo) {
      await dialog.getByLabel('Código Interno').fill(data.codigo);
    }

    // MUI Select fields
    if (data.grupo) {
      await this.selectMuiOption('Grupo de Mantenimiento', data.grupo);
    }

    if (data.rutina) {
      await this.selectMuiOption('Rutina Asignada', data.rutina);
    }
  }

  /**
   * Select a specific option from a MUI Select component
   */
  private async selectMuiOption(label: string, optionText: string) {
    await this.getDialog().getByLabel(label).click();
    const listbox = this.page.getByRole('listbox');
    await listbox.waitFor({ state: 'visible' });
    await listbox.getByRole('option', { name: optionText }).click();
  }

  /**
   * Select the first non-empty option from a MUI Select dropdown.
   * Skips the "Ninguno/Ninguna" placeholder option.
   * Retries if options haven't loaded yet (fetchAuxDATA may still be in-flight).
   */
  async selectFirstAvailableOption(label: string) {
    const maxAttempts = 3;

    for (let attempt = 1; attempt <= maxAttempts; attempt++) {
      await this.getDialog().getByLabel(label).click();
      const listbox = this.page.getByRole('listbox');
      await listbox.waitFor({ state: 'visible' });

      // Wait for real options beyond the placeholder to appear
      const options = listbox.getByRole('option');
      try {
        await options.nth(1).waitFor({ state: 'visible', timeout: 5000 });
      } catch {
        // Options may not have loaded yet
      }

      const count = await options.count();
      if (count > 1) {
        await options.nth(1).click();
        return; // Successfully selected
      }

      // Only "Ninguno" exists - close dropdown and retry
      await this.page.keyboard.press('Escape');
      if (attempt < maxAttempts) {
        await this.page.waitForTimeout(2000);
      }
    }

    // Last resort: click whatever is available
    await this.getDialog().getByLabel(label).click();
    const listbox = this.page.getByRole('listbox');
    await listbox.waitFor({ state: 'visible' });
    const options = listbox.getByRole('option');
    const count = await options.count();
    if (count > 0) {
      await options.first().click();
    } else {
      await this.page.keyboard.press('Escape');
    }
  }

  /**
   * Submit equipment form (handles both "Guardar" and "Crear" buttons).
   * Waits for the dialog to close on success, indicating the submit completed.
   */
  async submitEquipment() {
    const dialog = this.getDialog();
    const submitBtn = dialog.getByRole('button', { name: /Guardar|Crear/i });

    // Listen for potential validation alert
    let alertFired = false;
    this.page.once('dialog', async (d) => {
      alertFired = true;
      await d.accept();
    });

    await submitBtn.click();

    // Wait briefly for potential alert
    await this.page.waitForTimeout(500);

    if (!alertFired) {
      // Wait for dialog to close (successful submission)
      try {
        await dialog.waitFor({ state: 'hidden', timeout: 10000 });
      } catch {
        // Dialog might still be open if there was a server error
      }
    }

    await this.waitForLoadingComplete();
  }

  /**
   * Search for equipment by placa.
   * Note: Current page has no search input - waits for table to load.
   */
  async searchEquipment(placa: string) {
    await this.waitForLoadingComplete();
  }

  /**
   * Click edit button for specific equipment by placa
   */
  async editEquipment(placa: string) {
    const row = this.page.locator(`tr:has-text("${placa}")`).first();
    await row.getByRole('button').first().click();
    await this.getDialog().waitFor({ state: 'visible' });
  }

  /**
   * Delete equipment by placa
   */
  async deleteEquipment(placa: string) {
    const row = this.page.locator(`tr:has-text("${placa}")`).first();
    await row.getByRole('button').nth(1).click();
    await this.getDialog().waitFor({ state: 'visible' });
    await this.page.getByRole('button', { name: /Confirmar/i }).click();
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
      const firstCell = await rows.nth(i).locator('td').first().textContent();
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
    return await this.page.locator(`tr:has-text("${placa}")`).isVisible();
  }

  /**
   * Get validation error messages from MUI form helpers
   */
  async getValidationErrors(): Promise<string[]> {
    const errors = this.page.locator('.MuiFormHelperText-root.Mui-error, [role="alert"]');
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
    await this.getDialog().getByRole('button', { name: /Cancelar/i }).click();
  }

  /**
   * Wait for loading to complete
   */
  async waitForLoadingComplete() {
    const spinner = this.page.locator('[role="progressbar"]').first();
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
    return await this.page.locator('tbody tr').count();
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
    const title = this.getDialog().locator('h2').first();
    if (await title.isVisible()) {
      return await title.textContent();
    }
    return null;
  }
}
