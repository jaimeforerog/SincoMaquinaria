# 🚀 Guía de Despliegue - SincoMaquinaria

Esta guía proporciona instrucciones paso a paso para desplegar SincoMaquinaria en Azure usando GitHub Actions.

## 📋 Resumen Rápido

```bash
# 1. Configurar infraestructura (una sola vez)
cd infrastructure/azure
cp .env.example .env
# Editar .env con tus valores
./deploy.sh

# 2. Configurar GitHub Secrets (una sola vez)
# - AZURE_CREDENTIALS
# - POSTGRES_ADMIN_PASSWORD
# - JWT_SECRET_KEY

# 3. Push a main → despliegue automático
git push origin main
```

---

## 🎯 Opción 1: Despliegue Automático con GitHub Actions (Recomendado)

### Paso 1: Preparar Azure

#### 1.1 Login en Azure

```bash
az login
az account set --subscription "YOUR_SUBSCRIPTION_ID"
```

#### 1.2 Crear Resource Group

```bash
az group create \
  --name rg-sincomaquinaria-prod \
  --location eastus
```

#### 1.3 Desplegar Infraestructura

```bash
cd infrastructure/azure

# Copiar y configurar variables
cp .env.example .env
nano .env  # Editar con tus valores

# Ejecutar despliegue
chmod +x deploy.sh
./deploy.sh
```

**En Windows (PowerShell):**

```powershell
cd infrastructure\azure
copy .env.example .env
notepad .env  # Editar con tus valores
.\deploy.ps1
```

El script te pedirá:
- PostgreSQL admin password
- JWT secret key

**⏱️ Tiempo estimado: 10-15 minutos**

### Paso 2: Configurar GitHub Actions

#### 2.1 Crear Service Principal

```bash
# Obtener tu Subscription ID
SUBSCRIPTION_ID=$(az account show --query id -o tsv)

# Crear Service Principal
az ad sp create-for-rbac \
  --name "sincomaquinaria-github-actions" \
  --role contributor \
  --scopes /subscriptions/$SUBSCRIPTION_ID/resourceGroups/rg-sincomaquinaria-prod \
  --sdk-auth
```

**Copia el JSON completo que retorna** - lo necesitarás en el siguiente paso.

#### 2.2 Configurar GitHub Secrets

1. Ve a tu repositorio en GitHub
2. Settings → Secrets and variables → Actions → New repository secret
3. Crea los siguientes secrets:

| Secret Name | Valor | Ejemplo |
|-------------|-------|---------|
| `AZURE_CREDENTIALS` | JSON completo del Service Principal | `{"clientId": "...", ...}` |
| `POSTGRES_ADMIN_PASSWORD` | Password de PostgreSQL (mismo del Paso 1) | `MySecurePass123!` |
| `JWT_SECRET_KEY` | Clave JWT (mínimo 32 caracteres) | `YourVerySecure32CharKey...` |

#### 2.3 Verificar Workflow

El archivo `.github/workflows/azure-deploy.yml` ya está configurado.

### Paso 3: Desplegar Aplicación

```bash
# Hacer commit y push a main
git add .
git commit -m "Deploy to Azure"
git push origin main
```

El workflow se ejecutará automáticamente:

1. ✅ Build and Test
2. ✅ Build and Push Docker Image
3. ✅ Deploy to Azure Container Apps
4. ✅ Health Check

**Ver progreso en:** `GitHub → Actions tab`

### Paso 4: Verificar Despliegue

```bash
# Obtener URL de la aplicación
az containerapp show \
  --name ca-sincomaquinaria-prod \
  --resource-group rg-sincomaquinaria-prod \
  --query properties.configuration.ingress.fqdn \
  -o tsv
```

Abre la URL en tu navegador: `https://ca-sincomaquinaria-prod.XXXXXX.eastus.azurecontainerapps.io`

---

## 🛠️ Opción 2: Despliegue Manual Completo

