# E2E Testing con Playwright - SincoMaquinaria

## 📋 Tabla de Contenidos

- [Visión General](#visión-general)
- [Arquitectura](#arquitectura)
- [Instalación y Setup](#instalación-y-setup)
- [Ejecutar Tests](#ejecutar-tests)
- [Escribir Nuevos Tests](#escribir-nuevos-tests)
- [Debugging](#debugging)
- [CI/CD Integration](#cicd-integration)
- [Troubleshooting](#troubleshooting)
- [Best Practices](#best-practices)

---

## 🎯 Visión General

Este proyecto utiliza **Playwright** para tests end-to-end (E2E) que validan flujos críticos del sistema desde la perspectiva del usuario. Los tests E2E complementan los tests unitarios y de integración, proporcionando la máxima confianza en que la aplicación funciona correctamente en producción.

### ¿Por qué E2E Testing?

- ✅ **Prevenir deployment failures**: Detecta problemas de integración antes de producción
- ✅ **Verificar flujos completos**: Valida workflows desde login hasta completar tareas
- ✅ **Multi-browser testing**: Asegura compatibilidad en Chromium y Firefox
- ✅ **CI/CD Integration**: Los tests bloquean deployment si fallan

### Cobertura Actual

| Flujo | Tests | Estado |
|-------|-------|--------|
| **Authentication** | 2/8 | 🟡 En progreso |
| **Order Creation** | 0/10 | ⏳ Pendiente |
| **Equipment Management** | 0/8 | ⏳ Pendiente |
| **Excel Import** | 0/6 | ⏳ Pendiente |
| **Dashboard** | 0/6 | ⏳ Pendiente |
| **TOTAL** | **2/38** | **5%** |

---

## 🏗️ Arquitectura

### Estructura de Directorios

```
client-app/e2e/
├── e2e.config.ts              # Configuración centralizada (timeouts, URLs, credentials)
├── README.md                  # Esta documentación
│
├── fixtures/
│   └── test-data.ts           # Datos de prueba (usuarios, equipos, rutinas)
│
├── pages/                     # Page Object Models (POM)
│   ├── BasePage.ts            # Clase base con métodos comunes
│   ├── LoginPage.ts           # POM para la página de login
│   ├── DashboardPage.ts       # POM para el dashboard
│   ├── CreateOrderPage.ts     # POM para crear órdenes (pendiente)
│   ├── EquipmentConfigPage.ts # POM para gestión de equipos (pendiente)
│   └── ImportPage.ts          # POM para importación Excel (pendiente)
│
├── tests/                     # Test specs
│   ├── smoke.spec.ts          # Smoke tests (login + navegación básica)
│   ├── auth.spec.ts           # Tests de autenticación (pendiente)
│   ├── order-creation.spec.ts # Tests de creación de órdenes (pendiente)
│   ├── equipment.spec.ts      # Tests de gestión de equipos (pendiente)
│   ├── excel-import.spec.ts   # Tests de importación Excel (pendiente)
│   └── dashboard.spec.ts      # Tests de dashboard (pendiente)
│
└── utils/
    └── helpers.ts             # Funciones helper (login, cleanup, etc.)
```

### Page Object Model (POM)

Los tests usan el patrón **Page Object Model** para:
- Encapsular lógica de interacción con páginas
- Reutilizar selectores y métodos
- Facilitar mantenimiento cuando cambia la UI
- Hacer tests más legibles

**Ejemplo:**
```typescript
// ❌ Sin POM (acoplado a selectores)
await page.locator('input[name="email"]').fill('admin@test.com');
await page.locator('input[name="password"]').fill('Admin123!');
await page.locator('button[type="submit"]').click();

// ✅ Con POM (abstracción clara)
const loginPage = new LoginPage(page);
await loginPage.login(testData.users.admin.email, testData.users.admin.password);
```

### Configuración Centralizada

El archivo `e2e.config.ts` centraliza toda la configuración:

```typescript
import { TIMEOUTS, URLS, TEST_USER, TEST_PREFIXES } from './e2e.config';

// Usar en vez de hardcodear valores
await page.waitForResponse(url, { timeout: TIMEOUTS.apiResponse }); // ✅
await page.waitForResponse(url, { timeout: 30000 }); // ❌
```

**Beneficios:**
- ✅ Un solo lugar para ajustar timeouts globalmente
- ✅ No más "números mágicos" en el código
- ✅ Fácil configurar para diferentes entornos (CI vs local)

---

## 🚀 Instalación y Setup

### 1. Instalar Dependencias

```bash
cd client-app
npm install
```

Esto instalará Playwright automáticamente desde `package.json`.

### 2. Instalar Navegadores

```bash
npx playwright install chromium firefox
```

**Nota:** WebKit (Safari) está deshabilitado por defecto. Para habilitarlo:
```bash
npx playwright install webkit
```

### 3. Configurar Backend para Tests

Los tests E2E requieren que el backend esté corriendo con endpoints de prueba habilitados.

**Opción A: Usar backend en Development**
```bash
# Terminal 1 - Backend
dotnet run --project src/SincoMaquinaria/SincoMaquinaria.csproj

# Terminal 2 - Frontend (Playwright lo inicia automáticamente)
cd client-app
npm run test:e2e
```

**Opción B: Configurar usuario de prueba manualmente**
```bash
# Crear usuario de prueba vía API
curl -X POST http://localhost:5000/test/seed-test-user \
  -H "Content-Type: application/json"

# O usando PowerShell
Invoke-WebRequest -Uri http://localhost:5000/test/seed-test-user -Method POST
```

### 4. Verificar Setup

```bash
npm run test:e2e
```

Deberías ver algo como:
```
Running 4 tests using 1 worker
  ✓ smoke.spec.ts:18:3 › should login and load dashboard (chromium)
  ✓ smoke.spec.ts:18:3 › should login and load dashboard (firefox)
  ✓ smoke.spec.ts:31:3 › should navigate to main pages (chromium)
  ✓ smoke.spec.ts:31:3 › should navigate to main pages (firefox)

  4 passed (15s)
```

---

## 🧪 Ejecutar Tests

### Scripts Disponibles

```bash
# Ejecutar todos los tests E2E
npm run test:e2e

# Ejecutar tests con UI interactiva (recomendado para desarrollo)
npm run test:e2e:ui

# Ejecutar tests con navegador visible
npm run test:e2e:headed

# Ejecutar tests en modo debug (pausa en cada paso)
npm run test:e2e:debug

# Ejecutar un test específico
npx playwright test smoke.spec.ts

# Ejecutar solo tests de un navegador
npx playwright test --project=chromium
npx playwright test --project=firefox
```

### Modo UI (Recomendado)

El modo UI de Playwright es ideal para desarrollo:

```bash
npm run test:e2e:ui
```

**Características:**
- ✅ Interfaz visual para seleccionar tests
- ✅ Ver ejecución en tiempo real
- ✅ Time-travel debugging (avanzar/retroceder en pasos)
- ✅ Inspeccionar selectores
- ✅ Ver network traffic

### Ver Reportes

Después de ejecutar tests, ver el reporte HTML:

```bash
npx playwright show-report
```

El reporte incluye:
- ✅ Screenshots de fallos
- ✅ Videos de tests fallidos
- ✅ Traces para debugging
- ✅ Logs de consola y network

---

## ✍️ Escribir Nuevos Tests

### 1. Crear un Test Básico

**Archivo:** `e2e/tests/example.spec.ts`

```typescript
import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { testData } from '../fixtures/test-data';

test.describe('Example Feature', () => {
  test('should do something', async ({ page }) => {
    // Arrange - Setup
    const loginPage = new LoginPage(page);
    await loginPage.goto();

    // Act - Perform action
    await loginPage.login(
      testData.users.admin.email,
      testData.users.admin.password
    );

    // Assert - Verify result
    expect(page.url()).toMatch(/\/$|\/dashboard$/);
  });
});
```

### 2. Crear un Page Object

**Archivo:** `e2e/pages/ExamplePage.ts`

```typescript
import { Page } from '@playwright/test';
import { BasePage } from './BasePage';
import { TIMEOUTS } from '../e2e.config';

export class ExamplePage extends BasePage {
  // Selectores privados
  private readonly submitButton = 'button[type="submit"]';
  private readonly statusMessage = '[role="alert"]';

  constructor(page: Page) {
    super(page);
  }

  /**
   * Navigate to the page
   */
  async goto() {
    await super.goto('/example');
    await this.waitForElement(this.submitButton);
  }

  /**
   * Submit the form
   */
  async submit() {
    const responsePromise = this.page.waitForResponse(
      response => response.url().includes('/api/example'),
      { timeout: TIMEOUTS.apiResponse }
    );

    await this.clickElement(this.submitButton);
    await responsePromise;
  }

  /**
   * Get status message
   */
  async getStatus(): Promise<string | null> {
    return await this.getTextContent(this.statusMessage);
  }
}
```

### 3. Agregar Test Data

**Archivo:** `e2e/fixtures/test-data.ts`

```typescript
import { TEST_PREFIXES } from '../e2e.config';

export const testData = {
  // ... existing data ...

  newFeature: {
    testCase1: {
      id: `${TEST_PREFIXES.order}NEW-001`,
      description: 'Test case description',
    },
  },
};
```

### 4. Best Practices para Tests

✅ **DO:**
- Usar Page Object Models para todas las interacciones
- Importar timeouts de `e2e.config.ts`
- Usar `waitForResponse()` para operaciones asíncronas
- Agregar comentarios descriptivos (Arrange, Act, Assert)
- Usar `test.describe()` para agrupar tests relacionados
- Limpiar datos de prueba en `test.afterEach()`

❌ **DON'T:**
- Hardcodear timeouts (usar `TIMEOUTS` del config)
- Usar `page.waitForTimeout()` (preferir `waitForElement()`)
- Duplicar selectores (usar Page Objects)
- Crear dependencias entre tests (deben ser independientes)
- Usar datos de producción (usar `TEST_PREFIXES`)

---

## 🐛 Debugging

### 1. Debugging Local con UI Mode

```bash
npm run test:e2e:ui
```

1. Selecciona el test que falla
2. Haz clic en "Watch" para ver ejecución en tiempo real
3. Usa los controles de time-travel para ver cada paso
4. Inspecciona selectores con la herramienta "Pick locator"

### 2. Debugging con VS Code

1. Agregar breakpoint en el código del test
2. Ejecutar con debugger:

```bash
npm run test:e2e:debug
```

3. Alternativamente, usar la extensión de Playwright para VS Code

### 3. Ver Traces de Tests Fallidos

Cuando un test falla en CI o localmente:

```bash
npx playwright show-trace test-results/.../trace.zip
```

El trace viewer muestra:
- ✅ Cada acción ejecutada
- ✅ Screenshots en cada paso
- ✅ Network requests
- ✅ Console logs
- ✅ DOM snapshots

### 4. Screenshots y Videos

Playwright captura automáticamente:
- **Screenshots**: Solo en fallos (`screenshot: 'only-on-failure'`)
- **Videos**: Solo en fallos (`video: 'retain-on-failure'`)
- **Traces**: En primer retry (`trace: 'on-first-retry'`)

Ubicación: `client-app/test-results/`

### 5. Debugging de Selectores

Si un selector no funciona:

```bash
# Modo interactivo para probar selectores
npx playwright codegen http://localhost:5173
```

Esto abre un navegador donde puedes:
- Hacer clic en elementos para ver sus selectores
- Probar selectores en tiempo real
- Generar código Playwright automáticamente

---

## 🔄 CI/CD Integration

### GitHub Actions Workflow

Los E2E tests corren automáticamente en GitHub Actions en cada push a `main` o `develop`.

**Archivo:** `.github/workflows/ci-cd.yml`

```yaml
e2e-tests:
  name: E2E Tests
  runs-on: ubuntu-latest
  needs: [backend-test, frontend-build]
  continue-on-error: false  # Block deployment if E2E fails

  steps:
    - name: Checkout code
    - name: Setup .NET
    - name: Setup Node.js
    - name: Start backend server
    - name: Install Playwright browsers
    - name: Seed test user
    - name: Run E2E tests
    - name: Upload test results
```

### Ver Resultados en GitHub

1. Ve a la pestaña **Actions** en GitHub
2. Selecciona el workflow run
3. Busca el job "E2E Tests"
4. Descarga el artifact "playwright-report" para ver el reporte completo

### Configuración de Timeouts en CI

Los timeouts en CI son más largos que en local:

```typescript
// playwright.config.ts
timeout: process.env.CI ? 120000 : 60000, // 2 min in CI, 1 min local
```

### Retry Strategy

En CI, los tests se reintentan automáticamente en caso de fallo:

```typescript
retries: process.env.CI ? 2 : 0,
```

---

## 🔧 Troubleshooting

### Problema: "Test timeout exceeded"

**Síntoma:** Test falla con `Test timeout of 60000ms exceeded`

**Solución:**
1. Verificar que el backend esté corriendo
2. Verificar que el frontend esté corriendo (`http://localhost:5173`)
3. Aumentar timeout en `playwright.config.ts` si la operación es lenta

```typescript
test('slow operation', async ({ page }) => {
  test.setTimeout(120000); // 2 minutes for this specific test
  // ... rest of test
});
```

### Problema: "Waiting for selector timed out"

**Síntoma:** `page.waitForSelector('...') timed out`

**Solución:**
1. Verificar que el selector es correcto:
   ```bash
   npx playwright codegen http://localhost:5173
   ```

2. Esperar que el elemento esté visible:
   ```typescript
   await page.waitForSelector('button[type="submit"]', { state: 'visible' });
   ```

3. Usar selectores más robustos:
   ```typescript
   // ❌ Frágil
   await page.locator('div > button').click();

   // ✅ Robusto
   await page.getByRole('button', { name: 'Submit' }).click();
   ```

### Problema: "Navigation failed" en Firefox

**Síntoma:** `NS_BINDING_ABORTED` error en Firefox

**Solución:** Usar el helper `navigateRobustly` del smoke test:

```typescript
const navigateRobustly = async (url: string) => {
  try {
    await page.goto(url, { waitUntil: 'commit', timeout: TIMEOUTS.pageLoad });
    await page.waitForLoadState('domcontentloaded');
    if (browserName === 'firefox') {
      await page.waitForTimeout(TIMEOUTS.firefoxStabilization);
    }
  } catch (error) {
    console.log(`Navigation to ${url} failed, retrying...`);
    await page.waitForTimeout(TIMEOUTS.firefoxStabilization);
    await page.goto(url, { waitUntil: 'domcontentloaded' });
  }
};
```

### Problema: Tests fallan en CI pero pasan localmente

**Posibles causas:**
1. **Race conditions**: Tests corren en paralelo en local pero secuencialmente en CI
   - **Solución:** Configurar `workers: 1` en `playwright.config.ts`

2. **Datos de base de datos**: CI usa base de datos limpia, local puede tener datos antiguos
   - **Solución:** Usar `test.beforeEach()` para limpiar datos

3. **Timeouts más estrictos**: CI puede ser más lento
   - **Solución:** Aumentar timeouts para CI en config

### Problema: "Usuario de prueba no existe"

**Síntoma:** Login falla con "Invalid credentials"

**Solución:**
```bash
# Crear usuario de prueba manualmente
curl -X POST http://localhost:5000/test/seed-test-user \
  -H "Content-Type: application/json"

# O usando PowerShell
Invoke-WebRequest -Uri http://localhost:5000/test/seed-test-user -Method POST
```

### Problema: Tests fallan en paralelo

**Síntoma:** Tests pasan individualmente pero fallan cuando se ejecutan todos

**Solución:** Los tests E2E comparten la misma base de datos. Configurar ejecución secuencial:

```typescript
// playwright.config.ts
export default defineConfig({
  fullyParallel: false,
  workers: 1, // Always sequential
  // ...
});
```

---

## 📚 Best Practices

### 1. Esperar Correctamente

```typescript
// ❌ Evitar delays arbitrarios
await page.waitForTimeout(3000);

// ✅ Esperar por condiciones específicas
await page.waitForSelector('button[type="submit"]');
await page.waitForResponse(response => response.url().includes('/api/'));
await page.waitForLoadState('domcontentloaded');
```

### 2. Selectores Robustos

```typescript
// ❌ Selectores frágiles (dependen de estructura HTML)
await page.locator('div > div > button:nth-child(3)').click();

// ✅ Selectores semánticos (basados en roles y labels)
await page.getByRole('button', { name: 'Submit' }).click();
await page.getByLabel('Email').fill('test@test.com');
await page.getByText('Welcome').isVisible();
```

### 3. Datos de Prueba Únicos

```typescript
// ❌ Datos hardcodeados (pueden causar conflictos)
const placa = 'TEST-001';

// ✅ Datos únicos con timestamp
import { generateUniqueEquipo } from '../fixtures/test-data';
const equipo = generateUniqueEquipo(testData.equipos[0]);
```

### 4. Limpiar Después de Tests

```typescript
test.describe('Equipment Tests', () => {
  let createdEquipoId: string;

  test.afterEach(async ({ page }) => {
    // Cleanup - delete test data
    if (createdEquipoId) {
      await deleteEquipo(page, createdEquipoId);
    }
  });

  test('create equipment', async ({ page }) => {
    // ... test implementation
  });
});
```

### 5. Independencia de Tests

```typescript
// ❌ Tests dependientes (test 2 depende de test 1)
test('create order', async ({ page }) => {
  // Creates order with ID 123
});

test('edit order', async ({ page }) => {
  // Expects order 123 to exist (fails if test 1 no corrió)
});

// ✅ Tests independientes
test('edit order', async ({ page }) => {
  // Create order first
  const orderId = await createTestOrder(page);

  // Now edit it
  await editOrder(page, orderId);
});
```

### 6. Organizar Tests con describe()

```typescript
test.describe('Authentication Flow', () => {
  test.describe('Login', () => {
    test('valid credentials', async ({ page }) => { });
    test('invalid credentials', async ({ page }) => { });
  });

  test.describe('Logout', () => {
    test('clears session', async ({ page }) => { });
  });
});
```

---

## 📖 Recursos Adicionales

- **Documentación Oficial:** https://playwright.dev/
- **Best Practices:** https://playwright.dev/docs/best-practices
- **Debugging Guide:** https://playwright.dev/docs/debug
- **CI/CD Guide:** https://playwright.dev/docs/ci

---

## 🎯 Roadmap

### Corto Plazo (1-2 semanas)
- [ ] Implementar 10 tests de creación de órdenes
- [ ] Completar 8 tests de autenticación
- [ ] Agregar tests de gestión de equipos

### Mediano Plazo (1 mes)
- [ ] Tests de importación Excel (6 tests)
- [ ] Tests de dashboard en tiempo real (6 tests)
- [ ] Llegar a 60%+ de cobertura de flujos críticos

### Largo Plazo (3 meses)
- [ ] Visual regression testing
- [ ] Performance testing
- [ ] Tests de accesibilidad (a11y)
- [ ] Tests de diferentes roles de usuario

---

**Última actualización:** 2026-02-08
**Mantenido por:** Equipo de Desarrollo SincoMaquinaria
