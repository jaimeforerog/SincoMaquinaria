# E2E Testing Documentation - SincoMaquinaria

## Overview

This directory contains end-to-end (E2E) tests for the SincoMaquinaria application using Playwright. E2E tests verify critical user workflows from the browser perspective, ensuring the entire application works correctly from frontend to backend.

## Why E2E Testing?

E2E tests provide the highest confidence that the application works correctly:
- **Catch integration issues** between frontend and backend
- **Verify real user workflows** end-to-end
- **Prevent deployment failures** by testing in production-like environment
- **Complement unit tests** by testing the full stack together

## Test Coverage

### 5 Critical User Flows (38+ Tests)

1. **Authentication Flow** (`tests/auth.spec.ts`) - 12 tests
   - Login with valid/invalid credentials
   - Logout and session management
   - Token refresh on 401
   - Protected route access
   - Form validation

2. **Order Creation & Management** (`tests/order-creation.spec.ts`) - 10 tests
   - Create preventive/corrective orders
   - Form validation
   - Activity progress tracking
   - Order deletion

3. **Equipment Management** (`tests/equipment.spec.ts`) - 8 tests
   - CRUD operations on equipment
   - Search and filtering
   - Form validation
   - Duplicate prevention

4. **Excel Import** (`tests/excel-import.spec.ts`) - 6 tests
   - Import rutinas/equipos from Excel
   - Validation of file formats
   - Error handling

5. **Dashboard** (`tests/dashboard.spec.ts`) - 5 tests
   - KPI display
   - Real-time updates via WebSocket
   - Navigation to different sections

## Project Structure

```
e2e/
├── fixtures/               # Test data and setup utilities
│   ├── test-data.ts       # Reusable test data (users, equipos, rutinas)
│   └── setup-test-data.ts # Database population scripts
├── pages/                 # Page Object Models (POM)
│   ├── BasePage.ts       # Base class with common functionality
│   ├── LoginPage.ts      # Login page interactions
│   ├── DashboardPage.ts  # Dashboard page interactions
│   ├── CreateOrderPage.ts
│   ├── EquipmentConfigPage.ts
│   └── ImportPage.ts
├── tests/                 # Test specifications
│   ├── auth.spec.ts
│   ├── order-creation.spec.ts
│   ├── equipment.spec.ts
│   ├── excel-import.spec.ts
│   └── dashboard.spec.ts
├── utils/                 # Helper utilities
│   └── helpers.ts        # Login, logout, cleanup, etc.
└── README.md             # This file
```

## Running Tests

### Prerequisites

1. **Install dependencies:**
   ```bash
   cd client-app
   npm install
   ```

2. **Install Playwright browsers:**
   ```bash
   npx playwright install chromium firefox
   ```

3. **Start the backend server:**
   ```bash
   # In a separate terminal
   cd ..
   dotnet run --project src/SincoMaquinaria/SincoMaquinaria.csproj
   ```

### Run Tests

```bash
# Run all E2E tests (headless)
npm run test:e2e

# Run tests in headed mode (see browser)
npm run test:e2e:headed

# Run tests with UI mode (interactive)
npm run test:e2e:ui

# Run tests in debug mode
npm run test:e2e:debug

# Run specific test file
npx playwright test tests/auth.spec.ts

# Run tests in specific browser
npx playwright test --project=chromium
npx playwright test --project=firefox

# Run tests with specific tag
npx playwright test --grep @critical
```

### View Test Results

```bash
# Open HTML report
npx playwright show-report

# The report includes:
# - Test results with pass/fail status
# - Screenshots on failure
# - Videos of failed tests
# - Trace viewer for debugging
```

## Writing Tests

### Page Object Model (POM) Pattern

We use the Page Object Model pattern to keep tests maintainable:

```typescript
// Good - Using POM
test('should login successfully', async ({ page }) => {
  const loginPage = new LoginPage(page);
  await loginPage.goto();
  await loginPage.login('user@test.com', 'password');
  await loginPage.waitForDashboard();
});

// Bad - Direct selectors in test
test('should login successfully', async ({ page }) => {
  await page.goto('/login');
  await page.fill('input[name="email"]', 'user@test.com');
  await page.fill('input[name="password"]', 'password');
  await page.click('button[type="submit"]');
});
```

### Test Data Management

Use fixtures for reusable test data:

```typescript
import { testData, generateUniqueEquipo } from '../fixtures/test-data';

test('should create equipment', async ({ page }) => {
  const uniqueEquipo = generateUniqueEquipo(testData.equipos[0]);
  // Use uniqueEquipo.placa, uniqueEquipo.descripcion, etc.
});
```

### Authentication Helper

Use the helper function for authentication:

```typescript
import { loginAsAdmin } from '../utils/helpers';

test.beforeEach(async ({ page }) => {
  await loginAsAdmin(page);
  // Now you're authenticated and can access protected routes
});
```

### Cleanup

Always cleanup test data after tests:

```typescript
test.afterAll(async ({ browser }) => {
  const context = await browser.newContext();
  const page = await context.newPage();
  await loginAsAdmin(page);
  await cleanupAllTestData(page);
  await context.close();
});
```

## Debugging Tests

### 1. Use Headed Mode

```bash
npm run test:e2e:headed
```

This shows the browser so you can see what's happening.

### 2. Use Debug Mode

```bash
npm run test:e2e:debug
```

This opens Playwright Inspector for step-by-step debugging.

