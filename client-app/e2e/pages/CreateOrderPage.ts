import { Page } from '@playwright/test';
import { BasePage } from './BasePage';
import { TIMEOUTS } from '../e2e.config';

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
  private readonly equipoAutocomplete = 'input[aria-label="Buscar equipo por placa o descripción..."]';
  private readonly submitButton = 'button:has-text("Crear Orden")';
  private readonly loadingButton = 'button:has-text("Guardando...")';
  private readonly loadingSpinner = '[role="progressbar"]';

  constructor(page: Page) {
    super(page);
  }

  /**
   * Navigate to create order page
   */
  async goto() {
    await super.goto('/nueva-orden');
    await this.waitForElement(this.pageHeading);
  }

  /**
   * Select equipment from autocomplete
   */
  async selectEquipo(placaOrDesc: string) {
    // Click autocomplete to open it
    await this.page.click(this.equipoAutocomplete);

    // Type to search
    await this.page.fill(this.equipoAutocomplete, placaOrDesc);

    // Wait for dropdown to appear
    await this.wait(TIMEOUTS.short);

    // Click the option that contains the text
    await this.page.click(`li:has-text("${placaOrDesc}")`);

    // Wait for autocomplete to close
    await this.wait(TIMEOUTS.short / 2);
  }

  /**
   * Select order type (Correctivo or Preventivo)
   */
  async selectOrderType(tipo: 'Correctivo' | 'Preventivo') {
    // Open the select (Material-UI)
    await this.page.click('label:has-text("Tipo de Orden") + div');

    // Wait for dropdown
    await this.wait(TIMEOUTS.short / 2);

    // Click the option
    await this.page.click(`li[role="option"]:has-text("${tipo}")`);
  }

  /**
   * Select maintenance routine (only for Preventivo orders)
   */
  async selectRutina(rutinaDesc: string) {
    // Open the select
    await this.page.click('label:has-text("Rutina Sugerida") + div');

    // Wait for dropdown
    await this.wait(TIMEOUTS.short / 2);

    // Click the option that contains the description
    await this.page.click(`li[role="option"]:has-text("${rutinaDesc}")`);
  }

  /**
   * Select frequency (only for Preventivo orders)
   */
  async selectFrecuencia(frecuenciaHoras: number) {
    // Open the select
    await this.page.click('label:has-text("Frecuencia Mantenimiento") + div');

    // Wait for dropdown
    await this.wait(TIMEOUTS.short / 2);

    // Click the option with the frequency
    await this.page.click(`li[role="option"]:has-text("${frecuenciaHoras} horas")`);
  }

  /**
   * Set order date
   */
  async setFecha(fecha: string) {
    await this.page.fill('input[type="date"]', fecha);
  }

  /**
   * Click submit button to create order
   */
  async submit() {
    // Wait for the POST /ordenes request
    const responsePromise = this.page.waitForResponse(
      response => response.url().includes('/ordenes') && response.request().method() === 'POST',
      { timeout: TIMEOUTS.apiResponse }
    );

    await this.clickElement(this.submitButton);

    // Wait for response
    const response = await responsePromise;

    return response;
  }

  /**
   * Complete flow: Create preventive maintenance order
   */
  async createPreventiveOrder(params: {
    equipo: string;
    rutina?: string;
    frecuencia?: number;
    fecha?: string;
  }) {
    await this.selectEquipo(params.equipo);
    await this.selectOrderType('Preventivo');

    if (params.rutina) {
      await this.selectRutina(params.rutina);
    }

    if (params.frecuencia) {
      await this.selectFrecuencia(params.frecuencia);
    }

    if (params.fecha) {
      await this.setFecha(params.fecha);
    }

    return await this.submit();
  }

  /**
   * Complete flow: Create corrective maintenance order
   */
  async createCorrectiveOrder(params: {
    equipo: string;
    fecha?: string;
  }) {
    await this.selectEquipo(params.equipo);
    await this.selectOrderType('Correctivo');

    if (params.fecha) {
      await this.setFecha(params.fecha);
    }

    return await this.submit();
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
   * Check if loading
   */
  async isLoading(): Promise<boolean> {
    return await this.isVisible(this.loadingButton);
  }

  /**
   * Wait for redirect to order detail page
   */
  async waitForRedirectToOrderDetail() {
    await this.page.waitForURL(/\/ordenes\/[a-f0-9-]+/, { timeout: TIMEOUTS.navigation });
  }

  /**
   * Get created order ID from URL after redirect
   */
  async getCreatedOrderId(): Promise<string> {
    const url = this.page.url();
    const match = url.match(/\/ordenes\/([a-f0-9-]+)/);
    return match ? match[1] : '';
  }

  /**
   * Check if rutina select is visible (only for Preventivo)
   */
  async isRutinaSelectVisible(): Promise<boolean> {
    return await this.isVisible('label:has-text("Rutina Sugerida")');
  }

  /**
   * Check if frecuencia select is visible (only for Preventivo)
   */
  async isFrecuenciaSelectVisible(): Promise<boolean> {
    return await this.isVisible('label:has-text("Frecuencia Mantenimiento")');
  }

  /**
   * Check if rutina select is disabled
   */
  async isRutinaSelectDisabled(): Promise<boolean> {
    const select = this.page.locator('label:has-text("Rutina Sugerida") + div input');
    return await select.isDisabled();
  }

  /**
   * Get helper text from a field
   */
  async getHelperText(fieldLabel: string): Promise<string | null> {
    const helperText = this.page.locator(`label:has-text("${fieldLabel}") ~ p`);
    if (await helperText.isVisible()) {
      return await helperText.textContent();
    }
    return null;
  }
}
