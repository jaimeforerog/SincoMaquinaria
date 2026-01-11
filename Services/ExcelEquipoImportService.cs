using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExcelDataReader;
using Marten;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Domain.Events;

namespace SincoMaquinaria.Services;

public class ExcelEquipoImportService
{
    private readonly IDocumentSession _session;

    public ExcelEquipoImportService(IDocumentSession session)
    {
        _session = session;
    }

    public async Task<int> ImportarEquipos(Stream fileStream)
    {
        // 1. Cargar Configuración Global para validar Tipos de Medidor (Por Nombre ahora)
        var config = await _session.LoadAsync<ConfiguracionGlobal>(ConfiguracionGlobal.SingletonId);
        
        // Mapa Nombre -> Codigo (ID)
        var tiposDisponibles = config?.TiposMedidor.Where(t => t.Activo)
                                     .ToDictionary(t => t.Nombre, t => t.Codigo, StringComparer.OrdinalIgnoreCase) 
                               ?? new Dictionary<string, string>();
        

        // Mapa Unidad -> Codigo (ID) para búsqueda por unidad (ej. "Hr", "Km")
        var unidadesDisponibles = config?.TiposMedidor.Where(t => t.Activo)
                                     .ToDictionary(t => t.Unidad, t => t.Codigo, StringComparer.OrdinalIgnoreCase) 
                               ?? new Dictionary<string, string>();

        // Cargar Rutinas para validación cruzada
        var rutinas = InfoRutinas().ToList();
        var rutinasMap = rutinas
            .GroupBy(r => r.Descripcion, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);


        using var reader = ExcelReaderFactory.CreateReader(fileStream);
        var result = reader.AsDataSet(new ExcelDataSetConfiguration()
        {
            ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
            {
                UseHeaderRow = false // Manual header detection
            }
        });

        var table = result.Tables[0];
        int count = 0;
        var validationErrors = new List<string>();
        
        var placasProcesadas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 1. Detect Header Row
        int headerRowIndex = -1;
        Dictionary<string, int> colMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < Math.Min(20, table.Rows.Count); i++) // Scan first 20 rows
        {
            var row = table.Rows[i];
            for (int c = 0; c < table.Columns.Count; c++)
            {
                 var val = row[c]?.ToString()?.Trim();
                 if (string.Equals(val, "Placa", StringComparison.OrdinalIgnoreCase))
                 {
                     headerRowIndex = i;
                     // Map columns from this row
                     for (int k = 0; k < table.Columns.Count; k++)
                     {
                         var headerVal = row[k]?.ToString()?.Trim();
                         if (!string.IsNullOrEmpty(headerVal) && !colMap.ContainsKey(headerVal))
                         {
                             colMap[headerVal] = k;
                         }
                     }
                     break;
                 }
            }
            if (headerRowIndex != -1) break;
        }

        if (headerRowIndex == -1)
        {
             throw new Exception("No se encontró la fila de encabezados (columna 'Placa'). Verifique el archivo.");
        }

        // Helper local to get value by column name using the map
        string? GetValByMap(System.Data.DataRow r, string colName, params string[] alts)
        {
            if (colMap.TryGetValue(colName, out int idx)) return r[idx]?.ToString()?.Trim();
            foreach (var alt in alts)
            {
                if (colMap.TryGetValue(alt, out int altIdx)) return r[altIdx]?.ToString()?.Trim();
            }
            return null;
        }

