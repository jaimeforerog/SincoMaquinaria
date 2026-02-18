# 📘 Explicación Completa del Despliegue en Azure con CLI

Este documento explica **paso a paso** todos los comandos de Azure CLI que se ejecutaron durante el despliegue de SincoMaquinaria.

---

## 🔐 Paso 0: Autenticación en Azure

### Comando Ejecutado:
```powershell
az login --tenant 8b534db0-e52d-4583-aa00-d17e0150af15
```

### ¿Qué hace?
Autentica tu sesión en Azure CLI con tu cuenta de Azure.

### Desglose de Parámetros:
- `az login`: Comando base para autenticación
- `--tenant`: Especifica el ID del directorio/tenant de Azure
  - Valor usado: `8b534db0-e52d-4583-aa00-d17e0150af15`
  - Esto asegura que te conectes al tenant correcto si tienes múltiples

### Resultado:
- Se abrió un navegador web
- Seleccionaste la cuenta `jrfgonz@hotmail.com`
- Se autenticó con la suscripción "Azure subscription 1" (ID: `502f91f9-d690-4c3f-a879-d2f6ecbc896c`)

---

## 📦 Paso 1: Crear Resource Group

### Comando Ejecutado:
```powershell
az group create --name SincoMaquinariaRG --location centralus
```

### ¿Qué hace?
Crea un **Resource Group** (grupo de recursos), que es un contenedor lógico donde se almacenarán todos los recursos de la aplicación.

### Desglose de Parámetros:
- `az group create`: Comando para crear un resource group
- `--name SincoMaquinariaRG`: Nombre del resource group
- `--location centralus`: Región de Azure donde se crearán los recursos
  - Otras opciones: `eastus`, `westus`, `westeurope`, etc.

### ¿Por qué un Resource Group?
- Agrupa recursos relacionados
- Facilita la gestión y eliminación conjunta
- Permite aplicar políticas y permisos a nivel de grupo
- Facilita el seguimiento de costos

### Resultado:
```json
{
  "id": "/subscriptions/502f91f9-d690-4c3f-a879-d2f6ecbc896c/resourceGroups/SincoMaquinariaRG",
  "location": "centralus",
  "name": "SincoMaquinariaRG",
  "properties": {
    "provisioningState": "Succeeded"
  }
}
```

---

## 🐳 Paso 2: Crear Azure Container Registry (ACR)

### Comando Ejecutado:
```powershell
az acr create `
  --resource-group SincoMaquinariaRG `
  --name sincomaquinariaacr1045 `
  --sku Basic `
  --admin-enabled true
```

### ¿Qué hace?
Crea un **Azure Container Registry**, un registro privado de imágenes Docker donde se almacenará la imagen de tu aplicación.

### Desglose de Parámetros:
- `az acr create`: Comando para crear un container registry
- `--resource-group SincoMaquinariaRG`: Resource group donde se crea
- `--name sincomaquinariaacr1045`: Nombre único del registry
  - Debe ser único globalmente (por eso el sufijo numérico aleatorio)
  - Solo puede contener letras minúsculas y números
- `--sku Basic`: Nivel de servicio
  - `Basic`: $5/mes, 10 GB almacenamiento, para desarrollo
  - `Standard`: $20/mes, 100 GB, para producción
  - `Premium`: $100/mes, 500 GB, geo-replicación
- `--admin-enabled true`: Habilita credenciales de administrador
  - Permite autenticación con usuario/contraseña
  - Necesario para que App Service pueda descargar la imagen

### ¿Qué es ACR vs Docker Hub?
- **ACR**: Privado, integrado con Azure, mejor rendimiento en Azure
- **Docker Hub**: Público por defecto, límites de descarga gratuitos

### Resultado:
- Registry creado: `sincomaquinariaacr1045.azurecr.io`
- Estado: `Succeeded`

---

## 🔑 Paso 2.1: Obtener Credenciales del ACR

