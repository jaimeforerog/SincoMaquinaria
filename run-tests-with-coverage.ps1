# Script para ejecutar tests con cobertura de código
# Uso: .\run-tests-with-coverage.ps1

Write-Host "=== SincoMaquinaria - Test Coverage Report ===" -ForegroundColor Cyan
Write-Host ""

# Limpiar reportes anteriores
Write-Host "Limpiando reportes anteriores..." -ForegroundColor Yellow
if (Test-Path "TestResults") {
    Remove-Item -Recurse -Force TestResults
}
if (Test-Path "coverage-report") {
    Remove-Item -Recurse -Force coverage-report
}

# Ejecutar tests con cobertura
Write-Host "Ejecutando tests con cobertura..." -ForegroundColor Yellow
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings --results-directory:TestResults

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Los tests fallaron" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Instalar ReportGenerator si no está instalado
Write-Host "Verificando herramienta ReportGenerator..." -ForegroundColor Yellow
$reportGenInstalled = dotnet tool list -g | Select-String "dotnet-reportgenerator-globaltool"
if (-not $reportGenInstalled) {
    Write-Host "Instalando ReportGenerator..." -ForegroundColor Yellow
    dotnet tool install -g dotnet-reportgenerator-globaltool
}

# Generar reporte HTML
Write-Host "Generando reporte HTML..." -ForegroundColor Yellow
$coverageFile = Get-ChildItem -Path TestResults -Filter coverage.cobertura.xml -Recurse | Select-Object -First 1
if ($coverageFile) {
    reportgenerator -reports:"$($coverageFile.FullName)" -targetdir:coverage-report -reporttypes:"Html;TextSummary"

    # Mostrar resumen en consola
    Write-Host ""
    Write-Host "=== Resumen de Cobertura ===" -ForegroundColor Cyan
    Get-Content coverage-report\Summary.txt
    Write-Host ""
    Write-Host "Reporte completo generado en: coverage-report\index.html" -ForegroundColor Green
    Write-Host "Abrir reporte: start coverage-report\index.html" -ForegroundColor Yellow
} else {
    Write-Host "ERROR: No se encontró archivo de cobertura" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=== Completado ===" -ForegroundColor Green