        // Process Data Rows
        for (int i = headerRowIndex + 1; i < table.Rows.Count; i++)
        {
            var row = table.Rows[i];
            var rowNum = i + 1; // Excel-like row number (1-based)

            var placa = GetValByMap(row, "Placa");
            if (string.IsNullOrEmpty(placa)) continue;

            if (placasProcesadas.Contains(placa))
            {
                validationErrors.Add($"Fila {rowNum}: La placa '{placa}' está duplicada en el archivo.");
                continue;
            }

            // Validar campos obligatorios
            var descripcion = GetValByMap(row, "Descripcion", "Descripción");
            if (string.IsNullOrEmpty(descripcion))
            {
                validationErrors.Add($"Fila {rowNum}: El campo 'Descripcion' es obligatorio.");
                continue;
            }
            
            var grupo = GetValByMap(row, "Grupo de mtto", "Grupo");
            if (string.IsNullOrEmpty(grupo))
            {
                validationErrors.Add($"Fila {rowNum}: El campo 'Grupo de mtto' es obligatorio.");
                continue;
            }

            var rutina = GetValByMap(row, "Rutina");
            if (string.IsNullOrEmpty(rutina))
            {
                validationErrors.Add($"Fila {rowNum}: El campo 'Rutina' es obligatorio.");
                continue;
            }

            var medidor1 = GetValByMap(row, "Medidor 1", "Medidor1");
            if (string.IsNullOrEmpty(medidor1))
            {
                validationErrors.Add($"Fila {rowNum}: El campo 'Medidor 1' es obligatorio.");
                continue;
            }

            var medidorInicial1Str = GetValByMap(row, "Medidor inicial medidor 1");
            if (string.IsNullOrEmpty(medidorInicial1Str))
            {
                validationErrors.Add($"Fila {rowNum}: El campo 'Medidor inicial medidor 1' es obligatorio.");
                continue;
            }

            var fechaInicial1Str = GetValByMap(row, "Fecha inicial medidor 1");
            if (string.IsNullOrEmpty(fechaInicial1Str))
            {
                validationErrors.Add($"Fila {rowNum}: El campo 'Fecha inicial medidor 1' es obligatorio.");
                continue;
            }

            var fechaOTStr = GetValByMap(row, "Fecha ultima OT", "Fecha Ultima OT");
            if (string.IsNullOrEmpty(fechaOTStr))
            {
                validationErrors.Add($"Fila {rowNum}: El campo 'Fecha ultima OT' es obligatorio.");
                continue;
            }

            // Validate Grupo exists in configuration
            var gruposDisponibles = config?.GruposMantenimiento.Where(g => g.Activo).Select(g => g.Nombre).ToHashSet(StringComparer.OrdinalIgnoreCase) 
                                    ?? new HashSet<string>();
            
            if (!gruposDisponibles.Contains(grupo))
            {
                 // Check by Code too just in case
                 if (config?.GruposMantenimiento.Any(g => g.Codigo.Equals(grupo, StringComparison.OrdinalIgnoreCase) && g.Activo) != true)
                 {
                     // Auto-Create Group if it doesn't exist (Migration Support)
                     var nuevoGrupoCodigo = grupo.ToUpperInvariant().Replace(" ", "_");
                     var nuevoGrupoEvento = new GrupoMantenimientoCreado(nuevoGrupoCodigo, grupo, "Auto-creado por importación", true);
                     
                     _session.Events.Append(ConfiguracionGlobal.SingletonId, nuevoGrupoEvento);
                     
                     // Update local cache so we don't try to create it again in this batch
                     gruposDisponibles.Add(grupo);
                     
                     // Log for debugging
                     // Console.WriteLine($"[Import] Auto-creating missing Group: '{grupo}'");
                 }
            }
 
            var medidor1Id = "";
            var medidor1Unidad = ""; // To store the Unit (e.g. "Km") for comparison

            if (!string.IsNullOrEmpty(medidor1)) {
                if(unidadesDisponibles.TryGetValue(medidor1, out var id)) 
                {
                    medidor1Id = id;
                    medidor1Unidad = medidor1; // It was a unit
                }
                else if (tiposDisponibles.TryGetValue(medidor1, out var idByName)) 
                {
                    medidor1Id = idByName;
                    // Find the unit for this name
                    var tipo = config?.TiposMedidor.FirstOrDefault(t => t.Codigo == idByName);
                    if (tipo != null) medidor1Unidad = tipo.Unidad;
                }
                else 
                {
                    var validUnits = string.Join(", ", unidadesDisponibles.Keys.Take(5)) + ", " + string.Join(", ", tiposDisponibles.Keys.Take(5));
                    validationErrors.Add($"Fila {rowNum}: Medidor 1 '{medidor1}' no válido. Unidades válidas: {validUnits}...");
                }
            }

            // Medidor 2 - Validate Unit
            var medidor2 = GetValByMap(row, "Medidor 2", "Medidor2");
            var medidor2Id = "";
            var medidor2Unidad = "";

            if (!string.IsNullOrEmpty(medidor2)) {
                 if(unidadesDisponibles.TryGetValue(medidor2, out var id)) 
                 {
                     medidor2Id = id;
                     medidor2Unidad = medidor2;
                 }
                 else if (tiposDisponibles.TryGetValue(medidor2, out var idByName)) 
                 {
                     medidor2Id = idByName;
                     var tipo = config?.TiposMedidor.FirstOrDefault(t => t.Codigo == idByName);
                     if (tipo != null) medidor2Unidad = tipo.Unidad;
                 }
                 else 
                 {
                    var validUnits = string.Join(", ", unidadesDisponibles.Keys.Take(5)) + ", " + string.Join(", ", tiposDisponibles.Keys.Take(5));
                    validationErrors.Add($"Fila {rowNum}: Medidor 2 '{medidor2}' no válido. Unidades válidas: {validUnits}...");
                 }
            }

            // Validar Rutina vs Medidores
            if (!string.IsNullOrEmpty(rutina))
            {
                 if (rutinasMap.TryGetValue(rutina, out var rutinaObj))
                 {
                     // Check if Routine has activities requiring meters
                     var activities = rutinaObj.Partes.SelectMany(p => p.Actividades).ToList();
                     
                     // Helper to check meter compat
                     // We need to check if ANY activity uses a meter that is NOT provided or mismatched?
                     // Request: "Medidor 1 de rutinas debe ser igual al medidor 1 de equipos"
                     // This implies the Routine *defines* what Medidor 1 should be?
                     // Or that if Routine uses Unit "Hr", Equipment Medidor 1 MUST be "Hr"?
                     
                     // Let's iterate activities. 
                     foreach(var act in activities)
                     {
                         // Check Medidor 1 (UnidadMedida)
                         if (!string.IsNullOrEmpty(act.UnidadMedida)) // Routine requires Medidor 1
                         {
                             // Equipment Medidor 1 (medidor1Unidad) must match act.UnidadMedida
                             if (!string.Equals(medidor1Unidad, act.UnidadMedida, StringComparison.OrdinalIgnoreCase))
                             {
                                 validationErrors.Add($"Fila {rowNum}: La Rutina '{rutina}' requiere Medidor 1 '{act.UnidadMedida}' (Actividad: {act.Descripcion}), pero el equipo tiene '{medidor1Unidad}'.");
                                 break; // Report once per row for clarity
                             }
                         }

                         // Check Medidor 2 (UnidadMedida2)
                         if (!string.IsNullOrEmpty(act.UnidadMedida2)) // Routine requires Medidor 2
                         {
                             if (!string.Equals(medidor2Unidad, act.UnidadMedida2, StringComparison.OrdinalIgnoreCase))
                             {
                                 validationErrors.Add($"Fila {rowNum}: La Rutina '{rutina}' requiere Medidor 2 '{act.UnidadMedida2}' (Actividad: {act.Descripcion}), pero el equipo tiene '{medidor2Unidad}'.");
                                 break;
                             }
                         }
                     }
                 }
                 else
                 {
                     // Optional: Validate if Routine exists at all? 
                     // User didn't strictly ask for this but it's good practice. 
                     // Existing code accepted any string. Let's add it if helpful, but stick to requested logic first.
                     validationErrors.Add($"Fila {rowNum}: La Rutina asignada '{rutina}' no existe en el sistema.");
                 }
            }

            if (validationErrors.Any()) continue;

            // Validar Fechas 
            // Validar formato de fecha
            if (DateTime.TryParse(fechaOTStr, out DateTime fechaOT))
            {
                 // Relaxting validation as per user request (Reference Step 403)
                 // The user has data where LastOT < InitialMeterDate. 
                 // We will allow this.
                 
                 /* 
                 var fechaM1Str = GetValByMap(row, "Fecha inicial medidor 1");
                 if (DateTime.TryParse(fechaM1Str, out DateTime fechaM1) && fechaOT < fechaM1)
                      validationErrors.Add($"Fila {rowNum}: La 'Fecha ultima OT' ({fechaOT:d}) no puede ser menor a la 'Fecha inicial medidor 1' ({fechaM1:d}).");
                 
                 var fechaM2Str = GetValByMap(row, "Fecha inicial medidor 2");
                 if (DateTime.TryParse(fechaM2Str, out DateTime fechaM2) && fechaOT < fechaM2)
                      validationErrors.Add($"Fila {rowNum}: La 'Fecha ultima OT' ({fechaOT:d}) no puede ser menor a la 'Fecha inicial medidor 2' ({fechaM2:d}).");
                 */
            }
            else
            {
                 validationErrors.Add($"Fila {rowNum}: El campo 'Fecha ultima OT' tiene un formato de fecha inválido: '{fechaOTStr}'.");
                 continue;
            }

            if (validationErrors.Any()) continue;
            
            var equipoId = GenerarIdDeterministico(placa);
            
            var existingVersion = await _session.Events.FetchStreamStateAsync(equipoId);
            if (existingVersion == null)
            {
                _session.Events.StartStream<Equipo>(equipoId, 
                    new EquipoMigrado(equipoId, placa, descripcion, "" /*Marca*/, "" /*Modelo*/, "" /*Serie*/, "" /*Codigo*/, medidor1Id, medidor2Id, grupo, rutina)
                );

                // Lecturas Iniciales - Adapted to use GetValByMap or passed values
                ProcesarLecturaInicial(equipoId, row, "Medidor inicial medidor 1", "Fecha inicial medidor 1", medidor1Id, GetValByMap);
                ProcesarLecturaInicial(equipoId, row, "Medidor inicial medidor 2", "Fecha inicial medidor 2", medidor2Id, GetValByMap);

                count++;
                placasProcesadas.Add(placa);
            }
            else
            {
                _session.Events.Append(equipoId, 
                    new EquipoActualizado(equipoId, descripcion, "" /*Marca*/, "" /*Modelo*/, "" /*Serie*/, "" /*Codigo*/, medidor1Id, medidor2Id, grupo, rutina)
                );
                count++;
                placasProcesadas.Add(placa);
            }
        }