### Comandos Ejecutados:
```powershell
# Obtener nombre de usuario
$ACRUsername = az acr credential show --name sincomaquinariaacr1045 --query "username" --output tsv

# Obtener contraseña
$ACRPassword = az acr credential show --name sincomaquinariaacr1045 --query "passwords[0].value" --output tsv

# Obtener URL del login server
$ACRLoginServer = az acr show --name sincomaquinariaacr1045 --query "loginServer" --output tsv
```

### ¿Qué hacen?
Obtienen las credenciales necesarias para:
1. Subir imágenes Docker al registry
2. Configurar App Service para descargar la imagen

### Desglose de Parámetros:
- `az acr credential show`: Muestra las credenciales de admin del ACR
- `--query`: Filtro JMESPath para extraer valores específicos
  - `"username"`: Extrae solo el nombre de usuario
  - `"passwords[0].value"`: Extrae la primera contraseña
- `--output tsv`: Formato de salida (tab-separated values, sin formato)
  - Otras opciones: `json`, `table`, `yaml`

### Variables Resultantes:
- `$ACRUsername`: sincomaquinariaacr1045
- `$ACRPassword`: [contraseña generada automáticamente]
- `$ACRLoginServer`: sincomaquinariaacr1045.azurecr.io

---

## 🏗️ Paso 3: Construir y Subir Imagen Docker

### Comando Ejecutado:
```powershell
az acr build `
  --registry sincomaquinariaacr1045 `
  --resource-group SincoMaquinariaRG `
  --image sincomaquinaria:latest `
  .
```

### ¿Qué hace?
Construye la imagen Docker **en la nube** (no en tu máquina local) y la sube automáticamente al ACR.

### Desglose de Parámetros:
- `az acr build`: Servicio de construcción de ACR (ACR Tasks)
- `--registry sincomaquinariaacr1045`: ACR donde subir la imagen
- `--resource-group SincoMaquinariaRG`: Resource group del ACR
- `--image sincomaquinaria:latest`: Nombre y tag de la imagen
  - Formato: `nombre:tag`
  - `latest` es el tag por defecto para la última versión
- `.`: Contexto de construcción (directorio actual)
  - Busca un `Dockerfile` en este directorio
  - Envía todos los archivos necesarios a Azure

### ¿Por qué `az acr build` en lugar de `docker build`?
| Aspecto | `az acr build` | `docker build` + `docker push` |
|---------|----------------|-------------------------------|
| Construcción | En la nube | En tu máquina local |
| Velocidad | Más rápido (mejor ancho de banda) | Depende de tu conexión |
| Recursos | Usa recursos de Azure | Usa tu CPU/RAM |
| Subida | Automática | Manual con `docker push` |

### Proceso Interno:
1. Comprime el código fuente y lo sube a Azure
2. Ejecuta `docker build` en un servidor de Azure
3. Sube automáticamente la imagen al ACR
4. Limpia recursos temporales

### Resultado:
- Imagen creada: `sincomaquinariaacr1045.azurecr.io/sincomaquinaria:latest`
- Tamaño aproximado: ~500 MB (backend .NET + frontend React)

---

## 🗄️ Paso 4: Crear Servidor PostgreSQL

### Comando Ejecutado:
```powershell
az postgres flexible-server create `
  --resource-group SincoMaquinariaRG `
  --name sincomaquinariadb-3038 `
  --location centralus `
  --admin-user sincoadmin `
  --admin-password '+=wi^hCOf/76LJk;;auUVP{!' `
  --sku-name Standard_B1ms `
  --tier Burstable `
  --public-access 0.0.0.0 `
  --storage-size 32 `
  --version 14 `
  --yes
