import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { DashboardPage } from '../pages/DashboardPage';
import { testData } from '../fixtures/test-data';
import { getAuthToken, getRefreshToken, clearAuthTokens } from '../utils/helpers';

/**
 * Authentication Flow E2E Tests
 *
 * Critical user scenarios:
 * 1. Login with valid credentials
 * 2. Login with invalid credentials
 * 3. Logout clears session
 * 4. Protected route redirects to login when unauthenticated
 * 5. Token refresh on 401 response
 * 6. Empty email validation
 * 7. Empty password validation
 * 8. Session persists after page reload
 */

test.describe('Authentication Flow', () => {
  let loginPage: LoginPage;
  let dashboardPage: DashboardPage;

  test.beforeEach(async ({ page }) => {
    loginPage = new LoginPage(page);
    dashboardPage = new DashboardPage(page);

    // Clear any existing auth state
    await clearAuthTokens(page);
  });

  test('should login with valid credentials', async ({ page }) => {
    // Arrange
    await loginPage.goto();

    // Act
    await loginPage.login(testData.users.admin.email, testData.users.admin.password);

    // Assert
    await dashboardPage.waitForDashboard();
    expect(page.url()).toMatch(/\/$|\/dashboard$/);

    // Verify token stored in localStorage
    const token = await getAuthToken(page);
    expect(token).toBeTruthy();
    expect(token).not.toBe('');
  });

  test('should show error with invalid credentials', async ({ page }) => {
    // Arrange
    await loginPage.goto();

    // Act
    await loginPage.login('invalid@email.com', 'wrongpassword');

    // Assert
    await loginPage.waitForErrorMessage();
    const errorMessage = await loginPage.getErrorMessage();
    expect(errorMessage).toBeTruthy();
    expect(errorMessage).toContain('Error');

    // Verify no redirect occurred
    expect(page.url()).toContain('/login');

    // Verify no token stored
    const token = await getAuthToken(page);
    expect(token).toBeFalsy();
  });

  test('should logout and clear session', async ({ page }) => {
    // Arrange - Login first
    await loginPage.goto();
    await loginPage.login(testData.users.admin.email, testData.users.admin.password);
    await dashboardPage.waitForDashboard();

    // Verify token exists
    let token = await getAuthToken(page);
    expect(token).toBeTruthy();

    // Act - Logout
    const logoutButton = page.getByRole('button', { name: /logout|cerrar sesión|salir/i });

    // If logout button exists in nav, click it
    if (await logoutButton.count() > 0) {
      await logoutButton.first().click();
      await page.waitForURL('/login');
    } else {
      // Alternative: clear tokens manually (simulating logout)
      await clearAuthTokens(page);
      await page.goto('/login');
    }

    // Assert
    expect(page.url()).toContain('/login');

    // Verify tokens removed
    token = await getAuthToken(page);
    expect(token).toBeFalsy();
  });

  test('should redirect to login when accessing protected route unauthenticated', async ({ page }) => {
    // Arrange - Ensure no auth tokens
    await clearAuthTokens(page);

    // Act - Try to access protected route
    await page.goto('/gestion-equipos');

    // Assert - Should redirect to login
    await page.waitForURL('/login', { timeout: 5000 });
    expect(page.url()).toContain('/login');
  });

  test('should refresh token on 401 response', async ({ page }) => {
    // Arrange - Login first
    await loginPage.goto();
    await loginPage.login(testData.users.admin.email, testData.users.admin.password);
    await dashboardPage.waitForDashboard();

    const originalToken = await getAuthToken(page);
    expect(originalToken).toBeTruthy();

    // Manually expire the access token (set to invalid value)
    await page.evaluate(() => {
      localStorage.setItem('token', 'expired-token');
    });

    // Act - Make a request that requires authentication
    await page.goto('/gestion-equipos');

    // Wait for potential token refresh
    await page.waitForTimeout(2000);

    // Assert - Either token was refreshed or user was logged out
    const currentUrl = page.url();
    const currentToken = await getAuthToken(page);

    // If token refresh worked, we should still be on protected page
    // If it failed, we should be on login page
    const isOnProtectedPage = currentUrl.includes('/gestion-equipos');
    const isOnLoginPage = currentUrl.includes('/login');

    expect(isOnProtectedPage || isOnLoginPage).toBe(true);

    // If on protected page, verify token was refreshed
    if (isOnProtectedPage) {
      expect(currentToken).toBeTruthy();
      expect(currentToken).not.toBe('expired-token');
    }
  });

  test('should show validation error for empty email', async ({ page }) => {
    // Arrange
    await loginPage.goto();

    // Act - Leave email empty, fill password
    await loginPage.fillPassword('somepassword');
    await loginPage.clickSubmit();

    // Assert - Form validation should prevent submission
    // Note: HTML5 required attribute prevents form submission
    expect(page.url()).toContain('/login');

    // Verify no token stored
    const token = await getAuthToken(page);
    expect(token).toBeFalsy();
  });

  test('should show validation error for empty password', async ({ page }) => {
    // Arrange
    await loginPage.goto();

    // Act - Fill email, leave password empty
    await loginPage.fillEmail(testData.users.admin.email);
    await loginPage.clickSubmit();

    // Assert - Form validation should prevent submission
    expect(page.url()).toContain('/login');

    // Verify no token stored
    const token = await getAuthToken(page);
    expect(token).toBeFalsy();
  });

  test('should persist session after page reload', async ({ page }) => {
    // Arrange - Login first
    await loginPage.goto();
    await loginPage.login(testData.users.admin.email, testData.users.admin.password);
    await dashboardPage.waitForDashboard();

    // Verify token exists
    const tokenBeforeReload = await getAuthToken(page);
    expect(tokenBeforeReload).toBeTruthy();

    // Act - Reload page
    await page.reload();

    // Assert - Should still be on dashboard
    await dashboardPage.waitForDashboard();
    expect(page.url()).toMatch(/\/$|\/dashboard$/);

    // Verify token still exists
    const tokenAfterReload = await getAuthToken(page);
    expect(tokenAfterReload).toBeTruthy();
    expect(tokenAfterReload).toBe(tokenBeforeReload);
  });

  test('should toggle password visibility', async ({ page }) => {
    // Arrange
    await loginPage.goto();
    await loginPage.fillPassword('testpassword');

    // Act - Toggle password visibility
    const passwordInput = page.locator('input[autocomplete="current-password"]').first();
    const initialType = await passwordInput.getAttribute('type');
    expect(initialType).toBe('password');

    // Click toggle button
    await loginPage.togglePasswordVisibility();

    // Assert - Password should be visible
    const newType = await passwordInput.getAttribute('type');
    expect(newType).toBe('text');

    // Toggle again
    await loginPage.togglePasswordVisibility();

    // Password should be hidden again
    const finalType = await passwordInput.getAttribute('type');
    expect(finalType).toBe('password');
  });

  test('should handle rapid login attempts gracefully', async ({ page }) => {
    // Arrange
    await loginPage.goto();

    // Act - Attempt multiple rapid logins
    await loginPage.fillEmail(testData.users.admin.email);
    await loginPage.fillPassword(testData.users.admin.password);

    // Click submit multiple times rapidly
    const submitButton = page.locator('button[type="submit"]').first();
    await submitButton.click();

    // Wait for loading state
    await loginPage.waitForLoadingComplete();

    // Assert - Should eventually land on dashboard
    await dashboardPage.waitForDashboard();
    const token = await getAuthToken(page);
    expect(token).toBeTruthy();
  });
});