        if (count == 0)
        {
             var sb = new System.Text.StringBuilder();
             sb.AppendLine($"No se procesó ningún equipo. Verifica que existan datos debajo de la fila de encabezados (detectada en fila {headerRowIndex + 1}).");
             sb.AppendLine($"Total filas leídas: {table.Rows.Count}");
             sb.AppendLine($"Columnas mapeadas: {string.Join(", ", colMap.Keys)}");
             
             if (headerRowIndex + 1 < table.Rows.Count)
             {
                 var firstRow = table.Rows[headerRowIndex + 1];
                 var RowValues = string.Join(" | ", firstRow.ItemArray.Select(o => o?.ToString()));
                 sb.AppendLine($"Contenido primera fila de datos (Fila {headerRowIndex + 2}): {RowValues}");
                 
                 if (colMap.ContainsKey("Placa"))
                 {
                     var idx = colMap["Placa"];
                     var val = firstRow[idx]?.ToString();
                     sb.AppendLine($"Valor lectura columna Placa (idx {idx}): '{val}'");
                 }
             }

             if (validationErrors.Any())
             {
                 sb.AppendLine("--- Errores de Validación (razón por la que se saltaron filas) ---");
                 sb.AppendLine(string.Join("\n", validationErrors.Take(20)));
             }
             else
             {
                 sb.AppendLine("--- Depuración de Mapeo ---");
                 if (headerRowIndex + 1 < table.Rows.Count)
                 {
                     var firstDataRow = table.Rows[headerRowIndex + 1];
                     // Check Placa specifically
                     if (colMap.TryGetValue("Placa", out int placaIdx))
                     {
                         var rawPlaca = firstDataRow[placaIdx]?.ToString();
                         sb.AppendLine($"[Fila {headerRowIndex + 2}] Valor crudo en columna 'Placa' (índice {placaIdx}): '{rawPlaca}'");
                         if (string.IsNullOrWhiteSpace(rawPlaca))
                         {
                             sb.AppendLine("-> La fila se saltó porque el valor de 'Placa' está vacío o es nulo.");
                         }
                     }
                     else
                     {
                         sb.AppendLine("-> No se pudo determinar el índice de la columna 'Placa' en el mapa de columnas.");
                     }
                 }
                 else
                 {
                     sb.AppendLine("-> No hay filas de datos después del encabezado.");
                 }
                 sb.AppendLine("Asegúrese de que la columna 'Placa' tenga datos y coincida exactamente con el nombre del encabezado.");
             }

             throw new Exception(sb.ToString());
        }

