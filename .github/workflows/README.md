# CI/CD Pipeline - SincoMaquinaria

Este directorio contiene los workflows de GitHub Actions para CI/CD del proyecto SincoMaquinaria.

## Workflow Principal: `ci-cd.yml`

### Triggers
- **Push** a branches `main` y `develop`
- **Pull Requests** hacia `main` y `develop`

### Jobs

#### 1. Backend Build & Test
- **Runs on:** Ubuntu Latest
- **PostgreSQL Service:** PostgreSQL 14 para tests de integración
- **Pasos:**
  1. Checkout del código
  2. Setup .NET 9.0
  3. Restore dependencies
  4. Build en Release
  5. **Run tests con code coverage**
  6. Upload coverage a Codecov
  7. Publish test results

**Outputs:**
- Coverage reports (Cobertura format)
- Unit test results (.trx)

#### 2. Frontend Build & Test
- **Runs on:** Ubuntu Latest
- **Working Directory:** `./client-app`
- **Pasos:**
  1. Checkout del código
  2. Setup Node.js 20
  3. Install dependencies (npm ci)
  4. Lint (ESLint)
  5. Run tests (Vitest)
  6. Build (Vite)
  7. Upload build artifacts

**Outputs:**
- Frontend build artifacts (`dist/`)

#### 3. Docker Build
- **Runs on:** Ubuntu Latest
- **Depends on:** backend-test, frontend-build
- **Condition:** Solo en push a `main`
- **Pasos:**
  1. Checkout del código
  2. Setup Docker Buildx
  3. Login to Docker Hub
  4. Extract metadata (tags)
  5. **Build and push Docker image**

**Docker Tags:**
- `latest` - Branch principal
- `main-<sha>` - SHA del commit
- `develop-<sha>` - SHA del commit en develop

**Image Name:** `{DOCKER_HUB_USERNAME}/sincomaquinaria`

#### 4. Security Scan
- **Runs on:** Ubuntu Latest
- **Depends on:** backend-test
- **Tool:** Trivy vulnerability scanner
- **Pasos:**
  1. Checkout del código
  2. Run Trivy scan (filesystem)
  3. Upload results to GitHub Security

**Severity Levels:** CRITICAL, HIGH

#### 5. Code Quality
- **Runs on:** Ubuntu Latest
- **Pasos:**
  1. Checkout del código (full history for SonarCloud)
  2. Setup .NET
  3. Restore & Build
  4. (Optional) SonarCloud scan

## Secrets Requeridos

### Para Docker Build:
- `DOCKER_HUB_USERNAME` - Usuario de Docker Hub
- `DOCKER_HUB_TOKEN` - Token de acceso de Docker Hub

### Para Code Coverage:
- `CODECOV_TOKEN` - Token de Codecov (opcional)

### Para SonarCloud (opcional):
- `SONAR_TOKEN` - Token de SonarCloud

## Configurar Secrets

1. Ve a **Settings > Secrets and variables > Actions**
2. Agrega los siguientes secrets:

```
DOCKER_HUB_USERNAME=tu-usuario-dockerhub
DOCKER_HUB_TOKEN=tu-token-dockerhub
CODECOV_TOKEN=tu-token-codecov (opcional)
```

## Status Badges

Agrega estos badges a tu README.md:

```markdown
![CI/CD](https://github.com/tu-usuario/SincoMaquinaria/workflows/CI/CD%20Pipeline/badge.svg)
[![codecov](https://codecov.io/gh/tu-usuario/SincoMaquinaria/branch/main/graph/badge.svg)](https://codecov.io/gh/tu-usuario/SincoMaquinaria)
```

## Optimizaciones de Performance

El workflow incluye:
- **Caching de npm** - Acelera instalación de dependencias frontend
- **Docker layer caching** - Usa GitHub Actions cache para layers de Docker
- **Parallel jobs** - Backend y Frontend se ejecutan en paralelo
- **Artifact caching** - Build artifacts se cachean por 1 día

## Ejecución Local

Para simular el workflow localmente:

```bash
# Backend tests con coverage
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings

# Frontend tests
cd client-app
npm run test:ci

# Docker build
docker build -t sincomaquinaria:local .
```

## Troubleshooting

### Tests fallan en CI pero pasan localmente
- Verifica que PostgreSQL esté disponible (service container)
- Revisa variables de entorno
- Confirma que connection string use `localhost:5432`

### Docker build falla
- Verifica que secrets estén configurados
- Revisa los logs de Docker build
- Confirma que Dockerfile esté en la raíz

### Coverage no se sube
- Verifica que `CODECOV_TOKEN` esté configurado
- Confirma que el archivo coverage.cobertura.xml se genera
- Revisa permisos del workflow

## Próximas Mejoras

- [ ] Agregar deployment automático a Azure/AWS
- [ ] Implementar blue-green deployment
- [ ] Agregar smoke tests post-deployment
- [ ] Configurar environment protection rules
- [ ] Agregar manual approval para producción