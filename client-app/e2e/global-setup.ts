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
    // Retry admin user creation - DB schema may not be ready immediately
    let created = false;
    for (let attempt = 1; attempt <= 5; attempt++) {
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
        created = true;
        break;
      } else if (response.status() === 400) {
        console.log('ℹ️  Admin user already exists (this is OK)');
        created = true;
        break;
      } else {
        console.warn(`⚠️ Attempt ${attempt}/5: /auth/setup returned ${response.status()}, retrying in 2s...`);
        await new Promise(r => setTimeout(r, 2000));
      }
    }

    if (!created) {
      console.error('❌ Failed to create admin user after 5 attempts');
    }

    // Warm up Marten schemas by touching key endpoints
    // This prevents 500 errors during early test runs
    try {
      // Login to get auth token
      const loginRes = await page.request.post(`${URLS.backend}/auth/login`, {
        data: { Email: testData.users.admin.email, Password: testData.users.admin.password },
        headers: { 'Content-Type': 'application/json' },
        failOnStatusCode: false,
      });
      if (loginRes.ok()) {
        const loginData = await loginRes.json();
        const token = loginData.token;
        const headers = { 'Authorization': `Bearer ${token}` };

        // Touch ConfiguracionGlobal schema (grupos, medidores, fallas, causas)
        for (let i = 1; i <= 3; i++) {
          const res = await page.request.get(`${URLS.backend}/configuracion/grupos`, {
            headers, failOnStatusCode: false,
          });
          if (res.ok() || res.status() !== 500) break;
          console.log(`⏳ Warming up ConfiguracionGlobal schema (attempt ${i}/3)...`);
          await new Promise(r => setTimeout(r, 1000));
        }

        // Touch other schemas
        await page.request.get(`${URLS.backend}/equipos`, { headers, failOnStatusCode: false });
        await page.request.get(`${URLS.backend}/rutinas`, { headers, failOnStatusCode: false });
        console.log('✅ Schema warmup completed');
      }
    } catch {
      console.warn('⚠️ Schema warmup failed (non-critical)');
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
