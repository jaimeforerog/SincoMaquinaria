import { chromium, FullConfig } from '@playwright/test';
import { testData } from './fixtures/test-data';
import { URLS } from './e2e.config';

/**
 * Global setup - runs ONCE before all tests
 * Creates the admin user in the database
 */
async function globalSetup(config: FullConfig) {
  console.log('🔧 Global Setup: Creating admin user...');

  const browser = await chromium.launch();
  const page = await browser.newPage({
    baseURL: URLS.backend, // Use backend URL directly
  });

  try {
    // Try to create admin user via /auth/setup endpoint
    const response = await page.request.post(`${URLS.backend}/auth/setup`, {
      data: {
        Nombre: testData.users.admin.nombre,
        Email: testData.users.admin.email,
        Password: testData.users.admin.password,
      },
      headers: {
        'Content-Type': 'application/json',
      },
      failOnStatusCode: false,
    });

    if (response.ok()) {
      console.log('✅ Admin user created successfully');
    } else if (response.status() === 400) {
      console.log('ℹ️  Admin user already exists (this is OK)');
    } else {
      console.error(`❌ Unexpected response from /auth/setup: ${response.status()}`);
      const text = await response.text();
      console.error(`Response: ${text}`);
    }
  } catch (error) {
    console.error('❌ Error in global setup:', error);
    throw error;
  } finally {
    await browser.close();
  }

  console.log('✅ Global setup completed');
}

export default globalSetup;