### Paso 1: Desplegar Infraestructura

Igual que en Opción 1 - Paso 1.

### Paso 2: Build y Push Manual

```bash
# Obtener credenciales del ACR
ACR_NAME=$(az acr list --resource-group rg-sincomaquinaria-prod --query "[0].name" -o tsv)
ACR_LOGIN_SERVER=$(az acr show --name $ACR_NAME --query loginServer -o tsv)

# Login al ACR
az acr login --name $ACR_NAME

# Build Docker image
docker build -t $ACR_LOGIN_SERVER/sincomaquinaria:latest .

# Push image
docker push $ACR_LOGIN_SERVER/sincomaquinaria:latest
```

### Paso 3: Actualizar Container App

```bash
# Obtener nombre del Container App
CONTAINER_APP_NAME=$(az containerapp list --resource-group rg-sincomaquinaria-prod --query "[0].name" -o tsv)

# Actualizar con nueva imagen
az containerapp update \
  --name $CONTAINER_APP_NAME \
  --resource-group rg-sincomaquinaria-prod \
  --image $ACR_LOGIN_SERVER/sincomaquinaria:latest
```

---

## 🔧 Configuración Post-Despliegue

### Crear Usuario Administrador

```bash
# Conectar a PostgreSQL
POSTGRES_FQDN=$(az postgres flexible-server show \
  --resource-group rg-sincomaquinaria-prod \
  --name psql-sincomaquinaria-prod \
  --query fullyQualifiedDomainName -o tsv)

# Usar psql o un cliente GUI para ejecutar:
# INSERT INTO usuario (nombre, email, password_hash, rol, activo)
# VALUES ('Admin', 'admin@sinco.com', '<hash>', 'Admin', true);
```

O usar el endpoint `/auth/register` desde la aplicación.

### Configurar Dominio Personalizado (Opcional)

```bash
# Agregar dominio personalizado
az containerapp hostname add \
  --hostname www.sincomaquinaria.com \
  --name ca-sincomaquinaria-prod \
  --resource-group rg-sincomaquinaria-prod
```

### Configurar HTTPS con Certificado Personalizado

```bash
# Subir certificado
az containerapp ssl upload \
  --name ca-sincomaquinaria-prod \
  --resource-group rg-sincomaquinaria-prod \
  --certificate-file certificate.pfx \
  --certificate-password <password>

# Bind certificado al hostname
az containerapp hostname bind \
  --hostname www.sincomaquinaria.com \
  --name ca-sincomaquinaria-prod \
  --resource-group rg-sincomaquinaria-prod \
  --thumbprint <thumbprint>
```

---

## 📊 Monitoreo

### Ver Logs en Tiempo Real

```bash
az containerapp logs show \
  --name ca-sincomaquinaria-prod \
  --resource-group rg-sincomaquinaria-prod \
  --follow
```

### Application Insights

```bash
# Obtener URL del portal
az monitor app-insights component show \
  --app ai-sincomaquinaria-prod \
  --resource-group rg-sincomaquinaria-prod \
  --query '{Name:name, AppId:appId, InstrumentationKey:instrumentationKey}'
```

### Dashboard de Hangfire

URL: `https://YOUR_APP_URL.azurecontainerapps.io/hangfire`

---

## 🔄 Actualizaciones

### Despliegue de Nuevas Versiones

**Con GitHub Actions (automático):**

```bash
git commit -m "Nueva funcionalidad"
git push origin main
```

**Manual:**

```bash
# Build nueva versión
docker build -t $ACR_LOGIN_SERVER/sincomaquinaria:v1.1.0 .
docker push $ACR_LOGIN_SERVER/sincomaquinaria:v1.1.0

# Actualizar Container App
az containerapp update \
  --name ca-sincomaquinaria-prod \
  --resource-group rg-sincomaquinaria-prod \
  --image $ACR_LOGIN_SERVER/sincomaquinaria:v1.1.0
```

### Rollback a Versión Anterior

