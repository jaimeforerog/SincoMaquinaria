# SincoMaquinaria - Security Configuration Guide

## Table of Contents
1. [Quick Start](#quick-start)
2. [Environment Variables Configuration](#environment-variables-configuration)
3. [Development Setup](#development-setup)
4. [Production Setup](#production-setup)
5. [Docker Setup](#docker-setup)
6. [Security Best Practices](#security-best-practices)
7. [Troubleshooting](#troubleshooting)

---

## Quick Start

### First Time Setup

1. **Copy the environment template:**
   ```bash
   cp .env.example .env
   ```

2. **Generate a secure JWT key:**
   ```bash
   # On Linux/macOS
   openssl rand -base64 64

   # On Windows (PowerShell)
   [Convert]::ToBase64String((1..64 | ForEach-Object { Get-Random -Maximum 256 }))
   ```

3. **Edit `.env` file and update:**
   - `Jwt__Key` - Paste the generated key
   - `POSTGRES_PASSWORD` - Set a strong password (20+ characters)
   - `ConnectionStrings__DefaultConnection` - Update with your database password

4. **Verify `.env` is in `.gitignore`:**
   ```bash
   git check-ignore .env
   # Should output: .env
   ```

---

## Environment Variables Configuration

### Required Variables

These variables **MUST** be set before running the application:

| Variable | Description | Example |
|----------|-------------|---------|
| `Jwt__Key` | JWT signing key (min 32 chars) | `[generated-base64-key]` |
| `ConnectionStrings__DefaultConnection` | Database connection string | `host=localhost;port=5432;database=SincoMaquinaria;username=postgres;password=your_password` |
| `POSTGRES_PASSWORD` | PostgreSQL password | `Str0ng!P@ssw0rd123` |

### Optional Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `ASPNETCORE_ENVIRONMENT` | `Development` | Environment name |
| `ASPNETCORE_URLS` | `http://+:5000` | Listening URLs |
| `Jwt__Issuer` | `SincoMaquinaria` | JWT issuer claim |
| `Jwt__Audience` | `SincoMaquinariaApp` | JWT audience claim |
| `Jwt__ExpirationMinutes` | `480` | Token expiration (8 hours) |
| `Security__MaxFileUploadSizeMB` | `10` | Max upload size in MB |
| `Security__EnableAdminEndpoints` | `false` | Enable debug endpoints |

---

## Development Setup

### Option 1: Using .env file (Recommended)

1. Create `.env` from template:
   ```bash
   cp .env.example .env
   ```

2. Update `.env` with development values:
   ```ini
   Jwt__Key=dev_key_minimum_32_chars_long_for_testing_purposes_only
   POSTGRES_PASSWORD=postgres
   ConnectionStrings__DefaultConnection=host=localhost;port=5432;database=SincoMaquinaria;username=postgres;password=postgres
   ASPNETCORE_ENVIRONMENT=Development
   Security__EnableAdminEndpoints=true
   ```

3. Run the application:
   ```bash
   dotnet run
   ```

### Option 2: Using User Secrets (Alternative)

1. Initialize user secrets:
   ```bash
   dotnet user-secrets init
   ```

2. Set secrets:
   ```bash
   dotnet user-secrets set "Jwt:Key" "your-generated-key-here"
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "host=localhost;port=5432;database=SincoMaquinaria;username=postgres;password=your_password"
   ```

3. Run the application:
   ```bash
   dotnet run
   ```

### Option 3: Environment Variables (Command Line)

**Linux/macOS:**
```bash
export Jwt__Key="your-jwt-key"
export ConnectionStrings__DefaultConnection="host=localhost;port=5432;database=SincoMaquinaria;username=postgres;password=postgres"
dotnet run
```

**Windows (PowerShell):**
```powershell
$env:Jwt__Key="your-jwt-key"
$env:ConnectionStrings__DefaultConnection="host=localhost;port=5432;database=SincoMaquinaria;username=postgres;password=postgres"
dotnet run
```

---

## Production Setup

### Security Checklist

- [ ] Generate a strong, random JWT key (min 64 characters)
- [ ] Use a strong database password (20+ characters, mixed case, numbers, symbols)
- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Enable HTTPS and disable HTTP
- [ ] Update CORS origins to actual frontend domain
- [ ] Set `Security__EnableAdminEndpoints=false`
- [ ] Use a secrets manager (Azure Key Vault, AWS Secrets Manager)
- [ ] Enable SSL for PostgreSQL connection
- [ ] Set up firewall rules
- [ ] Configure proper logging (exclude sensitive data)

### Azure App Service

1. **Navigate to Configuration > Application Settings**

2. **Add the following settings:**
   ```
   Name: Jwt__Key
   Value: [your-production-jwt-key]

   Name: ConnectionStrings__DefaultConnection
   Value: [your-connection-string-with-ssl]

   Name: ASPNETCORE_ENVIRONMENT
   Value: Production

   Name: Security__AllowedOrigins__0
   Value: https://your-frontend-domain.com
   ```

3. **Use Azure Key Vault for sensitive values:**
   ```csharp
   // In Program.cs
   builder.Configuration.AddAzureKeyVault(
       new Uri($"https://{keyVaultName}.vault.azure.net/"),
       new DefaultAzureCredential());
   ```

### AWS Elastic Beanstalk

1. **Create secrets in AWS Secrets Manager**

2. **Configure environment properties:**
   ```json
   {
     "Jwt__Key": "{{resolve:secretsmanager:sinco/jwt:SecretString:key}}",
     "ConnectionStrings__DefaultConnection": "{{resolve:secretsmanager:sinco/db:SecretString:connectionString}}"
   }
   ```

### Docker Production

1. **Create production `.env` file (DO NOT COMMIT):**
   ```ini
   Jwt__Key=[generated-production-key]
   POSTGRES_PASSWORD=[strong-password]
   ConnectionStrings__DefaultConnection=host=db;port=5432;database=SincoMaquinaria;username=postgres;password=[strong-password]
   ASPNETCORE_ENVIRONMENT=Production
   Security__EnableAdminEndpoints=false
   Security__AllowedOrigins__0=https://your-domain.com
   ```

2. **Run with docker-compose:**
   ```bash
   docker-compose -f docker-compose.prod.yml up -d
   ```

---

## Docker Setup

### Development with Docker

1. **Copy and configure `.env`:**
   ```bash
   cp .env.example .env
   # Edit .env with your values
   ```

2. **Build and run:**
   ```bash
   docker-compose up -d
   ```

3. **View logs:**
   ```bash
   docker-compose logs -f backend
   ```

### Docker Compose Environment Variables

Docker Compose automatically loads `.env` file. Variables are mapped as:

```yaml
environment:
  - Jwt__Key=${Jwt__Key}
  - ConnectionStrings__DefaultConnection=${ConnectionStrings__DefaultConnection}
```

---

## Security Best Practices

### JWT Key Management

1. **Generate strong keys:**
   ```bash
   # Generate 64-byte key (recommended)
   openssl rand -base64 64
   ```

2. **Never commit keys to version control**

3. **Rotate keys periodically** (every 3-6 months)

4. **Use different keys per environment** (dev, staging, production)

### Database Security

1. **Use strong passwords:**
   - Minimum 20 characters
   - Mix of uppercase, lowercase, numbers, symbols
   - No dictionary words

2. **Enable SSL for connections:**
   ```
   ConnectionStrings__DefaultConnection=host=localhost;port=5432;database=SincoMaquinaria;username=postgres;password=your_password;SSL Mode=Require;Trust Server Certificate=false
   ```

3. **Restrict database access:**
   - Use firewall rules
   - Whitelist IP addresses
   - Use VPCs/Virtual Networks

### CORS Configuration

Production should only allow specific origins:

```ini
Security__AllowedOrigins__0=https://app.yourdomain.com
Security__AllowedOrigins__1=https://www.yourdomain.com
```

Never use `*` or `http://` in production.

### HTTPS Configuration

**Production must use HTTPS:**

```ini
ASPNETCORE_URLS=https://+:5001
ASPNETCORE_Kestrel__Certificates__Default__Path=/path/to/cert.pfx
ASPNETCORE_Kestrel__Certificates__Default__Password=cert_password
```

Or use a reverse proxy (nginx, IIS, Azure App Gateway).

---

## Troubleshooting

### Error: "JWT Key not configured"

**Cause:** `Jwt__Key` is empty or not set.

**Solution:**
1. Check `.env` file exists and contains `Jwt__Key`
2. Verify environment variable is set: `echo $Jwt__Key` (Linux) or `$env:Jwt__Key` (Windows)
3. Restart application after setting variable

### Error: "Connection string not found"

**Cause:** `ConnectionStrings__DefaultConnection` not set.

**Solution:**
1. Set in `.env` file
2. Or set environment variable with double underscores: `ConnectionStrings__DefaultConnection`

### Error: "Cannot connect to PostgreSQL"

**Cause:** Database not running or wrong credentials.

**Solution:**
1. Verify PostgreSQL is running: `docker ps` or `systemctl status postgresql`
2. Check connection string format
3. Verify password matches database password
4. Test connection: `psql -h localhost -U postgres -d SincoMaquinaria`

### Docker: "Environment variable not found"

**Cause:** `.env` file not in same directory as `docker-compose.yml`.

**Solution:**
1. Ensure `.env` is in root directory
2. Check file permissions: `chmod 644 .env`
3. Restart containers: `docker-compose down && docker-compose up -d`

---

## Additional Resources

- [ASP.NET Core Configuration](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [User Secrets in Development](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [Azure Key Vault](https://learn.microsoft.com/en-us/azure/key-vault/)
- [JWT Best Practices](https://tools.ietf.org/html/rfc8725)
- [OWASP Security Guidelines](https://owasp.org/www-project-web-security-testing-guide/)

---

## Support

For security issues, please contact the development team directly. Do not create public issues for security vulnerabilities.