```

### ¿Qué hace?
Crea un **servidor PostgreSQL Flexible Server** en Azure para la base de datos de la aplicación.

### Desglose de Parámetros:
- `az postgres flexible-server create`: Crea un servidor PostgreSQL flexible
- `--resource-group SincoMaquinariaRG`: Resource group
- `--name sincomaquinariadb-3038`: Nombre único del servidor
  - El FQDN será: `sincomaquinariadb-3038.postgres.database.azure.com`
- `--location centralus`: Misma región que los demás recursos (menor latencia)
- `--admin-user sincoadmin`: Usuario administrador de PostgreSQL
- `--admin-password '+=wi^hCOf/76LJk;;auUVP{!'`: Contraseña segura generada
- `--sku-name Standard_B1ms`: Tipo de máquina virtual
  - `B1ms`: 1 vCore, 2 GB RAM (~$12/mes)
  - Otras opciones: `B2s` (2 vCores), `D2s_v3` (2 vCores, más potente)
- `--tier Burstable`: Nivel de rendimiento
  - `Burstable`: Para cargas variables (desarrollo/pruebas)
  - `GeneralPurpose`: Para producción estable
  - `MemoryOptimized`: Para bases de datos grandes
- `--public-access 0.0.0.0`: Permitir acceso desde cualquier IP de Azure
  - Para producción, se recomienda restringir IPs específicas
- `--storage-size 32`: 32 GB de almacenamiento SSD
- `--version 14`: PostgreSQL versión 14
- `--yes`: Confirmar sin preguntar

### SKU Explained:
- **Standard_B1ms**: 
  - `Standard`: Familia estándar
  - `B`: Serie Burstable (ráfaga)
  - `1`: 1 vCore
  - `ms`: Memory Standard

### Resultado:
- Servidor: `sincomaquinariadb-3038.postgres.database.azure.com`
- Puerto: 5432 (por defecto)
- SSL: Habilitado por defecto

---

## 📊 Paso 4.1: Crear Base de Datos

### Comando Ejecutado:
```powershell
az postgres flexible-server db create `
  --resource-group SincoMaquinariaRG `
  --server-name sincomaquinariadb-3038 `
  --database-name SincoMaquinaria
```

### ¿Qué hace?
Crea la base de datos específica dentro del servidor PostgreSQL.

### Desglose de Parámetros:
- `az postgres flexible-server db create`: Crea una base de datos
- `--resource-group SincoMaquinariaRG`: Resource group del servidor
- `--server-name sincomaquinariadb-3038`: Servidor donde crear la BD
- `--database-name SincoMaquinaria`: Nombre de la base de datos

### Diferencia Servidor vs Base de Datos:
- **Servidor**: Instancia de PostgreSQL (máquina virtual con PostgreSQL instalado)
- **Base de Datos**: Base de datos específica dentro del servidor
- Un servidor puede tener múltiples bases de datos

---

## 🔗 Paso 4.2: Construir Connection String

### Código Ejecutado:
```powershell
$ConnectionString = "Host=sincomaquinariadb-3038.postgres.database.azure.com;Database=SincoMaquinaria;Username=sincoadmin;Password=+=wi^hCOf/76LJk;;auUVP{!;SSL Mode=Require;Trust Server Certificate=true"
```

### ¿Qué hace?
Construye la cadena de conexión que usará la aplicación para conectarse a PostgreSQL.

### Formato del Connection String:
```
Host={servidor};Database={bd};Username={user};Password={pwd};SSL Mode=Require;Trust Server Certificate=true
```

### Componentes:
- `Host`: FQDN del servidor PostgreSQL
- `Database`: Nombre de la base de datos
- `Username`: Usuario administrador
- `Password`: Contraseña del usuario
- `SSL Mode=Require`: Forzar conexión SSL/TLS encriptada
- `Trust Server Certificate=true`: Confiar en el certificado del servidor
  - En producción, deberías validar el certificado

---

## 🖥️ Paso 5: Crear App Service Plan

### Comando Ejecutado:
```powershell
az appservice plan create `
  --name SincoMaquinariaPlan `
  --resource-group SincoMaquinariaRG `
  --sku F1 `
  --is-linux
```

### ¿Qué hace?
Crea un **App Service Plan**, que define los recursos de cómputo (CPU, RAM) para hospedar aplicaciones web.

### Desglose de Parámetros:
- `az appservice plan create`: Crea un plan de App Service
- `--name SincoMaquinariaPlan`: Nombre del plan
- `--resource-group SincoMaquinariaRG`: Resource group
- `--sku F1`: Nivel de servicio
  - `F1`: **Free** - Gratis, 60 min CPU/día, 1 GB RAM
  - `B1`: **Basic** - $13/mes, CPU ilimitada, 1.75 GB RAM
  - `S1`: **Standard** - $70/mes, auto-scaling, staging slots
  - `P1v2`: **Premium** - $150/mes, mejor rendimiento