```bash
# Listar revisiones
az containerapp revision list \
  --name ca-sincomaquinaria-prod \
  --resource-group rg-sincomaquinaria-prod \
  --query "[].{Name:name, Active:properties.active, Created:properties.createdTime}" \
  -o table

# Activar revisión anterior
az containerapp revision activate \
  --revision <revision-name> \
  --name ca-sincomaquinaria-prod \
  --resource-group rg-sincomaquinaria-prod
```

---

## 🧪 Ambientes Múltiples

Para crear ambientes de staging/development:

```bash
# Staging
cd infrastructure/azure
cp .env.example .env.staging
nano .env.staging  # Cambiar ENVIRONMENT=staging

# Desplegar staging
ENVIRONMENT=staging RESOURCE_GROUP=rg-sincomaquinaria-staging ./deploy.sh

# Development
ENVIRONMENT=dev RESOURCE_GROUP=rg-sincomaquinaria-dev ./deploy.sh
```

Modificar GitHub Actions workflow para desplegar a staging en branches específicas.

---

## 🚨 Troubleshooting

### Error: "Container App not starting"

```bash
# Ver logs detallados
az containerapp logs show \
  --name ca-sincomaquinaria-prod \
  --resource-group rg-sincomaquinaria-prod \
  --tail 200

# Verificar variables de entorno
az containerapp show \
  --name ca-sincomaquinaria-prod \
  --resource-group rg-sincomaquinaria-prod \
  --query properties.template.containers[0].env
```

### Error: "Cannot connect to PostgreSQL"

```bash
# Verificar firewall
az postgres flexible-server firewall-rule list \
  --resource-group rg-sincomaquinaria-prod \
  --name psql-sincomaquinaria-prod

# Agregar regla para Azure Services
az postgres flexible-server firewall-rule create \
  --resource-group rg-sincomaquinaria-prod \
  --name psql-sincomaquinaria-prod \
  --rule-name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

### Error: "Health check failing"

```bash
# Test health endpoint
APP_URL=$(az containerapp show \
  --name ca-sincomaquinaria-prod \
  --resource-group rg-sincomaquinaria-prod \
  --query properties.configuration.ingress.fqdn -o tsv)

curl -v https://$APP_URL/health

# Revisar configuración de probes
az containerapp show \
  --name ca-sincomaquinaria-prod \
  --resource-group rg-sincomaquinaria-prod \
  --query properties.template.containers[0].probes
```

---

## 📚 Recursos Adicionales

- [Documentación Completa de Azure](infrastructure/azure/README.md)
- [GitHub Actions Workflow](.github/workflows/azure-deploy.yml)
- [Bicep Template](infrastructure/azure/main.bicep)

---

## ✅ Checklist de Despliegue

- [ ] Azure CLI instalado y configurado
- [ ] Infraestructura desplegada (`./deploy.sh`)
- [ ] Service Principal creado
- [ ] GitHub Secrets configurados
  - [ ] AZURE_CREDENTIALS
  - [ ] POSTGRES_ADMIN_PASSWORD
  - [ ] JWT_SECRET_KEY
- [ ] Push a main ejecutado
- [ ] GitHub Actions workflow completado exitosamente
- [ ] Health check pasando
- [ ] Usuario admin creado
- [ ] Application Insights configurado
- [ ] Logs verificados

---

## 💡 Mejores Prácticas

1. **Secretos**: Nunca commitear `.env` files
2. **Backups**: Configurar backups automáticos de PostgreSQL
3. **Monitoreo**: Revisar Application Insights regularmente
4. **Costos**: Monitorear costos en Azure Portal
5. **Updates**: Mantener dependencias actualizadas
6. **Testing**: Ejecutar tests antes de cada despliegue
7. **Rollback Plan**: Tener plan de rollback documentado

---

**¿Necesitas ayuda?** Consulta la [documentación completa](infrastructure/azure/README.md) o revisa los logs en Application Insights.
