# ⚡ Quick Start - Azure Deployment

## 🚀 Despliegue en 5 Minutos

```bash
# 1. Login
az login

# 2. Configurar
cd infrastructure/azure
cp .env.example .env
nano .env  # Configurar POSTGRES_ADMIN_PASSWORD y JWT_SECRET_KEY

# 3. Desplegar
./deploy.sh

# 4. Configurar GitHub Secrets (ver abajo)

# 5. Push
git push origin main
```

---

## 📝 Comandos Esenciales

### Despliegue

```bash
# Desplegar infraestructura
./deploy.sh

# Desplegar solo aplicación (manual)
ACR_NAME=$(az acr list -g rg-sincomaquinaria-prod --query "[0].name" -o tsv)
az acr login --name $ACR_NAME
docker build -t $ACR_NAME.azurecr.io/sincomaquinaria:latest .
docker push $ACR_NAME.azurecr.io/sincomaquinaria:latest
```

### Monitoreo

```bash
# Ver logs en tiempo real
az containerapp logs show \
  --name ca-sincomaquinaria-prod \
  --resource-group rg-sincomaquinaria-prod \
  --follow

# Ver últimas 100 líneas
az containerapp logs show \
  --name ca-sincomaquinaria-prod \
  --resource-group rg-sincomaquinaria-prod \
  --tail 100
```

### Información

```bash
# URL de la aplicación
az containerapp show \
  --name ca-sincomaquinaria-prod \
  --resource-group rg-sincomaquinaria-prod \
  --query properties.configuration.ingress.fqdn \
  -o tsv

# Estado de recursos
az resource list \
  --resource-group rg-sincomaquinaria-prod \
  --output table
```

### Escalado

```bash
# Escalar manualmente
az containerapp update \
  --name ca-sincomaquinaria-prod \
  --resource-group rg-sincomaquinaria-prod \
  --min-replicas 2 \
  --max-replicas 10

# Ver replicas actuales
az containerapp replica list \
  --name ca-sincomaquinaria-prod \
  --resource-group rg-sincomaquinaria-prod \
  --query "[].{Name:name, State:properties.runningState}" \
  -o table
```

### Rollback

```bash
# Listar revisiones
az containerapp revision list \
  --name ca-sincomaquinaria-prod \
  --resource-group rg-sincomaquinaria-prod \
  -o table

# Activar revisión anterior
az containerapp revision activate \
  --revision <nombre-revision> \
  --name ca-sincomaquinaria-prod \
  --resource-group rg-sincomaquinaria-prod
```

### Secrets Management

```bash
# Listar secrets
az containerapp secret list \
  --name ca-sincomaquinaria-prod \
  --resource-group rg-sincomaquinaria-prod

# Actualizar secret
az containerapp secret set \
  --name ca-sincomaquinaria-prod \
  --resource-group rg-sincomaquinaria-prod \
  --secrets jwt-key="NEW_SECRET_VALUE"
```

### Base de Datos

```bash
# Conectar a PostgreSQL
POSTGRES_HOST=$(az postgres flexible-server show \
  --name psql-sincomaquinaria-prod \
  --resource-group rg-sincomaquinaria-prod \
  --query fullyQualifiedDomainName -o tsv)

psql -h $POSTGRES_HOST -U sincoAdmin -d SincoMaquinaria

# Backup manual
pg_dump -h $POSTGRES_HOST -U sincoAdmin -d SincoMaquinaria > backup.sql
```

### Redis

```bash
# Ver info de Redis
az redis show \
  --name redis-sincomaquinaria-prod \
  --resource-group rg-sincomaquinaria-prod

# Obtener connection string
az redis list-keys \
  --name redis-sincomaquinaria-prod \
  --resource-group rg-sincomaquinaria-prod
```

---

## 🔑 GitHub Secrets

Configurar en `Settings > Secrets and variables > Actions`:

### 1. AZURE_CREDENTIALS