- `--is-linux`: Usar contenedores Linux (requerido para Docker)

### ¿Qué es un App Service Plan?
Piensa en ello como el "servidor" o "máquina virtual" donde correrán tus aplicaciones:
- Múltiples Web Apps pueden compartir el mismo plan
- El plan define: CPU, RAM, disco, ubicación
- Pagas por el plan, no por cada app

### Limitaciones del Tier F1:
- ⚠️ 60 minutos de CPU por día
- ⚠️ 1 GB RAM
- ⚠️ 1 GB almacenamiento
- ⚠️ No custom domains
- ⚠️ No SSL personalizado
- ✅ Ideal para desarrollo/demos

---

## 🌐 Paso 6: Crear Web App for Containers

### Comando Ejecutado:
```powershell
az webapp create `
  --resource-group SincoMaquinariaRG `
  --plan SincoMaquinariaPlan `
  --name sincomaquinaria-app-1601 `
  --deployment-container-image-name "sincomaquinariaacr1045.azurecr.io/sincomaquinaria:latest"
```

### ¿Qué hace?
Crea una **Web App** que ejecutará tu contenedor Docker.

### Desglose de Parámetros:
- `az webapp create`: Crea una aplicación web
- `--resource-group SincoMaquinariaRG`: Resource group
- `--plan SincoMaquinariaPlan`: App Service Plan a usar
- `--name sincomaquinaria-app-1601`: Nombre único de la app
  - El dominio será: `sincomaquinaria-app-1601.azurewebsites.net`
  - Debe ser único globalmente
- `--deployment-container-image-name`: Imagen Docker a desplegar
  - Formato: `{registry}/{imagen}:{tag}`
  - Apunta a la imagen que acabamos de construir en ACR

### URLs Generadas:
- **Producción**: https://sincomaquinaria-app-1601.azurewebsites.net
- **SCM/Kudu**: https://sincomaquinaria-app-1601.scm.azurewebsites.net
- **FTP**: ftps://waws-prod-dm1-179.ftp.azurewebsites.windows.net

---

## ⚙️ Paso 6.1: Configurar Variables de Entorno

### Comando Ejecutado:
```powershell
az webapp config appsettings set `
  --resource-group SincoMaquinariaRG `
  --name sincomaquinaria-app-1601 `
  --settings `
    ASPNETCORE_ENVIRONMENT="Production" `
    ConnectionStrings__DefaultConnection=$ConnectionString `
    Jwt__Key="kxk6k05Mr96g/UzxBPzBYPx1/q9y6PAlEJTVw2eNTB28VL/bVKoP7ZpnffKPKeIvqipI4I3iyDv9EtPTTsGokQ==" `
    DOCKER_REGISTRY_SERVER_URL="https://sincomaquinariaacr1045.azurecr.io" `
    DOCKER_REGISTRY_SERVER_USERNAME=$ACRUsername `
    DOCKER_REGISTRY_SERVER_PASSWORD=$ACRPassword `
    WEBSITES_PORT=5000
```

### ¿Qué hace?
Configura las **variables de entorno** que tu aplicación necesita para funcionar.

### Desglose de Variables:

#### 1. `ASPNETCORE_ENVIRONMENT="Production"`
- Define el entorno de ASP.NET Core
- Valores comunes: `Development`, `Staging`, `Production`
- Afecta: logging, error pages, optimizaciones

#### 2. `ConnectionStrings__DefaultConnection=$ConnectionString`
- Cadena de conexión a PostgreSQL
- Formato especial: `__` (doble underscore) representa `:` en JSON
- Se traduce a: `{ "ConnectionStrings": { "DefaultConnection": "..." } }`

#### 3. `Jwt__Key="[clave secreta]"`
- Clave secreta para firmar tokens JWT
- Debe ser una cadena larga y aleatoria
- **Crítico para seguridad**: nunca la compartas públicamente

