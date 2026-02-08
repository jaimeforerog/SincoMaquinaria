import { Page } from '@playwright/test';
import { BasePage } from './BasePage';

/**
 * Page Object Model for Create Order page
 *
 * Handles all interactions with the order creation form including:
 * - Selecting equipment
 * - Selecting order type (Preventivo/Correctivo)
 * - Selecting rutina for preventive orders
 * - Selecting frequency
 * - Submitting the form
 */
export class CreateOrderPage extends BasePage {
  // Selectors
  private readonly pageHeading = 'text=Nueva Orden de Trabajo';
  private readonly equipoAutocomplete = '#equipoId-autocomplete, [role="combobox"]';
  private readonly tipoSelect = '#tipo, select[name="tipo"]';
  private readonly rutinaSelect = '#rutinaId, select[name="rutinaId"]';
  private readonly frecuenciaSelect = '#frecuencia, select[name="frecuencia"]';
  private readonly numeroInput = 'input[name="numero"]';
  private readonly submitButton = 'button[type="submit"], button:has-text("Guardar")';
  private readonly loadingSpinner = '[role="progressbar"]';
  private readonly successMessage = '[role="alert"]';

  constructor(page: Page) {
    super(page);
  }

  /**
   * Navigate to create order page
   */
  async goto() {
    await super.goto('/nueva-orden');
    await this.waitForPageLoad();
  }

  /**
   * Wait for page to load
   */
  async waitForPageLoad() {
    // Wait for either the heading or the form elements to be visible
    await this.page.waitForLoadState('networkidle');
  }

  /**
   * Select equipment from autocomplete
   */
  async selectEquipo(placa: string) {
    // Click on the autocomplete input
    const autocomplete = this.page.locator('input').filter({ hasText: '' }).or(
      this.page.locator('input[placeholder*="equipo"], input[placeholder*="Equipo"]')
    ).first();

    await autocomplete.click();
    await autocomplete.fill(placa);

    // Wait for dropdown options to appear
    await this.wait(500);

    // Click on the option that matches the placa
    await this.page.locator(`[role="option"]:has-text("${placa}")`).first().click();
  }

  /**
   * Select order type (Preventivo or Correctivo)
   */
  async selectOrderType(tipo: 'Preventivo' | 'Correctivo') {
    const select = this.page.locator(this.tipoSelect).first();
    await select.selectOption(tipo);
  }

  /**
   * Select rutina for preventive maintenance
   */
  async selectRutina(rutinaName: string) {
    const select = this.page.locator(this.rutinaSelect).first();

    // Wait for rutinas to load
    await this.wait(500);

    // Select by visible text
    await select.selectOption({ label: rutinaName });
  }

  /**
   * Select frequency for preventive maintenance
   */
  async selectFrequency(frecuencia: string) {
    const select = this.page.locator(this.frecuenciaSelect).first();
    await select.selectOption(frecuencia);
  }

  /**
   * Fill order number
   */
  async fillNumero(numero: string) {
    const input = this.page.locator(this.numeroInput).first();
    await input.clear();
    await input.fill(numero);
  }

  /**
   * Submit the order form
   */
  async submit() {
    const button = this.page.locator(this.submitButton).first();
    await button.click();
  }

  /**
   * Create a preventive maintenance order (full flow)
   */
  async createPreventiveOrder(
    placa: string,
    rutinaName: string,
    frecuencia: string
  ) {
    await this.selectEquipo(placa);
    await this.selectOrderType('Preventivo');
    await this.selectRutina(rutinaName);
    await this.selectFrequency(frecuencia);
    await this.submit();
  }

  /**
   * Create a corrective maintenance order (full flow)
   */
  async createCorrectiveOrder(placa: string) {
    await this.selectEquipo(placa);
    await this.selectOrderType('Correctivo');
    await this.submit();
  }

  /**
   * Wait for success message
   */
  async waitForSuccessMessage() {
    await this.waitForElement(this.successMessage);
  }

  /**
   * Get success message text
   */
  async getSuccessMessage(): Promise<string | null> {
    const message = this.page.locator(this.successMessage).first();
    if (await message.isVisible()) {
      return await message.textContent();
    }
    return null;
  }

  /**
   * Get validation errors
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
   * Check if submit button is disabled
   */
  async isSubmitDisabled(): Promise<boolean> {
    const button = this.page.locator(this.submitButton).first();
    return await button.isDisabled();
  }

  /**
   * Wait for navigation after successful order creation
   */
  async waitForRedirect() {
    // After creating order, it usually redirects to order detail or history
    await this.page.waitForURL(/\/(orden|historial)/);
  }

  /**
   * Check if loading spinner is visible
   */
  async isLoading(): Promise<boolean> {
    const spinner = this.page.locator(this.loadingSpinner);
    return await spinner.isVisible();
  }

  /**
   * Wait for loading to complete
   */
  async waitForLoadingComplete() {
    const spinner = this.page.locator(this.loadingSpinner);
    if (await spinner.isVisible()) {
      await spinner.waitFor({ state: 'hidden', timeout: 10000 });
    }
  }

  /**
   * Get current order number value
   */
  async getNumeroValue(): Promise<string> {
    const input = this.page.locator(this.numeroInput).first();
    return await input.inputValue();
  }

  /**
   * Check if rutina select is visible (only for Preventivo)
   */
  async isRutinaSelectVisible(): Promise<boolean> {
    return await this.isVisible(this.rutinaSelect);
  }

  /**
   * Check if frequency select is visible (only for Preventivo)
   */
  async isFrecuenciaSelectVisible(): Promise<boolean> {
    return await this.isVisible(this.frecuenciaSelect);
  }
}