```bash
# Crear Service Principal
az ad sp create-for-rbac \
  --name "sincomaquinaria-github-actions" \
  --role contributor \
  --scopes /subscriptions/{SUBSCRIPTION_ID}/resourceGroups/rg-sincomaquinaria-prod \
  --sdk-auth
```

Copiar el JSON completo como secret.

### 2. POSTGRES_ADMIN_PASSWORD

Password seguro (mín. 8 caracteres, con mayúsculas, números y símbolos).

### 3. JWT_SECRET_KEY

Clave de mínimo 32 caracteres:

```bash
# Generar key aleatoria
openssl rand -base64 32
```

---

## 🔍 Troubleshooting Rápido

### App no inicia

```bash
# Ver últimos logs
az containerapp logs show \
  --name ca-sincomaquinaria-prod \
  --resource-group rg-sincomaquinaria-prod \
  --tail 200

# Reiniciar app
az containerapp revision restart \
  --name ca-sincomaquinaria-prod \
  --resource-group rg-sincomaquinaria-prod
```

### Error de PostgreSQL

```bash
# Verificar firewall
az postgres flexible-server firewall-rule list \
  --name psql-sincomaquinaria-prod \
  --resource-group rg-sincomaquinaria-prod

# Agregar regla si falta
az postgres flexible-server firewall-rule create \
  --name psql-sincomaquinaria-prod \
  --resource-group rg-sincomaquinaria-prod \
  --rule-name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

### Health check falla

```bash
# Test manual
APP_URL=$(az containerapp show \
  --name ca-sincomaquinaria-prod \
  --resource-group rg-sincomaquinaria-prod \
  --query properties.configuration.ingress.fqdn -o tsv)

curl -v https://$APP_URL/health
```

---

## 💰 Costos

Monitorear costos:

```bash
# Ver costos del resource group
az consumption usage list \
  --start-date 2026-01-01 \
  --end-date 2026-01-31 \
  --query "[?contains(instanceName,'sincomaquinaria')].{Resource:instanceName, Cost:pretaxCost}" \
  -o table
```

**Estimado mensual**: ~$120-160 USD

Para reducir costos:
- Reducir min replicas a 0 en dev/staging
- Usar SKUs menores en staging
- Deshabilitar Application Insights en dev

---

## 🔐 Security Checklist

- [ ] `.env` no commiteado (verificar `.gitignore`)
- [ ] Secrets en GitHub Actions configurados
- [ ] PostgreSQL firewall configurado
- [ ] SSL/TLS habilitado (automático)
- [ ] JWT secret key seguro (32+ caracteres)
- [ ] PostgreSQL admin password fuerte

---

## 📊 URLs Importantes

Después del despliegue:

```bash
# Aplicación
https://ca-sincomaquinaria-prod.XXXXXX.eastus.azurecontainerapps.io

# Swagger
https://ca-sincomaquinaria-prod.XXXXXX.eastus.azurecontainerapps.io/swagger

# Hangfire Dashboard
https://ca-sincomaquinaria-prod.XXXXXX.eastus.azurecontainerapps.io/hangfire

# Health Check
https://ca-sincomaquinaria-prod.XXXXXX.eastus.azurecontainerapps.io/health

# Application Insights
https://portal.azure.com → ai-sincomaquinaria-prod
```

---

## 📚 Documentación Completa

- [README detallado](README.md)
- [Guía de despliegue completa](../../DEPLOYMENT.md)
- [Bicep template](main.bicep)
- [GitHub Actions workflow](../../.github/workflows/azure-deploy.yml)

---

**Pro Tip**: Guarda estos comandos como aliases en tu `.bashrc` o `.zshrc`:

```bash
alias az-sinco-logs='az containerapp logs show --name ca-sincomaquinaria-prod --resource-group rg-sincomaquinaria-prod --follow'
alias az-sinco-url='az containerapp show --name ca-sincomaquinaria-prod --resource-group rg-sincomaquinaria-prod --query properties.configuration.ingress.fqdn -o tsv'
alias az-sinco-restart='az containerapp revision restart --name ca-sincomaquinaria-prod --resource-group rg-sincomaquinaria-prod'
```
