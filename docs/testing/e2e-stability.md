# E2E Test Stability Guide

## Overview

This document describes the strategies used to stabilize E2E tests in SincoMaquinaria, ensuring they pass consistently in both local and CI environments.

## Table of Contents

1. [Stabilization Strategies](#stabilization-strategies)
2. [Running Tests Locally](#running-tests-locally)
3. [Best Practices for New Tests](#best-practices-for-new-tests)
4. [Troubleshooting Common Issues](#troubleshooting-common-issues)
5. [CI/CD Configuration](#cicd-configuration)

---

## Stabilization Strategies

### 1. **Eliminated Fixed Timeouts**

**Problem**: `page.waitForTimeout(500)` introduces flakiness - tests may pass locally but fail in CI with slower machines.

**Solution**: Replaced all fixed timeouts with conditional waits:

```typescript
// ❌ Bad - Fixed timeout
await page.waitForTimeout(2000);

// ✅ Good - Wait for condition
await page.waitForFunction(
  () => localStorage.getItem('authToken') !== null,
  { timeout: 5000 }
);
```

**Changed in**:
- `helpers.ts` - `loginAsAdmin()`, `login()`
- `setup-test-data.ts` - `setupBasicTestData()`
- `auth.spec.ts` - token refresh test

### 2. **Improved Test Data Cleanup**

**Problem**: Tests failing due to leftover data from previous runs, causing unique constraint violations.

**Solution**: Enhanced cleanup with proper ordering to avoid foreign key constraints:

```typescript
// Order matters! Delete in reverse dependency order:
// 1. Orders (depend on equipos and rutinas)
// 2. Equipos (may have orders)
// 3. Rutinas (may have orders)
```

**Changes**:
- Added detailed logging for cleanup operations
- Cleanup runs in `beforeEach` AND `afterAll` hooks
- Graceful error handling if entities don't exist

### 3. **Retry Logic with Exponential Backoff**

**Problem**: Transient network issues or database locks causing intermittent failures.

**Solution**: Implemented retry helper with exponential backoff:

```typescript
const rutinaId = await retryWithBackoff(
  () => createRutina(page, uniqueRutina, token),
  2, // max 2 retries
  500 // 500ms initial delay
);
// Retry delays: 500ms, 1000ms, 2000ms
```

**Applied to**:
- Equipment creation
- Rutina creation
- Order creation

### 4. **Firefox-Specific Stabilization**

**Problem**: Firefox had `NS_BINDING_ABORTED` errors due to premature navigation.

**Solution**: Wait for `networkidle` state instead of fixed timeout:

```typescript
if (browserName === 'firefox') {
  await page.waitForLoadState('networkidle', { timeout: 5000 })
    .catch(() => {
      console.log('[Firefox] Network idle timeout, continuing anyway');
    });
}
```

### 5. **Test Isolation**

**Problem**: Tests affecting each other due to shared database state.

**Solution**:
- Each test cleans up before starting (`beforeEach`)
- Sequential execution (workers: 1 in `playwright.config.ts`)
- Unique test data prefixes (`E2E-`, `TEST-`)

### 6. **Enhanced Error Logging**

Added comprehensive logging for debugging:

```typescript
console.log('[Setup] Created equipo: E2E-XCMG-001 (abc-123-def)');
console.log('[Cleanup] Deleted 3 test orders');
console.warn('[Cleanup] Failed to delete equipo xyz: 404 Not Found');
```

---

## Running Tests Locally

### Prerequisites

1. **PostgreSQL** running on localhost:5432
2. **.NET 9.0** SDK installed
3. **Node.js 20+** installed

### Option 1: With Backend Already Running

```bash
# Terminal 1: Start backend
cd src/SincoMaquinaria
dotnet run

# Terminal 2: Run E2E tests
cd client-app
./run-e2e-tests-local.sh
```

### Option 2: Automated (Backend + Tests)

```bash
cd client-app
./run-e2e-tests-with-backend.sh
```

This script:
1. Builds backend
2. Starts backend in background
3. Waits for health check
4. Runs E2E tests
5. Cleans up backend on exit

### Option 3: Manual

```bash
# 1. Start backend
cd src/SincoMaquinaria
export ConnectionStrings__DefaultConnection="Host=localhost;Database=SincoMaquinaria_Test;Username=postgres;Password=postgres"
export ASPNETCORE_ENVIRONMENT="Development"
dotnet run --urls http://localhost:5000

# 2. In another terminal, run tests
cd client-app
npm run test:e2e

# 3. View report
npx playwright show-report
```

### Running Specific Tests

```bash
# Run only auth tests
npx playwright test auth.spec.ts

# Run in headed mode (see browser)
npx playwright test --headed

# Run in debug mode
npx playwright test --debug

# Run only Chromium
npx playwright test --project=chromium
```

---

## Best Practices for New Tests

### ✅ DO

1. **Use Page Object Model**
   ```typescript
   const loginPage = new LoginPage(page);
   await loginPage.login(email, password);
   ```

2. **Wait for Conditions, Not Time**
   ```typescript
   await page.waitForURL('/dashboard');
   await page.waitForSelector('[data-testid="kpi-equipos"]');
   ```

3. **Clean Up Test Data**
   ```typescript
   test.beforeEach(async ({ page }) => {
     await loginAsAdmin(page);
     await cleanupAllTestData(page);
   });
   ```

4. **Use Unique Identifiers**
   ```typescript
   const uniquePlaca = `E2E-${Date.now()}`;
   ```

5. **Handle Async Properly**
   ```typescript
   const response = await page.waitForResponse(
     res => res.url().includes('/api/equipos') && res.status() === 201
   );
   ```

### ❌ DON'T

1. **Don't Use Fixed Timeouts**
   ```typescript
   // ❌ Avoid
   await page.waitForTimeout(2000);
   ```

2. **Don't Hardcode Test Data**
   ```typescript
   // ❌ Avoid - will fail on second run
   await createEquipo({ placa: 'ABC-123' });

   // ✅ Use
   await createEquipo({ placa: `E2E-${Date.now()}` });
   ```

3. **Don't Ignore Cleanup**
   ```typescript
   // ❌ Leaves data in database
   await createTestOrder(...);
   // Test ends - data remains

   // ✅ Clean up
   test.afterAll(async () => {
     await cleanupAllTestData(page);
   });
   ```

4. **Don't Rely on Test Order**
   ```typescript
   // ❌ Test 2 depends on Test 1 running first
   // ✅ Each test should be independent
   ```

---

## Troubleshooting Common Issues

### Issue: Tests fail with "ECONNREFUSED"

**Cause**: Backend is not running

**Solution**:
```bash
# Check if backend is running
curl http://localhost:5000/health

# If not, start it
cd src/SincoMaquinaria && dotnet run
```

### Issue: Tests fail with "Timeout waiting for authToken"

**Cause**: Login failed or token not stored in localStorage

**Solution**:
1. Check backend logs for authentication errors
2. Verify test user exists:
   ```bash
   # Seed test user manually
   curl -X POST http://localhost:5000/test/seed-test-user
   ```
3. Run test in headed mode to see what's happening:
   ```bash
   npx playwright test --headed auth.spec.ts
   ```

### Issue: Tests fail with "Unique constraint violation"

**Cause**: Test data from previous run not cleaned up

**Solution**:
```bash
# Manually clean test database
psql -d SincoMaquinaria_Test -c "DELETE FROM equipos WHERE placa LIKE 'E2E-%';"
psql -d SincoMaquinaria_Test -c "DELETE FROM rutinas WHERE nombre LIKE '%E2E%';"

# Or run cleanup from a test
npx playwright test --grep "cleanup"
```

### Issue: Firefox tests fail but Chromium passes

**Cause**: Firefox has stricter navigation handling

**Solution**:
- Check if using `waitForLoadState('networkidle')` for Firefox
- Verify no navigation race conditions
- See `smoke.spec.ts` for Firefox-specific handling

### Issue: Tests are flaky (pass sometimes, fail sometimes)

**Possible Causes**:
1. Race condition (fix with proper waits)
2. Test data collision (ensure unique IDs)
3. Network timing (add retry logic)
4. Missing cleanup (add beforeEach cleanup)

**Debug**:
```bash
# Run test 10 times to identify flakiness
for i in {1..10}; do
  echo "Run $i/10"
  npx playwright test problematic.spec.ts
done
```

---

## CI/CD Configuration

### GitHub Actions Workflow

E2E tests run as part of the CI/CD pipeline in `.github/workflows/ci-cd.yml`:

```yaml
e2e-tests:
  name: E2E Tests
  runs-on: ubuntu-latest
  needs: [backend-test, frontend-build]
  # ✅ No continue-on-error - tests must pass!

  services:
    postgres:
      image: postgres:14
      # ... health checks configured

  steps:
    - name: Start backend server
      run: |
        dotnet run --project src/SincoMaquinaria &
        # Wait for health check

    - name: Run E2E tests
      working-directory: ./client-app
      run: npm run test:e2e
```

### Key Differences from Local

1. **Sequential Execution**: CI uses `workers: 1` to avoid race conditions
2. **Retries**: `retries: 2` in CI (configured in `playwright.config.ts`)
3. **Artifacts**: Screenshots and videos uploaded on failure
4. **Timeout**: Longer timeout in CI (120s vs 60s local)

### Monitoring Test Stability

After deployment, check:

1. **GitHub Actions Summary**: All tests should be green
2. **Playwright Report Artifact**: Download and view detailed report
3. **Test Flakiness**: If same test fails occasionally, investigate

---

## Maintenance

### When Adding New Features

1. Add E2E test for critical user journey
2. Follow Page Object Model pattern
3. Use existing helpers (`loginAsAdmin`, `cleanupTestData`)
4. Ensure proper cleanup in hooks

### Monthly Review

- [ ] Check for new flaky tests
- [ ] Review and update timeouts if needed
- [ ] Ensure cleanup is working (no test data accumulation)
- [ ] Update this document with new patterns

---

## References

- [Playwright Best Practices](https://playwright.dev/docs/best-practices)
- [Testing Authentication Flows](https://playwright.dev/docs/auth)
- [Page Object Model](https://playwright.dev/docs/pom)
- [CI/CD Integration](https://playwright.dev/docs/ci)

---

## Changelog

| Date | Change | Author |
|------|--------|--------|
| 2026-02-12 | Initial E2E stabilization - removed continue-on-error | Claude |
| 2026-02-12 | Added retry logic, improved cleanup, eliminated timeouts | Claude |