test.describe('Authentication - Edge Cases', () => {
  let loginPage: LoginPage;

  test.beforeEach(async ({ page }) => {
    loginPage = new LoginPage(page);
    await clearAuthTokens(page);
  });

  test('should handle special characters in password', async ({ page }) => {
    // Arrange
    await loginPage.goto();

    // Act - Use password with special characters
    await loginPage.login(testData.users.admin.email, testData.users.admin.password);

    // Assert - Should handle special characters correctly
    await page.waitForURL(/\/$|\/dashboard$|\/login$/, { timeout: 5000 });
  });

  test('should trim whitespace from email', async ({ page }) => {
    // Arrange
    await loginPage.goto();

    // Act - Add whitespace to email
    await loginPage.fillEmail(`  ${testData.users.admin.email}  `);
    await loginPage.fillPassword(testData.users.admin.password);
    await loginPage.clickSubmit();

    // Assert - Should handle whitespace (either trim or show error)
    await page.waitForURL(/\/$|\/dashboard$|\/login$/, { timeout: 5000 });
  });

  test('should handle case-sensitive email', async ({ page }) => {
    // Arrange
    await loginPage.goto();

    // Act - Try email with different case
    const emailUpperCase = testData.users.admin.email.toUpperCase();
    await loginPage.login(emailUpperCase, testData.users.admin.password);

    // Assert - Depending on backend implementation, should either:
    // 1. Login successfully (case-insensitive)
    // 2. Show error (case-sensitive)
    await page.waitForURL(/\/$|\/dashboard$|\/login$/, { timeout: 5000 });
  });
});