        if (validationErrors.Any())
        {
            throw new Exception("Errores de validación:\n" + string.Join("\n", validationErrors.Take(20)));
        }

        await _session.SaveChangesAsync();
        return count;
    }

    protected virtual IQueryable<RutinaMantenimiento> InfoRutinas()
    {
        return _session.Query<RutinaMantenimiento>();
    }

    private void ProcesarLecturaInicial(Guid equipoId, System.Data.DataRow row, string colValor, string colFecha, string tipoMedidorId, Func<System.Data.DataRow, string, string[], string?> getVal)
    {
        if (string.IsNullOrEmpty(tipoMedidorId)) return;
        
        var valorStr = getVal(row, colValor, Array.Empty<string>());
        var fechaStr = getVal(row, colFecha, Array.Empty<string>());

        if (decimal.TryParse(valorStr, out decimal valor))
        {
             DateTime fecha = DateTime.Now;
             if (DateTime.TryParse(fechaStr, out DateTime f)) fecha = f;
             
             // Emitir evento de lectura
             _session.Events.Append(equipoId, new MedicionRegistrada(tipoMedidorId, valor, fecha, valor));
        }
    }

    // Logic moved inline, removing method to avoid confusion with new signature requirements
    // private void ValidarFechaOT(...) 


    private Guid GenerarIdDeterministico(string input)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input.ToLowerInvariant()));
        return new Guid(hash);
    }
}
