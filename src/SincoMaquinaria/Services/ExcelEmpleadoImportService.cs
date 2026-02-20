using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExcelDataReader;
using Marten;
using Microsoft.Extensions.Logging;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Domain.Events;

namespace SincoMaquinaria.Services;

public class ExcelEmpleadoImportService
{
    private readonly IDocumentSession _session;
    private readonly ILogger<ExcelEmpleadoImportService> _logger;

    public ExcelEmpleadoImportService(IDocumentSession session, ILogger<ExcelEmpleadoImportService> logger)
    {
        _session = session;
        _logger = logger;
    }

    public async Task<int> ImportarEmpleados(Stream fileStream, Guid? usuarioId = null, string? usuarioNombre = null)
    {
        // Allowed positions - Get from enum
        var cargosValidos = EnumExtensions.GetEnumValues<CargoEmpleado>();
        
        using var reader = ExcelReaderFactory.CreateReader(fileStream);
        var result = reader.AsDataSet(new ExcelDataSetConfiguration()
        {
            ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
            {
                UseHeaderRow = true
            }
        });

        var table = result.Tables[0];
        int count = 0;
        var validationErrors = new List<string>();
        
        // Check for duplicates within the file
        var documentosEnArchivo = new HashSet<string>();

        // Check against existing employees to avoid duplicates
        var empleadosExistentes = await InfoEmpleados()
                                                .Select(e => e.Identificacion)
                                                .ToListAsync();
        var documentosExistentes = new HashSet<string>(empleadosExistentes, StringComparer.OrdinalIgnoreCase);

        // Normalize columns map
        var colMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for(int i=0; i<table.Columns.Count; i++) 
            colMap[table.Columns[i].ColumnName] = i;

        // Helper to get value
        string? GetVal(System.Data.DataRow r, string key) 
        {
            if (colMap.TryGetValue(key, out int idx)) return r[idx]?.ToString()?.Trim();
            // Fallback for fuzzy match
            var fuzzy = colMap.Keys.FirstOrDefault(k => k.Contains(key, StringComparison.OrdinalIgnoreCase));
            if (fuzzy != null) return r[colMap[fuzzy]]?.ToString()?.Trim();
            return null;
        }

        foreach (System.Data.DataRow row in table.Rows)
        {
            var rowNum = table.Rows.IndexOf(row) + 2; // +2 considering header and 0-index

            var nombres = GetVal(row, "Nombres");
            var apellidos = GetVal(row, "Apellidos");
            var nombre = (!string.IsNullOrEmpty(nombres) || !string.IsNullOrEmpty(apellidos)) 
                ? $"{nombres} {apellidos}".Trim() 
                : GetVal(row, "Nombre");

            var documento = GetVal(row, "No. Identificación") ?? GetVal(row, "Identificacion") ?? GetVal(row, "Documento");
            
            // Cargo: Try to get it, otherwise default to "Operario" if missing in template
            var cargo = GetVal(row, "Cargo");
            if (string.IsNullOrEmpty(cargo)) 
            {
                cargo = "Operario"; // Default for templates without Cargo column
            }
            
            var especialidad = GetVal(row, "Especialidad");
            
            // Valor Hora: Match "Valor $ (Hr)" or "Valor hora"
            var valorHoraStr = GetVal(row, "Valor $ (Hr)") ?? GetVal(row, "Valor hora") ?? "0";
            decimal.TryParse(valorHoraStr, out var valorHora); 
            
            // Default "Activo"
            var estado = EstadoEquipo.Activo;

            // Validations
            if (string.IsNullOrEmpty(nombre))
            {
                validationErrors.Add($"Fila {rowNum}: El nombre es obligatorio.");
            }

            if (string.IsNullOrEmpty(documento))
            {
                validationErrors.Add($"Fila {rowNum}: El documento de identidad es obligatorio.");
            }
            else
            {
               if (documentosEnArchivo.Contains(documento))
               {
                   validationErrors.Add($"Fila {rowNum}: El documento '{documento}' está duplicado en el archivo.");
               }
               if (documentosExistentes.Contains(documento))
               {
                   validationErrors.Add($"Fila {rowNum}: El documento '{documento}' ya existe en el sistema.");
               }
               documentosEnArchivo.Add(documento);
            }

            if (string.IsNullOrEmpty(cargo))
            {
                 validationErrors.Add($"Fila {rowNum}: El cargo es obligatorio.");
            }
            else if (!cargo.IsValidEnum<CargoEmpleado>())
            {
                 var valoresPermitidos = string.Join(", ", cargosValidos);
                 validationErrors.Add($"Fila {rowNum}: El cargo '{cargo}' no es válido. Valores permitidos: {valoresPermitidos}.");
            }
            else
            {
                // Normalize: "1" -> "Operario", "operario" -> "Operario"
                cargo = cargo.ToEnum<CargoEmpleado>().ToString();
            }

            if (validationErrors.Any()) continue;

            // Process
            var empleadoId = Guid.NewGuid();
            _session.Events.StartStream<Empleado>(empleadoId, 
                new EmpleadoCreado(empleadoId, nombre!, documento!, cargo!, especialidad ?? "", valorHora, estado, usuarioId, usuarioNombre, DateTimeOffset.Now)
            );
            count++;
        }

        if (validationErrors.Any())
        {
            throw new Exception("Errores de validación:\n" + string.Join("\n", validationErrors.Take(10)));
        }

        await _session.SaveChangesAsync();
        _logger.LogInformation("Importación de empleados completada: {Count} empleados creados", count);
        return count;
    }

    protected virtual IQueryable<Empleado> InfoEmpleados()
    {
        return _session.Query<Empleado>();
    }
}