### 3. Use UI Mode

```bash
npm run test:e2e:ui
```

This opens Playwright's interactive UI for running and debugging tests.

### 4. Add Screenshots

```typescript
await page.screenshot({ path: 'debug-screenshot.png' });
```

### 5. Add Console Logs

```typescript
console.log('Current URL:', page.url());
console.log('Token:', await getAuthToken(page));
```

### 6. Use Trace Viewer

When a test fails, Playwright automatically generates a trace. View it with:

```bash
npx playwright show-trace test-results/.../trace.zip
```

## CI/CD Integration

E2E tests run automatically in GitHub Actions on every push:

1. **Backend tests** run first
2. **Frontend tests** run in parallel
3. **E2E tests** run after both pass
4. **Deploy** only happens if all tests pass

### GitHub Actions Workflow

The E2E tests job:
- Starts PostgreSQL database
- Builds and runs the backend
- Installs Playwright browsers
- Runs E2E tests
- Uploads test results and videos as artifacts

### Viewing Results in CI

- Go to GitHub Actions tab
- Click on the workflow run
- Download "playwright-report" artifact
- Open `index.html` to view the report

## Best Practices

### ✅ DO

- Use Page Object Models for maintainability
- Use unique test data (E2E- prefix) to avoid conflicts
- Cleanup test data after tests
- Use `loginAsAdmin()` helper for authentication
- Use `generateUniqueEquipo()` for unique test data
- Wait for elements with Playwright's auto-waiting
- Take screenshots on failure (automatic)
- Use descriptive test names

### ❌ DON'T

- Hardcode timeouts (use `waitForElement` instead)
- Use `page.waitForTimeout()` unless absolutely necessary
- Create test data without cleanup
- Use production data in tests
- Share state between tests
- Test implementation details (test user workflows)
- Skip tests without good reason

## Troubleshooting

### Tests fail with "Timeout waiting for element"

**Cause:** Element selector is wrong or page is slow to load

**Solution:**
1. Check the selector in browser DevTools
2. Increase timeout if page is legitimately slow
3. Wait for network requests to complete
4. Use Playwright Inspector to debug

### Tests fail with "Authentication failed"

**Cause:** Test user doesn't exist or credentials are wrong

**Solution:**
1. Verify test user exists in database
2. Check `testData.users.admin` credentials
3. Ensure backend is running
4. Check API endpoints are accessible

### Tests are flaky (sometimes pass, sometimes fail)

**Cause:** Race conditions or timing issues

**Solution:**
1. Use Playwright's auto-waiting (don't add manual waits)
2. Wait for API responses with `waitForResponse()`
3. Enable retries in CI (already configured)
4. Check for real race conditions in application code

### Backend is not running

**Cause:** Backend server is not started before tests

**Solution:**
1. Start backend manually: `dotnet run --project src/SincoMaquinaria/SincoMaquinaria.csproj`
2. Or configure `webServer.command` in `playwright.config.ts`

### Browsers not installed

**Cause:** Playwright browsers not installed

**Solution:**
```bash
npx playwright install chromium firefox
```

## Configuration

### Playwright Config (`playwright.config.ts`)

Key settings:
- **baseURL**: `http://localhost:5173` (Vite dev server)
- **testDir**: `./e2e`
- **retries**: 2 in CI, 0 locally
- **workers**: 1 in CI, unlimited locally
- **timeout**: 60 seconds per test
- **browsers**: Chromium, Firefox (WebKit optional)

### Environment Variables

Set these in `.env` or GitHub Secrets:

```bash
# Backend URL
API_URL=http://localhost:5000

# Frontend URL
BASE_URL=http://localhost:5173

# Test user credentials
TEST_ADMIN_EMAIL=admin@test.com
TEST_ADMIN_PASSWORD=Admin123!
```

## Adding New Tests

### 1. Create Page Object Model (if needed)

```typescript
// e2e/pages/MyNewPage.ts
import { Page } from '@playwright/test';
import { BasePage } from './BasePage';

export class MyNewPage extends BasePage {
  private readonly selector = 'button.my-button';

  constructor(page: Page) {
    super(page);
  }

  async clickButton() {
    await this.page.click(this.selector);
  }
}
```

### 2. Create Test Spec

```typescript
// e2e/tests/my-new-feature.spec.ts
import { test, expect } from '@playwright/test';
import { MyNewPage } from '../pages/MyNewPage';
import { loginAsAdmin } from '../utils/helpers';

test.describe('My New Feature', () => {
  test('should do something', async ({ page }) => {
    await loginAsAdmin(page);
    const myPage = new MyNewPage(page);
    await myPage.clickButton();
    expect(page.url()).toContain('/expected-url');
  });
});
```

### 3. Run New Tests

```bash
npx playwright test tests/my-new-feature.spec.ts
```

## Resources

- [Playwright Documentation](https://playwright.dev/)
- [Playwright Best Practices](https://playwright.dev/docs/best-practices)
- [Page Object Model Pattern](https://playwright.dev/docs/pom)
- [Playwright Test Generator](https://playwright.dev/docs/codegen)

## Support

For questions or issues:
1. Check this README
2. Check Playwright documentation
3. Run tests with `--debug` flag
4. Create an issue in the repository

---

**Last Updated:** 2026-02-07
**Test Count:** 38+ E2E tests
**Coverage:** 5 critical user flows
**Framework:** Playwright v1.49+