#### 4. `DOCKER_REGISTRY_SERVER_URL`
- URL del Azure Container Registry
- Permite a App Service autenticarse en el registry privado

#### 5. `DOCKER_REGISTRY_SERVER_USERNAME` y `PASSWORD`
- Credenciales para descargar la imagen del ACR
- App Service las usa automáticamente al hacer pull de la imagen

#### 6. `WEBSITES_PORT=5000`
- Puerto donde tu aplicación escucha
- App Service redirige el tráfico HTTP/HTTPS a este puerto
- Tu Dockerfile debe exponer este mismo puerto

### ¿Por qué Variables de Entorno?
- ✅ Separa configuración del código
- ✅ Permite cambiar configs sin recompilar
- ✅ Seguridad: no commits de secretos al código
- ✅ Diferentes configs por entorno (dev/staging/prod)

---

## 📊 Resumen Visual del Flujo

```
┌─────────────────────────────────────────────────────────────┐
│  1. AUTENTICACIÓN                                           │
│  az login --tenant [tenant-id]                             │
└─────────────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────┐
│  2. RESOURCE GROUP                                          │
│  az group create --name SincoMaquinariaRG                   │
│                  --location centralus                        │
└─────────────────────────────────────────────────────────────┘
                          │
          ┌───────────────┴───────────────┐
          ▼                               ▼
┌─────────────────────┐         ┌─────────────────────┐
│  3. ACR             │         │  4. PostgreSQL      │
│  az acr create      │         │  az postgres ...    │
│                     │         │                     │
│  + Obtener creds    │         │  + Crear database   │
│  + BUILD imagen     │         │  + Connection str   │
└─────────────────────┘         └─────────────────────┘
          │                               │
          └───────────────┬───────────────┘
                          ▼
┌─────────────────────────────────────────────────────────────┐
│  5. APP SERVICE PLAN                                         │
│  az appservice plan create --sku F1 --is-linux              │
└─────────────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────┐
│  6. WEB APP                                                  │
│  az webapp create --deployment-container-image-name ...     │
│                                                              │
│  + Configurar variables de entorno                          │
└─────────────────────────────────────────────────────────────┘
                          │
                          ▼
              ✅ Aplicación Desplegada
```

---

## 🎯 Comandos de Verificación Post-Despliegue

### Ver estado de la aplicación:
```bash
az webapp show \
  --name sincomaquinaria-app-1601 \
  --resource-group SincoMaquinariaRG \
  --query "{Estado:state, URL:defaultHostName}" \
  --output table
```

### Ver logs en tiempo real:
```bash
az webapp log tail \
  --name sincomaquinaria-app-1601 \
  --resource-group SincoMaquinariaRG
```

### Ver configuración actual:
```bash
az webapp config appsettings list \
  --name sincomaquinaria-app-1601 \
  --resource-group SincoMaquinariaRG \
  --output table
```

### Reiniciar la aplicación:
```bash
az webapp restart \
  --name sincomaquinaria-app-1601 \
  --resource-group SincoMaquinariaRG
```

### Ver información del plan:
```bash
az appservice plan show \
  --name SincoMaquinariaPlan \
  --resource-group SincoMaquinariaRG \
  --query "{Plan:name, SKU:sku.name, Estado:status}" \
  --output table
```

---

## 💰 Gestión de Costos

### Ver recursos y sus costos:
```bash
# Listar todos los recursos del resource group
az resource list \
  --resource-group SincoMaquinariaRG \
  --output table

# Ver consumo (requiere algunos días de datos)
az consumption usage list \
  --output table
```

### Detener la app para ahorrar cuota:
```bash
az webapp stop \
  --name sincomaquinaria-app-1601 \
  --resource-group SincoMaquinariaRG
```

### Iniciar la app nuevamente:
```bash
az webapp start \
  --name sincomaquinaria-app-1601 \
  --resource-group SincoMaquinariaRG
```

---

## 🔧 Comandos de Mantenimiento

