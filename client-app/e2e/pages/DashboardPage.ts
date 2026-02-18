import { Page } from '@playwright/test';
import { BasePage } from './BasePage';
import { TIMEOUTS } from '../e2e.config';

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
   * Read KPI value from a card, waiting for the h3 element to appear
   */
  private async readKpiValue(href: string): Promise<number> {
    const card = this.page.locator(`a[href="${href}"]`).first();
    const valueElement = card.locator('h3').first();
    try {
      await valueElement.waitFor({ state: 'visible', timeout: 15000 });
      const text = await valueElement.textContent();
      return parseInt(text || '0', 10);
    } catch {
      return 0;
    }
  }

  /**
   * Get equipos count from KPI card
   */
  async getEquiposCount(): Promise<number> {
    await this.waitForLoadingComplete();
    return this.readKpiValue('/gestion-equipos');
  }

  /**
   * Get rutinas count from KPI card
   */
  async getRutinasCount(): Promise<number> {
    await this.waitForLoadingComplete();
    return this.readKpiValue('/editar-rutinas');
  }

  /**
   * Get ordenes activas count from KPI card
   */
  async getOrdenesActivasCount(): Promise<number> {
    await this.waitForLoadingComplete();
    return this.readKpiValue('/historial');
  }

  /**
   * Wait for loading spinner to disappear
   */
  async waitForLoadingComplete() {
    try {
      // Wait for first spinner to appear
      await this.page.locator(this.loadingSpinner).first().waitFor({ state: 'visible', timeout: TIMEOUTS.short });
      // Wait for ALL spinners to disappear (dashboard has multiple KPI spinners)
      await this.page.waitForFunction(
        () => document.querySelectorAll('[role="progressbar"]').length === 0,
        { timeout: TIMEOUTS.pageLoad }
      );
    } catch {
      // Spinners might not appear if data loads quickly
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

    // Read all KPI values directly (loading already complete)
    const [equipos, rutinas, ordenesActivas] = await Promise.all([
      this.readKpiValue('/gestion-equipos'),
      this.readKpiValue('/editar-rutinas'),
      this.readKpiValue('/historial'),
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

      await this.wait(TIMEOUTS.short / 4); // Wait before checking again
    }

    throw new Error(`KPI ${kpi} did not update to ${expectedValue} within ${timeout}ms`);
  }
}
