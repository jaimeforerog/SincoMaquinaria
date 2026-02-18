# =======================================
# Stage 1: Build Frontend (React + Vite)
# =======================================
FROM node:20 AS node-build

WORKDIR /app/client-app
ENV NODE_OPTIONS="--max-old-space-size=4096"

# Copy package files
COPY client-app/package.json client-app/package-lock.json ./

# Install dependencies
RUN npm ci --legacy-peer-deps

# Copy frontend source
COPY client-app/ ./

# Build for production
RUN npm run build

# =======================================
# Stage 2: Build Backend (.NET 9)
# =======================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS dotnet-build

WORKDIR /src

# Copy csproj and restore dependencies
COPY src/SincoMaquinaria/SincoMaquinaria.csproj ./src/SincoMaquinaria/
RUN dotnet restore src/SincoMaquinaria/SincoMaquinaria.csproj

# Copy source code (exclude test project)
COPY src/SincoMaquinaria/ ./src/SincoMaquinaria/

# Publish only the main application project
RUN dotnet publish src/SincoMaquinaria/SincoMaquinaria.csproj -c Release -o /app/publish --no-restore

# =======================================
# Stage 3: Runtime
# =======================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy backend binaries from build stage
# Note: we are already in /app
COPY --from=dotnet-build /app/publish ./

# Copy frontend static files from node-build stage
COPY --from=node-build /app/client-app/dist ./wwwroot

# Create logs directory with proper permissions
RUN mkdir -p /app/logs && chmod 755 /app/logs

# Create non-root user for security
RUN groupadd --gid 1000 appuser && \
    useradd --uid 1000 --gid appuser --shell /bin/bash appuser && \
    chown -R appuser:appuser /app

USER appuser

# Expose port
EXPOSE 5000

# Configure health check
HEALTHCHECK --interval=30s --timeout=10s --retries=3 \
  CMD curl --fail http://localhost:5000/health || exit 1

# Set entry point
ENTRYPOINT ["dotnet", "SincoMaquinaria.dll"]
