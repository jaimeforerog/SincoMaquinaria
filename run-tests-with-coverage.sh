#!/bin/bash
# Script para ejecutar tests con cobertura de código
# Uso: ./run-tests-with-coverage.sh

echo "=== SincoMaquinaria - Test Coverage Report ==="
echo ""

# Limpiar reportes anteriores
echo "Limpiando reportes anteriores..."
rm -rf TestResults
rm -rf coverage-report

# Ejecutar tests con cobertura
echo "Ejecutando tests con cobertura..."
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings --results-directory:TestResults

if [ $? -ne 0 ]; then
    echo "ERROR: Los tests fallaron"
    exit 1
fi

# Instalar ReportGenerator si no está instalado
echo "Verificando herramienta ReportGenerator..."
if ! dotnet tool list -g | grep -q "dotnet-reportgenerator-globaltool"; then
    echo "Instalando ReportGenerator..."
    dotnet tool install -g dotnet-reportgenerator-globaltool
fi

# Generar reporte HTML
echo "Generando reporte HTML..."
COVERAGE_FILE=$(find TestResults -name "coverage.cobertura.xml" | head -n 1)
if [ -n "$COVERAGE_FILE" ]; then
    reportgenerator -reports:"$COVERAGE_FILE" -targetdir:coverage-report -reporttypes:"Html;TextSummary"

    # Mostrar resumen en consola
    echo ""
    echo "=== Resumen de Cobertura ==="
    cat coverage-report/Summary.txt
    echo ""
    echo "Reporte completo generado en: coverage-report/index.html"
    echo "Abrir reporte: xdg-open coverage-report/index.html (Linux) o open coverage-report/index.html (Mac)"
else
    echo "ERROR: No se encontró archivo de cobertura"
    exit 1
fi

echo ""
echo "=== Completado ==="