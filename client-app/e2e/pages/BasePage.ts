import { Page, Locator } from '@playwright/test';

/**
 * Base Page Object Model
 *
 * All page objects should extend this class to inherit common functionality:
 * - Navigation
 * - Waiting for elements
 * - Common interactions
 */
export class BasePage {
  constructor(protected page: Page) {}

  /**
   * Navigate to a specific path (retries on Firefox NS_BINDING_ABORTED)
   */
  async goto(path: string) {
    try {
      await this.page.goto(path);
    } catch (error: any) {
      if (error.message?.includes('NS_BINDING_ABORTED')) {
        await this.page.waitForTimeout(500);
        await this.page.goto(path);
      } else {
        throw error;
      }
    }
  }

  /**
   * Wait for navigation to a specific URL
   */
  async waitForNavigation(url: string | RegExp) {
    await this.page.waitForURL(url);
  }

  /**
   * Wait for element to be visible
   */
  async waitForElement(selector: string) {
    await this.page.waitForSelector(selector, { state: 'visible' });
  }

  /**
   * Wait for element to be hidden
   */
  async waitForElementHidden(selector: string) {
    await this.page.waitForSelector(selector, { state: 'hidden' });
  }

  /**
   * Click element with automatic waiting
   */
  async clickElement(selector: string) {
    await this.page.click(selector);
  }

  /**
   * Fill input with automatic waiting
   */
  async fillInput(selector: string, value: string) {
    await this.page.fill(selector, value);
  }

  /**
   * Get text content from element
   */
  async getTextContent(selector: string): Promise<string | null> {
    return await this.page.textContent(selector);
  }

  /**
   * Check if element is visible
   */
  async isVisible(selector: string): Promise<boolean> {
    return await this.page.isVisible(selector);
  }

  /**
   * Get element by role
   */
  getByRole(
    role: 'button' | 'link' | 'textbox' | 'heading' | 'listitem' | 'cell',
    options?: { name?: string | RegExp }
  ): Locator {
    return this.page.getByRole(role, options);
  }

  /**
   * Get element by label text
   */
  getByLabel(text: string | RegExp): Locator {
    return this.page.getByLabel(text);
  }

  /**
   * Get element by placeholder text
   */
  getByPlaceholder(text: string | RegExp): Locator {
    return this.page.getByPlaceholder(text);
  }

  /**
   * Get element by text content
   */
  getByText(text: string | RegExp): Locator {
    return this.page.getByText(text);
  }

  /**
   * Wait for API response
   */
  async waitForResponse(urlPattern: string | RegExp) {
    return await this.page.waitForResponse(urlPattern);
  }

  /**
   * Wait for specific time (use sparingly, prefer waitForElement)
   */
  async wait(milliseconds: number) {
    await this.page.waitForTimeout(milliseconds);
  }

  /**
   * Reload the current page
   */
  async reload() {
    await this.page.reload();
  }

  /**
   * Get current URL
   */
  getCurrentUrl(): string {
    return this.page.url();
  }

  /**
   * Take a screenshot (useful for debugging)
   */
  async screenshot(path: string) {
    await this.page.screenshot({ path });
  }
}
