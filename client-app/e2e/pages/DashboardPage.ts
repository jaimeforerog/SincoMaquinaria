import { Page } from '@playwright/test';
import { BasePage } from './BasePage';

/**
 * Page Object Model for Dashboard page
 *
 * Handles all interactions with the dashboard including:
 * - Reading KPI values
 * - Navigating to different sections
 * - Waiting for real-time updates
 */
export class DashboardPage extends BasePage {
  // Selectors
  private readonly dashboardHeading = 'text=Dashboard Operativo';
  private readonly equiposCard = 'text=Equipos';
  private readonly rutinasCard = 'text=Rutinas';
  private readonly ordenesActivasCard = 'text=Órdenes Activas';
  private readonly loadingSpinner = '[role="progressbar"]';

  constructor(page: Page) {
    super(page);
  }

  /**
   * Navigate to dashboard page
   */
  async goto() {
    await super.goto('/');
    await this.waitForDashboard();
  }

  /**
   * Wait for dashboard to load
   */
  async waitForDashboard() {
    await this.waitForElement(this.dashboardHeading);
  }

  /**
   * Get equipos count from KPI card
   */
  async getEquiposCount(): Promise<number> {
    // Wait for loading to complete
    await this.waitForLoadingComplete();

    // Find the equipos card and extract the number
    const card = this.page.locator('a[href="/gestion-equipos"]').first();
    const valueElement = card.locator('h3').first();
    const text = await valueElement.textContent();
    return parseInt(text || '0', 10);
  }

  /**
   * Get rutinas count from KPI card
   */
  async getRutinasCount(): Promise<number> {
    // Wait for loading to complete
    await this.waitForLoadingComplete();

    // Find the rutinas card and extract the number
    const card = this.page.locator('a[href="/editar-rutinas"]').first();
    const valueElement = card.locator('h3').first();
    const text = await valueElement.textContent();
    return parseInt(text || '0', 10);
  }

  /**
   * Get ordenes activas count from KPI card
   */
  async getOrdenesActivasCount(): Promise<number> {
    // Wait for loading to complete
    await this.waitForLoadingComplete();

    // Find the ordenes activas card and extract the number
    const card = this.page.locator('a[href="/historial"]').first();
    const valueElement = card.locator('h3').first();
    const text = await valueElement.textContent();
    return parseInt(text || '0', 10);
  }

  /**
   * Wait for loading spinner to disappear
   */
  async waitForLoadingComplete() {
    const spinner = this.page.locator(this.loadingSpinner);
    // Wait for the spinner to appear and then disappear
    try {
      await spinner.waitFor({ state: 'visible', timeout: 1000 });
      await spinner.waitFor({ state: 'hidden', timeout: 10000 });
    } catch {
      // Spinner might not appear if data loads quickly
    }
  }

  /**
   * Click on Equipos card to navigate
   */
  async navigateToEquipos() {
    await this.page.locator('a[href="/gestion-equipos"]').first().click();
    await this.waitForNavigation('/gestion-equipos');
  }

  /**
   * Click on Rutinas card to navigate
   */
  async navigateToRutinas() {
    await this.page.locator('a[href="/editar-rutinas"]').first().click();
    await this.waitForNavigation('/editar-rutinas');
  }

  /**
   * Click on Órdenes Activas card to navigate
   */
  async navigateToHistorial() {
    await this.page.locator('a[href="/historial"]').first().click();
    await this.waitForNavigation('/historial');
  }

  /**
   * Check if dashboard is loaded
   */
  async isDashboardLoaded(): Promise<boolean> {
    return await this.isVisible(this.dashboardHeading);
  }

  /**
   * Get all KPI values at once
   */
  async getAllKPIs(): Promise<{
    equipos: number;
    rutinas: number;
    ordenesActivas: number;
  }> {
    await this.waitForLoadingComplete();

    const [equipos, rutinas, ordenesActivas] = await Promise.all([
      this.getEquiposCount(),
      this.getRutinasCount(),
      this.getOrdenesActivasCount(),
    ]);

    return { equipos, rutinas, ordenesActivas };
  }

  /**
   * Wait for KPI update (useful for testing real-time updates)
   */
  async waitForKPIUpdate(kpi: 'equipos' | 'rutinas' | 'ordenesActivas', expectedValue: number, timeout = 10000) {
    const startTime = Date.now();

    while (Date.now() - startTime < timeout) {
      let currentValue: number;

      if (kpi === 'equipos') {
        currentValue = await this.getEquiposCount();
      } else if (kpi === 'rutinas') {
        currentValue = await this.getRutinasCount();
      } else {
        currentValue = await this.getOrdenesActivasCount();
      }

      if (currentValue === expectedValue) {
        return true;
      }

      await this.wait(500); // Wait 500ms before checking again
    }

    throw new Error(`KPI ${kpi} did not update to ${expectedValue} within ${timeout}ms`);
  }
}
