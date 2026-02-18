import { Page } from '@playwright/test';
import { BasePage } from './BasePage';

/**
 * Page Object Model for Import page (Excel import functionality)
 *
 * Handles all interactions with Excel import including:
 * - Selecting import tabs (Rutinas, Equipos, Empleados)
 * - Uploading Excel files
 * - Triggering import process
 * - Reading success/error messages
 */
export class ImportPage extends BasePage {
  // Selectors
  private readonly pageHeading = 'text=Importar';
  private readonly rutinasTab = '[role="tab"]:has-text("Rutinas")';
  private readonly equiposTab = '[role="tab"]:has-text("Equipos")';
  private readonly empleadosTab = '[role="tab"]:has-text("Empleados")';
  private readonly fileInput = 'input[type="file"]';
  private readonly importButton = 'button:has-text("Importar")';
  private readonly successMessage = '[role="alert"].MuiAlert-standardSuccess, .success-message';
  private readonly errorMessage = '[role="alert"].MuiAlert-standardError, .error-message';
  private readonly loadingSpinner = '[role="progressbar"]';

  constructor(page: Page) {
    super(page);
  }

  /**
   * Navigate to import page
   */
  async goto() {
    await super.goto('/importar-rutinas');
    await this.waitForPageLoad();
  }

  /**
   * Wait for page to load
   */
  async waitForPageLoad() {
    await this.page.waitForLoadState('networkidle');
  }

  /**
   * Select Rutinas tab
   */
  async selectRutinasTab() {
    const tab = this.page.locator(this.rutinasTab).first();
    await tab.click();
    await this.wait(300);
  }

  /**
   * Select Equipos tab
   */
  async selectEquiposTab() {
    const tab = this.page.locator(this.equiposTab).first();
    await tab.click();
    await this.wait(300);
  }

  /**
   * Select Empleados tab
   */
  async selectEmpleadosTab() {
    const tab = this.page.locator(this.empleadosTab).first();
    await tab.click();
    await this.wait(300);
  }

  /**
   * Select tab by name
   */
  async selectTab(tab: 'Rutinas' | 'Equipos' | 'Empleados') {
    switch (tab) {
      case 'Rutinas':
        await this.selectRutinasTab();
        break;
      case 'Equipos':
        await this.selectEquiposTab();
        break;
      case 'Empleados':
        await this.selectEmpleadosTab();
        break;
    }
  }

  /**
   * Upload Excel file
   */
  async uploadFile(filePath: string) {
    const input = this.page.locator(this.fileInput).first();
    await input.setInputFiles(filePath);
    await this.wait(500); // Wait for file to be processed
  }

  /**
   * Click import button
   */
  async clickImport() {
    const button = this.page.locator(this.importButton).first();
    await button.click();
  }

  /**
   * Upload and import file (complete flow)
   */
  async uploadAndImport(filePath: string) {
    await this.uploadFile(filePath);
    await this.clickImport();
    await this.waitForLoadingComplete();
  }

  /**
   * Get success message text
   */
  async getSuccessMessage(): Promise<string | null> {
    const message = this.page.locator(this.successMessage).first();
    try {
      await message.waitFor({ state: 'visible', timeout: 5000 });
      return await message.textContent();
    } catch {
      return null;
    }
  }

  /**
   * Get error messages
   */
  async getErrorMessages(): Promise<string[]> {
    const errors = this.page.locator(this.errorMessage);
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
   * Wait for success message to appear
   */
  async waitForSuccessMessage() {
    await this.waitForElement(this.successMessage);
  }

  /**
   * Wait for error message to appear
   */
  async waitForErrorMessage() {
    await this.waitForElement(this.errorMessage);
  }

  /**
   * Wait for loading to complete
   */
  async waitForLoadingComplete() {
    const spinner = this.page.locator(this.loadingSpinner).first();
    try {
      await spinner.waitFor({ state: 'visible', timeout: 2000 });
      await spinner.waitFor({ state: 'hidden', timeout: 30000 });
    } catch {
      // Spinner might not appear if import is very fast
    }
  }

  /**
   * Check if import button is disabled
   */
  async isImportButtonDisabled(): Promise<boolean> {
    const button = this.page.locator(this.importButton).first();
    return await button.isDisabled();
  }

  /**
   * Check if a tab is active
   */
  async isTabActive(tab: 'Rutinas' | 'Equipos' | 'Empleados'): Promise<boolean> {
    let tabLocator;
    switch (tab) {
      case 'Rutinas':
        tabLocator = this.page.locator(this.rutinasTab).first();
        break;
      case 'Equipos':
        tabLocator = this.page.locator(this.equiposTab).first();
        break;
      case 'Empleados':
        tabLocator = this.page.locator(this.empleadosTab).first();
        break;
    }

    const ariaSelected = await tabLocator.getAttribute('aria-selected');
    return ariaSelected === 'true';
  }

  /**
   * Clear uploaded file
   */
  async clearFile() {
    const input = this.page.locator(this.fileInput).first();
    await input.setInputFiles([]);
  }

  /**
   * Get uploaded file name
   */
  async getUploadedFileName(): Promise<string | null> {
    // This depends on how the UI displays the uploaded file name
    const fileNameDisplay = this.page.locator('.file-name, .uploaded-file').first();
    if (await fileNameDisplay.isVisible()) {
      return await fileNameDisplay.textContent();
    }
    return null;
  }

  /**
   * Import rutinas from Excel file (complete flow)
   */
  async importRutinas(filePath: string) {
    await this.selectRutinasTab();
    await this.uploadAndImport(filePath);
  }

  /**
   * Import equipos from Excel file (complete flow)
   */
  async importEquipos(filePath: string) {
    await this.selectEquiposTab();
    await this.uploadAndImport(filePath);
  }

  /**
   * Import empleados from Excel file (complete flow)
   */
  async importEmpleados(filePath: string) {
    await this.selectEmpleadosTab();
    await this.uploadAndImport(filePath);
  }

  /**
   * Extract count from success message
   * E.g., "Se importaron 5 rutinas exitosamente" -> 5
   */
  async getImportedCount(): Promise<number | null> {
    const message = await this.getSuccessMessage();
    if (!message) return null;

    const match = message.match(/(\d+)/);
    return match ? parseInt(match[1], 10) : null;
  }
}