### Actualizar la imagen Docker:
```bash
# 1. Reconstruir imagen con nuevos cambios
az acr build \
  --registry sincomaquinariaacr1045 \
  --resource-group SincoMaquinariaRG \
  --image sincomaquinaria:latest \
  .

# 2. Reiniciar la app para que use la nueva imagen
az webapp restart \
  --name sincomaquinaria-app-1601 \
  --resource-group SincoMaquinariaRG
```

### Escalar verticalmente (cambiar SKU):
```bash
# Actualizar a Basic B1 (~$13/mes)
az appservice plan update \
  --name SincoMaquinariaPlan \
  --resource-group SincoMaquinariaRG \
  --sku B1

# Volver a Free F1
az appservice plan update \
  --name SincoMaquinariaPlan \
  --resource-group SincoMaquinariaRG \
  --sku F1
```

### Escalar horizontalmente (más instancias):
```bash
# Solo disponible en SKU S1 o superior
az appservice plan update \
  --name SincoMaquinariaPlan \
  --resource-group SincoMaquinariaRG \
  --number-of-workers 2
```

---

## 🗑️ Limpieza y Eliminación

### Eliminar todo (destruye todos los recursos):
```bash
az group delete \
  --name SincoMaquinariaRG \
  --yes \
  --no-wait
```

**⚠️ ADVERTENCIA**: Esto eliminará:
- Web App
- App Service Plan
- Container Registry (y todas las imágenes)
- Servidor PostgreSQL (y todas las bases de datos)
- **TODOS los datos se perderán permanentemente**

### Eliminar solo la Web App (mantener BD y registry):
```bash
az webapp delete \
  --name sincomaquinaria-app-1601 \
  --resource-group SincoMaquinariaRG
```

---

## 📚 Recursos Adicionales

### Documentación Oficial:
- [Azure CLI Reference](https://docs.microsoft.com/en-us/cli/azure/)
- [App Service Docs](https://docs.microsoft.com/en-us/azure/app-service/)
- [Container Registry Docs](https://docs.microsoft.com/en-us/azure/container-registry/)
- [PostgreSQL Flexible Server Docs](https://docs.microsoft.com/en-us/azure/postgresql/flexible-server/)

### Herramientas Útiles:
- [Azure Portal](https://portal.azure.com) - Interfaz gráfica
- [Azure DevOps](https://dev.azure.com) - CI/CD pipelines
- [Azure Monitor](https://portal.azure.com/#blade/Microsoft_Azure_Monitoring) - Monitoreo y alertas

### Comandos de Ayuda:
```bash
# Ayuda general de Azure CLI
az --help

# Ayuda de un comando específico
az webapp create --help

# Listar todas las ubicaciones disponibles
az account list-locations --output table

# Ver tu suscripción actual
az account show
```

---

## 🎓 Conceptos Clave Aprendidos

1. **Resource Groups**: Contenedores lógicos para organizar recursos
2. **Container Registry**: Almacenamiento privado de imágenes Docker  
3. **ACR Build**: Construcción de imágenes en la nube
4. **App Service Plan**: Define los recursos de cómputo
5. **Web App**: La aplicación en sí, usa el plan
6. **Environment Variables**: Configuración separada del código
7. **SKUs**: Niveles de servicio con diferentes capacidades y costos
8. **Flexible Server**: PostgreSQL gestionado por Azure

---

## ✅ Checklist de Verificación

Después del despliegue, verifica:

- [ ] Resource Group creado: `SincoMaquinariaRG`
- [ ] ACR creado: `sincomaquinariaacr1045.azurecr.io`
- [ ] Imagen Docker construida y subida
- [ ] PostgreSQL servidor creado: `sincomaquinariadb-3038`
- [ ] Base de datos `SincoMaquinaria` creada
- [ ] App Service Plan creado: `SincoMaquinariaPlan`
- [ ] Web App creada: `sincomaquinaria-app-1601`
- [ ] Variables de entorno configuradas
- [ ] Aplicación accesible en: https://sincomaquinaria-app-1601.azurewebsites.net

---

**Fecha de este despliegue**: 12 de enero de 2026
**Suscripción**: Azure subscription 1
**Cuenta**: jrfgonz@hotmail.com
