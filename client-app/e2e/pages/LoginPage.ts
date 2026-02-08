import { Page } from '@playwright/test';
import { BasePage } from './BasePage';

/**
 * Page Object Model for Login page
 *
 * Handles all interactions with the login page including:
 * - Navigating to login
 * - Entering credentials
 * - Submitting form
 * - Reading error messages
 */
export class LoginPage extends BasePage {
  // Selectors
  private readonly emailInput = 'input[name="email"], input[autocomplete="username"]';
  private readonly passwordInput = 'input[name="password"], input[autocomplete="current-password"]';
  private readonly submitButton = 'button[type="submit"]';
  private readonly errorAlert = '[role="alert"]';
  private readonly loadingSpinner = '[role="progressbar"]';

  constructor(page: Page) {
    super(page);
  }

  /**
   * Navigate to login page
   */
  async goto() {
    await super.goto('/login');
    await this.waitForElement(this.emailInput);
  }

  /**
   * Fill email field
   */
  async fillEmail(email: string) {
    const input = this.page.locator(this.emailInput).first();
    await input.fill(email);
  }

  /**
   * Fill password field
   */
  async fillPassword(password: string) {
    const input = this.page.locator(this.passwordInput).first();
    await input.fill(password);
  }

  /**
   * Click submit button
   */
  async clickSubmit() {
    const button = this.page.locator(this.submitButton).first();
    await button.click();
  }

  /**
   * Perform login with credentials
   */
  async login(email: string, password: string) {
    await this.fillEmail(email);
    await this.fillPassword(password);
    await this.clickSubmit();
  }

  /**
   * Get error message text
   */
  async getErrorMessage(): Promise<string | null> {
    const alert = this.page.locator(this.errorAlert).first();
    if (await alert.isVisible()) {
      return await alert.textContent();
    }
    return null;
  }

  /**
   * Wait for error message to appear
   */
  async waitForErrorMessage() {
    await this.waitForElement(this.errorAlert);
  }

  /**
   * Wait for redirect to dashboard after successful login
   */
  async waitForDashboard() {
    await this.waitForNavigation(/^.*\/$|^.*\/dashboard$/);
  }

  /**
   * Check if loading spinner is visible
   */
  async isLoading(): Promise<boolean> {
    return await this.isVisible(this.loadingSpinner);
  }

  /**
   * Wait for loading to complete
   */
  async waitForLoadingComplete() {
    const spinner = this.page.locator(this.loadingSpinner);
    if (await spinner.isVisible()) {
      await spinner.waitFor({ state: 'hidden' });
    }
  }

  /**
   * Toggle password visibility
   */
  async togglePasswordVisibility() {
    await this.page.getByLabel('toggle password visibility').click();
  }

  /**
   * Check if submit button is disabled
   */
  async isSubmitButtonDisabled(): Promise<boolean> {
    const button = this.page.locator(this.submitButton).first();
    return await button.isDisabled();
  }

  /**
   * Get the value of email input
   */
  async getEmailValue(): Promise<string> {
    const input = this.page.locator(this.emailInput).first();
    return await input.inputValue();
  }

  /**
   * Get the value of password input
   */
  async getPasswordValue(): Promise<string> {
    const input = this.page.locator(this.passwordInput).first();
    return await input.inputValue();
  }

  /**
   * Clear email field
   */
  async clearEmail() {
    const input = this.page.locator(this.emailInput).first();
    await input.clear();
  }

  /**
   * Clear password field
   */
  async clearPassword() {
    const input = this.page.locator(this.passwordInput).first();
    await input.clear();
  }
}
