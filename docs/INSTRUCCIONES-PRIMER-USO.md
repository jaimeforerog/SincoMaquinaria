# 🚀 Instrucciones de Primer Uso - SincoMaquinaria

## 📋 Información de Despliegue

- **URL de la Aplicación:** https://sincomaquinaria-app-1601.azurewebsites.net
- **Estado Actual:** Detenida (esperando reseteo de cuota en 24 horas)
- **Plan Azure:** F1 (Free) - Se recomienda actualizar a B1 para uso continuo

---

## ⏰ ¿Cuándo estará disponible la aplicación?

La aplicación estará disponible **mañana a las 9:00 AM** aproximadamente, cuando se resetee la cuota diaria del tier gratuito de Azure.

**Alternativamente**, puedes actualizar al plan B1 ahora mismo (~$13/mes) para acceso inmediato:

```powershell
az appservice plan update --name SincoMaquinariaPlan --resource-group SincoMaquinariaRG --sku B1
az webapp start --name sincomaquinaria-app-1601 --resource-group SincoMaquinariaRG
```

---

## 👤 Configuración del Primer Usuario

### Opción 1: Script Automatizado (Recomendado)

Ejecuta este script PowerShell desde la raíz del proyecto:

```powershell
.\setup-admin-user.ps1
```

**Personalizando las credenciales:**

```powershell
.\setup-admin-user.ps1 `
  -Email "tucorreo@ejemplo.com" `
  -Password "TuPasswordSegura123!" `
  -Nombre "Tu Nombre"
```

### Opción 2: Manual con cURL

```bash
curl -X POST https://sincomaquinaria-app-1601.azurewebsites.net/auth/setup \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@sincomaquinaria.com",
    "password": "Admin123!",
    "nombre": "Administrador"
  }'
```

### Opción 3: Manual con PowerShell

```powershell
$body = @{
    email = "admin@sincomaquinaria.com"
    password = "Admin123!"
    nombre = "Administrador"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://sincomaquinaria-app-1601.azurewebsites.net/auth/setup" `
  -Method Post `
  -Body $body `
  -ContentType "application/json"
```

---

## 🔐 Credenciales por Defecto

Si usas el script sin parámetros, se crearán estas credenciales:

| Campo | Valor |
|-------|-------|
| **Email** | admin@sincomaquinaria.com |
| **Password** | Admin123! |

**⚠️ IMPORTANTE:** Cambia la contraseña después del primer inicio de sesión.

---

## 📝 Pasos Completos para el Primer Uso

### 1️⃣ Verificar que la aplicación esté disponible

```powershell
# Opción A: Con navegador
Start-Process "https://sincomaquinaria-app-1601.azurewebsites.net"

# Opción B: Con PowerShell
Invoke-WebRequest -Uri "https://sincomaquinaria-app-1601.azurewebsites.net" -Method Head
```

Si recibes **Error 403** o **503**, la app aún no está disponible.

### 2️⃣ Crear el usuario administrador

```powershell
.\setup-admin-user.ps1
```

**Respuesta esperada:**
```
✓ Usuario administrador creado exitosamente!

=== CREDENCIALES DE ACCESO ===
Email:    admin@sincomaquinaria.com
Password: Admin123!
```

### 3️⃣ Iniciar sesión en la aplicación

1. Abre: https://sincomaquinaria-app-1601.azurewebsites.net
2. Ingresa tu email y password
3. ¡Listo! Ya puedes usar la aplicación

---

## 🔄 Crear Usuarios Adicionales

Una vez que tienes tu usuario administrador, puedes crear más usuarios desde:

### Opción A: Interfaz Web
- Navega a la sección de **Configuración** → **Usuarios**
- Click en **Agregar Usuario**

### Opción B: API (requiere token de admin)

```powershell
# 1. Primero, obtén el token de autenticación
$loginBody = @{
    email = "admin@sincomaquinaria.com"
    password = "Admin123!"
} | ConvertTo-Json

$authResponse = Invoke-RestMethod `
  -Uri "https://sincomaquinaria-app-1601.azurewebsites.net/auth/login" `
  -Method Post `
  -Body $loginBody `
  -ContentType "application/json"

$token = $authResponse.token

# 2. Crear nuevo usuario
$newUserBody = @{
    email = "usuario@ejemplo.com"
    password = "Password123!"
    nombre = "Nuevo Usuario"
    rol = "User"  # Puede ser "User" o "Admin"
} | ConvertTo-Json

Invoke-RestMethod `
  -Uri "https://sincomaquinaria-app-1601.azurewebsites.net/auth/register" `
  -Method Post `
  -Headers @{ Authorization = "Bearer $token" } `
  -Body $newUserBody `
  -ContentType "application/json"
```

---

## ❓ Solución de Problemas

### Error: "Ya existen usuarios en el sistema"

✅ **Esto es NORMAL**. Significa que ya creaste el usuario administrador previamente.

Simplemente inicia sesión con las credenciales que configuraste la primera vez.

### Error 403: "This web app is stopped"

La aplicación está detenida. Opciones:

1. **Esperar 24 horas** para que se resetee la cuota
2. **Actualizar al plan B1:**
   ```powershell
   az appservice plan update --name SincoMaquinariaPlan --resource-group SincoMaquinariaRG --sku B1
   az webapp start --name sincomaquinaria-app-1601 --resource-group SincoMaquinariaRG
   ```

### Error 503: "Service Unavailable"

La aplicación está iniciando. Espera 2-3 minutos y vuelve a intentar.

### No recuerdo mi contraseña

Si olvidaste la contraseña del administrador, deberás:

1. Conectarte a la base de datos PostgreSQL
2. Resetear la contraseña manualmente, O
3. Eliminar todos los usuarios y volver a ejecutar el setup

---

## 💡 Consejos

- 📸 **Toma screenshot** de las credenciales cuando las crees
- 🔒 **Guarda las credenciales** en un gestor de contraseñas
- 🔄 **Cambia la contraseña** por defecto en tu primer inicio de sesión
- 💾 **Haz backups** regulares de tu base de datos PostgreSQL

---

## 🆘 Comandos Útiles de Azure

```powershell
# Ver estado de la aplicación
az webapp show --name sincomaquinaria-app-1601 --resource-group SincoMaquinariaRG --query "state"

# Iniciar la aplicación
az webapp start --name sincomaquinaria-app-1601 --resource-group SincoMaquinariaRG

# Detener la aplicación
az webapp stop --name sincomaquinaria-app-1601 --resource-group SincoMaquinariaRG

# Ver logs en tiempo real
az webapp log tail --name sincomaquinaria-app-1601 --resource-group SincoMaquinariaRG

# Actualizar a plan B1 (recomendado)
az appservice plan update --name SincoMaquinariaPlan --resource-group SincoMaquinariaRG --sku B1
```

---

## 📞 Soporte

Si tienes problemas, verifica:
1. El walkthrough completo en: `walkthrough.md`
2. Los logs de Azure con: `az webapp log tail ...`
3. El estado de la app en: https://portal.azure.com
